using Newtonsoft.Json;
using System;
using System.IO;

namespace HaroohiePals.MarioKartToolbox.Application.Settings;

sealed class ApplicationSettingsService : IApplicationSettingsService
{
    private readonly string _filePath;

    private ApplicationSettings _settings = new();

    public ref readonly ApplicationSettings Settings => ref _settings;

    public event EventHandler ApplicationSettingsChanged;

    public ApplicationSettingsService(string settingsFilePath)
    {
        _filePath = settingsFilePath;
        LoadFromFile();
    }

    public void Set(IApplicationSettingsService.SetDelegate setFunc)
    {
        setFunc(ref _settings);
        OnApplicationSettingsChanged();
    }

    private void OnApplicationSettingsChanged()
    {
        ApplicationSettingsChanged?.Invoke(this, EventArgs.Empty);
        SaveToFile();
    }

    private void LoadFromFile()
    {
        if (!File.Exists(_filePath))
            return;

        try
        {
            _settings = JsonConvert.DeserializeObject<ApplicationSettings>(File.ReadAllText(_filePath));
        }
        catch
        {

        }
    }

    private void SaveToFile()
    {
        try
        {
            File.WriteAllText(_filePath, JsonConvert.SerializeObject(_settings, Formatting.Indented));
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}