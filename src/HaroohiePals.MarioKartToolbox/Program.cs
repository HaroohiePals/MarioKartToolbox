using Autofac;
using HaroohiePals.Gui.View.Modal;
using HaroohiePals.MarioKartToolbox.Application.Clipboard;
using HaroohiePals.MarioKartToolbox.Application.Clipboard.Json;
using HaroohiePals.MarioKartToolbox.Application.Discord;
using HaroohiePals.MarioKartToolbox.Application.Settings;
using HaroohiePals.MarioKartToolbox.Gui.View.CourseEditor;
using HaroohiePals.MarioKartToolbox.Gui.View.Main;
using HaroohiePals.MarioKartToolbox.Gui.ViewModel.Main;
using HaroohiePals.MarioKartToolbox.Infrastructure.Discord;
using HaroohiePals.MarioKartToolbox.Resources;
using HaroohiePals.NitroKart.MapObj;
using HaroohiePals.NitroKart.Validation.Course;
using HaroohiePals.NitroKart.Validation.MapData;
using System;
using System.IO;
using TextCopy;

namespace HaroohiePals.MarioKartToolbox;

class Program
{
    private const string APPLICATION_SETTINGS_FILE_PATH = "Preferences.json";
    private const string DISCORD_CLIENT_ID = "1040294010071298129";
    private const string DISCORD_DEFAULT_GAME_NAME = "Mario Kart DS";

    static void Main(string[] args)
    {
        InitializeWorkingDir();

        var builder = new ContainerBuilder();
        builder.Register(_ => new ApplicationSettingsService(APPLICATION_SETTINGS_FILE_PATH))
            .As<IApplicationSettingsService>()
            .SingleInstance();
        builder.Register(_ => new DiscordRichPresenceService(DISCORD_CLIENT_ID))
            .As<IDiscordRichPresenceService>()
            .SingleInstance();
        builder.RegisterType<ApplicationDiscordRichPresenceService>()
            .As<IApplicationDiscordRichPresenceService>()
            .SingleInstance();
        builder.RegisterType<ModalService>()
            .As<IModalService>()
            .SingleInstance();
        builder.RegisterType<MainWindowFactory>()
            .As<IMainWindowFactory>()
            .SingleInstance();
        builder.RegisterType<CourseEditorViewFactory>()
            .As<ICourseEditorViewFactory>()
            .SingleInstance();
        builder.RegisterType<JsonMapDataClipboardSerializer>()
            .As<IMapDataClipboardSerializer>();
        builder.RegisterType<Clipboard>()
            .As<IClipboard>();
        builder.RegisterType<OSMapDataClipboard>()
            .As<IMapDataClipboard>()
            .SingleInstance();
        // builder.RegisterType<ListMapDataClipboard>()
        //     .As<IMapDataClipboard>()
        //     .SingleInstance();
        builder.RegisterType<MkdsMapObjDatabase>()
            .As<IMkdsMapObjDatabase>()
            .SingleInstance();

        //Validation
        builder.RegisterType<MkdsCourseValidatorFactory>()
            .As<IMkdsCourseValidatorFactory>()
            .SingleInstance();
        builder.RegisterType<MkdsMapDataValidatorFactory>()
            .As<IMkdsMapDataValidatorFactory>()
            .SingleInstance();

        builder.RegisterType<MainWindowViewModel>();
        builder.RegisterType<MainWindow>();
        var container = builder.Build();

        using (var scope = container.BeginLifetimeScope())
        {
            var discordRichPresenceService = scope.Resolve<IApplicationDiscordRichPresenceService>();
            discordRichPresenceService.SetApplicationState(RichPresenceApplicationState.Idle);
            discordRichPresenceService.SetGameName(DISCORD_DEFAULT_GAME_NAME);
            var mainWindow = scope.Resolve<MainWindow>();
            mainWindow.Run();
        }
    }

    private static void InitializeWorkingDir()
    {
        //Create working dir
        string baseDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        string workingDir = Path.Combine(baseDir, "MarioKartToolbox");
        Directory.CreateDirectory(workingDir);

        //Set working dir
        Directory.SetCurrentDirectory(workingDir);

        //Copy default imgui.ini inside this folder
        if (!File.Exists("imgui.ini"))
            File.WriteAllText("imgui.ini", Config.ImGuiIni);

        //Copy theme files (I don't really like this, it's kinda hacky)
        if (!Directory.Exists("Themes"))
        {
            Directory.CreateDirectory("Themes");
            string themesPath = Path.Combine(AppContext.BaseDirectory, "Themes");
            foreach (var file in Directory.EnumerateFiles(themesPath, "*.json"))
                File.Copy(file, Path.Combine("Themes", new FileInfo(file).Name));
        }
    }
}