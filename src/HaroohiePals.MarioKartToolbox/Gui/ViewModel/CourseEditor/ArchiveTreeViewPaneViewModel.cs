using HaroohiePals.IO.Archive;
using NativeFileDialogs.Net;
using System.IO;

namespace HaroohiePals.MarioKartToolbox.Gui.ViewModel.CourseEditor;

class ArchiveTreeViewPaneViewModel
{
    public Archive Archive { get; }

    public ArchiveTreeViewPaneViewModel(Archive archive)
    {
        Archive = archive;
    }

    public void Export(string path)
    {
        var result = Nfd.SaveDialog(out string outPath, defaultName: path);

        if (result == NfdStatus.Ok)
        {
            using (var stream = File.Create(outPath))
            {
                stream.Write(Archive.GetFileDataSpan(path));
            }
        }
    }
}