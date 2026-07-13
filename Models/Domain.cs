using System.Windows.Media;
using LaunchControl.ViewModels;

namespace LaunchControl.Models;

/// <summary>Common status semantics used across the console.</summary>
public enum StatusLevel
{
    Go,        // nominal / GO
    Caution,   // warning / hold-worthy
    NoGo,      // fault / constraint violated
    Inhibit,   // interlocked / inhibited
    Standby,   // offline / not yet evaluated
    Info       // active / informational
}

/// <summary>Launch Commit Criteria / Readiness item outcome.</summary>
public enum ReadinessState
{
    Go,
    NoGo,
    Hold,
    NotEvaluated
}

/// <summary>Sequencer / countdown run state.</summary>
public enum SequenceState
{
    Idle,
    Running,
    Holding,
    Aborted,
    Complete
}

public enum OpsMode
{
    Test,
    Maintenance,
    Launch
}

/// <summary>A single subsystem tile shown on the Overview.</summary>
public sealed class SubsystemStatus : ObservableObject
{
    public string Id { get; init; } = "";
    public string Name { get; init; } = "";

    private StatusLevel _level = StatusLevel.Standby;
    public StatusLevel Level { get => _level; set { if (SetProperty(ref _level, value)) OnPropertyChanged(nameof(Accent)); } }

    private string _primaryValue = "—";
    public string PrimaryValue { get => _primaryValue; set => SetProperty(ref _primaryValue, value); }

    private string _primaryUnit = "";
    public string PrimaryUnit { get => _primaryUnit; set => SetProperty(ref _primaryUnit, value); }

    private string _detail = "";
    public string Detail { get => _detail; set => SetProperty(ref _detail, value); }

    private int _activeAlarms;
    public int ActiveAlarms { get => _activeAlarms; set { if (SetProperty(ref _activeAlarms, value)) OnPropertyChanged(nameof(HasAlarms)); } }
    public bool HasAlarms => ActiveAlarms > 0;

    public Brush Accent => StatusPalette.Brush(Level);
}

/// <summary>A row in the Launch Readiness Matrix (LCC evaluation).</summary>
public sealed class ReadinessItem : ObservableObject
{
    public string Id { get; init; } = "";
    public string Criterion { get; init; } = "";
    public string Owner { get; init; } = "";
    public string Group { get; init; } = "";

    private ReadinessState _state = ReadinessState.NotEvaluated;
    public ReadinessState State { get => _state; set { if (SetProperty(ref _state, value)) { OnPropertyChanged(nameof(Accent)); OnPropertyChanged(nameof(StateText)); } } }

    private string _measured = "—";
    public string Measured { get => _measured; set => SetProperty(ref _measured, value); }

    private string _limit = "";
    public string Limit { get => _limit; set => SetProperty(ref _limit, value); }

    private string _updatedUtc = "";
    public string UpdatedUtc { get => _updatedUtc; set => SetProperty(ref _updatedUtc, value); }

    public string StateText => State switch
    {
        ReadinessState.Go => "GO",
        ReadinessState.NoGo => "NO-GO",
        ReadinessState.Hold => "HOLD",
        _ => "—"
    };

    public Brush Accent => State switch
    {
        ReadinessState.Go => StatusPalette.Brush(StatusLevel.Go),
        ReadinessState.NoGo => StatusPalette.Brush(StatusLevel.NoGo),
        ReadinessState.Hold => StatusPalette.Brush(StatusLevel.Caution),
        _ => StatusPalette.Brush(StatusLevel.Standby)
    };
}

/// <summary>A step in the automatic/semi-automatic launch sequence.</summary>
public sealed class SequenceStep : ObservableObject
{
    public int Index { get; init; }
    public string TMark { get; init; } = "";      // e.g. "T-00:10:00"
    public double TSeconds { get; init; }          // seconds relative to T-0 (negative before launch)
    public string Title { get; init; } = "";
    public bool AutoHold { get; init; }            // built-in hold point
    public bool RequiresApproval { get; init; }    // semi-auto gate

    private SequenceStepState _state = SequenceStepState.Pending;
    public SequenceStepState State { get => _state; set { if (SetProperty(ref _state, value)) { OnPropertyChanged(nameof(Accent)); OnPropertyChanged(nameof(StateGlyph)); } } }

    public Brush Accent => State switch
    {
        SequenceStepState.Complete => StatusPalette.Brush(StatusLevel.Go),
        SequenceStepState.Active => StatusPalette.Brush(StatusLevel.Info),
        SequenceStepState.Hold => StatusPalette.Brush(StatusLevel.Caution),
        SequenceStepState.Failed => StatusPalette.Brush(StatusLevel.NoGo),
        _ => StatusPalette.Brush(StatusLevel.Standby)
    };

    public string StateGlyph => State switch
    {
        SequenceStepState.Complete => "\u2713",  // check
        SequenceStepState.Active => "\u25B6",    // play
        SequenceStepState.Hold => "\u23F8",      // pause
        SequenceStepState.Failed => "\u2715",    // x
        _ => "\u25CB"                              // circle
    };
}

public enum SequenceStepState { Pending, Active, Complete, Hold, Failed }

/// <summary>Palette helper — mirrors Themes/Palette.xaml so code can hand out brushes.</summary>
public static class StatusPalette
{
    public static readonly Brush Go      = Freeze("#2FD07A");
    public static readonly Brush Caution = Freeze("#F5C542");
    public static readonly Brush NoGo    = Freeze("#FF5C5C");
    public static readonly Brush Info    = Freeze("#3DBEE6");
    public static readonly Brush Inhibit = Freeze("#B57BFF");
    public static readonly Brush Standby = Freeze("#6C7A89");

    public static Brush Brush(StatusLevel l) => l switch
    {
        StatusLevel.Go => Go,
        StatusLevel.Caution => Caution,
        StatusLevel.NoGo => NoGo,
        StatusLevel.Inhibit => Inhibit,
        StatusLevel.Info => Info,
        _ => Standby
    };

    private static Brush Freeze(string hex)
    {
        var b = new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex)!);
        b.Freeze();
        return b;
    }
}
