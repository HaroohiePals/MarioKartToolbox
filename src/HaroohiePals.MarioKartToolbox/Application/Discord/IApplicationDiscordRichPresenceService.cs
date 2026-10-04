#nullable enable
namespace HaroohiePals.MarioKartToolbox.Application.Discord;

interface IApplicationDiscordRichPresenceService
{
    void SetGameName(string gameName);
    void SetCourseName(string? courseName);
    void SetApplicationState(RichPresenceApplicationState state);
}