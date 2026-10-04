#nullable enable
using HaroohiePals.MarioKartToolbox.Application.Settings;
using HaroohiePals.MarioKartToolbox.Infrastructure.Discord;
using System;

namespace HaroohiePals.MarioKartToolbox.Application.Discord;

sealed class ApplicationDiscordRichPresenceService : IApplicationDiscordRichPresenceService
{
    private readonly IDiscordRichPresenceService _richPresenceService;
    private readonly IApplicationSettingsService _applicationSettings;

    private string _gameName = string.Empty;
    private string? _courseName;

    private RichPresenceApplicationState _applicationState = RichPresenceApplicationState.Idle;

    public ApplicationDiscordRichPresenceService(IDiscordRichPresenceService richPresenceService,
        IApplicationSettingsService applicationSettings)
    {
        _richPresenceService = richPresenceService;
        _applicationSettings = applicationSettings;

        _applicationSettings.ApplicationSettingsChanged += OnApplicationSettingsChanged;
        UpdateRichPresence();
    }

    public void SetGameName(string gameName)
    {
        _gameName = gameName;
        UpdateRichPresence();
    }

    public void SetCourseName(string? courseName)
    {
        _courseName = courseName;
        UpdateRichPresence();
    }

    public void SetApplicationState(RichPresenceApplicationState state)
    {
        _applicationState = state;
        UpdateRichPresence();
    }

    private void OnApplicationSettingsChanged(object? sender, EventArgs e)
    {
        UpdateRichPresence();
    }

    private void UpdateDescription()
    {
        string description = string.Empty;
        switch (_applicationState)
        {
            case RichPresenceApplicationState.Idle:
                description = "Idle";
                break;
            case RichPresenceApplicationState.CharacterKartEditor:
                description = "Edit Characters and Karts";
                break;
            case RichPresenceApplicationState.RomExplorer:
                description = "Browsing through the contents";
                break;
            case RichPresenceApplicationState.CourseEditor:
                description = _applicationSettings.Settings.Discord.ShowCourseName &&
                              !string.IsNullOrEmpty(_courseName)
                    ? $"Editing \"{_courseName}\""
                    : "Editing a course";
                break;
        }

        _richPresenceService.SetState(description);
    }

    private void UpdateRichPresence()
    {
        UpdateDescription();
        _richPresenceService.SetDetails(_applicationSettings.Settings.Discord.ShowRomTitle
            ? _gameName
            : string.Empty);

        if (_applicationSettings.Settings.Discord.EnableRichPresence == _richPresenceService.IsEnabled)
            return;

        if (_applicationSettings.Settings.Discord.EnableRichPresence)
            _richPresenceService.EnableRichPresence();
        else
            _richPresenceService.DisableRichPresence();
    }
}