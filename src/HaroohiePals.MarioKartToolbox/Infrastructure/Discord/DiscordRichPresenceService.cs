using DiscordRPC;
using System;

namespace HaroohiePals.MarioKartToolbox.Infrastructure.Discord;

class DiscordRichPresenceService : IDiscordRichPresenceService, IDisposable
{
    private const string ASSETS_LARGE_IMAGE_KEY = "main";
    private const string ASSETS_LARGE_IMAGE_TEXT = "";

    private readonly string _clientId;

    private DiscordRpcClient _rpcClient;

    private string _details = string.Empty;
    private string _state = string.Empty;

    public bool IsEnabled => _rpcClient is not null;

    public DiscordRichPresenceService(string clientId)
    {
        _clientId = clientId;
    }

    public void EnableRichPresence()
    {
        if (_rpcClient is not null)
            return;

        _rpcClient = new DiscordRpcClient(_clientId);
        _rpcClient.Initialize();
        _rpcClient.SetPresence(new RichPresence
        {
            Details = _details,
            State = _state,
            Assets = new Assets
            {
                LargeImageKey = ASSETS_LARGE_IMAGE_KEY,
                LargeImageText = ASSETS_LARGE_IMAGE_TEXT
            },
            Timestamps = Timestamps.Now
        });
    }

    public void DisableRichPresence()
    {
        if (_rpcClient is null)
            return;

        _rpcClient.Dispose();
        _rpcClient = null;
    }

    public void SetDetails(string details)
    {
        if (_details == details)
            return;

        _details = details;
        _rpcClient?.UpdateDetails(details);
    }

    public void SetState(string state)
    {
        if (_state == state)
            return;

        _state = state;
        _rpcClient?.UpdateState(state);
    }

    public void Dispose()
    {
        _rpcClient?.Dispose();
        _rpcClient = null;
    }
}