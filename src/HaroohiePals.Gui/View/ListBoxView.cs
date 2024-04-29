using ImGuiNET;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace HaroohiePals.Gui.View;

public class ListBoxView : IView
{
    private string _label;
    private object[] _items;
    private object _lastSelectedItem;
    private List<object> _selected = new();
    public Vector2 Size = Vector2.Zero;
    public bool MultiSelect = true;

    public IEnumerable<object> Selection => _selected.ToList();

    public ListBoxView(string label, IEnumerable<object> items)
    {
        _label = label;
        SetItems(items);
    }

    public void SetItems(IEnumerable<object> items)
    {
        _items = items.ToArray();
        _selected.Clear();
    }

    private void HandleSelection(object item)
    {
        if (MultiSelect && ImGui.GetIO().KeyCtrl)
        {
            if (_selected.Contains(item))
                _selected.Remove(item);
            else
                _selected.Add(item);
        }
        else if (MultiSelect && _lastSelectedItem != null && ImGui.GetIO().KeyShift)
        {
            int lastIndex = _items.ToList().IndexOf(_lastSelectedItem);
            int currIndex = _items.ToList().IndexOf(item);

            int from = System.Math.Min(lastIndex, currIndex);
            int to = System.Math.Max(lastIndex, currIndex);

            for (int i = from; i <= to; i++)
            {
                if (!_selected.Contains(_items[i]))
                    _selected.Add(_items[i]);
            }
        }
        else
        {
            _selected.Clear();
            _selected.Add(item);
        }

        _lastSelectedItem = _selected.Contains(item) ? item : null;
    }

    public bool Draw()
    {
        bool result = false;

        if (ImGui.BeginListBox(_label, Size))
        {
            int index = 0;

            foreach (var item in _items)
            {
                float cursorX = ImGui.GetCursorPosX();

                if (ImGui.Selectable($"##ListBoxItem_{index++}", _selected.Contains(item), ImGuiSelectableFlags.AllowOverlap, new Vector2(0, 0)))
                {
                    HandleSelection(item);
                    result = true;
                }

                ImGui.SameLine();
                ImGui.SetCursorPosX(cursorX);

                string shownText = item.ToString();
                bool grayedOut = shownText.StartsWith("(X)");

                if (grayedOut)
                {
                    shownText = shownText.Replace("(X)", "");
                    ImGui.TextDisabled(shownText);
                }
                else
                {
                    ImGui.Text(shownText);
                }
            }

            ImGui.EndListBox();
        }

        return result;
    }
}
