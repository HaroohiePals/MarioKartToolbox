﻿using HaroohiePals.Gui;
using HaroohiePals.Gui.View.Menu;
using HaroohiePals.Gui.View.Modal;
using HaroohiePals.MarioKartToolbox.Application.Settings;
using HaroohiePals.MarioKartToolbox.Gui.ViewModel.Main;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace HaroohiePals.MarioKartToolbox.Gui.View.Main;

class MainWindow : ImGuiViewWindow
{
    private const string WINDOW_TITLE = "Mario Kart Toolbox";

    private readonly MainWindowViewModel _viewModel;
    private readonly IApplicationSettingsService _applicationSettingsService;

    private IReadOnlyCollection<MenuItem> _mainMenuItems => 
    [
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

    public MainWindow(IModalService modalService, IApplicationSettingsService applicationSettingsService, MainWindowViewModel viewModel)
        : base(new ImGuiGameWindowSettings(WINDOW_TITLE, new Vector2i(1400, 900), viewModel.GetUiScaleSetting(), IconConsts.Icons, [ ImGuiFont.Default ]),
            modalService)
    {
        _viewModel = viewModel;
        _applicationSettingsService = applicationSettingsService;

        _viewModel.SetMainWindowContent += (content) =>
        {
            Content = content;
        };

        _applicationSettingsService.ApplicationSettingsChanged += OnApplicationSettingsChanged;

        MainMenuItems = _mainMenuItems;

        SetIcon(Resources.Icons.main128, Resources.Icons.main32);

        LoadFinished += OnFinishLoad;
    }

    private void OnApplicationSettingsChanged(object sender, EventArgs e)
    {
        // Refresh menu
        MainMenuItems = _mainMenuItems;
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