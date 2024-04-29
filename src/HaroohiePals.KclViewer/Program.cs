using HaroohiePals.Gui.View.Modal;
using HaroohiePals.KclViewer.Gui;

namespace HaroohiePals.KclViewer;

internal class Program
{
    static void Main(string[] args)
    {
        var modalService = new ModalService();

        new KclViewerWindow(modalService).Run();
    }
}