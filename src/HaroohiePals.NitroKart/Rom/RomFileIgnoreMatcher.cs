#nullable enable
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace HaroohiePals.NitroKart.Rom;

sealed class RomFileIgnoreMatcher
{
    private readonly Regex[] _regexes;

    public RomFileIgnoreMatcher(string[] patterns)
    {
        if (patterns.Length == 0)
        {
            _regexes = [];
            return;
        }

        var list = new List<Regex>();

        foreach (string pattern in patterns)
        {
            if (string.IsNullOrWhiteSpace(pattern))
                continue;

            list.Add(new Regex(GlobToRegex(pattern), 
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant));
        }

        _regexes = list.ToArray();
    }

    private static string GlobToRegex(string glob)
    {
        string normalized = glob.Replace('\\', '/').Trim();

        // gitignore semantics:
        //  - leading '/' anchors to the root
        //  - otherwise, a pattern with no '/' inside matches the name at any depth
        //  - a pattern with '/' inside is anchored to the root
        bool rootAnchored;
        if (normalized.StartsWith('/'))
        {
            rootAnchored = true;
            normalized = normalized.TrimStart('/');
        }
        else
        {
            rootAnchored = normalized.Contains('/');
        }

        var sb = new StringBuilder();
        sb.Append(rootAnchored ? "^" : "(^|.*/)");

        for (int i = 0; i < normalized.Length; i++)
        {
            char c = normalized[i];
            switch (c)
            {
                case '*' when i + 1 < normalized.Length && normalized[i + 1] == '*':
                    sb.Append(".*");
                    i++;
                    break;
                case '*':
                    sb.Append("[^/]*");
                    break;
                case '?':
                    sb.Append("[^/]");
                    break;
                case '/':
                    sb.Append('/');
                    break;
                default:
                    sb.Append(Regex.Escape(c.ToString()));
                    break;
            }
        }

        sb.Append('$');
        return sb.ToString();
    }

    public bool IsIgnored(string fsRoot, string fullPath)
    {
        if (_regexes.Length == 0)
            return false;

        string rel = Path.GetRelativePath(fsRoot, fullPath).Replace('\\', '/');

        foreach (var regex in _regexes)
        {
            if (regex.IsMatch(rel))
                return true;
        }
        
        return false;
    }
}
