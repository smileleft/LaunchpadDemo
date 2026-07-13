using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using LaunchControl.Models;

namespace LaunchControl.Converters;

public sealed class StatusLevelToBrush : IValueConverter
{
    public object Convert(object value, Type t, object p, CultureInfo c)
        => value is StatusLevel l ? StatusPalette.Brush(l) : StatusPalette.Standby;
    public object ConvertBack(object v, Type t, object p, CultureInfo c) => Binding.DoNothing;
}

public sealed class StatusLevelToText : IValueConverter
{
    public object Convert(object value, Type t, object p, CultureInfo c) => value switch
    {
        StatusLevel.Go => "GO",
        StatusLevel.Caution => "CAUTION",
        StatusLevel.NoGo => "NO-GO",
        StatusLevel.Inhibit => "INHIBIT",
        StatusLevel.Info => "ACTIVE",
        _ => "STANDBY"
    };
    public object ConvertBack(object v, Type t, object p, CultureInfo c) => Binding.DoNothing;
}

/// <summary>True when bound enum equals the parameter (string name).</summary>
public sealed class EnumEquals : IValueConverter
{
    public object Convert(object value, Type t, object p, CultureInfo c)
        => value?.ToString() == p?.ToString();
    public object ConvertBack(object v, Type t, object p, CultureInfo c) => Binding.DoNothing;
}

/// <summary>Bool → Visibility (true=Visible). Non-empty string → Visible too.</summary>
public sealed class BoolToVis : IValueConverter
{
    public object Convert(object value, Type t, object p, CultureInfo c) => value switch
    {
        bool b => b ? Visibility.Visible : Visibility.Collapsed,
        string s => string.IsNullOrEmpty(s) ? Visibility.Collapsed : Visibility.Visible,
        int i => i != 0 ? Visibility.Visible : Visibility.Collapsed,
        _ => Visibility.Collapsed
    };
    public object ConvertBack(object v, Type t, object p, CultureInfo c) => Binding.DoNothing;
}

/// <summary>Returns Bg2 background if the row is the active step, else transparent.</summary>
public sealed class StepStateToRowBrush : IValueConverter
{
    private static readonly Brush Active = New("#182130");
    private static readonly Brush Hold   = New("#33290A");
    private static readonly Brush Fail   = New("#361417");
    public object Convert(object value, Type t, object p, CultureInfo c) => value switch
    {
        SequenceStepState.Active => Active,
        SequenceStepState.Hold => Hold,
        SequenceStepState.Failed => Fail,
        _ => Brushes.Transparent
    };
    public object ConvertBack(object v, Type t, object p, CultureInfo c) => Binding.DoNothing;
    private static Brush New(string hex) { var b=new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex)!); b.Freeze(); return b; }
}
