﻿using System.Reflection;
using System.Windows;
using ReactiveUI;
using Serilog;
using Splat;
using WoWAHDataProject.Code;
namespace WoWAHDataProject;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : System.Windows.Application
{

    public App()
    {
        Locator.CurrentMutable.RegisterViewsForViewModels(Assembly.GetCallingAssembly());
    }
    public void AppExit(object sender, ExitEventArgs e)
    {
        if (Egg.音)
        {
            Egg.封印Egg(Egg.音ミク首尾よく[0].Item1, Egg.音ミク首尾よく[0].Item2);
            Egg.封印Egg(Egg.音ミク失敗[0].Item1, Egg.音ミク失敗[0].Item2);
        }
        Log.Information("Application is exiting");
    }
}