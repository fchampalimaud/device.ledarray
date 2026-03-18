using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Media;

namespace Harp.LedArray.Design.Controls;

public class GroupBox : HeaderedContentControl
{
    public static readonly StyledProperty<IBrush?> HeaderBackgroundProperty =
        AvaloniaProperty.Register<GroupBox, IBrush?>(nameof(HeaderBackground));

    public static readonly AttachedProperty<IBrush?> SurfaceBackgroundProperty =
        AvaloniaProperty.RegisterAttached<GroupBox, AvaloniaObject, IBrush?>(
            "SurfaceBackground",
            defaultValue: null,
            inherits: true);

    public IBrush? HeaderBackground
    {
        get => GetValue(HeaderBackgroundProperty);
        set => SetValue(HeaderBackgroundProperty, value);
    }

    public static void SetSurfaceBackground(AvaloniaObject element, IBrush? value) =>
        element.SetValue(SurfaceBackgroundProperty, value);

    public static IBrush? GetSurfaceBackground(AvaloniaObject element) =>
        element.GetValue(SurfaceBackgroundProperty);
}
