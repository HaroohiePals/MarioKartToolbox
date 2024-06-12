using HaroohiePals.Gui;
using HaroohiePals.Gui.View.Modal;
using HaroohiePals.KCollision;
using NativeFileDialogs.Net;
using OpenTK.Mathematics;

namespace HaroohiePals.KclViewer.Gui;

internal sealed class KclViewerWindow : ImGuiViewWindow
{
    private const string WINDOW_TITLE = "KCL Viewer";
    private static readonly Vector2i WindowSize = new Vector2i(1200, 800);

    private string _kclFilePath;

    public KclViewerWindow(IModalService modalService)
        : base(ImGuiGameWindowSettings.Default with { Title = WINDOW_TITLE, Size = WindowSize }, modalService)
    {
        MainMenuItems = [
            new("File")
            {
                Items = new()
                {
                    new("Open", OpenFile)
                }
            }
        ];
    }

    private void OpenFile()
    {
        var result = Nfd.OpenDialog(out string outPath, new Dictionary<string, string> { { "KCollision Data", "kcl" } });

        if (result != NfdStatus.Ok)
            return;

        _kclFilePath = outPath;

        var kcl = Kcl.Load(File.OpenRead(_kclFilePath));

        if (Content is IDisposable disposable)
        {
            disposable.Dispose();
        }

        Content = new KclViewerContentView(kcl);
    }

    
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing && Content is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}