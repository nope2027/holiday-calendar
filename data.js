/*
  휴장 캘린더 데이터 (한국 기준)
  기준일: 2026-09-24  ·  범위: 2026-01 ~ 2027-12

  필드
    c  나라 KR / CN / JP / US
    s  시작일, e 종료일(선택)
    n  한글 이름, o 원래 이름(선택)
    open/close  그날만 바뀌는 정규장 시작·마감 시각(거래소 현지 시각)
    k  종류  hol=공휴일(평일이면 휴장)  mkt=증시만 휴장  open=공휴일이지만 개장
             early=조기폐장  time=거래시간 변경
    d  한 줄 설명(선택)
    g  같은 줄로 묶을 그룹(선택), gn 그룹 이름(선택)
    t  true = 공식 발표 전(예정)
    cell 한국 달력 칸에 쓸 짧은 이름(선택)  문자열 또는 {날짜: 이름}
    dv 일본 파생(닛케이 선물·옵션) 휴일거래 여부  true/false

  출처
    한국  우주항공청 2027년 월력요항(2026.6), 공휴일법 개정(노동절·제헌절, 2026),
          한국거래소 휴장 공지(6/3·7/17), KRX 연말 휴장 관행
    중국  上海证券交易所 2026年休市安排 (2025.12.22) — 2027년은 국무원 발표 전
    일본  内閣府「国民の祝日」, JPX 休業日一覧, JPX 祝日取引 실시일
    미국  NYSE 2025·2026·2027 Holiday & Early Closings Calendar, SIFMA 권고(채권)
*/
window.HOLIDAY_DATA = {
  updated: "2026-09-24",
  from: "2026-01",
  to: "2027-12",

  countries: {
    KR: { name: "한국", short: "한", market: "KRX",      marketName: "코스피·코스닥" },
    CN: { name: "중국", short: "중", market: "SSE·SZSE", marketName: "상하이·선전" },
    JP: { name: "일본", short: "일", market: "JPX",      marketName: "도쿄증권거래소" },
    US: { name: "미국", short: "미", market: "NYSE",     marketName: "뉴욕·나스닥" }
  },

  // 왼쪽 '24시간 시세' — 바이낸스 TradFi 무기한 선물 (심볼 확인: 2026-10-09)
  // 탭별 기본 종목입니다. 위젯의 설정 › 시세 종목에서 추가·삭제하면 그 목록을 따로 저장해서 씁니다.
  quotes: {
    tabs: [
      // '@'로 시작하는 것은 지수(아래 indices — 현물 지수와 지수 선물), 나머지는 바이낸스 무기한 선물 기호
      { id:"all", name:"전체", items:["@KOSPI","@KOSDAQ","@DJI","@IXIC","@NKY","@SSEC","@KR200","@US30","@NDX","@SPX","@N225","SAMSUNG","SKHYNIX","HYUNDAI","NAVER","LGELECTRONICS","HANMI","SAMSUNGEM"] },
      { id:"kr",  name:"한국", items:["@KOSPI","@KOSDAQ","@KR200","SAMSUNG","SKHYNIX","HYUNDAI","NAVER","LGELECTRONICS","HANMI","SAMSUNGEM","KODEX200","EWY"] },
      { id:"us",  name:"미국", items:["@DJI","@IXIC","@US30","@NDX","@SPX","SPY","QQQ","IWM","SMH","NVDA","AAPL","MSFT","AMZN","GOOGL","META","TSLA"] },
      { id:"jp",  name:"일본", items:["@NKY","@N225","EWJ","SONY"], note:"일본 종목은 EWJ·소니뿐" }
    ],
    indices: {
      // 증시 지수 = 현물 지수 (src "cash" · 거래소 장 시간에만 움직임). 시세 코드 두 곳 (확인: 2026-10-22)
      //   tc = 텐센트 증권 공개 시세 (qt.gtimg.cn) — 있으면 시세는 이것 먼저
      //   em = 동방재부(Eastmoney) 공개 시세 (push2.eastmoney.com) — 일별 차트는 이것 먼저, 시세는 tc 가 없거나 안 될 때
      //   kl = em 차트가 안 될 때 텐센트 일별 차트 받는 방법 (kline·fqkline)
      //   mkt = 장 시간·휴장일을 따를 시장 (KR·CN·JP·US = 위 휴장 달력, 그 밖은 아래 markets), ex = 거래소(표시용)
      //   fut = 같은 지수의 24시간 선물, alias = 찾기용 다른 이름 / 세계 증시 목록(눌러서 넣기·빼기)은 아래 world
      "@KOSPI":  { n:"코스피",       sub:"KOSPI",          ex:"한국거래소",           src:"cash", tc:"gzKS11",   em:"100.KS11",     mkt:"KR", dg:2, fut:"@KR200", alias:"코스피 kospi ks11 종합주가지수" },
      "@KOSDAQ": { n:"코스닥",       sub:"KOSDAQ",         ex:"한국거래소",           src:"cash", tc:"gzKOSDAQ",                     mkt:"KR", dg:2, alias:"코스닥 kosdaq kq11" },
      "@KS200":  { n:"코스피200",    sub:"KOSPI 200",      ex:"한국거래소",           src:"cash",                em:"100.KOSPI200", mkt:"KR", dg:2, fut:"@KR200", alias:"코스피200 kospi200 ks200 k200" },
      "@DJI":    { n:"다우",         sub:"DJIA",           ex:"미국 뉴욕",            src:"cash", tc:"usDJI",    em:"100.DJIA",     mkt:"US", dg:2, fut:"@US30", kl:"kline", alias:"다우존스 dow dji djia 다우지수" },
      "@IXIC":   { n:"나스닥",       sub:"나스닥 종합",    ex:"미국 나스닥",          src:"cash", tc:"usIXIC",   em:"100.NDX",      mkt:"US", dg:2, kl:"kline", alias:"나스닥 nasdaq ixic comp 나스닥종합 나스닥지수" },
      "@INX":    { n:"S&P500",       sub:"S&P 500",        ex:"미국 뉴욕",            src:"cash", tc:"usINX",    em:"100.SPX",      mkt:"US", dg:2, fut:"@SPX", kl:"kline", alias:"에스앤피 snp sp500 spx gspc inx" },
      "@NDX100": { n:"나스닥100",    sub:"NASDAQ-100",     ex:"미국 나스닥",          src:"cash", tc:"usNDX",    em:"100.NDX100",   mkt:"US", dg:2, fut:"@NDX", alias:"나스닥100 nasdaq100 ndx nq100" },
      "@NKY":    { n:"니케이225",    sub:"N225",           ex:"도쿄증권거래소",       src:"cash", tc:"gzN225",   em:"100.N225",     mkt:"JP", dg:2, fut:"@N225", alias:"니케이 닛케이 nikkei n225 nky 일경 일본" },
      "@TWII":   { n:"대만 가권",    sub:"TAIEX",          ex:"대만증권거래소",       src:"cash", tc:"gzTWII",   em:"100.TWII",     mkt:"TW", dg:2, alias:"대만 가권 가권지수 taiex twii 타이완" },
      "@SSEC":   { n:"상해종합",     sub:"SSE",            ex:"상해증권거래소",       src:"cash", tc:"sh000001", em:"1.000001",     mkt:"CN", dg:2, kl:"fqkline", alias:"상해 상하이 shanghai sse ssec 000001 상증 중국" },
      "@SZSE":   { n:"선전성분",     sub:"SZSE",           ex:"선전증권거래소",       src:"cash", tc:"sz399001", em:"0.399001",     mkt:"CN", dg:2, kl:"fqkline", alias:"선전 심천 shenzhen szse 399001 중국" },
      "@CSI300": { n:"CSI300",       sub:"호심300",        ex:"상해·선전",            src:"cash", tc:"sh000300", em:"1.000300",     mkt:"CN", dg:2, kl:"fqkline", alias:"csi300 호심300 후선300 hs300 000300 중국" },
      "@CHINEXT":{ n:"창업판",       sub:"ChiNext",        ex:"선전증권거래소",       src:"cash", tc:"sz399006", em:"0.399006",     mkt:"CN", dg:2, kl:"fqkline", alias:"창업판 차이넥스트 chinext 399006 중국" },
      "@STAR50": { n:"과창판50",     sub:"STAR 50",        ex:"상해증권거래소",       src:"cash", tc:"sh000688", em:"1.000688",     mkt:"CN", dg:2, kl:"fqkline", alias:"과창판 커촹반 star50 star 000688 중국" },
      "@A50":    { n:"중국 A50",     sub:"FTSE China A50", ex:"FTSE 러셀",            src:"cash",                em:"100.XIN9",     mkt:"CN", dg:2, alias:"a50 중국a50 ftse china xin9 차이나" },
      "@HSI":    { n:"항셍",         sub:"HSI",            ex:"홍콩거래소",           src:"cash", tc:"hkHSI",    em:"100.HSI",      mkt:"HK", dg:2, kl:"fqkline", alias:"항셍 hangseng hsi 홍콩" },
      "@HSCEI":  { n:"홍콩H지수",    sub:"HSCEI",          ex:"홍콩거래소",           src:"cash", tc:"hkHSCEI",  em:"100.HSCEI",    mkt:"HK", dg:2, kl:"fqkline", alias:"홍콩h h지수 hscei 항셍h 항셍중국기업 홍콩" },
      "@HSTECH": { n:"항셍테크",     sub:"HSTECH",         ex:"홍콩거래소",           src:"cash", tc:"hkHSTECH", em:"124.HSTECH",   mkt:"HK", dg:2, kl:"fqkline", alias:"항셍테크 hstech 홍콩테크 홍콩" },
      "@SENSEX": { n:"인도 센섹스",  sub:"SENSEX",         ex:"봄베이증권거래소",     src:"cash",                em:"100.SENSEX",   mkt:"IN", dg:2, alias:"인도 센섹스 sensex bse india" },
      "@VNI":    { n:"베트남 VN",    sub:"VN-Index",       ex:"호찌민증권거래소",     src:"cash",                em:"100.VNINDEX",  mkt:"VN", dg:2, alias:"베트남 vn지수 vnindex vietnam 호치민" },
      "@JKSE":   { n:"인도네시아",   sub:"JCI",            ex:"인도네시아증권거래소", src:"cash",                em:"100.JKSE",     mkt:"ID", dg:2, alias:"인도네시아 자카르타 jkse jci indonesia" },
      "@STI":    { n:"싱가포르 STI", sub:"STI",            ex:"싱가포르거래소",       src:"cash",                em:"100.STI",      mkt:"SG", dg:2, alias:"싱가포르 sti straits singapore" },
      "@KLCI":   { n:"말레이시아",   sub:"FBM KLCI",       ex:"말레이시아거래소",     src:"cash",                em:"100.KLSE",     mkt:"MY", dg:2, alias:"말레이시아 klci klse malaysia" },
      "@SET":    { n:"태국 SET",     sub:"SET",            ex:"태국증권거래소",       src:"cash",                em:"100.SET",      mkt:"TH", dg:2, alias:"태국 set thailand 방콕" },
      "@ASX200": { n:"호주 ASX200",  sub:"S&P/ASX 200",    ex:"호주증권거래소",       src:"cash",                em:"100.AS51",     mkt:"AU", dg:1, alias:"호주 asx asx200 as51 australia" },
      "@SX5E":   { n:"유로스톡스50", sub:"EURO STOXX 50",  ex:"유로존 (STOXX)",       src:"cash",                em:"100.SX5E",     mkt:"EU", dg:2, alias:"유로스톡스 유로스탁스 eurostoxx stoxx50 sx5e 유럽" },
      "@DAX":    { n:"독일 DAX",     sub:"DAX 40",         ex:"프랑크푸르트 Xetra",   src:"cash",                em:"100.GDAXI",    mkt:"DE", dg:2, alias:"독일 dax gdaxi germany 닥스" },
      "@FTSE":   { n:"영국 FTSE",    sub:"FTSE 100",       ex:"런던증권거래소",       src:"cash",                em:"100.FTSE",     mkt:"UK", dg:2, alias:"영국 ftse ftse100 uk 풋시" },
      "@CAC":    { n:"프랑스 CAC",   sub:"CAC 40",         ex:"유로넥스트 파리",      src:"cash",                em:"100.FCHI",     mkt:"FR", dg:2, alias:"프랑스 cac cac40 fchi france" },
      "@IBEX":   { n:"스페인 IBEX",  sub:"IBEX 35",        ex:"마드리드증권거래소",   src:"cash",                em:"100.IBEX",     mkt:"ES", dg:2, alias:"스페인 ibex ibex35 spain" },
      "@MIB":    { n:"이탈리아 MIB", sub:"FTSE MIB",       ex:"이탈리아증권거래소",   src:"cash",                em:"100.MIB",      mkt:"IT", dg:2, alias:"이탈리아 mib ftsemib italy" },
      "@SMI":    { n:"스위스 SMI",   sub:"SMI",            ex:"스위스증권거래소 SIX", src:"cash",                em:"100.SSMI",     mkt:"CH", dg:2, alias:"스위스 smi ssmi swiss" },
      "@TSX":    { n:"캐나다 TSX",   sub:"S&P/TSX",        ex:"토론토증권거래소",     src:"cash",                em:"100.TSX",      mkt:"CA", dg:2, alias:"캐나다 tsx gsptse canada 토론토" },
      "@IBOV":   { n:"브라질",       sub:"Ibovespa",       ex:"브라질증권거래소 B3",  src:"cash",                em:"100.BVSP",     mkt:"BR", dg:0, alias:"브라질 보베스파 ibovespa ibov bvsp brazil" },
      // 증시 지수 위 한 줄 (src "mac"): 동방재부(Eastmoney) 공개 시세 (확인: 2026-10-23) — 15초마다, 누르면 차트. s = 한 줄에 쓰는 짧은 이름
      //   bp = 금리: 등락을 bp(0.01%p)로 표시
      "@VIX":   { n:"VIX",          s:"VIX",   sub:"CBOE 변동성 지수",        ex:"미국 CBOE", src:"mac", em:"167.VIX",   dg:2 },
      "@US10Y": { n:"미국채 10년물", s:"미국채 10년", sub:"미국 국채 10년물 금리 %", ex:"미국 국채", src:"mac", em:"171.US10Y", dg:3, bp:true },
      // 지수 선물 (24시간 무기한 선물 가격): hl = 하이퍼리퀴드 trade.xyz, gate = 게이트
      "@KR200": { n:"코스피200", sub:"KR200 · 선물", src:"hl",   id:"xyz:KR200",  dg:2, alias:"코스피 kospi kospi200 ks200" },
      "@US30":  { n:"다우",      sub:"US30 · 선물",  src:"gate", id:"US30_USDT",  dg:0, alias:"다우존스 dow dji ym us30" },
      "@NDX":   { n:"나스닥100", sub:"XYZ100 · 선물", src:"hl",  id:"xyz:XYZ100", dg:1, alias:"나스닥 nasdaq ndx nq nas100" },
      "@SPX":   { n:"S&P500",    sub:"SP500 · 선물", src:"hl",   id:"xyz:SP500",  dg:2, alias:"에스앤피 sp500 spx es us500" },
      "@N225":  { n:"니케이225", sub:"JP225 · 선물", src:"hl",   id:"xyz:JP225",  dg:0, alias:"니케이 닛케이 nikkei n225 jp225" }
    },
    // 시세 목록 맨 위(증시 지수 위) 한 줄 — 순서대로
    macro: ["@VIX","@US10Y"],
    // 시세 편집 화면의 '세계 증시' — 눌러서 지금 탭에 넣기·빼기 (순서대로 보임)
    world: [
      { g:"한국",        items:["@KOSPI","@KOSDAQ","@KS200"] },
      { g:"미국",        items:["@DJI","@IXIC","@INX","@NDX100"] },
      { g:"일본·대만",   items:["@NKY","@TWII"] },
      { g:"중국",        items:["@SSEC","@SZSE","@CSI300","@CHINEXT","@STAR50","@A50"] },
      { g:"홍콩",        items:["@HSI","@HSCEI","@HSTECH"] },
      { g:"아시아·호주", items:["@SENSEX","@VNI","@JKSE","@STI","@KLCI","@SET","@ASX200"] },
      { g:"유럽",        items:["@SX5E","@DAX","@FTSE","@CAC","@IBEX","@MIB","@SMI"] },
      { g:"미주",        items:["@TSX","@IBOV"] }
    ],
    // 증시 지수 장 시간 — 한국·중국·일본·미국은 위 휴장 달력을 따름. 여기 시장은 정규장 시간(현지 시각)만 앎:
    //   휴장일 달력이 없어서 장 시간이 20분 넘게 지났는데 오늘 거래 기록이 없으면 '휴장 추정'으로 표시
    //   lunch = 점심 휴장, fri = 금요일만 다른 점심 휴장, usStdClose = 미국이 서머타임이 아닐 때(11~3월) 마감
    markets: {
      TW: { name:"대만",       tz:"Asia/Taipei",       open:"09:00", close:"13:30" },
      HK: { name:"홍콩",       tz:"Asia/Hong_Kong",    open:"09:30", close:"16:00", lunch:["12:00","13:00"] },
      IN: { name:"인도",       tz:"Asia/Kolkata",      open:"09:15", close:"15:30" },
      VN: { name:"베트남",     tz:"Asia/Ho_Chi_Minh",  open:"09:00", close:"14:45", lunch:["11:30","13:00"] },
      ID: { name:"인도네시아", tz:"Asia/Jakarta",      open:"09:00", close:"16:00", lunch:["12:00","13:30"], fri:["11:30","14:00"] },
      SG: { name:"싱가포르",   tz:"Asia/Singapore",    open:"09:00", close:"17:00", lunch:["12:00","13:00"] },
      MY: { name:"말레이시아", tz:"Asia/Kuala_Lumpur", open:"09:00", close:"17:00", lunch:["12:30","14:30"] },
      TH: { name:"태국",       tz:"Asia/Bangkok",      open:"10:00", close:"16:30", lunch:["12:30","14:30"] },
      AU: { name:"호주",       tz:"Australia/Sydney",  open:"10:00", close:"16:00" },
      EU: { name:"유로존",     tz:"Europe/Berlin",     open:"09:00", close:"17:30" },
      DE: { name:"독일",       tz:"Europe/Berlin",     open:"09:00", close:"17:30" },
      UK: { name:"영국",       tz:"Europe/London",     open:"08:00", close:"16:30" },
      FR: { name:"프랑스",     tz:"Europe/Paris",      open:"09:00", close:"17:30" },
      ES: { name:"스페인",     tz:"Europe/Madrid",     open:"09:00", close:"17:30" },
      IT: { name:"이탈리아",   tz:"Europe/Rome",       open:"09:00", close:"17:30" },
      CH: { name:"스위스",     tz:"Europe/Zurich",     open:"09:00", close:"17:30" },
      CA: { name:"캐나다",     tz:"America/Toronto",   open:"09:30", close:"16:00" },
      BR: { name:"브라질",     tz:"America/Sao_Paulo", open:"10:00", close:"17:00", usStdClose:"18:00" }
    },
    // 원화로 바꿔 보여줄 종목 (한국 주식). 설정에서 추가한 한국 주식은 자동으로 여기에 더해짐
    krw: ["SAMSUNG","SKHYNIX","HYUNDAI","NAVER","LGELECTRONICS","HANMI","SAMSUNGEM","KODEX200"],
    // 이름표: 기호 → [이름, 작은 설명]
    names: {
      SAMSUNG:["삼성전자"], SKHYNIX:["SK하이닉스"], HYUNDAI:["현대차"], NAVER:["NAVER"], LGELECTRONICS:["LG전자"],
      HANMI:["한미반도체"], SAMSUNGEM:["삼성전기"], KODEX200:["KODEX 200","코스피200 연동 ETF"],
      EWY:["한국 MSCI","EWY · ETF"], KORU:["한국 3배","KORU · ETF"],
      CSOPSAMSUNG2L:["삼성전자 2배","홍콩 ETF"], CSOPSKHYNIX2L:["SK하이닉스 2배","홍콩 ETF"],
      SPY:["S&P500","SPY · ETF"], QQQ:["나스닥100","QQQ · ETF"], IWM:["러셀2000","IWM · ETF"], SMH:["반도체","SMH · ETF"],
      SOXL:["반도체 3배","SOXL · ETF"], SOXS:["반도체 −3배","SOXS · ETF"], TQQQ:["나스닥100 3배","TQQQ · ETF"],
      SQQQ:["나스닥100 −3배","SQQQ · ETF"], TZA:["러셀2000 −3배","TZA · ETF"], XLE:["에너지","XLE · ETF"],
      XBI:["바이오","XBI · ETF"], GDX:["금광주","GDX · ETF"], URNM:["우라늄","URNM · ETF"], UVXY:["VIX 1.5배","UVXY · ETF"],
      TMF:["미국채20년 3배","TMF · ETF"], TBT:["미국채20년 −2배","TBT · ETF"], BITO:["비트코인 선물","BITO · ETF"],
      EWJ:["일본","EWJ · MSCI 일본 ETF"], EWT:["대만","EWT · ETF"], EWZ:["브라질","EWZ · ETF"],
      NVDL:["엔비디아 2배","NVDL · ETF"], TSLL:["테슬라 2배","TSLL · ETF"], MUU:["마이크론 2배","MUU · ETF"],
      NVDA:["엔비디아","NVDA"], AAPL:["애플","AAPL"], MSFT:["마이크로소프트","MSFT"], GOOGL:["알파벳(구글)","GOOGL"],
      AMZN:["아마존","AMZN"], META:["메타","META"], TSLA:["테슬라","TSLA"], AVGO:["브로드컴","AVGO"], AMD:["AMD","AMD"],
      TSM:["TSMC","TSM · ADR"], ASML:["ASML","ASML"], MU:["마이크론","MU"], INTC:["인텔","INTC"], QCOM:["퀄컴","QCOM"],
      ARM:["ARM","ARM"], SMCI:["슈퍼마이크로","SMCI"], DELL:["델","DELL"], IBM:["IBM","IBM"], ORCL:["오라클","ORCL"],
      CRM:["세일즈포스","CRM"], NOW:["서비스나우","NOW"], ADBE:["어도비","ADBE"], NFLX:["넷플릭스","NFLX"], DIS:["디즈니","DIS"],
      UBER:["우버","UBER"], PLTR:["팔란티어","PLTR"], COIN:["코인베이스","COIN"], MSTR:["스트래티지","MSTR"], HOOD:["로빈후드","HOOD"],
      CRCL:["써클","CRCL"], PYPL:["페이팔","PYPL"], V:["비자","V"], JPM:["JP모건","JPM"], GS:["골드만삭스","GS"],
      BRKB:["버크셔 해서웨이","BRK.B"], WMT:["월마트","WMT"], COST:["코스트코","COST"], HD:["홈디포","HD"], KO:["코카콜라","KO"],
      MCD:["맥도날드","MCD"], NKE:["나이키","NKE"], XOM:["엑슨모빌","XOM"], LLY:["일라이릴리","LLY"], NVO:["노보노디스크","NVO · ADR"],
      MRK:["머크","MRK"], MRNA:["모더나","MRNA"], UNH:["유나이티드헬스","UNH"], CSCO:["시스코","CSCO"], TXN:["텍사스인스트루먼트","TXN"],
      AMAT:["어플라이드머티리얼즈","AMAT"], LRCX:["램리서치","LRCX"], KLAC:["KLA","KLAC"], MRVL:["마벨","MRVL"], ANET:["아리스타","ANET"],
      WDC:["웨스턴디지털","WDC"], SNDK:["샌디스크","SNDK"], LITE:["루멘텀","LITE"], COHR:["코히런트","COHR"], CRDO:["크레도","CRDO"],
      ALAB:["아스테라랩스","ALAB"], CRWV:["코어위브","CRWV"], NBIS:["네비우스","NBIS"], IREN:["아이렌","IREN"], APP:["앱러빈","APP"],
      CRWD:["크라우드스트라이크","CRWD"], PANW:["팔로알토","PANW"], NET:["클라우드플레어","NET"], DDOG:["데이터독","DDOG"],
      SNOW:["스노우플레이크","SNOW"], MDB:["몽고DB","MDB"], TEAM:["아틀라시안","TEAM"], SHOP:["쇼피파이","SHOP"], RDDT:["레딧","RDDT"],
      RKLB:["로켓랩","RKLB"], ASTS:["AST스페이스모바일","ASTS"], IONQ:["아이온큐","IONQ"], OKLO:["오클로","OKLO"], GEV:["GE 버노바","GEV"],
      VRT:["버티브","VRT"], VST:["비스트라","VST"], CAT:["캐터필러","CAT"], BX:["블랙스톤","BX"], ACN:["액센츄어","ACN"],
      GME:["게임스탑","GME"], AMC:["AMC","AMC"], DJT:["트럼프 미디어","DJT"], CVNA:["카바나","CVNA"], HIMS:["힘스앤허스","HIMS"],
      SOFI:["소파이","SOFI"], MARA:["마라 홀딩스","MARA"], BMNR:["비트마인","BMNR"], ZM:["줌","ZM"], EBAY:["이베이","EBAY"],
      SONY:["소니","SONY · ADR"], BABA:["알리바바","BABA · ADR"], PDD:["PDD(테무)","PDD · ADR"],
      HK0700:["텐센트","0700 · 홍콩"], TENCENT:["텐센트","홍콩"], HK1810:["샤오미","1810 · 홍콩"], BYD:["BYD","홍콩"],
      MEITUAN:["메이퇀","홍콩"], KUAISHOU:["콰이쇼우","홍콩"], POPMART:["팝마트","홍콩"], ZHIPU:["즈푸 AI","홍콩"],
      MINIMAX:["미니맥스","홍콩"], HK0992:["레노버","0992 · 홍콩"], CXMT:["창신메모리","중국"], UNITREE:["유니트리","중국"],
      OPENAI:["오픈AI","상장 전"], ANTHROPIC:["앤트로픽","상장 전"],
      XAU:["금","XAU · 원자재"], XAG:["은","XAG · 원자재"], XPT:["백금","XPT · 원자재"], XPD:["팔라듐","XPD · 원자재"],
      COPPER:["구리","원자재"], CL:["WTI 원유","CL · 원자재"], BZ:["브렌트유","BZ · 원자재"], NATGAS:["천연가스","원자재"],
      USDBRL:["달러/헤알","외환"],
      RIVN:["리비안","RIVN"], DKNG:["드래프트킹스","DKNG"], HPE:["HP엔터프라이즈","HPE"], NOK:["노키아","NOK · ADR"],
      GLW:["코닝","GLW"], CIEN:["시에나","CIEN"], TER:["테라다인","TER"], TTWO:["테이크투","TTWO"], ZS:["지스케일러","ZS"],
      GTLB:["깃랩","GTLB"], PATH:["유아이패스","PATH"], AKAM:["아카마이","AKAM"], BE:["블룸에너지","BE"], WEN:["웬디스","WEN"],
      TEM:["템퍼스AI","TEM"], VKTX:["바이킹 테라퓨틱스","VKTX"], MP:["MP머티리얼즈","MP"], GPRO:["고프로","GPRO"],
      BTC:["비트코인","BTC · 코인"], ETH:["이더리움","ETH · 코인"], SOL:["솔라나","SOL · 코인"], XRP:["리플","XRP · 코인"],
      DOGE:["도지코인","DOGE · 코인"], BNB:["바이낸스코인","BNB · 코인"]
    }
  },

  /* 히트맵 (시세 → '히트맵' 버튼): 크기 = 시가총액, 색 = 오늘 등락률
     kr·jp: [종목 코드, 이름, 업종, (24시간 시세 기호 — 있으면 '24시간 차트' 버튼)]  ·  시세: 텐센트 (kr+코드 · jp+코드)
     us: 동방재부 시가총액 상위 100개에서 skip(본국 상장이 따로 있는 해외 기업 ADR·ETN·중복 종목)을 뺌
         list = 업종 (텐센트로 받을 때 목록) · more = 업종만 · 업종: T 기술 C 커뮤니케이션 D 경기소비재 S 필수소비재 I 산업재
         H 헬스케어 F 금융 E 에너지 M 소재 U 유틸리티 R 부동산 · n = 한글 이름 (quotes.names 에 없는 것만)
     short = 칸이 좁을 때·요약 목록에 쓰는 짧은 이름 (말풍선·표는 원래 이름) */
  heatmap: {
    short: {
      "207940":"삼성바이오", "373220":"LG엔솔", "138040":"메리츠금융", "086790":"하나금융", "316140":"우리금융", "071050":"한국금융",
      "006800":"미래에셋", "329180":"HD현대중", "009540":"HD한국조선", "012450":"한화에어로", "047810":"KAI", "034020":"두산에너빌",
      "267260":"HD일렉트릭", "298040":"효성중공", "277810":"레인보우", "096770":"SK이노", "141080":"리가켐", "323410":"카카오뱅크",
      "010140":"삼성중공", "005490":"POSCO", "017670":"SKT", "015760":"한전",
      "8306":"MUFG", "8316":"SMFG", "8035":"도쿄일렉", "9984":"소뱅그룹", "7011":"미쓰비시중공", "7012":"가와사키중공",
      MSFT:"마소", GOOGL:"알파벳", TXN:"TI", AMAT:"어플라이드", AXP:"아멕스", BAC:"BofA", BRK_B:"버크셔", JNJ:"J&J", UNH:"유나이티드",
      CRWD:"크라우드", ISRG:"인튜이티브", BSX:"보스턴사이언", CEG:"컨스텔레이션", FCX:"프리포트", GD:"GD", NOC:"노스럽"
    },
    kr: [
      ["005930","삼성전자","반도체","SAMSUNG"], ["000660","SK하이닉스","반도체","SKHYNIX"], ["042700","한미반도체","반도체","HANMI"], ["058470","리노공업","반도체"],
      ["009150","삼성전기","IT·전자","SAMSUNGEM"], ["066570","LG전자","IT·전자","LGELECTRONICS"], ["011070","LG이노텍","IT·전자"], ["018260","삼성SDS","IT·전자"],
      ["373220","LG에너지솔루션","2차전지"], ["006400","삼성SDI","2차전지"], ["247540","에코프로비엠","2차전지"], ["086520","에코프로","2차전지"],
      ["003670","포스코퓨처엠","2차전지"], ["450080","에코프로머티","2차전지"],
      ["207940","삼성바이오로직스","바이오"], ["068270","셀트리온","바이오"], ["196170","알테오젠","바이오"], ["000100","유한양행","바이오"],
      ["128940","한미약품","바이오"], ["028300","HLB","바이오"], ["141080","리가켐바이오","바이오"],
      ["005380","현대차","자동차","HYUNDAI"], ["000270","기아","자동차"], ["012330","현대모비스","자동차"],
      ["105560","KB금융","금융"], ["055550","신한지주","금융"], ["086790","하나금융지주","금융"], ["316140","우리금융지주","금융"],
      ["138040","메리츠금융지주","금융"], ["024110","기업은행","금융"], ["323410","카카오뱅크","금융"], ["032830","삼성생명","금융"],
      ["000810","삼성화재","금융"], ["006800","미래에셋증권","금융"], ["071050","한국금융지주","금융"], ["039490","키움증권","금융"],
      ["329180","HD현대중공업","조선·해운"], ["009540","HD한국조선해양","조선·해운"], ["042660","한화오션","조선·해운"], ["010140","삼성중공업","조선·해운"], ["011200","HMM","조선·해운"],
      ["012450","한화에어로스페이스","방산·우주"], ["064350","현대로템","방산·우주"], ["079550","LIG넥스원","방산·우주"], ["047810","한국항공우주","방산·우주"], ["272210","한화시스템","방산·우주"],
      ["034020","두산에너빌리티","전력·기계"], ["267260","HD현대일렉트릭","전력·기계"], ["298040","효성중공업","전력·기계"], ["277810","레인보우로보틱스","전력·기계"],
      ["005490","POSCO홀딩스","소재·에너지"], ["010130","고려아연","소재·에너지"], ["051910","LG화학","소재·에너지"], ["096770","SK이노베이션","소재·에너지"], ["010950","S-Oil","소재·에너지"],
      ["035420","NAVER","인터넷·게임","NAVER"], ["035720","카카오","인터넷·게임"], ["259960","크래프톤","인터넷·게임"], ["352820","하이브","인터넷·게임"],
      ["017670","SK텔레콤","통신·유틸"], ["030200","KT","통신·유틸"], ["015760","한국전력","통신·유틸"],
      ["003550","LG","지주·기타"], ["034730","SK","지주·기타"], ["402340","SK스퀘어","지주·기타"], ["028260","삼성물산","지주·기타"],
      ["033780","KT&G","지주·기타"], ["001040","CJ","지주·기타"], ["000720","현대건설","지주·기타"]
    ],
    jp: [
      ["7203","도요타","자동차"], ["7267","혼다","자동차"], ["6902","덴소","자동차"], ["5108","브리지스톤","자동차"],
      ["8306","미쓰비시UFJ","금융"], ["8316","미쓰이스미토모","금융"], ["8411","미즈호","금융"], ["7182","유초은행","금융"],
      ["8591","오릭스","금융"], ["8766","도쿄해상","금융"], ["8725","MS&AD","금융"], ["8630","SOMPO","금융"],
      ["6758","소니","전자·정밀","SONY"], ["6501","히타치","전자·정밀"], ["6861","키엔스","전자·정밀"], ["6981","무라타","전자·정밀"],
      ["6503","미쓰비시전기","전자·정밀"], ["7741","HOYA","전자·정밀"], ["7751","캐논","전자·정밀"], ["4901","후지필름","전자·정밀"], ["6594","니덱","전자·정밀"],
      ["8035","도쿄일렉트론","반도체"], ["6857","어드반테스트","반도체"], ["6146","디스코","반도체"], ["285A","키옥시아","반도체"], ["6723","르네사스","반도체"],
      ["8058","미쓰비시상사","상사"], ["8001","이토추","상사"], ["8031","미쓰이물산","상사"], ["8053","스미토모상사","상사"], ["8002","마루베니","상사"], ["8015","도요타통상","상사"],
      ["9432","NTT","통신·IT"], ["9433","KDDI","통신·IT"], ["9434","소프트뱅크","통신·IT"], ["9984","소프트뱅크그룹","통신·IT"], ["6702","후지쓰","통신·IT"], ["6098","리크루트","통신·IT"],
      ["7011","미쓰비시중공업","기계·중공업"], ["7012","가와사키중공업","기계·중공업"], ["7013","IHI","기계·중공업"], ["6367","다이킨","기계·중공업"],
      ["6273","SMC","기계·중공업"], ["6954","화낙","기계·중공업"], ["6301","고마쓰","기계·중공업"],
      ["4502","다케다","제약·의료"], ["4568","다이이치산쿄","제약·의료"], ["4519","주가이제약","제약·의료"], ["4543","테루모","제약·의료"], ["4578","오쓰카","제약·의료"],
      ["9983","패스트리테일링","소비·서비스"], ["7974","닌텐도","소비·서비스"], ["4661","오리엔탈랜드","소비·서비스"], ["2914","JT","소비·서비스"], ["2802","아지노모토","소비·서비스"],
      ["4063","신에쓰화학","소재·운송·부동산"], ["9022","JR도카이","소재·운송·부동산"], ["9020","JR동일본","소재·운송·부동산"], ["9101","일본유선","소재·운송·부동산"], ["8801","미쓰이부동산","소재·운송·부동산"]
    ],
    us: {
      list: "AAPL:T MSFT:T NVDA:T GOOGL:C AMZN:D SPCX:I META:C TSLA:D AVGO:T LLY:H BRK_B:F ANT:T WMT:S JPM:F MU:T V:F AMD:T JNJ:H XOM:E MA:F " +
            "ABBV:H COST:S KO:S CVX:E PG:S PLTR:T CSCO:T HD:D MRK:H BAC:F GE:I ORCL:T UNH:H PANW:T INTC:T PM:S RTX:I MS:F TMO:H AMGN:H " +
            "CAT:I AMAT:T TXN:T NFLX:C WFC:F LIN:M LRCX:T GS:F IBM:T GILD:H CRWD:T DIS:C AXP:F ANET:T MCD:D PEP:S VZ:C GEV:I DELL:T C:F " +
            "ABT:H WELL:R QCOM:T APH:T NEE:U UNP:I BLK:F TMUS:C KLAC:T DE:I ARM:T SHOP:T WMB:E CRM:T PFE:H",
      more: "ISRG:H SPGI:F BKNG:D LOW:D HON:I BA:I SCHW:F ETN:I BSX:H SYK:H VRTX:H PGR:F COP:E LMT:I UBER:I ADI:T SBUX:D PLD:R BMY:H " +
            "CMCSA:C T:C SO:U DUK:U MO:S CB:F ADBE:T NOW:T INTU:T APP:T MRVL:T HOOD:F COIN:F MSTR:T CEG:U VST:U NEM:M FCX:M KKR:F APO:F " +
            "COF:F ICE:F CME:F MCK:H HCA:H CVS:H ELV:H CI:H TJX:D ABNB:D DASH:D SNPS:T CDNS:T ADP:I PH:I GD:I NOC:I HWM:I TT:I MMM:I " +
            "FDX:I UPS:I WM:I ECL:M SHW:M EOG:E SLB:E KMI:E OKE:E AMT:R EQIX:R SPG:R O:R PNC:F USB:F MMC:F AON:F MDT:H ZTS:H REGN:H " +
            "PYPL:F NKE:D SPOT:C MELI:D NU:F CRH:M RDDT:C SNOW:T NET:T DDOG:T CRWV:T VRT:I SMCI:T WDC:T SNDK:T",
      skip: "BRK_A GOOG TSM SKHY ASML NVS AZN SMFG HSBC SHEL RY TM SAP MUFG BABA KOF TD TTE NVO BHP SONY SAN UL BUD BTI DEO GSK RIO BP " +
            "ING BBVA MFG HMC NTES PDD JD BIDU TCOM INFY HDB IBN SNY ENB CNQ CNI CP BMO BNS CM SU TRI E EQNR ABEV ITUB VALE PBR NGG ARGX " +
            "LYG BCS NWG UBS CS SHG KB WIT TAK STLA RACE PHG NOK ERIC SONYY",
      n: { SPCX:"스페이스X", ANT:"앤트로픽", BRK_B:"버크셔 해서웨이", JNJ:"존슨앤드존슨", MA:"마스터카드", ABBV:"애브비", CVX:"셰브런",
           PG:"P&G", BAC:"뱅크오브아메리카", GE:"GE에어로스페이스", PM:"필립모리스", RTX:"RTX", MS:"모건스탠리", TMO:"써모피셔",
           AMGN:"암젠", WFC:"웰스파고", LIN:"린데", GILD:"길리어드", AXP:"아메리칸익스프레스", PEP:"펩시코", VZ:"버라이즌", C:"씨티그룹",
           ABT:"애보트", WELL:"웰타워", APH:"암페놀", NEE:"넥스트에라", UNP:"유니언퍼시픽", BLK:"블랙록", TMUS:"T모바일", DE:"디어",
           WMB:"윌리엄스", PFE:"화이자", ISRG:"인튜이티브서지컬", SPGI:"S&P글로벌", BKNG:"부킹홀딩스", LOW:"로우스", HON:"허니웰",
           BA:"보잉", SCHW:"찰스슈왑", ETN:"이튼", BSX:"보스턴사이언티픽", SYK:"스트라이커", VRTX:"버텍스", PGR:"프로그레시브",
           COP:"코노코필립스", LMT:"록히드마틴", ADI:"아날로그디바이스", SBUX:"스타벅스", PLD:"프로로지스", BMY:"BMS", CMCSA:"컴캐스트",
           T:"AT&T", SO:"서던컴퍼니", DUK:"듀크에너지", MO:"알트리아", CB:"처브", INTU:"인튜이트", CEG:"컨스텔레이션에너지",
           NEM:"뉴몬트", FCX:"프리포트맥모란", APO:"아폴로", COF:"캐피털원", MCK:"맥케슨", ELV:"엘레반스", CI:"시그나",
           ABNB:"에어비앤비", DASH:"도어대시", SNPS:"시놉시스", CDNS:"케이던스", PH:"파커하니핀", GD:"제너럴다이내믹스",
           NOC:"노스럽그러먼", HWM:"하우멧", TT:"트레인", FDX:"페덱스", WM:"웨이스트매니지먼트", ECL:"에코랩", SHW:"셔윈윌리엄스",
           KMI:"킨더모건", OKE:"원오크", AMT:"아메리칸타워", EQIX:"에퀴닉스", SPG:"사이먼프로퍼티", O:"리얼티인컴", USB:"US뱅코프",
           MMC:"마시앤맥레넌", AON:"에이온", MDT:"메드트로닉", ZTS:"조에티스", REGN:"리제네론", SPOT:"스포티파이",
           MELI:"메르카도리브레", NU:"누홀딩스" }
    }
  },

  // 선물·옵션 만기일 — 거래소 규칙(그 달 몇째 무슨 요일)으로 날짜를 계산해 달력에 표시 (확인 2026-11-27)
  //   그날이 그 나라 휴장이면 앞 거래일로 앞당김 (휴장일은 아래 events 로 판단)
  //   wd 요일(0=일·4=목·5=금) · nth 몇째 · big 큰 만기 달 · cell 달력 칸 글자 · bn/bo 큰 만기 이름/원래 이름 · mn/mo 매달 만기
  //   d 설명 · bd 큰 만기만 덧붙일 설명 · et 그 설명의 현지 시각(한국 시각으로 바꿔 덧붙임)
  //   한국: 코스피200 선물 3·6·9·12월 · 옵션 매달, 최종거래일 '결제월의 두 번째 목요일'(휴장일이면 순차적으로 앞당김),
  //         최종거래일 거래 08:45~15:20 — 증권사 장내파생상품 거래설명서(키움 2025.6 · 유안타 2026.10) · 미래에셋 상품 안내
  //   일본: SQ일 = 둘째 금요일(휴업일이면 앞당김), 거래는 그 전 거래일까지, 라지 선물 3·6·9·12월 — JPX 日経225先物·オプション 取引要綱
  //   미국: 월물 옵션 '셋째 금요일'(OCC) · S&P500 선물(E-mini) 3·6·9·12월 셋째 금요일 09:30 ET 거래 끝(CME),
  //         셋째 금요일이 휴장이면 목요일 (2026-06-18 준틴스 때 실제로 목요일 만기)
  expiry: {
    KR: { wd:4, nth:2, big:[3,6,9,12], cell:"만기",
          bn:"코스피200 선물·옵션 동시만기", bo:"네 마녀의 날", mn:"코스피200 옵션 만기",
          bd:"지수·주식 선물·옵션 함께 만기", d:"거래 08:45~15:20 (평소 15:45까지)" },
    JP: { wd:5, nth:2, big:[3,6,9,12], cell:"SQ",
          bn:"닛케이225 메이저 SQ", bo:"メジャーSQ", mn:"닛케이225 미니 SQ", mo:"ミニSQ",
          bd:"닛케이225 라지 선물·옵션 분기물 정산", d:"SQ = 이날 구성 종목 시가 · 거래는 전 거래일 15:45까지" },
    US: { wd:5, nth:3, big:[3,6,9,12], cell:"만기",
          bn:"쿼드러플 위칭", bo:"Quad Witching", mn:"월물 옵션 만기", mo:"Monthly OpEx",
          bd:"S&P500 선물(E-mini) 거래 끝 09:30 ET", et:"09:30", d:"주식·지수 월물 옵션 만기" }
  },

  notes: [
    "중국 2027년 휴장일은 아직 발표 전입니다(보통 11월 국무원 발표). 법정 휴일만 '예정'으로 표시했습니다.",
    "한국 2027년 KRX 휴장일은 12월 공식 공지 전이며, 공휴일 기준으로 표시했습니다.",
    "미국 날짜는 현지(뉴욕) 기준입니다. 미국 휴장일 = 한국시간 그날 밤 정규장 없음.",
    "선물·옵션 만기일은 거래소 규칙(한국 둘째 목요일 · 일본 둘째 금요일 · 미국 셋째 금요일)으로 계산한 날짜입니다. 그날이 휴장이면 앞 거래일로 앞당겼고, 거래소가 따로 바꾸면 다를 수 있습니다."
  ],

  sources: [
    ["한국", "우주항공청 「2027년 월력요항」(2026.6) · 공휴일법 개정 노동절·제헌절(2026) · KRX 휴장 공지"],
    ["중국", "上海证券交易所「2026年休市安排」(2025.12)"],
    ["일본", "内閣府「国民の祝日について」 · JPX「営業時間・休業日一覧」「祝日取引」"],
    ["미국", "NYSE Holidays & Early Closings 2025–2027 · SIFMA Holiday Schedule"],
    ["만기일", "한국 = 증권사 장내파생상품 거래설명서(코스피200 선물·옵션) · 일본 = JPX 日経225先物·オプション 取引要綱 · 미국 = CME E-mini S&P 500 계약 명세 · OCC 주식옵션 명세 (확인 2026-11-27)"]
  ],

  events: [
    /* ───────── 한국 2026 ───────── */
    { c:"KR", s:"2026-01-01", n:"신정", k:"hol" },
    { c:"KR", s:"2026-01-02", n:"새해 첫 거래일", k:"time", open:"10:00", d:"개장 10:00 (1시간 늦게)" },
    { c:"KR", s:"2026-02-16", e:"2026-02-18", n:"설날 연휴", k:"hol", d:"2/17(화) 설날",
      cell:{ "2026-02-16":"설 연휴", "2026-02-17":"설날", "2026-02-18":"설 연휴" } },
    { c:"KR", s:"2026-03-01", n:"삼일절", k:"hol", g:"kr-0301-26" },
    { c:"KR", s:"2026-03-02", n:"대체공휴일", k:"hol", g:"kr-0301-26", d:"삼일절(일) 대체", cell:"대체휴일" },
    { c:"KR", s:"2026-05-01", n:"노동절", k:"hol", d:"2026년부터 법정 공휴일" },
    { c:"KR", s:"2026-05-05", n:"어린이날", k:"hol" },
    { c:"KR", s:"2026-05-24", n:"부처님오신날", k:"hol", g:"kr-0524-26" },
    { c:"KR", s:"2026-05-25", n:"대체공휴일", k:"hol", g:"kr-0524-26", d:"부처님오신날(일) 대체", cell:"대체휴일" },
    { c:"KR", s:"2026-06-03", n:"전국동시지방선거", k:"hol", cell:"지방선거" },
    { c:"KR", s:"2026-06-06", n:"현충일", k:"hol", d:"토요일 · 대체공휴일 없음" },
    { c:"KR", s:"2026-07-17", n:"제헌절", k:"hol", d:"2026년 공휴일 재지정 (18년 만)" },
    { c:"KR", s:"2026-08-15", n:"광복절", k:"hol", g:"kr-0815-26" },
    { c:"KR", s:"2026-08-17", n:"대체공휴일", k:"hol", g:"kr-0815-26", d:"광복절(토) 대체", cell:"대체휴일" },
    { c:"KR", s:"2026-09-24", e:"2026-09-26", n:"추석 연휴", k:"hol", d:"9/25(금) 추석",
      cell:{ "2026-09-24":"추석 연휴", "2026-09-25":"추석", "2026-09-26":"추석 연휴" } },
    { c:"KR", s:"2026-10-03", n:"개천절", k:"hol", g:"kr-1003-26" },
    { c:"KR", s:"2026-10-05", n:"대체공휴일", k:"hol", g:"kr-1003-26", d:"개천절(토) 대체", cell:"대체휴일" },
    { c:"KR", s:"2026-10-09", n:"한글날", k:"hol" },
    { c:"KR", s:"2026-11-19", n:"수능일 시간 변경", k:"time", t:true, open:"10:00", close:"16:30",
      d:"예년 기준 개장 10:00 · 마감 16:30 (KRX 공지 전)" },
    { c:"KR", s:"2026-12-25", n:"성탄절", k:"hol" },
    { c:"KR", s:"2026-12-31", n:"연말 휴장일", k:"mkt", d:"KRX 연말 휴장 (공휴일 아님)" },

    /* ───────── 한국 2027 ───────── */
    { c:"KR", s:"2027-01-01", n:"신정", k:"hol" },
    { c:"KR", s:"2027-01-04", n:"새해 첫 거래일", k:"time", t:true, open:"10:00", d:"예년 기준 개장 10:00 (1시간 늦게)" },
    { c:"KR", s:"2027-02-06", e:"2027-02-08", n:"설날 연휴", k:"hol", g:"kr-seol-27", d:"2/7(일) 설날",
      cell:{ "2027-02-06":"설 연휴", "2027-02-07":"설날", "2027-02-08":"설 연휴" } },
    { c:"KR", s:"2027-02-09", n:"대체공휴일", k:"hol", g:"kr-seol-27", d:"설날(일) 대체", cell:"대체휴일" },
    { c:"KR", s:"2027-03-01", n:"삼일절", k:"hol" },
    { c:"KR", s:"2027-05-01", n:"노동절", k:"hol", g:"kr-0501-27" },
    { c:"KR", s:"2027-05-03", n:"대체공휴일", k:"hol", g:"kr-0501-27", d:"노동절(토) 대체", cell:"대체휴일" },
    { c:"KR", s:"2027-05-05", n:"어린이날", k:"hol" },
    { c:"KR", s:"2027-05-13", n:"부처님오신날", k:"hol" },
    { c:"KR", s:"2027-06-06", n:"현충일", k:"hol", d:"일요일 · 대체공휴일 없음" },
    { c:"KR", s:"2027-07-17", n:"제헌절", k:"hol", g:"kr-0717-27" },
    { c:"KR", s:"2027-07-19", n:"대체공휴일", k:"hol", g:"kr-0717-27", d:"제헌절(토) 대체", cell:"대체휴일" },
    { c:"KR", s:"2027-08-15", n:"광복절", k:"hol", g:"kr-0815-27" },
    { c:"KR", s:"2027-08-16", n:"대체공휴일", k:"hol", g:"kr-0815-27", d:"광복절(일) 대체", cell:"대체휴일" },
    { c:"KR", s:"2027-09-14", e:"2027-09-16", n:"추석 연휴", k:"hol", d:"9/15(수) 추석",
      cell:{ "2027-09-14":"추석 연휴", "2027-09-15":"추석", "2027-09-16":"추석 연휴" } },
    { c:"KR", s:"2027-10-03", n:"개천절", k:"hol", g:"kr-1003-27" },
    { c:"KR", s:"2027-10-04", n:"대체공휴일", k:"hol", g:"kr-1003-27", d:"개천절(일) 대체", cell:"대체휴일" },
    { c:"KR", s:"2027-10-09", n:"한글날", k:"hol", g:"kr-1009-27" },
    { c:"KR", s:"2027-10-11", n:"대체공휴일", k:"hol", g:"kr-1009-27", d:"한글날(토) 대체", cell:"대체휴일" },
    { c:"KR", s:"2027-12-25", n:"성탄절", k:"hol", g:"kr-1225-27" },
    { c:"KR", s:"2027-12-27", n:"대체공휴일", k:"hol", g:"kr-1225-27", d:"성탄절(토) 대체", cell:"대체휴일" },
    { c:"KR", s:"2027-12-31", n:"연말 휴장일", k:"mkt", t:true, d:"KRX 연말 휴장 (공휴일 아님)" },

    /* ───────── 중국 2026 (상하이거래소 공식) ───────── */
    { c:"CN", s:"2026-01-01", e:"2026-01-03", n:"신정 연휴", o:"元旦", k:"hol", d:"1/5(월) 개장" },
    { c:"CN", s:"2026-02-15", e:"2026-02-23", n:"춘절 연휴", o:"春节", k:"hol", d:"중국 설 연휴 · 2/24(화) 개장" },
    { c:"CN", s:"2026-04-04", e:"2026-04-06", n:"청명절 연휴", o:"清明节", k:"hol", d:"성묘하는 날 · 4/7(화) 개장" },
    { c:"CN", s:"2026-05-01", e:"2026-05-05", n:"노동절 연휴", o:"劳动节", k:"hol", d:"5/6(수) 개장" },
    { c:"CN", s:"2026-06-19", e:"2026-06-21", n:"단오절 연휴", o:"端午节", k:"hol", d:"6/22(월) 개장" },
    { c:"CN", s:"2026-09-25", e:"2026-09-27", n:"중추절 연휴", o:"中秋节", k:"hol", d:"중국 추석 · 9/28(월) 개장" },
    { c:"CN", s:"2026-10-01", e:"2026-10-07", n:"국경절 연휴", o:"国庆节", k:"hol", d:"건국 기념 황금연휴 · 10/8(목) 개장" },

    /* ───────── 중국 2027 (법정 휴일만 · 국무원 발표 전) ───────── */
    { c:"CN", s:"2027-01-01", n:"신정", o:"元旦", k:"hol", t:true, d:"법정 휴일 · 연휴 일정 발표 전" },
    { c:"CN", s:"2027-02-05", e:"2027-02-08", n:"춘절", o:"春节", k:"hol", t:true, d:"법정 휴일 2/5~2/8 · 실제 휴장은 더 길 수 있음" },
    { c:"CN", s:"2027-04-05", n:"청명절", o:"清明节", k:"hol", t:true, d:"법정 휴일 · 연휴 일정 발표 전" },
    { c:"CN", s:"2027-05-01", e:"2027-05-02", n:"노동절", o:"劳动节", k:"hol", t:true, d:"법정 휴일이 주말 · 평일 휴장일은 발표 후 확정" },
    { c:"CN", s:"2027-06-09", n:"단오절", o:"端午节", k:"hol", t:true, d:"법정 휴일 · 연휴 일정 발표 전" },
    { c:"CN", s:"2027-09-15", n:"중추절", o:"中秋节", k:"hol", t:true, d:"법정 휴일 · 연휴 일정 발표 전" },
    { c:"CN", s:"2027-10-01", e:"2027-10-03", n:"국경절", o:"国庆节", k:"hol", t:true, d:"법정 휴일 10/1~10/3 · 보통 10/7까지 연휴" },

    /* ───────── 일본 2026 (JPX 휴업일) ───────── */
    { c:"JP", s:"2026-01-01", n:"새해 첫날", o:"元日", k:"hol", g:"jp-ny-26", gn:"연시 휴장", dv:false, d:"새해 첫날" },
    { c:"JP", s:"2026-01-02", e:"2026-01-03", n:"연시 휴장", o:"年始休業", k:"mkt", g:"jp-ny-26", dv:false, d:"거래소 휴업일 (공휴일 아님)" },
    { c:"JP", s:"2026-01-12", n:"성인의 날", o:"成人の日", k:"hol", dv:true, d:"그해 성인이 된 사람을 축하 (1월 둘째 월요일)" },
    { c:"JP", s:"2026-02-11", n:"건국기념일", o:"建国記念の日", k:"hol", dv:true, d:"일본 건국을 기념하는 날" },
    { c:"JP", s:"2026-02-23", n:"천황 탄생일", o:"天皇誕生日", k:"hol", dv:true, d:"현 천황(나루히토)의 생일" },
    { c:"JP", s:"2026-03-20", n:"춘분의 날", o:"春分の日", k:"hol", dv:true, d:"자연을 기리고 생물을 아끼는 날" },
    { c:"JP", s:"2026-04-29", n:"쇼와의 날", o:"昭和の日", k:"hol", dv:true, d:"쇼와 천황 생일 · 골든위크 시작" },
    { c:"JP", s:"2026-05-03", n:"헌법기념일", o:"憲法記念日", k:"hol", g:"jp-gw-26", gn:"골든위크", d:"일본국 헌법 시행 기념" },
    { c:"JP", s:"2026-05-04", n:"녹색의 날", o:"みどりの日", k:"hol", g:"jp-gw-26", dv:true, d:"자연과 친해지는 날" },
    { c:"JP", s:"2026-05-05", n:"어린이날", o:"こどもの日", k:"hol", g:"jp-gw-26", dv:true, d:"어린이의 행복을 비는 날" },
    { c:"JP", s:"2026-05-06", n:"대체휴일", o:"振替休日", k:"hol", g:"jp-gw-26", dv:true, d:"헌법기념일(일) 대체" },
    { c:"JP", s:"2026-07-20", n:"바다의 날", o:"海の日", k:"hol", dv:true, d:"바다의 은혜에 감사하는 날 (7월 셋째 월요일)" },
    { c:"JP", s:"2026-08-11", n:"산의 날", o:"山の日", k:"hol", dv:true, d:"산과 친해지는 날" },
    { c:"JP", s:"2026-09-21", n:"경로의 날", o:"敬老の日", k:"hol", g:"jp-sw-26", gn:"실버위크", dv:true, d:"노인을 공경하는 날 (9월 셋째 월요일)" },
    { c:"JP", s:"2026-09-22", n:"국민의 휴일", o:"国民の休日", k:"hol", g:"jp-sw-26", dv:true, d:"두 공휴일 사이에 낀 평일" },
    { c:"JP", s:"2026-09-23", n:"추분의 날", o:"秋分の日", k:"hol", g:"jp-sw-26", dv:true, d:"조상을 기리는 날" },
    { c:"JP", s:"2026-10-12", n:"스포츠의 날", o:"スポーツの日", k:"hol", dv:true, d:"스포츠를 즐기는 날 (10월 둘째 월요일)" },
    { c:"JP", s:"2026-11-03", n:"문화의 날", o:"文化の日", k:"hol", dv:true, d:"자유와 평화, 문화를 기리는 날" },
    { c:"JP", s:"2026-11-23", n:"근로감사의 날", o:"勤労感謝の日", k:"hol", dv:false, d:"노동을 존중하고 서로 감사하는 날" },
    { c:"JP", s:"2026-12-31", n:"연말 휴장", o:"年末休業", k:"mkt", dv:false, d:"거래소 휴업일 (공휴일 아님)" },

    /* ───────── 일본 2027 ───────── */
    { c:"JP", s:"2027-01-01", n:"새해 첫날", o:"元日", k:"hol", g:"jp-ny-27", gn:"연시 휴장", dv:false, d:"새해 첫날" },
    { c:"JP", s:"2027-01-02", e:"2027-01-03", n:"연시 휴장", o:"年始休業", k:"mkt", g:"jp-ny-27", dv:false, d:"거래소 휴업일 (공휴일 아님)" },
    { c:"JP", s:"2027-01-11", n:"성인의 날", o:"成人の日", k:"hol", dv:true, d:"그해 성인이 된 사람을 축하 (1월 둘째 월요일)" },
    { c:"JP", s:"2027-02-11", n:"건국기념일", o:"建国記念の日", k:"hol", dv:true, d:"일본 건국을 기념하는 날" },
    { c:"JP", s:"2027-02-23", n:"천황 탄생일", o:"天皇誕生日", k:"hol", dv:true, d:"현 천황(나루히토)의 생일" },
    { c:"JP", s:"2027-03-21", n:"춘분의 날", o:"春分の日", k:"hol", g:"jp-0321-27", d:"자연을 기리고 생물을 아끼는 날" },
    { c:"JP", s:"2027-03-22", n:"대체휴일", o:"振替休日", k:"hol", g:"jp-0321-27", dv:true, d:"춘분의 날(일) 대체" },
    { c:"JP", s:"2027-04-29", n:"쇼와의 날", o:"昭和の日", k:"hol", dv:true, d:"쇼와 천황 생일 · 골든위크 시작" },
    { c:"JP", s:"2027-05-03", n:"헌법기념일", o:"憲法記念日", k:"hol", g:"jp-gw-27", gn:"골든위크", dv:true, d:"일본국 헌법 시행 기념" },
    { c:"JP", s:"2027-05-04", n:"녹색의 날", o:"みどりの日", k:"hol", g:"jp-gw-27", dv:true, d:"자연과 친해지는 날" },
    { c:"JP", s:"2027-05-05", n:"어린이날", o:"こどもの日", k:"hol", g:"jp-gw-27", dv:true, d:"어린이의 행복을 비는 날" },
    { c:"JP", s:"2027-07-19", n:"바다의 날", o:"海の日", k:"hol", dv:true, d:"바다의 은혜에 감사하는 날 (7월 셋째 월요일)" },
    { c:"JP", s:"2027-08-11", n:"산의 날", o:"山の日", k:"hol", dv:true, d:"산과 친해지는 날" },
    { c:"JP", s:"2027-09-20", n:"경로의 날", o:"敬老の日", k:"hol", dv:false, d:"노인을 공경하는 날 (9월 셋째 월요일)" },
    { c:"JP", s:"2027-09-23", n:"추분의 날", o:"秋分の日", k:"hol", dv:true, d:"조상을 기리는 날" },
    { c:"JP", s:"2027-10-11", n:"스포츠의 날", o:"スポーツの日", k:"hol", dv:true, d:"스포츠를 즐기는 날 (10월 둘째 월요일)" },
    { c:"JP", s:"2027-11-03", n:"문화의 날", o:"文化の日", k:"hol", dv:true, d:"자유와 평화, 문화를 기리는 날" },
    { c:"JP", s:"2027-11-23", n:"근로감사의 날", o:"勤労感謝の日", k:"hol", dv:true, d:"노동을 존중하고 서로 감사하는 날" },
    { c:"JP", s:"2027-12-31", n:"연말 휴장", o:"年末休業", k:"mkt", dv:false, d:"거래소 휴업일 (공휴일 아님)" },
    // 2028년 연시(1/1~1/3)는 JPX 규정상 항상 휴장 — 2027년 말 '개장일' 계산용
    { c:"JP", s:"2028-01-01", n:"새해 첫날", o:"元日", k:"hol", g:"jp-ny-28", gn:"연시 휴장", dv:false, d:"새해 첫날" },
    { c:"JP", s:"2028-01-02", e:"2028-01-03", n:"연시 휴장", o:"年始休業", k:"mkt", g:"jp-ny-28", dv:false, d:"거래소 휴업일 (공휴일 아님)" },

    /* ───────── 미국 2026 (NYSE) ───────── */
    { c:"US", s:"2026-01-01", n:"새해 첫날", o:"New Year's Day", k:"hol" },
    { c:"US", s:"2026-01-19", n:"마틴 루서 킹 데이", o:"MLK Jr. Day", k:"hol", d:"킹 목사 기념일 (1월 셋째 월요일)" },
    { c:"US", s:"2026-02-16", n:"대통령의 날", o:"Presidents' Day", k:"hol", d:"워싱턴 탄생일 (2월 셋째 월요일)" },
    { c:"US", s:"2026-04-03", n:"성금요일", o:"Good Friday", k:"mkt", d:"부활절 직전 금요일 · 연방공휴일은 아니고 증시만 휴장" },
    { c:"US", s:"2026-05-25", n:"메모리얼 데이", o:"Memorial Day", k:"hol", d:"전몰장병 추모일 (5월 마지막 월요일)" },
    { c:"US", s:"2026-06-19", n:"준틴스", o:"Juneteenth", k:"hol", d:"노예해방 기념일" },
    { c:"US", s:"2026-07-03", n:"독립기념일 (대체)", o:"Independence Day", k:"hol", d:"7/4가 토요일이라 금요일 휴장" },
    { c:"US", s:"2026-09-07", n:"노동절", o:"Labor Day", k:"hol", d:"9월 첫째 월요일" },
    { c:"US", s:"2026-10-12", n:"콜럼버스 데이", o:"Columbus Day", k:"open", d:"연방공휴일 · 증시는 정상 개장, 채권시장 휴장" },
    { c:"US", s:"2026-11-11", n:"재향군인의 날", o:"Veterans Day", k:"open", d:"연방공휴일 · 증시는 정상 개장, 채권시장 휴장" },
    { c:"US", s:"2026-11-26", n:"추수감사절", o:"Thanksgiving Day", k:"hol", d:"11월 넷째 목요일" },
    { c:"US", s:"2026-11-27", close:"13:00", n:"조기폐장 (블랙프라이데이)", o:"Early close", k:"early", d:"13:00 마감 · 한국시간 11/28(토) 03:00" },
    { c:"US", s:"2026-12-24", close:"13:00", n:"조기폐장 (크리스마스이브)", o:"Early close", k:"early", d:"13:00 마감 · 한국시간 12/25(금) 03:00" },
    { c:"US", s:"2026-12-25", n:"크리스마스", o:"Christmas Day", k:"hol" },

    /* ───────── 미국 2027 (NYSE) ───────── */
    { c:"US", s:"2027-01-01", n:"새해 첫날", o:"New Year's Day", k:"hol" },
    { c:"US", s:"2027-01-18", n:"마틴 루서 킹 데이", o:"MLK Jr. Day", k:"hol", d:"킹 목사 기념일 (1월 셋째 월요일)" },
    { c:"US", s:"2027-02-15", n:"대통령의 날", o:"Presidents' Day", k:"hol", d:"워싱턴 탄생일 (2월 셋째 월요일)" },
    { c:"US", s:"2027-03-26", n:"성금요일", o:"Good Friday", k:"mkt", d:"부활절 직전 금요일 · 연방공휴일은 아니고 증시만 휴장" },
    { c:"US", s:"2027-05-31", n:"메모리얼 데이", o:"Memorial Day", k:"hol", d:"전몰장병 추모일 (5월 마지막 월요일)" },
    { c:"US", s:"2027-06-18", n:"준틴스 (대체)", o:"Juneteenth", k:"hol", d:"6/19가 토요일이라 금요일 휴장" },
    { c:"US", s:"2027-07-05", n:"독립기념일 (대체)", o:"Independence Day", k:"hol", d:"7/4가 일요일이라 월요일 휴장" },
    { c:"US", s:"2027-09-06", n:"노동절", o:"Labor Day", k:"hol", d:"9월 첫째 월요일" },
    { c:"US", s:"2027-10-11", n:"콜럼버스 데이", o:"Columbus Day", k:"open", d:"연방공휴일 · 증시는 정상 개장, 채권시장 휴장" },
    { c:"US", s:"2027-11-11", n:"재향군인의 날", o:"Veterans Day", k:"open", d:"연방공휴일 · 증시는 정상 개장, 채권시장 휴장" },
    { c:"US", s:"2027-11-25", n:"추수감사절", o:"Thanksgiving Day", k:"hol", d:"11월 넷째 목요일" },
    { c:"US", s:"2027-11-26", close:"13:00", n:"조기폐장 (블랙프라이데이)", o:"Early close", k:"early", d:"13:00 마감 · 한국시간 11/27(토) 03:00" },
    { c:"US", s:"2027-12-24", n:"크리스마스 (대체)", o:"Christmas Day", k:"hol", d:"12/25가 토요일이라 금요일 휴장" },
    { c:"US", s:"2027-12-31", n:"새해 첫날 (연방 대체)", o:"New Year's Day 2028", k:"open", d:"연방 대체공휴일 · 증시는 정상 개장" }
  ]
};
