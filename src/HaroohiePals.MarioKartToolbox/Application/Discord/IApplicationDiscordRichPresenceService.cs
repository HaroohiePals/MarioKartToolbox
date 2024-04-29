namespace HaroohiePals.MarioKartToolbox.Application.Discord;

interface IApplicationDiscordRichPresenceService
{
    void SetGameName(string gameName);
    void SetApplicationState(RichPresenceApplicationState state);
}