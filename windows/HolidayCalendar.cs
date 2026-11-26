// 휴장 캘린더 — 바탕화면 위젯 (WinForms + WebView2)
// 화면(app\index.html)을 테두리 없는 창에 띄우고, 항상 위·위치 기억·트레이·자동 실행을 맡는다.
using System;
using System.Diagnostics;
using System.Drawing;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
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
                string path = Path.Combine(Program.DataDir, "error.log");
                var fi = new FileInfo(path);
                if (fi.Exists && fi.Length > 1024 * 1024)   // 1MB 넘으면 error.old.log 로 넘기고 새로 시작
                {
                    File.Copy(path, Path.Combine(Program.DataDir, "error.old.log"), true);
                    File.Delete(path);
                }
                File.AppendAllText(path,
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

        // 2026-11-26 (사용자 PC 진단): 모니터가 꺼졌다 켜진 뒤 창이 배경색만 보이고 안 그려짐 (화면 프로세스·JS 는 살아 있음, 창 크기를 1px 바꾸면 돌아옴)
        //  원인: Chromium '창 가림 계산'(CalculateNativeWinOcclusion)이 화면이 꺼질 때 창을 '가려짐'으로 판정한 뒤 다시 계산하지 않음 → 끔
        //  그 위에 안전장치: 화면 켜짐·절전 해제·잠금 해제·화면 설정 변경 때 강제로 다시 그림(Wake), 화면 프로세스가 죽으면 다시 읽기·새로 만들기
        const string BrowserArgs = "--disable-features=CalculateNativeWinOcclusion";
        static readonly Guid GuidSessionDisplay = new Guid("2B84C20E-AD23-4DDF-93DB-05FFBD7EFCA5");   // 이 세션 화면 켜짐·꺼짐 (원격 데스크톱이면 원격 화면 기준)
        static readonly Guid GuidConsoleDisplay = new Guid("6FE69556-704A-47A0-8F24-C28D936FDA47");   // 본체 모니터 켜짐·꺼짐 (GUID_CONSOLE_DISPLAY_STATE)
        static readonly FieldInfo CtlField = typeof(WebView2).GetField("_coreWebView2Controller", BindingFlags.Instance | BindingFlags.NonPublic);

        readonly Settings st;
        WebView2 web;                      // 브라우저 프로세스가 죽으면 새로 만듦 (readonly 아님)
        CoreWebView2 curCore;
        IntPtr powerNotify, consoleNotify;
        bool screenOff, consoleOff, recreating;
        DateTime lastReload = DateTime.MinValue;
        readonly Queue<DateTime> recreates = new Queue<DateTime>();
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

            web = NewWeb();
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
            // 화면(모니터) 켜짐·꺼짐 알림 받기 → WM_POWERBROADCAST (등록하면 지금 상태도 한 번 옴)
            try
            {
                var g = GuidSessionDisplay; powerNotify = Native.RegisterPowerSettingNotification(Handle, ref g, 0 /* DEVICE_NOTIFY_WINDOW_HANDLE */);
                var c = GuidConsoleDisplay; consoleNotify = Native.RegisterPowerSettingNotification(Handle, ref c, 0);
            }
            catch (Exception ex) { Log.Write(ex); }
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            foreach (var h in new[] { powerNotify, consoleNotify })
                if (h != IntPtr.Zero) { try { Native.UnregisterPowerSettingNotification(h); } catch { } }
            powerNotify = consoleNotify = IntPtr.Zero;
            base.OnHandleDestroyed(e);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            SystemEvents.PowerModeChanged += OnPowerModeChanged;
            SystemEvents.DisplaySettingsChanged += OnDisplaySettingsChanged;
            SystemEvents.SessionSwitch += OnSessionSwitch;
            StartWeb(true);
        }

        WebView2 NewWeb()
        {
            var w = new WebView2();
            w.Dock = DockStyle.Fill;
            w.DefaultBackgroundColor = BackColor;
            return w;
        }

        // 화면(WebView2) 준비: 처음 한 번 + 브라우저 프로세스가 죽어 새로 만들 때
        async void StartWeb(bool first)
        {
            try
            {
                string udf = Path.Combine(Program.DataDir, "WebView2");
                var opts = new CoreWebView2EnvironmentOptions();
                opts.AdditionalBrowserArguments = BrowserArgs;
                var env = await CoreWebView2Environment.CreateAsync(null, udf, opts);
                await web.EnsureCoreWebView2Async(env);
                var core = web.CoreWebView2;
                curCore = core;
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
                core.ProcessFailed += OnProcessFailed;
                core.NewWindowRequested += (s, a) => { a.Handled = true; OpenNews(a.Uri); };   // 뉴스 링크 → 기본 브라우저 (아래 주소만)
                core.NavigationStarting += (s, a) =>
                {
                    if (!a.Uri.StartsWith("https://" + HostName + "/", StringComparison.OrdinalIgnoreCase)) a.Cancel = true;
                };
                web.ZoomFactor = st.Zoom;
                core.Navigate("https://" + HostName + "/index.html");
                if (!first) Log.Write("화면 다시 만듦");
            }
            catch (WebView2RuntimeNotFoundException)
            {
                if (!first) { Log.Write("화면 다시 만들기: WebView2 런타임을 찾지 못함"); return; }
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
                if (!first) { RetryRecreateLater(); return; }
                MessageBox.Show("화면을 불러오지 못했습니다.\n" + ex.Message + "\n\n자세한 내용: " + Path.Combine(Program.DataDir, "error.log"),
                    Program.AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally { if (!first) recreating = false; }
        }

        // ── 화면이 멈추거나 죽었을 때 ──
        CoreWebView2Controller Ctl()
        {
            try { return web == null || web.IsDisposed || CtlField == null ? null : CtlField.GetValue(web) as CoreWebView2Controller; }
            catch { return null; }
        }

        CoreWebView2 Core()
        {
            try { return web == null || web.IsDisposed ? null : web.CoreWebView2; }
            catch { return null; }   // 브라우저 프로세스가 죽은 뒤에는 예외
        }

        // 강제로 다시 그리기: 부모 창 위치 알림 + (필요하면) 보이기 껐다 켜기 + 크기 1px 바꿨다 되돌리기
        async void Wake(string why, int delayMs, bool toggleVisible)
        {
            try
            {
                if (delayMs > 0) await Task.Delay(delayMs);
                if (IsDisposed || screenOff || !Visible) return;
                var ctl = Ctl();
                if (ctl == null)
                {
                    if (CtlField == null && Core() != null) { web.Visible = false; web.Visible = true; }   // WebView2 SDK 가 바뀌어 컨트롤러를 못 찾을 때
                    return;
                }
                ctl.NotifyParentWindowPositionChanged();
                if (toggleVisible)
                {
                    ctl.IsVisible = false;
                    await Task.Delay(60);
                    if (IsDisposed || Ctl() != ctl) return;
                    ctl.IsVisible = Visible && !screenOff;
                }
                var r = web.ClientRectangle;
                ctl.Bounds = new Rectangle(r.X, r.Y, r.Width, r.Height + 1);
                await Task.Delay(60);
                if (IsDisposed || Ctl() != ctl) return;
                ctl.Bounds = web.ClientRectangle;
                if (toggleVisible) Log.Write("화면 다시 그림: " + why);   // 절전·화면·잠금 신호 때만 기록 (트레이에서 다시 열 때는 안 남김)
            }
            catch (Exception ex) { Log.Write(ex); }
        }

        // 이 세션의 화면 상태: 0 꺼짐 · 1 켜짐 · 2 어둡게
        void OnSessionDisplay(int state)
        {
            if (state == 0)
            {
                if (screenOff) return;
                screenOff = true;
                var ctl = Ctl();   // 꺼져 있는 동안은 화면을 '숨김'으로 → 화면의 시세·뉴스·시계도 멈춤 (CPU·GPU 아낌)
                if (ctl != null && Visible) { try { ctl.IsVisible = false; } catch (Exception ex) { Log.Write(ex); } }
                return;
            }
            if (!screenOff) return;
            ScreenBack("화면 켜짐");
        }

        // 본체 모니터 상태: 꺼짐 → 켜짐일 때 다시 그림만 (멈춤은 위 세션 화면 기준)
        void OnConsoleDisplay(int state)
        {
            bool was = consoleOff;
            consoleOff = state == 0;
            if (was && !consoleOff && !screenOff) { Wake("모니터 켜짐", 400, true); Wake("모니터 켜짐 (확인)", 3000, true); }
        }

        void ScreenBack(string why) { ScreenBack(why, 400); }
        void ScreenBack(string why, int delayMs)
        {
            screenOff = false;
            var ctl = Ctl();   // 먼저 바로 보이게 (아래 Wake 가 실패해도 화면은 돌아옴)
            if (ctl != null && Visible) { try { ctl.IsVisible = true; } catch (Exception ex) { Log.Write(ex); } }
            Wake(why, delayMs, true); Wake(why + " (확인)", delayMs + 2600, true);
        }

        // 화면이 꺼졌다고 알고 있는데 사용자가 창을 누르면(알림을 놓친 경우) 바로 되살림
        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            if (screenOff) ScreenBack("창 누름");
        }

        void OnPowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            if (e.Mode == PowerModes.Resume) UiPost(() => ScreenBack("절전에서 깨어남", 1500));
        }

        void OnDisplaySettingsChanged(object sender, EventArgs e) { UiPost(() => Wake("화면 설정 바뀜", 800, true)); }

        void OnSessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            var r = e.Reason;
            if (r == SessionSwitchReason.SessionUnlock || r == SessionSwitchReason.ConsoleConnect || r == SessionSwitchReason.RemoteConnect)
                UiPost(() => { if (screenOff) ScreenBack("잠금 해제·연결"); else Wake("잠금 해제·연결", 800, true); });
        }

        void UiPost(Action a)
        {
            try { if (IsHandleCreated && !IsDisposed) BeginInvoke(a); } catch { }
        }

        void OnProcessFailed(object sender, CoreWebView2ProcessFailedEventArgs e)
        {
            CoreWebView2ProcessFailedKind kind;
            try { kind = e.ProcessFailedKind; } catch { return; }
            string more = "";
            try { more = " · " + e.Reason + " · 종료 코드 " + e.ExitCode; } catch { }
            Log.Write("WebView2 프로세스 문제: " + kind + more);
            if (kind == CoreWebView2ProcessFailedKind.BrowserProcessExited || kind == CoreWebView2ProcessFailedKind.RenderProcessExited) ready = false;
            var src = sender as CoreWebView2;
            UiPost(() => OnProcessFailedUi(src, kind));
        }

        void OnProcessFailedUi(CoreWebView2 src, CoreWebView2ProcessFailedKind kind)
        {
            if (src != null && src != curCore) return;   // 새로 만든 뒤에 도착한 옛 알림
            switch (kind)
            {
                case CoreWebView2ProcessFailedKind.BrowserProcessExited:
                    Recreate("브라우저 프로세스 종료");
                    break;
                case CoreWebView2ProcessFailedKind.RenderProcessExited:
                case CoreWebView2ProcessFailedKind.RenderProcessUnresponsive:
                    if ((DateTime.Now - lastReload).TotalMinutes < 3) { Recreate("화면 프로세스 문제가 3분 안에 다시 생김"); break; }
                    lastReload = DateTime.Now;
                    ready = false;
                    var core = Core();
                    if (core == null) { Recreate("다시 읽기 못 함"); break; }
                    try { core.Reload(); Log.Write("화면 다시 읽음: " + kind); }
                    catch (Exception ex) { Log.Write(ex); Recreate("다시 읽기 실패"); }
                    break;
                case CoreWebView2ProcessFailedKind.GpuProcessExited:
                    Wake("GPU 프로세스 다시 시작", 1500, false);   // WebView2 가 GPU 프로세스를 새로 띄움 → 한 번 다시 그림
                    break;
            }
        }

        // 브라우저 프로세스가 죽으면 이 WebView2 는 다시 못 씀 → 새로 만들어 같은 화면을 다시 띄움 (30분에 3번까지)
        void Recreate(string why)
        {
            if (recreating || IsDisposed) return;
            var now = DateTime.Now;
            while (recreates.Count > 0 && (now - recreates.Peek()).TotalMinutes > 30) recreates.Dequeue();
            if (recreates.Count >= 3) { Log.Write("화면 다시 만들기 건너뜀 (30분에 3번 넘음): " + why); return; }
            recreates.Enqueue(now);
            Log.Write("화면 다시 만들기: " + why);
            recreating = true; ready = false; curCore = null;
            var old = web;
            try
            {
                web = NewWeb();
                Controls.Add(web);
                web.BringToFront();
                if (old != null) { Controls.Remove(old); old.Dispose(); }
            }
            catch (Exception ex) { Log.Write(ex); }
            StartWeb(false);
        }

        async void RetryRecreateLater()
        {
            recreating = false;
            await Task.Delay(15000);
            if (!IsDisposed && curCore == null) Recreate("다시 만들기 재시도");
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
                case "open": OpenNews(JsonString(json, "url")); break;
            }
        }

        // Esc 는 WebView2 가 '가속 키'로 따로 넘기므로 화면에 직접 전달
        bool ForwardEscape()
        {
            var core = Core();
            if (core == null) return false;
            try { core.PostWebMessageAsJson("{\"type\":\"key\",\"key\":\"Escape\"}"); return true; }
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
            var core = Core();
            if (!ready || core == null) return;
            string json = "{\"type\":\"state\",\"pin\":" + (st.Pin ? "true" : "false") +
                ",\"autostart\":" + (AutoStart.IsOn() ? "true" : "false") +
                ",\"zoom\":" + st.Zoom.ToString("0.###", CultureInfo.InvariantCulture) +
                ",\"layout\":\"" + st.Layout + "\"" +
                ",\"w\":" + LogicalSize().Width.ToString(CultureInfo.InvariantCulture) +
                ",\"h\":" + LogicalSize().Height.ToString(CultureInfo.InvariantCulture) +
                ",\"compact\":" + (st.Compact ? "true" : "false") + "}";
            try { core.PostWebMessageAsJson(json); } catch (Exception ex) { Log.Write(ex); }
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
            var core = Core();
            if (!ready || core == null) return;
            try { core.PostWebMessageAsJson(json); } catch (Exception ex) { Log.Write(ex); }
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
            bool wasOff = screenOff;   // 화면이 꺼졌다고 알고 있었어도 트레이에서 여는 건 사용자가 보고 있다는 뜻
            if (!Visible) Show();
            if (WindowState != FormWindowState.Normal) WindowState = FormWindowState.Normal;
            EnsureOnScreen();
            TopMost = st.Pin;
            Activate();
            Native.SetForegroundWindow(Handle);
            if (wasOff) ScreenBack("트레이에서 열기"); else Wake("다시 보이기", 150, false);
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
            SystemEvents.PowerModeChanged -= OnPowerModeChanged;
            SystemEvents.DisplaySettingsChanged -= OnDisplaySettingsChanged;
            SystemEvents.SessionSwitch -= OnSessionSwitch;
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
            if (m.Msg == 0x0218 /* WM_POWERBROADCAST */ && m.WParam.ToInt64() == 0x8013 /* PBT_POWERSETTINGCHANGE */ && m.LParam != IntPtr.Zero)
            {
                try
                {
                    var ps = (Native.POWERBROADCAST_SETTING)Marshal.PtrToStructure(m.LParam, typeof(Native.POWERBROADCAST_SETTING));
                    if (ps.DataLength >= 4 && ps.PowerSetting == GuidSessionDisplay) OnSessionDisplay(ps.Data);
                    else if (ps.DataLength >= 4 && ps.PowerSetting == GuidConsoleDisplay) OnConsoleDisplay(ps.Data);
                }
                catch (Exception ex) { Log.Write(ex); }
            }
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

        // 뉴스 기사: 아래 뉴스 사이트의 https·http 주소만 기본 브라우저로 연다 (그 밖의 주소·파일·프로그램은 무시)
        static readonly string[] NewsHosts = { "newsis.com", "financialjuice.com", "wsj.com", "nytimes.com" };
        static void OpenNews(string url)
        {
            Uri u;
            if (string.IsNullOrEmpty(url) || !Uri.TryCreate(url, UriKind.Absolute, out u)) return;
            if (u.Scheme != Uri.UriSchemeHttps && u.Scheme != Uri.UriSchemeHttp) return;
            string h = u.Host.ToLowerInvariant();
            foreach (string d in NewsHosts)
                if (h == d || h.EndsWith("." + d, StringComparison.Ordinal)) { OpenExternal(u.AbsoluteUri); return; }
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

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public struct POWERBROADCAST_SETTING { public Guid PowerSetting; public uint DataLength; public int Data; }
        [DllImport("user32.dll", SetLastError = true)] public static extern IntPtr RegisterPowerSettingNotification(IntPtr hRecipient, ref Guid powerSettingGuid, int flags);
        [DllImport("user32.dll", SetLastError = true)] public static extern bool UnregisterPowerSettingNotification(IntPtr handle);
    }
}
