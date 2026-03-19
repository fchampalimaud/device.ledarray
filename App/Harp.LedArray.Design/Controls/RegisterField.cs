using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Media;

namespace Harp.LedArray.Design.Controls;

public class RegisterField : HeaderedContentControl
{
    public static readonly StyledProperty<string?> DescriptionProperty =
        AvaloniaProperty.Register<RegisterField, string?>(nameof(Description));
    
    public static readonly StyledProperty<IBrush?> IconForegroundProperty =
        AvaloniaProperty.Register<RegisterField, IBrush?>(nameof(IconForeground));

    public string? Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }
    
    public IBrush? IconForeground
    {
        get => GetValue(IconForegroundProperty);
        set => SetValue(IconForegroundProperty, value);
    }
}
