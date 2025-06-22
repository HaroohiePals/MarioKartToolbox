using HaroohiePals.Gui.View.Modal;
using HaroohiePals.KclViewer.Gui;

namespace HaroohiePals.KclViewer;

static class Program
{
    private static void Main(string[] args)
    {
        var modalService = new ModalService();
        new KclViewerWindow(modalService).Run();
    }
}