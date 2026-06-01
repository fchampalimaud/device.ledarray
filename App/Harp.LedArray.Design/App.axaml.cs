using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

using Harp.LedArray.Design.ViewModels;
using Harp.LedArray.Design.Views;

namespace Harp.LedArray.Design;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = new LedArrayViewModel()
            };
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new LedArrayView
            {
                DataContext = new LedArrayViewModel()
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
    
    private void NativeMenuItem_OnClick(object sender, EventArgs e)
    { 
        if (Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime owner &&
            owner.MainWindow is Window ownerWindow)
        {
            var about = new About() { DataContext = new AboutViewModel() };
            about.ShowDialog(ownerWindow);
        }
    }
}
