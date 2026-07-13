using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Threading;
using LaunchControl.Models;
using LaunchControl.Services;

namespace LaunchControl.ViewModels;

public sealed class ConsoleViewModel : ObservableObject
{
    private readonly TelemetryService _tel = new();
    private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromMilliseconds(250) };
    private DateTime _lastTick = DateTime.UtcNow;

    public ConsoleViewModel()
    {
        BuildSubsystems();
        BuildReadiness();
        BuildSequence();

        HoldCommand   = new RelayCommand(Hold,   () => Sequence == SequenceState.Running);
        ResumeCommand = new RelayCommand(Resume, () => Sequence == SequenceState.Holding);
        RecycleCommand= new RelayCommand(Recycle,() => Sequence is SequenceState.Holding or SequenceState.Aborted);
        AbortCommand  = new RelayCommand(Abort,  () => Sequence is SequenceState.Running or SequenceState.Holding);
        StartCommand  = new RelayCommand(StartSequence, () => Sequence is SequenceState.Idle or SequenceState.Complete);

        _timer.Tick += OnTick;
        _timer.Start();
        RecomputeReadiness();
    }

    // ============================================================= Header

    private OpsMode _mode = OpsMode.Launch;
    public OpsMode Mode { get => _mode; set => SetProperty(ref _mode, value); }

    public string MissionName => "KSLV-X  •  FLIGHT 07  •  PAD-2";

    private string _utcClock = "";
    public string UtcClock { get => _utcClock; set => SetProperty(ref _utcClock, value); }

    private string _operator = "OPR: J.KIM  (CONDUCTOR)";
    public string Operator { get => _operator; set => SetProperty(ref _operator, value); }

    // ============================================================= Overview

    public ObservableCollection<SubsystemStatus> Subsystems { get; } = new();

    private void BuildSubsystems()
    {
        Subsystems.Add(new SubsystemStatus { Id="PROP", Name="추진제 시스템",   Level=StatusLevel.Info,    PrimaryValue="94.2", PrimaryUnit="% LOX", Detail="LOX 94.2%  RP-1 97.1%" });
        Subsystems.Add(new SubsystemStatus { Id="PRESS",Name="가압·퍼지",       Level=StatusLevel.Go,      PrimaryValue="315", PrimaryUnit="bar He", Detail="He 315 bar  N2 210 bar" });
        Subsystems.Add(new SubsystemStatus { Id="PWR",  Name="전력 시스템",     Level=StatusLevel.Go,      PrimaryValue="28.1", PrimaryUnit="V DC", Detail="GND + FLIGHT BATT OK" });
        Subsystems.Add(new SubsystemStatus { Id="MECH", Name="기계설비",        Level=StatusLevel.Go,      PrimaryValue="RET", PrimaryUnit="ARMS", Detail="T/E 수직  엄빌리컬 접속" });
        Subsystems.Add(new SubsystemStatus { Id="FIRE", Name="방재·가스감지",   Level=StatusLevel.Caution, PrimaryValue="0.6", PrimaryUnit="% LEL", Detail="Zone 3 가스 상승" });
        Subsystems.Add(new SubsystemStatus { Id="WX",   Name="기상",           Level=StatusLevel.Go,      PrimaryValue="7.4", PrimaryUnit="m/s", Detail="풍속 7.4  돌풍 11.2" });
        Subsystems.Add(new SubsystemStatus { Id="ACC",  Name="접근통제·CCTV",  Level=StatusLevel.Go,      PrimaryValue="CLR", PrimaryUnit="PAD", Detail="패드 인원 0  게이트 잠금" });
        Subsystems.Add(new SubsystemStatus { Id="ILK",  Name="인터록·Inhibit", Level=StatusLevel.Inhibit, PrimaryValue="2", PrimaryUnit="ACTIVE", Detail="점화 inhibit  릴리즈 inhibit" });
    }

    // ============================================================= Readiness

    public ObservableCollection<ReadinessItem> Readiness { get; } = new();

    private void BuildReadiness()
    {
        Readiness.Add(new ReadinessItem { Id="LCC-01", Group="PROP",  Criterion="LOX 탱크 수위 ≥ 92%",        Owner="PROP",   Measured="94.2 %",   Limit="≥ 92 %",     State=ReadinessState.Go });
        Readiness.Add(new ReadinessItem { Id="LCC-02", Group="PROP",  Criterion="RP-1 탱크 수위 ≥ 95%",       Owner="PROP",   Measured="97.1 %",   Limit="≥ 95 %",     State=ReadinessState.Go });
        Readiness.Add(new ReadinessItem { Id="LCC-03", Group="PRESS", Criterion="헬륨 저장 압력 ≥ 300 bar",   Owner="PRESS",  Measured="315 bar",  Limit="≥ 300 bar",  State=ReadinessState.Go });
        Readiness.Add(new ReadinessItem { Id="LCC-04", Group="PRESS", Criterion="탱크 얼리지 압력 밴드",       Owner="PRESS",  Measured="2.1 bar",  Limit="1.8–2.4 bar",State=ReadinessState.Go });
        Readiness.Add(new ReadinessItem { Id="LCC-05", Group="PWR",   Criterion="비행 배터리 전압 ≥ 27.5V",   Owner="AVI",    Measured="28.1 V",   Limit="≥ 27.5 V",   State=ReadinessState.Go });
        Readiness.Add(new ReadinessItem { Id="LCC-06", Group="AVI",   Criterion="비행 컴퓨터 헬스 OK",        Owner="AVI",    Measured="OK",       Limit="OK",         State=ReadinessState.Go });
        Readiness.Add(new ReadinessItem { Id="LCC-07", Group="RANGE", Criterion="레인지 세이프티 GREEN",      Owner="RSO",    Measured="GREEN",    Limit="GREEN",      State=ReadinessState.Go });
        Readiness.Add(new ReadinessItem { Id="LCC-08", Group="WX",    Criterion="지상 풍속 ≤ 12 m/s",         Owner="WX",     Measured="7.4 m/s",  Limit="≤ 12 m/s",   State=ReadinessState.Go });
        Readiness.Add(new ReadinessItem { Id="LCC-09", Group="WX",    Criterion="낙뢰 감지 반경 15km 청정",   Owner="WX",     Measured="CLEAR",    Limit="NO STRIKE",  State=ReadinessState.Go });
        Readiness.Add(new ReadinessItem { Id="LCC-10", Group="FIRE",  Criterion="패드 가스 농도 < 1.0% LEL",  Owner="SAFETY", Measured="0.6 % LEL",Limit="< 1.0 % LEL",State=ReadinessState.Hold });
        Readiness.Add(new ReadinessItem { Id="LCC-11", Group="ACC",   Criterion="패드 인원 소개 완료",         Owner="SAFETY", Measured="0 명",      Limit="= 0",        State=ReadinessState.Go });
        Readiness.Add(new ReadinessItem { Id="LCC-12", Group="MECH",  Criterion="엄빌리컬 접속 확인",          Owner="MECH",   Measured="MATED",    Limit="MATED",      State=ReadinessState.Go });
        Readiness.Add(new ReadinessItem { Id="LCC-13", Group="MECH",  Criterion="T/E 수직 · 잠금",            Owner="MECH",   Measured="LOCKED",   Limit="LOCKED",     State=ReadinessState.Go });
        Readiness.Add(new ReadinessItem { Id="LCC-14", Group="COMM",  Criterion="텔레메트리 링크 마진 ≥ 6dB", Owner="COMM",   Measured="9.2 dB",   Limit="≥ 6 dB",     State=ReadinessState.Go });
    }

    private int _goCount, _noGoCount, _holdCount;
    public int GoCount   { get => _goCount;   set => SetProperty(ref _goCount, value); }
    public int NoGoCount { get => _noGoCount; set => SetProperty(ref _noGoCount, value); }
    public int HoldCount { get => _holdCount; set => SetProperty(ref _holdCount, value); }

    private string _overallReadiness = "NO-GO";
    public string OverallReadiness { get => _overallReadiness; set => SetProperty(ref _overallReadiness, value); }

    private StatusLevel _overallLevel = StatusLevel.Caution;
    public StatusLevel OverallLevel { get => _overallLevel; set => SetProperty(ref _overallLevel, value); }

    private void RecomputeReadiness()
    {
        GoCount   = Readiness.Count(r => r.State == ReadinessState.Go);
        NoGoCount = Readiness.Count(r => r.State == ReadinessState.NoGo);
        HoldCount = Readiness.Count(r => r.State == ReadinessState.Hold);

        if (NoGoCount > 0)      { OverallReadiness = "NO-GO"; OverallLevel = StatusLevel.NoGo; }
        else if (HoldCount > 0) { OverallReadiness = "HOLD";  OverallLevel = StatusLevel.Caution; }
        else                    { OverallReadiness = "GO";    OverallLevel = StatusLevel.Go; }
    }

    // ============================================================= Sequencer

    public ObservableCollection<SequenceStep> Steps { get; } = new();

    private void BuildSequence()
    {
        void Add(int i, string t, double s, string title, bool hold=false, bool appr=false)
            => Steps.Add(new SequenceStep { Index=i, TMark=t, TSeconds=s, Title=title, AutoHold=hold, RequiresApproval=appr });

        Add(1,  "T-04:00:00", -14400, "발사 운용 개시 · 콘솔 정렬");
        Add(2,  "T-03:30:00", -12600, "패드 소개 · 접근통제 활성화");
        Add(3,  "T-02:00:00", -7200,  "추진제 이송 준비 · 라인 냉각");
        Add(4,  "T-01:30:00", -5400,  "LOX 로딩 개시", appr:true);
        Add(5,  "T-01:00:00", -3600,  "RP-1 로딩 · 얼리지 가압");
        Add(6,  "T-00:45:00", -2700,  "탱크 압력 안정화 HOLD", hold:true);
        Add(7,  "T-00:30:00", -1800,  "비행 컴퓨터 정렬 · GNC 로드");
        Add(8,  "T-00:15:00", -900,   "텔레메트리 · 레인지 최종 확인");
        Add(9,  "T-00:10:00", -600,   "발사 준비완료 HOLD (GO/NO-GO)", hold:true, appr:true);
        Add(10, "T-00:05:00", -300,   "터미널 카운트 진입 · 자동 시퀀스");
        Add(11, "T-00:02:00", -120,   "엄빌리컬 분리 준비");
        Add(12, "T-00:00:50", -50,    "점화 시퀀스 개시", appr:true);
        Add(13, "T-00:00:10", -10,    "엔진 스타트 · 헬스 체크");
        Add(14, "T-00:00:00", 0,      "리프트오프 커맨드");
        _currentStepIdx = 0;
    }

    private int _currentStepIdx;

    private SequenceState _sequence = SequenceState.Idle;
    public SequenceState Sequence
    {
        get => _sequence;
        set { if (SetProperty(ref _sequence, value)) { OnPropertyChanged(nameof(SequenceText)); RaiseCmds(); } }
    }
    public string SequenceText => Sequence switch
    {
        SequenceState.Running  => "COUNTING",
        SequenceState.Holding  => "HOLDING",
        SequenceState.Aborted  => "ABORTED",
        SequenceState.Complete => "LIFTOFF",
        _ => "IDLE"
    };

    // countdown in seconds relative to T-0 (negative before launch)
    private double _tMinus = -600; // start clock parked at T-10:00 for demo
    public double TMinusSeconds { get => _tMinus; set { if (SetProperty(ref _tMinus, value)) OnPropertyChanged(nameof(ClockText)); } }

    public string ClockText
    {
        get
        {
            var neg = TMinusSeconds < 0;
            var t = TimeSpan.FromSeconds(Math.Abs(TMinusSeconds));
            var sign = neg ? "T-" : "T+";
            return $"{sign}{(int)t.TotalHours:00}:{t.Minutes:00}:{t.Seconds:00}";
        }
    }

    private string _holdReason = "";
    public string HoldReason { get => _holdReason; set => SetProperty(ref _holdReason, value); }

    public ICommand StartCommand   { get; }
    public ICommand HoldCommand    { get; }
    public ICommand ResumeCommand  { get; }
    public ICommand RecycleCommand { get; }
    public ICommand AbortCommand   { get; }

    private void RaiseCmds()
    {
        (StartCommand   as RelayCommand)?.RaiseCanExecuteChanged();
        (HoldCommand    as RelayCommand)?.RaiseCanExecuteChanged();
        (ResumeCommand  as RelayCommand)?.RaiseCanExecuteChanged();
        (RecycleCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (AbortCommand   as RelayCommand)?.RaiseCanExecuteChanged();
    }

    private void StartSequence()
    {
        if (Sequence == SequenceState.Complete) Recycle();
        Sequence = SequenceState.Running;
        HoldReason = "";
        SetActiveStep(_currentStepIdx);
    }

    private void Hold()
    {
        Sequence = SequenceState.Holding;
        HoldReason = "수동 HOLD 지시";
        if (_currentStepIdx < Steps.Count) Steps[_currentStepIdx].State = SequenceStepState.Hold;
    }

    private void Resume()
    {
        Sequence = SequenceState.Running;
        HoldReason = "";
        if (_currentStepIdx < Steps.Count) Steps[_currentStepIdx].State = SequenceStepState.Active;
    }

    private void Recycle()
    {
        Sequence = SequenceState.Idle;
        HoldReason = "";
        TMinusSeconds = -600;
        _currentStepIdx = 0;
        foreach (var s in Steps) s.State = SequenceStepState.Pending;
    }

    private void Abort()
    {
        Sequence = SequenceState.Aborted;
        HoldReason = "ABORT 실행 — 시퀀스 정지";
        if (_currentStepIdx < Steps.Count) Steps[_currentStepIdx].State = SequenceStepState.Failed;
    }

    private void SetActiveStep(int idx)
    {
        for (int i = 0; i < Steps.Count; i++)
            Steps[i].State = i < idx ? SequenceStepState.Complete
                            : i == idx ? SequenceStepState.Active
                            : SequenceStepState.Pending;
    }

    // ============================================================= Propellant P&ID telemetry

    private double _loxLevel = 94.2;
    public double LoxLevel { get => _loxLevel; set { if (SetProperty(ref _loxLevel, value)) OnPropertyChanged(nameof(LoxFillPixels)); } }
    public double LoxFillPixels => Math.Clamp(LoxLevel, 0, 100) * 1.70;

    private double _rp1Level = 97.1;
    public double Rp1Level { get => _rp1Level; set { if (SetProperty(ref _rp1Level, value)) OnPropertyChanged(nameof(Rp1FillPixels)); } }
    public double Rp1FillPixels => Math.Clamp(Rp1Level, 0, 100) * 1.70;

    private double _loxStoragePressure = 0.42;
    public double LoxStoragePressure { get => _loxStoragePressure; set => SetProperty(ref _loxStoragePressure, value); }

    private double _rp1StoragePressure = 0.31;
    public double Rp1StoragePressure { get => _rp1StoragePressure; set => SetProperty(ref _rp1StoragePressure, value); }

    private double _loxFlow = 18.6;
    public double LoxFlow { get => _loxFlow; set => SetProperty(ref _loxFlow, value); }

    private double _rp1Flow = 7.8;
    public double Rp1Flow { get => _rp1Flow; set => SetProperty(ref _rp1Flow, value); }

    // ============================================================= Pressurization & purge telemetry

    private double _gn2BankPressure = 22.8;
    public double Gn2BankPressure { get => _gn2BankPressure; set => SetProperty(ref _gn2BankPressure, value); }

    private double _heBankPressure = 31.5;
    public double HeBankPressure { get => _heBankPressure; set => SetProperty(ref _heBankPressure, value); }

    private double _heRegulatedPressure = 3.20;
    public double HeRegulatedPressure { get => _heRegulatedPressure; set => SetProperty(ref _heRegulatedPressure, value); }

    private double _vehiclePressPressure = 2.84;
    public double VehiclePressPressure { get => _vehiclePressPressure; set => SetProperty(ref _vehiclePressPressure, value); }

    private double _purgeFlow = 42.5;
    public double PurgeFlow { get => _purgeFlow; set => SetProperty(ref _purgeFlow, value); }

    private double _purgeOxygen = 0.62;
    public double PurgeOxygen { get => _purgeOxygen; set => SetProperty(ref _purgeOxygen, value); }

    private double _purgeDewPoint = -46.8;
    public double PurgeDewPoint { get => _purgeDewPoint; set => SetProperty(ref _purgeDewPoint, value); }

    private double _purgeLineTemperature = 18.4;
    public double PurgeLineTemperature { get => _purgeLineTemperature; set => SetProperty(ref _purgeLineTemperature, value); }

    // ============================================================= Tick loop

    private void OnTick(object? sender, EventArgs e)
    {
        var now = DateTime.UtcNow;
        var dt = (now - _lastTick).TotalSeconds;
        _lastTick = now;

        UtcClock = now.ToString("yyyy-MM-dd HH:mm:ss 'UTC'");

        if (Sequence == SequenceState.Running)
        {
            TMinusSeconds += dt; // clock advances toward T-0

            // advance step when we reach its T-mark
            if (_currentStepIdx < Steps.Count)
            {
                var step = Steps[_currentStepIdx];
                if (TMinusSeconds >= step.TSeconds - 0.001)
                {
                    // auto-hold point → stop and wait for Resume
                    if (step.AutoHold && step.State == SequenceStepState.Active)
                    {
                        Sequence = SequenceState.Holding;
                        HoldReason = $"자동 HOLD — {step.Title}";
                        step.State = SequenceStepState.Hold;
                    }
                    else
                    {
                        step.State = SequenceStepState.Complete;
                        _currentStepIdx++;
                        if (_currentStepIdx < Steps.Count)
                        {
                            Steps[_currentStepIdx].State = SequenceStepState.Active;
                        }
                        else
                        {
                            Sequence = SequenceState.Complete;
                            TMinusSeconds = 0;
                        }
                    }
                }
            }
        }

        DriveTelemetry();
    }

    private double _lelPhase;
    private void DriveTelemetry()
    {
        // Subsystem live values
        foreach (var s in Subsystems)
        {
            switch (s.Id)
            {
                case "PROP":
                    LoxLevel = _tel.Jitter(94.2, 0.15);
                    Rp1Level = _tel.Jitter(97.1, 0.08);
                    LoxStoragePressure = _tel.Jitter(0.42, 0.01);
                    Rp1StoragePressure = _tel.Jitter(0.31, 0.008);
                    LoxFlow = _tel.Jitter(18.6, 0.25);
                    Rp1Flow = _tel.Jitter(7.8, 0.12);
                    s.PrimaryValue = LoxLevel.ToString("0.0");
                    s.Detail = $"LOX {LoxLevel:0.0}%  RP-1 {Rp1Level:0.0}%";
                    break;
                case "PRESS":
                    Gn2BankPressure = _tel.Jitter(22.8, 0.18);
                    HeBankPressure = _tel.Jitter(31.5, 0.22);
                    HeRegulatedPressure = _tel.Jitter(3.20, 0.025);
                    VehiclePressPressure = _tel.Jitter(2.84, 0.018);
                    PurgeFlow = _tel.Jitter(42.5, 0.7);
                    PurgeOxygen = Math.Max(0.1, _tel.Jitter(0.62, 0.035));
                    PurgeDewPoint = _tel.Jitter(-46.8, 0.6);
                    PurgeLineTemperature = _tel.Jitter(18.4, 0.25);
                    s.PrimaryValue = HeBankPressure.ToString("0.0");
                    s.Detail = $"GN2 {Gn2BankPressure:0.0} MPa  He {HeBankPressure:0.0} MPa";
                    break;
                case "PWR":
                    s.PrimaryValue = _tel.Jitter(28.1, 0.05).ToString("0.0");
                    break;
                case "WX":
                    var w = _tel.Jitter(7.4, 0.6);
                    s.PrimaryValue = w.ToString("0.0");
                    s.Detail = $"풍속 {w:0.0}  돌풍 {w + 3.8:0.0}";
                    break;
                case "FIRE":
                    _lelPhase += 0.02;
                    var lel = 0.6 + Math.Sin(_lelPhase) * 0.25;
                    s.PrimaryValue = lel.ToString("0.0");
                    s.Level = lel > 1.0 ? StatusLevel.NoGo : lel > 0.7 ? StatusLevel.Caution : StatusLevel.Go;
                    // reflect into readiness LCC-10
                    var lcc = Readiness.FirstOrDefault(r => r.Id == "LCC-10");
                    if (lcc != null)
                    {
                        lcc.Measured = $"{lel:0.0} % LEL";
                        lcc.State = lel > 1.0 ? ReadinessState.NoGo : lel > 0.7 ? ReadinessState.Hold : ReadinessState.Go;
                    }
                    break;
            }
        }
        RecomputeReadiness();
    }
}
