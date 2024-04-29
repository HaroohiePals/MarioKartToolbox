using System;

namespace HaroohiePals.MarioKartToolbox.Application.Settings;

interface IApplicationSettingsService
{
    public delegate void SetDelegate(ref ApplicationSettings settings);

    event EventHandler ApplicationSettingsChanged;

    ref readonly ApplicationSettings Settings { get; }

    void Set(SetDelegate setFunc);
}