using HaroohiePals.Gui;
using HaroohiePals.Gui.View.Modal;
using HaroohiePals.MarioKartToolbox.Gui.ViewModel.Main;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.IO;
using System.Reflection;

namespace HaroohiePals.MarioKartToolbox.Gui.View.Main;

class MainWindow : ImGuiViewWindow
{
    private const string WINDOW_TITLE = "Mario Kart Toolbox";

    private readonly MainWindowViewModel _viewModel;

    public MainWindow(IModalService modalService, MainWindowViewModel viewModel)
        : base(new ImGuiGameWindowSettings(WINDOW_TITLE, new Vector2i(1400, 900), viewModel.GetUiScaleSetting(), IconConsts.Icons),
            modalService)
    {
        _viewModel = viewModel;
        _viewModel.SetMainWindowContent += (content) =>
        {
            Content = content;
        };

        MainMenuItems = [
            new("File")
            {
                Items = new()
                {
                    new("New"),
                    new("Open", _viewModel.OpenFile)
                }
            },
            new("Edit"),
            new("Tools")
            {
                Items = new()
                {
                    new("Preferences", _viewModel.ShowPreferences),
                }
            },
            new("Window")
            {
                Items = new()
                {
                    new("Restore default layout", _viewModel.RestoreDefaultLayout)
                }
            },
            new("About", _viewModel.ShowAbout)
        ];

        SetIcon(Resources.Icons.main128, Resources.Icons.main32);

        LoadFinished += OnFinishLoad;
    }

    private void OnFinishLoad()
    {
        Console.WriteLine($"OpenGL Version: {GL.GetString(StringName.Version)}");

        string informationalVersion = Assembly.GetExecutingAssembly()
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;

        Title = $"{WINDOW_TITLE} {informationalVersion}";

        string[] args = Environment.GetCommandLineArgs();
        if (args.Length > 1 && File.Exists(args[1]))
            _viewModel.OpenFile(args[1]);

        _viewModel.LoadTheme();
    }
}