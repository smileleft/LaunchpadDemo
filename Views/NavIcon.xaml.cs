using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LaunchControl.Views;

public partial class NavIcon : UserControl
{
    public event RoutedEventHandler? Click;
    public NavIcon()
    {
        InitializeComponent();
        Loaded += (_, _) => Apply();
        bd.MouseEnter += (_, _) => { if (!IsActive) SetHover(true); };
        bd.MouseLeave += (_, _) => { if (!IsActive) SetHover(false); };
        bd.MouseLeftButtonUp += (_, e) => { Click?.Invoke(this, new RoutedEventArgs()); e.Handled = true; };
    }

    public static readonly DependencyProperty GlyphProperty =
        DependencyProperty.Register(nameof(Glyph), typeof(string), typeof(NavIcon), new PropertyMetadata(""));
    public string Glyph { get => (string)GetValue(GlyphProperty); set => SetValue(GlyphProperty, value); }

    public static readonly DependencyProperty LabelProperty =
        DependencyProperty.Register(nameof(Label), typeof(string), typeof(NavIcon), new PropertyMetadata(""));
    public string Label { get => (string)GetValue(LabelProperty); set => SetValue(LabelProperty, value); }

    public static readonly DependencyProperty IsActiveProperty =
        DependencyProperty.Register(nameof(IsActive), typeof(bool), typeof(NavIcon),
            new PropertyMetadata(false, (d, _) => ((NavIcon)d).Apply()));
    public bool IsActive { get => (bool)GetValue(IsActiveProperty); set => SetValue(IsActiveProperty, value); }

    private Brush Res(string key) => (Brush)FindResource(key);

    private void Apply()
    {
        if (bd == null) return;
        if (IsActive)
        {
            bd.Background = Res("Bg2");
            bd.BorderBrush = Res("Accent");
            glyph.Foreground = Res("Accent");
            label.Foreground = Res("TextPrimary");
        }
        else
        {
            bd.Background = Brushes.Transparent;
            bd.BorderBrush = Brushes.Transparent;
            glyph.Foreground = Res("TextMuted");
            label.Foreground = Res("TextMuted");
        }
    }

    private void SetHover(bool on)
    {
        glyph.Foreground = on ? Res("TextSecondary") : Res("TextMuted");
        label.Foreground = on ? Res("TextSecondary") : Res("TextMuted");
    }
}
