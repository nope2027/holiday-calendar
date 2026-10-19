// 휴장 캘린더 — 바탕화면 위젯 (WinForms + WebView2)
// 화면(app\index.html)을 테두리 없는 창에 띄우고, 항상 위·위치 기억·트레이·자동 실행을 맡는다.
using System;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using Microsoft.Win32;

[assembly: System.Reflection.AssemblyTitle("휴장 캘린더")]
[assembly: System.Reflection.AssemblyProduct("휴장 캘린더")]
[assembly: System.Reflection.AssemblyDescription("한·중·일·미 휴장일 바탕화면 위젯")]
[assembly: System.Reflection.AssemblyVersion("1.0.0.0")]
[assembly: System.Reflection.AssemblyFileVersion("1.0.0.0")]

namespace HolidayCalendar
{
    static class Program
    {
        public const string AppTitle = "휴장 캘린더";
        public static readonly string DataDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "HolidayCalendar");
        public static EventWaitHandle ShowEvent;

        [STAThread]
        static void Main()
        {
            // 화면 배율(DPI)을 모니터별로 정확히 — 창을 만들기 전에 호출해야 함
            try { Native.SetProcessDpiAwarenessContext(new IntPtr(-4)); }
            catch { try { Native.SetProcessDPIAware(); } catch { } }

            bool first;
            using (var mutex = new Mutex(true, "HolidayCalendar_SingleInstance_v1", out first))
            {
                if (!first)
                {
                    // 이미 실행 중 → 기존 창을 앞으로 불러냄
                    try { EventWaitHandle.OpenExisting("HolidayCalendar_Show_v1").Set(); } catch { }
                    return;
                }
                ShowEvent = new EventWaitHandle(false, EventResetMode.AutoReset, "HolidayCalendar_Show_v1");
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.ThreadException += (s, e) => Log.Write(e.Exception);
                AppDomain.CurrentDomain.UnhandledException += (s, e) => Log.Write(e.ExceptionObject as Exception);
                Application.Run(new WidgetForm());
            }
        }
    }

    static class Log
    {
        public static void Write(Exception ex) { Write(ex == null ? "(null)" : ex.ToString()); }
        public static void Write(string text)
        {
            try
            {
                Directory.CreateDirectory(Program.DataDir);
                File.AppendAllText(Path.Combine(Program.DataDir, "error.log"),
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "  " + text + Environment.NewLine, Encoding.UTF8);
            }
            catch { }
        }
    }

    // 설정: %LOCALAPPDATA%\HolidayCalendar\settings.ini
    class Settings
    {
        public int X = int.MinValue, Y = int.MinValue;
        public bool Pin = true;
        public double Zoom = 1.15;
        public string Layout = "v";
        public int W, H;              // 창 크기(화면 배율·글자 크기 적용 전, CSS px). 0 = 기본값
        public bool TrayTipShown;
        public bool Compact;          // 시계만 보기
        public int CW, CH;            // 시계만 보기 창 크기(CSS px)

        static string FilePath { get { return Path.Combine(Program.DataDir, "settings.ini"); } }

        public static Settings Load()
        {
            var s = new Settings();
            try
            {
                if (!File.Exists(FilePath)) return s;
                foreach (var raw in File.ReadAllLines(FilePath, Encoding.UTF8))
                {
                    int eq = raw.IndexOf('=');
                    if (eq <= 0) continue;
                    string k = raw.Substring(0, eq).Trim(), v = raw.Substring(eq + 1).Trim();
                    int iv; double dv;
                    switch (k)
                    {
                        case "x": if (int.TryParse(v, NumberStyles.Integer, CultureInfo.InvariantCulture, out iv)) s.X = iv; break;
                        case "y": if (int.TryParse(v, NumberStyles.Integer, CultureInfo.InvariantCulture, out iv)) s.Y = iv; break;
                        case "pin": s.Pin = v == "1"; break;
                        case "zoom":
                            if (double.TryParse(v, NumberStyles.Float, CultureInfo.InvariantCulture, out dv) && dv >= 0.7 && dv <= 1.6) s.Zoom = dv;
                            break;
                        case "layout": s.Layout = v == "h" ? "h" : "v"; break;
                        case "w": if (int.TryParse(v, NumberStyles.Integer, CultureInfo.InvariantCulture, out iv)) s.W = iv; break;
                        case "h": if (int.TryParse(v, NumberStyles.Integer, CultureInfo.InvariantCulture, out iv)) s.H = iv; break;
                        case "traytip": s.TrayTipShown = v == "1"; break;
                        case "compact": s.Compact = v == "1"; break;
                        case "cw": if (int.TryParse(v, NumberStyles.Integer, CultureInfo.InvariantCulture, out iv)) s.CW = iv; break;
                        case "ch": if (int.TryParse(v, NumberStyles.Integer, CultureInfo.InvariantCulture, out iv)) s.CH = iv; break;
                    }
                }
            }
            catch (Exception ex) { Log.Write(ex); }
            return s;
        }

        public void Save()
        {
            try
            {
                Directory.CreateDirectory(Program.DataDir);
                var sb = new StringBuilder();
                sb.AppendLine("x=" + X.ToString(CultureInfo.InvariantCulture));
                sb.AppendLine("y=" + Y.ToString(CultureInfo.InvariantCulture));
                sb.AppendLine("pin=" + (Pin ? "1" : "0"));
                sb.AppendLine("zoom=" + Zoom.ToString("0.###", CultureInfo.InvariantCulture));
                sb.AppendLine("layout=" + Layout);
                sb.AppendLine("w=" + W.ToString(CultureInfo.InvariantCulture));
                sb.AppendLine("h=" + H.ToString(CultureInfo.InvariantCulture));
                sb.AppendLine("traytip=" + (TrayTipShown ? "1" : "0"));
                sb.AppendLine("compact=" + (Compact ? "1" : "0"));
                sb.AppendLine("cw=" + CW.ToString(CultureInfo.InvariantCulture));
                sb.AppendLine("ch=" + CH.ToString(CultureInfo.InvariantCulture));
                File.WriteAllText(FilePath, sb.ToString(), Encoding.UTF8);
            }
            catch (Exception ex) { Log.Write(ex); }
        }
    }

    static class AutoStart
    {
        const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
        const string ValueName = "HolidayCalendar";
        static string Command { get { return "\"" + Application.ExecutablePath + "\""; } }

        public static bool IsOn()
        {
            try
            {
                using (var k = Registry.CurrentUser.OpenSubKey(RunKey, false))
                {
                    var v = k == null ? null : k.GetValue(ValueName) as string;
                    return v != null && string.Equals(v.Trim(), Command, StringComparison.OrdinalIgnoreCase);
                }
            }
            catch { return false; }
        }

        public static void Set(bool on)
        {
            try
            {
                using (var k = Registry.CurrentUser.CreateSubKey(RunKey))
                {
                    if (on) k.SetValue(ValueName, Command, RegistryValueKind.String);
                    else if (k.GetValue(ValueName) != null) k.DeleteValue(ValueName, false);
                }
            }
            catch (Exception ex) { Log.Write(ex); }
        }
    }

    class WidgetForm : Form
    {
        static readonly Size BaseVertical = new Size(520, 900);
        static readonly Size BaseHorizontal = new Size(960, 600);
        static readonly Size MinLogical = new Size(400, 560);
        const int ResizeBandLogical = 6;   // 창 가장자리에서 크기 조절이 잡히는 폭
        int band = 6;
        const string HostName = "holiday-calendar.example";

        readonly Settings st;
        readonly WebView2 web;
        readonly NotifyIcon tray;
        readonly ToolStripMenuItem miPin, miAuto, miCompact;
        readonly System.Windows.Forms.Timer saveTimer;
        bool sizing, ready;

        public WidgetForm()
        {
            st = Settings.Load();

            Text = Program.AppTitle;
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.Manual;
            ShowInTaskbar = false;
            MaximizeBox = false;
            MinimizeBox = false;
            AutoScaleMode = AutoScaleMode.None;
            bool dark = SystemUsesDarkTheme();
            BackColor = dark ? Color.FromArgb(22, 23, 27) : Color.White;
            Icon = LoadAppIcon(new Size(32, 32));

            web = new WebView2();
            web.Dock = DockStyle.Fill;
            web.DefaultBackgroundColor = BackColor;
            Controls.Add(web);

            // 알림 영역(트레이) 아이콘
            var menu = new ContextMenuStrip();
            var miOpen = new ToolStripMenuItem("휴장 캘린더 열기", null, (s, e) => ShowWidget());
            miOpen.Font = new Font(miOpen.Font, FontStyle.Bold);
            miPin = new ToolStripMenuItem("항상 위에 표시", null, (s, e) => SetPin(!st.Pin));
            miAuto = new ToolStripMenuItem("Windows 시작할 때 자동 실행", null, (s, e) => SetAutoStart(!AutoStart.IsOn()));
            var miReset = new ToolStripMenuItem("위치 초기화", null, (s, e) => { ResetPosition(); ShowWidget(); });
            miCompact = new ToolStripMenuItem("시계만 보기 (두 번 클릭)", null, (s, e) => { ShowWidget(); PostToPage("{\"type\":\"toggleCompact\"}"); });
            var miExit = new ToolStripMenuItem("종료", null, (s, e) => ExitApp());
            menu.Items.AddRange(new ToolStripItem[] { miOpen, new ToolStripSeparator(), miCompact, miPin, miAuto, miReset, new ToolStripSeparator(), miExit });
            menu.Opening += (s, e) => { miPin.Checked = st.Pin; miAuto.Checked = AutoStart.IsOn(); miCompact.Checked = st.Compact; };

            tray = new NotifyIcon();
            tray.Text = Program.AppTitle;
            tray.Icon = LoadAppIcon(SystemInformation.SmallIconSize) ?? Icon ?? SystemIcons.Application;
            tray.ContextMenuStrip = menu;
            tray.Visible = true;
            tray.MouseClick += (s, e) => { if (e.Button == MouseButtons.Left) ToggleWidget(); };

            saveTimer = new System.Windows.Forms.Timer();
            saveTimer.Interval = 600;
            saveTimer.Tick += (s, e) => { saveTimer.Stop(); SavePosition(); };

            TopMost = st.Pin;

            // 두 번째 실행 → 이 창을 보여줌
            if (Program.ShowEvent != null)
                ThreadPool.RegisterWaitForSingleObject(Program.ShowEvent, (state, timedOut) =>
                {
                    try { if (IsHandleCreated) BeginInvoke(new Action(ShowWidget)); } catch { }
                }, null, -1, false);
        }

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ClassStyle |= 0x00020000; // CS_DROPSHADOW
                return cp;
            }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            try { int pref = 2; Native.DwmSetWindowAttribute(Handle, 33, ref pref, 4); } catch { } // Windows 11 둥근 모서리
            ApplySize();
            PlaceWindow();
            EnsureOnScreen();
        }

        protected override async void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            try
            {
                string udf = Path.Combine(Program.DataDir, "WebView2");
                var env = await CoreWebView2Environment.CreateAsync(null, udf, null);
                await web.EnsureCoreWebView2Async(env);
                var core = web.CoreWebView2;
                var cs = core.Settings;
                cs.AreDefaultContextMenusEnabled = false;
                cs.AreDevToolsEnabled = false;
                cs.IsStatusBarEnabled = false;
                cs.IsZoomControlEnabled = false;
                cs.IsPinchZoomEnabled = false;
                cs.IsBuiltInErrorPageEnabled = true;
                try { cs.AreBrowserAcceleratorKeysEnabled = false; } catch { }
                core.SetVirtualHostNameToFolderMapping(HostName, Path.Combine(AppDir, "app"), CoreWebView2HostResourceAccessKind.Allow);
                core.WebMessageReceived += OnWebMessage;
                core.NewWindowRequested += (s, a) => { a.Handled = true; };
                core.NavigationStarting += (s, a) =>
                {
                    if (!a.Uri.StartsWith("https://" + HostName + "/", StringComparison.OrdinalIgnoreCase)) a.Cancel = true;
                };
                web.ZoomFactor = st.Zoom;
                core.Navigate("https://" + HostName + "/index.html");
            }
            catch (WebView2RuntimeNotFoundException)
            {
                var r = MessageBox.Show(
                    "이 프로그램은 Microsoft Edge WebView2 런타임이 필요합니다.\n\n" +
                    "[예] 설치 페이지 열기\n[아니요] 대신 웹 브라우저로 달력 열기",
                    Program.AppTitle, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Information);
                if (r == DialogResult.Yes) OpenExternal("https://developer.microsoft.com/microsoft-edge/webview2/");
                else if (r == DialogResult.No) OpenExternal(Path.Combine(AppDir, "app", "index.html"));
                ExitApp();
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                MessageBox.Show("화면을 불러오지 못했습니다.\n" + ex.Message + "\n\n자세한 내용: " + Path.Combine(Program.DataDir, "error.log"),
                    Program.AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        static string AppDir { get { return AppDomain.CurrentDomain.BaseDirectory; } }

        // ── 화면(웹)과 주고받기 ──
        void OnWebMessage(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            string json;
            try { json = e.WebMessageAsJson; } catch { return; }
            string type = JsonString(json, "type");
            switch (type)
            {
                case "ready": ready = true; PushState(); break;
                case "drag": DragWindow(); break;
                case "pin": SetPin(JsonBool(json, "value")); break;
                case "autostart": SetAutoStart(JsonBool(json, "value")); break;
                case "zoom":
                    double z;
                    if (double.TryParse(JsonRaw(json, "value"), NumberStyles.Float, CultureInfo.InvariantCulture, out z) && z >= 0.7 && z <= 1.6)
                    {
                        st.Zoom = z; web.ZoomFactor = z; ApplySize(); EnsureOnScreen(); st.Save(); PushState();
                    }
                    break;
                case "layout":
                    st.Layout = JsonString(json, "value") == "h" ? "h" : "v";
                    var preset = st.Layout == "h" ? BaseHorizontal : BaseVertical;
                    int pw, ph;
                    st.W = int.TryParse(JsonRaw(json, "w"), NumberStyles.Integer, CultureInfo.InvariantCulture, out pw) && pw >= MinLogical.Width ? pw : preset.Width;
                    st.H = int.TryParse(JsonRaw(json, "h"), NumberStyles.Integer, CultureInfo.InvariantCulture, out ph) && ph >= MinLogical.Height ? ph : preset.Height;
                    ApplySize(); EnsureOnScreen(); st.Save(); PushState();
                    break;
                case "resizeBy":
                    // 화면 쪽에서 패널을 켜고 끌 때 창 너비를 그만큼 늘리거나 줄임 (글자 크기 적용 전 px)
                    int dw;
                    if (int.TryParse(JsonRaw(json, "dw"), NumberStyles.Integer, CultureInfo.InvariantCulture, out dw) && dw != 0 && Math.Abs(dw) < 2000)
                    {
                        var cur = LogicalSize();
                        st.W = Math.Max(MinLogical.Width, cur.Width + dw);
                        st.H = cur.Height;
                        int oldRight = Right;
                        ApplySize();
                        // 오른쪽 끝에 붙여 둔 창이면 왼쪽으로 넓어지도록
                        Rectangle wa = Screen.FromRectangle(Bounds).WorkingArea;
                        if (oldRight >= wa.Right - (int)(40 * DpiScale())) Location = new Point(oldRight - Width, Top);
                        EnsureOnScreen(); st.Layout = st.W >= 860 ? "h" : "v"; st.Save(); PushState();
                    }
                    break;
                case "resize":
                    int dir = 0;
                    switch (JsonString(json, "edge"))
                    {
                        case "l": dir = 1; break; case "r": dir = 2; break; case "t": dir = 3; break;
                        case "tl": dir = 4; break; case "tr": dir = 5; break; case "b": dir = 6; break;
                        case "bl": dir = 7; break; case "br": dir = 8; break;
                    }
                    if (dir > 0) StartResize(dir);
                    break;
                case "bg":
                    var mm = Regex.Match(JsonString(json, "value"), @"rgba?\(\s*(\d+)\s*,\s*(\d+)\s*,\s*(\d+)");
                    if (mm.Success)
                    {
                        var c = Color.FromArgb(Math.Min(255, int.Parse(mm.Groups[1].Value)), Math.Min(255, int.Parse(mm.Groups[2].Value)), Math.Min(255, int.Parse(mm.Groups[3].Value)));
                        BackColor = c;
                        try { web.DefaultBackgroundColor = c; } catch { }
                    }
                    break;
                case "compact":
                    bool on = JsonBool(json, "value");
                    int cw, ch;
                    if (on && int.TryParse(JsonRaw(json, "w"), NumberStyles.Integer, CultureInfo.InvariantCulture, out cw) &&
                        int.TryParse(JsonRaw(json, "h"), NumberStyles.Integer, CultureInfo.InvariantCulture, out ch) &&
                        cw >= 160 && ch >= 50 && cw <= 2000 && ch <= 1200) { st.CW = cw; st.CH = ch; }
                    SetCompact(on);
                    break;
                case "hide": HideWidget(); break;
                case "resetPosition": ResetPosition(); EnsureOnScreen(); break;
                case "exit": ExitApp(); break;
            }
        }

        // Esc 는 WebView2 가 '가속 키'로 따로 넘기므로 화면에 직접 전달
        bool ForwardEscape()
        {
            if (web.CoreWebView2 == null) return false;
            try { web.CoreWebView2.PostWebMessageAsJson("{\"type\":\"key\",\"key\":\"Escape\"}"); return true; }
            catch (Exception ex) { Log.Write(ex); return false; }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape && ForwardEscape()) return true;
            return base.ProcessCmdKey(ref msg, keyData);
        }

        protected override bool ProcessDialogKey(Keys keyData)
        {
            if (keyData == Keys.Escape && ForwardEscape()) return true;
            return base.ProcessDialogKey(keyData);
        }

        void PushState()
        {
            if (!ready || web.CoreWebView2 == null) return;
            string json = "{\"type\":\"state\",\"pin\":" + (st.Pin ? "true" : "false") +
                ",\"autostart\":" + (AutoStart.IsOn() ? "true" : "false") +
                ",\"zoom\":" + st.Zoom.ToString("0.###", CultureInfo.InvariantCulture) +
                ",\"layout\":\"" + st.Layout + "\"" +
                ",\"w\":" + LogicalSize().Width.ToString(CultureInfo.InvariantCulture) +
                ",\"h\":" + LogicalSize().Height.ToString(CultureInfo.InvariantCulture) +
                ",\"compact\":" + (st.Compact ? "true" : "false") + "}";
            try { web.CoreWebView2.PostWebMessageAsJson(json); } catch (Exception ex) { Log.Write(ex); }
        }

        static string JsonRaw(string json, string key)
        {
            var m = Regex.Match(json ?? "", "\"" + key + "\"\\s*:\\s*(\"(?<s>[^\"]*)\"|(?<v>[^,}\\s]+))");
            if (!m.Success) return "";
            return m.Groups["s"].Success ? m.Groups["s"].Value : m.Groups["v"].Value;
        }
        static string JsonString(string json, string key) { return JsonRaw(json, key); }
        static bool JsonBool(string json, string key) { return JsonRaw(json, key) == "true"; }

        // ── 창 동작 ──
        void SetPin(bool on)
        {
            st.Pin = on;
            TopMost = on;
            st.Save();
            PushState();
        }

        void PostToPage(string json)
        {
            if (!ready || web.CoreWebView2 == null) return;
            try { web.CoreWebView2.PostWebMessageAsJson(json); } catch (Exception ex) { Log.Write(ex); }
        }

        // 시계만 보기 ↔ 전체: 왼쪽 위(화면 오른쪽 끝에 붙어 있으면 오른쪽 위)를 기준으로 크기만 바꿈
        void SetCompact(bool on)
        {
            if (on && (st.CW <= 0 || st.CH <= 0)) { st.CW = 440; st.CH = 120; }
            Rectangle wa = Screen.FromRectangle(Bounds).WorkingArea;
            int oldRight = Right;
            bool dockRight = oldRight >= wa.Right - (int)(40 * DpiScale());
            st.Compact = on;
            ApplySize();
            if (dockRight) Location = new Point(oldRight - Width, Top);
            EnsureOnScreen();
            st.X = Left; st.Y = Top;
            st.Save();
            PushState();
        }

        void SetAutoStart(bool on)
        {
            AutoStart.Set(on);
            PushState();
        }

        // 화면 오른쪽 아래 손잡이 등에서 요청 → Windows 기본 크기 조절 시작
        void StartResize(int wmsz)
        {
            Native.ReleaseCapture();
            Native.SendMessage(Handle, 0x0112 /* WM_SYSCOMMAND */, new IntPtr(0xF000 + wmsz) /* SC_SIZE + 방향 */, IntPtr.Zero);
            RememberSize();
        }

        // 지금 창 크기를 '글자 크기·화면 배율 적용 전' 크기로 저장
        void RememberSize()
        {
            if (!IsHandleCreated || WindowState != FormWindowState.Normal) return;
            if (st.Compact) { SavePosition(); return; }
            float f = DpiScale() * (float)st.Zoom;
            if (f <= 0) return;
            st.W = (int)Math.Round(Width / f);
            st.H = (int)Math.Round(Height / f);
            st.Layout = st.W >= 860 ? "h" : "v";
            st.X = Left; st.Y = Top;
            st.Save();
            PushState();
        }

        protected override void OnResizeEnd(EventArgs e)
        {
            base.OnResizeEnd(e);
            RememberSize();
        }

        void DragWindow()
        {
            Native.ReleaseCapture();
            Native.SendMessage(Handle, 0x00A1 /* WM_NCLBUTTONDOWN */, new IntPtr(2) /* HTCAPTION */, IntPtr.Zero);
            SavePosition();
        }

        void ShowWidget()
        {
            if (!Visible) Show();
            if (WindowState != FormWindowState.Normal) WindowState = FormWindowState.Normal;
            EnsureOnScreen();
            TopMost = st.Pin;
            Activate();
            Native.SetForegroundWindow(Handle);
        }

        void HideWidget()
        {
            Hide();
            if (!st.TrayTipShown)
            {
                st.TrayTipShown = true; st.Save();
                try { tray.ShowBalloonTip(4000, Program.AppTitle, "화면 오른쪽 아래 알림 영역의 아이콘을 누르면 다시 열립니다.", ToolTipIcon.Info); } catch { }
            }
        }

        void ToggleWidget()
        {
            if (Visible) HideWidget(); else ShowWidget();
        }

        void ExitApp()
        {
            SavePosition();
            try { tray.Visible = false; } catch { }
            Close();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            SavePosition();
            try { tray.Visible = false; tray.Dispose(); } catch { }
            base.OnFormClosing(e);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            Application.Exit();
        }

        // ── 크기·위치 ──
        float DpiScale()
        {
            int dpi = 96;
            try { if (IsHandleCreated) dpi = (int)Native.GetDpiForWindow(Handle); } catch { }
            if (dpi <= 0) dpi = 96;
            return dpi / 96f;
        }

        void ApplySize() { ApplySize(DpiScale()); }

        Size LogicalSize()
        {
            var b = st.Layout == "h" ? BaseHorizontal : BaseVertical;
            int w = st.W >= MinLogical.Width ? st.W : b.Width;
            int h = st.H >= MinLogical.Height ? st.H : b.Height;
            return new Size(w, h);
        }

        void ApplySize(float dpiScale)
        {
            if (st.Compact && st.CW > 0 && st.CH > 0)
            {
                double fc = dpiScale * st.Zoom;
                band = Math.Max(4, (int)Math.Round(ResizeBandLogical * dpiScale));
                int cw = (int)Math.Round(st.CW * fc) + 2 * band, ch = (int)Math.Round(st.CH * fc) + 2 * band;
                sizing = true;
                Padding = new Padding(band);
                MinimumSize = new Size(Math.Min(cw, 120), Math.Min(ch, 40));
                Size = new Size(cw, ch);
                sizing = false;
                return;
            }
            var L = LogicalSize();
            double f = dpiScale * st.Zoom;
            int w = (int)Math.Round(L.Width * f), h = (int)Math.Round(L.Height * f);
            Rectangle wa = Screen.FromPoint(new Point(Left + 20, Top + 20)).WorkingArea;
            w = Math.Min(w, wa.Width - 8);
            h = Math.Min(h, wa.Height - 8);
            int mw = Math.Min((int)Math.Round(MinLogical.Width * f), w), mh = Math.Min((int)Math.Round(MinLogical.Height * f), h);
            band = Math.Max(4, (int)Math.Round(ResizeBandLogical * dpiScale));
            sizing = true;
            Padding = new Padding(band);
            MinimumSize = new Size(mw, mh);
            Size = new Size(w, h);
            sizing = false;
        }

        void PlaceWindow()
        {
            if (st.X != int.MinValue && st.Y != int.MinValue && IsVisibleSomewhere(new Rectangle(st.X, st.Y, Width, Height)))
                Location = new Point(st.X, st.Y);
            else
                ResetPosition();
        }

        void ResetPosition()
        {
            Rectangle wa = Screen.PrimaryScreen.WorkingArea;
            int margin = (int)(24 * DpiScale());
            Location = new Point(wa.Right - Width - margin, wa.Top + margin);
            SavePosition();
        }

        static bool IsVisibleSomewhere(Rectangle r)
        {
            foreach (var s in Screen.AllScreens)
            {
                var i = Rectangle.Intersect(s.WorkingArea, r);
                if (i.Width >= 120 && i.Height >= 60) return true;
            }
            return false;
        }

        void EnsureOnScreen()
        {
            Rectangle wa = Screen.FromRectangle(Bounds).WorkingArea;
            int x = Math.Max(wa.Left, Math.Min(Left, wa.Right - Width));
            int y = Math.Max(wa.Top, Math.Min(Top, wa.Bottom - Height));
            if (x != Left || y != Top) Location = new Point(x, y);
        }

        void SavePosition()
        {
            if (!IsHandleCreated || WindowState != FormWindowState.Normal) return;
            st.X = Left; st.Y = Top;
            st.Save();
        }

        protected override void OnLocationChanged(EventArgs e)
        {
            base.OnLocationChanged(e);
            if (IsHandleCreated && !sizing) { saveTimer.Stop(); saveTimer.Start(); }
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            if (WindowState == FormWindowState.Maximized) BeginInvoke(new Action(() => { WindowState = FormWindowState.Normal; ApplySize(); PlaceWindow(); }));
        }

        protected override void WndProc(ref Message m)
        {
            const int WM_SYSCOMMAND = 0x0112, WM_NCLBUTTONDBLCLK = 0x00A3, WM_DPICHANGED = 0x02E0, WM_SETTINGCHANGE = 0x001A, WM_NCHITTEST = 0x0084;
            if (m.Msg == WM_NCLBUTTONDBLCLK) { PostToPage("{\"type\":\"toggleCompact\"}"); return; } // 최대화 대신 시계만 보기
            if (m.Msg == WM_NCHITTEST)
            {
                // 창 가장자리(화면이 없는 얇은 띠)를 잡으면 크기 조절
                base.WndProc(ref m);
                if (m.Result.ToInt64() == 1 /* HTCLIENT */ && WindowState == FormWindowState.Normal && !st.Compact)
                {
                    long lp = m.LParam.ToInt64();
                    var cp = PointToClient(new Point((short)(lp & 0xFFFF), (short)((lp >> 16) & 0xFFFF)));
                    int W = ClientSize.Width, H = ClientSize.Height, k = band * 3;
                    bool l = cp.X < band, r = cp.X >= W - band, t = cp.Y < band, b = cp.Y >= H - band;
                    bool lk = cp.X < k, rk = cp.X >= W - k, tk = cp.Y < k, bk = cp.Y >= H - k;
                    int ht = 0;
                    if ((t && lk) || (l && tk)) ht = 13;        // HTTOPLEFT
                    else if ((t && rk) || (r && tk)) ht = 14;   // HTTOPRIGHT
                    else if ((b && lk) || (l && bk)) ht = 16;   // HTBOTTOMLEFT
                    else if ((b && rk) || (r && bk)) ht = 17;   // HTBOTTOMRIGHT
                    else if (l) ht = 10; else if (r) ht = 11; else if (t) ht = 12; else if (b) ht = 15;
                    if (ht != 0) m.Result = new IntPtr(ht);
                }
                return;
            }
            if (m.Msg == WM_SYSCOMMAND)
            {
                int cmd = m.WParam.ToInt32() & 0xFFF0;
                if (cmd == 0xF030 /* SC_MAXIMIZE */) return;
                if (cmd == 0xF020 /* SC_MINIMIZE */) { HideWidget(); return; }
            }
            if (m.Msg == WM_DPICHANGED)
            {
                int dpi = m.WParam.ToInt32() & 0xFFFF;
                var rc = (Native.RECT)Marshal.PtrToStructure(m.LParam, typeof(Native.RECT));
                Location = new Point(rc.Left, rc.Top);
                ApplySize(dpi / 96f);
                m.Result = IntPtr.Zero;
                return;
            }
            base.WndProc(ref m);
            if (m.Msg == WM_SETTINGCHANGE && ready)
            {
                // 작업 표시줄·테마가 바뀌면 화면 밖으로 나가지 않게
                BeginInvoke(new Action(() => { EnsureOnScreen(); }));
            }
        }

        static Icon LoadAppIcon(Size size)
        {
            try
            {
                using (var stream = typeof(WidgetForm).Assembly.GetManifestResourceStream("app.ico"))
                    if (stream != null) return new Icon(stream, size);
            }
            catch (Exception ex) { Log.Write(ex); }
            return null;
        }

        static bool SystemUsesDarkTheme()
        {
            try
            {
                using (var k = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"))
                {
                    object v = k == null ? null : k.GetValue("AppsUseLightTheme");
                    return v is int && (int)v == 0;
                }
            }
            catch { return false; }
        }

        static void OpenExternal(string target)
        {
            try { Process.Start(new ProcessStartInfo(target) { UseShellExecute = true }); }
            catch (Exception ex) { Log.Write(ex); }
        }
    }

    static class Native
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT { public int Left, Top, Right, Bottom; }

        [DllImport("user32.dll")] public static extern bool SetProcessDpiAwarenessContext(IntPtr value);
        [DllImport("user32.dll")] public static extern bool SetProcessDPIAware();
        [DllImport("user32.dll")] public static extern uint GetDpiForWindow(IntPtr hwnd);
        [DllImport("user32.dll")] public static extern bool ReleaseCapture();
        [DllImport("user32.dll")] public static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("dwmapi.dll")] public static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int value, int size);
    }
}
