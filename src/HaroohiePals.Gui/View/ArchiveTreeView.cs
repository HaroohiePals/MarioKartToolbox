using HaroohiePals.Gui.View.Tree;
using HaroohiePals.IO.Archive;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace HaroohiePals.Gui.View;

public class ArchiveTreeView : IView
{
    public record NodeData(string Path, bool IsDirectory);

    private IReadOnlyArchive _archive;
    private readonly TreeView _tree;

    private readonly IReadOnlyDictionary<string, char> _fileExtIcons;

    public event Action<ArchiveTreeView, string, bool> Activate;

    public IReadOnlyArchive Archive
    {
        get => _archive;
        set
        {
            _archive = value;
            Invalidate();
        }
    }

    public NodeData SelectedItem => (NodeData)_tree.SelectedNode?.Data;

    public string Id { get; }

    public ArchiveTreeView(string id, IReadOnlyDictionary<string, char> fileExtIcons = null)
    {
        Id = id;
        _tree = new(Id);

        _fileExtIcons = fileExtIcons;
    }

    public void Invalidate()
    {
        _tree.Nodes.Clear();
        if (_archive == null)
            return;

        _tree.Nodes.AddRange(CreateNodesForFolder(IO.Archive.Archive.RootPath));
    }

    private void OnItemActivate(TreeNode node)
    {
        Activate?.Invoke(this, ((NodeData)node.Data).Path, ((NodeData)node.Data).IsDirectory);
    }

    private TreeNode[] CreateNodesForFolder(string path)
    {
        var nodes = new List<TreeNode>();
        var directories = _archive.EnumerateDirectories(path, false).OrderBy(s => s);
        foreach (string directory in directories)
        {
            var node = new TreeNode(FontAwesome6.Folder + "  " + directory, false);
            node.Data = new NodeData(IO.Archive.Archive.JoinPath(path, directory), true);
            node.Children.AddRange(CreateNodesForFolder(IO.Archive.Archive.JoinPath(path, directory)));
            node.Activate = OnItemActivate;
            nodes.Add(node);
        }

        var files = _archive.EnumerateFiles(path, false).OrderBy(s => s);
        foreach (string file in files)
        {
            string icon = FontAwesome6.File;
            if (_fileExtIcons != null)
            {
                string fileExt = new FileInfo(file).Extension?.ToLower();

                if (!string.IsNullOrEmpty(fileExt) && _fileExtIcons.ContainsKey(fileExt))
                    icon = $"{_fileExtIcons[fileExt]}";
            }

            var node = new TreeNode(icon + "  " + file, true);
            node.Data = new NodeData(IO.Archive.Archive.JoinPath(path, file), false);
            node.Activate = OnItemActivate;
            nodes.Add(node);
        }

        return nodes.ToArray();
    }

    public bool Draw()
    {
        return _tree.Draw();
    }
}