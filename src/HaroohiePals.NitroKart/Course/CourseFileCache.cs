using HaroohiePals.IO.Archive;
using System;
using System.Collections.Generic;
using System.IO;

namespace HaroohiePals.NitroKart.Course;

public class CourseFileCache : Archive
{
    private Archive _archive;
    private Dictionary<string, object> _cache = new();
    private Dictionary<string, byte[]> _newData = new();

    public delegate void FileUpdatedEventHandler(CourseFileCache cache, string path);

    public event FileUpdatedEventHandler FileUpdated;

    public CourseFileCache(Archive archive)
    {
        _archive = archive;
    }

    public override IEnumerable<string> EnumerateFiles(string path, bool fullPath)
    {
        foreach (string file in _archive.EnumerateFiles(path, fullPath))
        {
            string fullFile = NormalizePath(fullPath ? file : JoinPath(path, file));
            if (_newData.TryGetValue(fullFile, out var data) && data == null)
                continue;

            yield return file;
        }

        string normPath = NormalizePath(path);

        foreach (var data in _newData)
        {
            if (!data.Key.StartsWith(normPath))
                continue;

            string name = data.Key.Substring(normPath.Length + 1);
            if (name.Contains(PathSeparator))
                continue;

            yield return fullPath ? data.Key : name;
        }
    }

    public override IEnumerable<string> EnumerateDirectories(string path, bool fullPath)
        => _archive.EnumerateDirectories(path, fullPath);

    public override byte[] GetFileData(string path)
    {
        path = NormalizePath(path);

        if (_newData.TryGetValue(path, out var data))
            return data;

        return _archive.GetFileData(path);
    }

    public override Stream OpenFileReadStream(string path)
        => new MemoryStream(GetFileData(path), false);

    public override void SetFileData(string path, byte[] data)
    {
        path = NormalizePath(path);

        _newData[path] = data;
        _cache.Remove(path);

        FileUpdated?.Invoke(this, path);
    }

    public T GetFile<T>(string path)
    {
        path = NormalizePath(path);

        if (_cache.TryGetValue(path, out var value))
            return (T)value;

        value = Activator.CreateInstance(typeof(T), GetFileData(path));
        _cache.Add(path, value);
        return (T)value;
    }

    public T GetFileOrDefault<T>(string path, T defaultValue = default)
    {
        path = NormalizePath(path);

        if (!ExistsFile(path))
            return defaultValue;

        if (_cache.TryGetValue(path, out var value))
            return (T)value;

        value = Activator.CreateInstance(typeof(T), GetFileData(path));
        _cache.Add(path, value);
        return (T)value;
    }

    public T GetFileOrDefault<T>(string path, Func<byte[], T> constructFunc, T defaultValue = default)
    {
        path = NormalizePath(path);

        if (!ExistsFile(path))
            return defaultValue;

        if (_cache.TryGetValue(path, out var value))
            return (T)value;

        value = constructFunc(GetFileData(path));
        _cache.Add(path, value);
        return (T)value;
    }

    public override bool ExistsFile(string path)
    {
        path = NormalizePath(path);

        if (_newData.TryGetValue(path, out var data))
            return data != null;

        return _archive.ExistsFile(path);
    }

    public override bool ExistsDirectory(string path)
        => _archive.ExistsDirectory(path);

    public override void DeleteFile(string path)
    {
        path = NormalizePath(path);

        bool existsArc = _archive.ExistsFile(path);
        bool existsNew = _newData.ContainsKey(path);

        if (!existsArc && !existsNew)
            return;

        if (!existsArc && existsNew)
            _newData.Remove(path);
        else
            _newData[path] = null;

        _cache.Remove(path);

        FileUpdated?.Invoke(this, path);
    }

    public void Flush()
    {
        foreach (var data in _newData)
        {
            if (data.Value == null)
                _archive.DeleteFile(data.Key);
            else
                _archive.SetFileData(data.Key, data.Value);
        }

        _newData.Clear();
    }
}