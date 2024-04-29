namespace HaroohiePals.MarioKartToolbox.Infrastructure.Discord;

interface IDiscordRichPresenceService
{
    bool IsEnabled { get; }

    void EnableRichPresence();
    void DisableRichPresence();
    void SetDetails(string details);
    void SetState(string state);
}