using HaroohiePals.Actions;
using HaroohiePals.Core.ComponentModel;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace HaroohiePals.Gui.View.PropertyGrid;

public class PropertyGridView : IView
{
    public PropertyGridFlags Flags = PropertyGridFlags.ShowCategories;

    private List<PropertyGridItem> _items;
    private object[] _selectedObjects;

    private List<IPropertyEditor> _editors = new();

    private ActionStack _actionStack;
    private List<IAction> _tempActions;
    private IAtomicActionBuilder _atomicActionBuilder;

    public PropertyGridView(ActionStack actionStack = null)
    {
        InitDefaultEditors();
        _actionStack = actionStack ?? new ActionStack();
    }

    private PropertyGridView(object[] selectedObjects, List<IPropertyEditor> editors,
        ActionStack actionStack = null)
    {
        _editors = editors ?? new List<IPropertyEditor>();
        _actionStack = actionStack ?? new ActionStack();
        Select(selectedObjects);
    }

    public void Select(params object[] selectedObjects)
    {
        _selectedObjects = selectedObjects;
        Refresh();
    }

    public void Select(IEnumerable<object> selectedObjects)
    {
        _selectedObjects = selectedObjects.ToArray();
        Refresh();
    }

    public void Refresh()
    {
        if (_selectedObjects != null)
            _items = CreateItems(_selectedObjects).ToList();
    }

    public void Clear()
    {
        _selectedObjects = null;
        _items?.Clear();
    }

    public bool Draw()
    {
        if (_items == null || _items.Count() == 0)
            return false;

        bool result = false;

        List<PropertyGridItem> items = _items;

        if (Flags.HasFlag(PropertyGridFlags.SortProperty))
        {
            if (Flags.HasFlag(PropertyGridFlags.SortPropertyDescending))
                items = _items.OrderByDescending(x => x.DisplayName).ToList();
            else
                items = _items.OrderBy(x => x.DisplayName).ToList();
        }

        if (Flags.HasFlag(PropertyGridFlags.ShowCategories))
        {
            var categories = _items.Select(x => x.Category).Distinct().ToList();

            if (Flags.HasFlag(PropertyGridFlags.SortCategory))
            {
                if (Flags.HasFlag(PropertyGridFlags.SortCategoryDescending))
                    categories = categories.OrderByDescending(x => x).ToList();
                else
                    categories = categories.OrderBy(x => x).ToList();
            }

            foreach (var cat in categories)
            {
                var catProps = items.Where(x => x.Category == cat);

                if (ImGui.CollapsingHeader(cat, ImGuiTreeNodeFlags.DefaultOpen))
                {
                    var tableResult = DrawTable(catProps, cat);

                    if (!result && tableResult)
                        result = tableResult;
                }
            }
        }
        else
        {
            result = DrawTable(items);
        }

        return result;
    }

    public void RegisterEditor<T>(int index = -1) where T : IPropertyEditor, new()
    {
        RegisterEditor(new T(), index);
    }

    public void RegisterEditor(IPropertyEditor editor, int index = -1)
    {
        if (index <= -1)
            _editors?.Add(editor);
        else
            _editors.Insert(index, editor);
    }

    private void InitDefaultEditors()
    {
        RegisterEditor<StringPropertyEditor>();
        RegisterEditor<BooleanPropertyEditor>();
        RegisterEditor<EnumPropertyEditor>();

        // todo: Probably move this out
        RegisterEditor<Rgb555ArrayPropertyEditor>();

        RegisterEditor<ArrayPropertyEditor>();
        RegisterEditor<ScalarPropertyEditor>();
        RegisterEditor<ColorPropertyEditor>();

        // todo: Probably move those out
        RegisterEditor<Rgb555PropertyEditor>();
        RegisterEditor<Vector2PropertyEditor>();
        RegisterEditor<Vector3PropertyEditor>();
    }

    private IPropertyEditor GetEditor(PropertyInfo propertyInfo)
    {
        foreach (var editor in _editors)
        {
            if (editor.IsEditorForProperty(propertyInfo))
                return editor;
        }

        return null;
    }

    private PropertyGridItem CreateItem(object[] selectedObjects, PropertyInfo propertyInfo)
    {
        return new PropertyGridItem
        {
            Name = propertyInfo.Name,
            DisplayName = propertyInfo.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName ?? propertyInfo.Name,
            Description = propertyInfo.GetCustomAttribute<DescriptionAttribute>()?.Description ?? null,
            Category = propertyInfo.GetCustomAttribute<CategoryAttribute>()?.Category ?? "Misc",
            IsReadOnly = propertyInfo.GetCustomAttribute<ReadOnlyAttribute>()?.IsReadOnly ?? false,

            PropertyInfo = propertyInfo,
            Type = propertyInfo.PropertyType,
            SourceObjects = selectedObjects,

            Editor = GetEditor(propertyInfo)
        };
    }

    private IEnumerable<PropertyGridItem> CreateItems(object[] selectedObjects)
    {
        if (selectedObjects == null || selectedObjects.Length == 0)
        {
            return null;
        }

        var properties = PropertyGridUtil.SelectUniqueProperties(selectedObjects);

        var items = new List<PropertyGridItem>();

        foreach (var prop in properties)
        {
            if (!(prop.GetCustomAttribute<BrowsableAttribute>()?.Browsable ?? true))
                continue;

            var item = CreateItem(selectedObjects, prop);

            var nested = prop.GetCustomAttribute<NestedAttribute>();
            if (nested != null && nested.NestType == NestType.Category)
            {
                var itemsToAdd =
                    CreateItems(selectedObjects.Select(y => y.GetType().GetProperty(prop.Name).GetValue(y))
                        .ToArray()).ToList();

                itemsToAdd.ForEach(x => x.Category = item.DisplayName);

                items.AddRange(itemsToAdd);
            }
            else
            {
                if (nested != null && nested.NestType == NestType.Nested)
                {
                    item.NestedPropertyGridView = new PropertyGridView(selectedObjects
                            .Select(y => y.GetType().GetProperty(prop.Name).GetValue(y)).ToArray(), _editors,
                        _actionStack);
                    item.NestedPropertyGridView.Flags = PropertyGridFlags.None;
                }

                //Skip properties without an editor
                if (item.Editor != null || item.NestedPropertyGridView != null)
                    items.Add(item);
            }
        }

        return items;
    }

    private bool DrawTable(IEnumerable<PropertyGridItem> items, string label = "Misc")
    {
        bool result = false;

        if (ImGui.BeginTable($"Grid_{label}", 2, ImGuiTableFlags.SizingStretchSame | ImGuiTableFlags.Resizable))
        {
            ImGui.TableSetupColumn($"##GridColName_{label}", ImGuiTableColumnFlags.None, 0.5f);
            ImGui.TableSetupColumn($"##GridColInput_{label}", ImGuiTableColumnFlags.None, 1.5f);

            foreach (var property in items)
            {
                string inputLabel = $"##Input_{label}_{property.Name}";
                var inputResult = DrawInput(inputLabel, property);

                if (!result && inputResult)
                    result = true;
            }

            ImGui.EndTable();
        }

        return result;
    }

    private bool DrawInput(string label, PropertyGridItem property)
    {
        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGui.Text(property.DisplayName);

        if (property.Description != null)
        {
            ImGui.SameLine();
            ImGui.TextDisabled("(?)");
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.PushTextWrapPos(ImGui.GetFontSize() * 35.0f);
                ImGui.TextUnformatted(property.Description);
                ImGui.PopTextWrapPos();
                ImGui.EndTooltip();
            }
        }

        ImGui.TableNextColumn();

        // Make field editor stretch the entire column size
        ImGui.PushItemWidth(-1);

        return DrawEditor(label, property);
    }

    private bool DrawEditor(string label, PropertyGridItem property)
    {
        if (_tempActions == null)
            _tempActions = new List<IAction>();
        if (_atomicActionBuilder == null)
            _atomicActionBuilder = _actionStack.PerformAtomic();

        bool result = false;

        var context = new PropertyEditorContext
        {
            TempActions = _tempActions,
            Property = property,
            Widget = this
        };

        if (property.IsReadOnly)
            ImGui.BeginDisabled();

        try
        {
            if (property.NestedPropertyGridView != null)
            {
                result = property.NestedPropertyGridView.Draw();
            }
            else if (property.Editor != null)
            {
                property.Editor.Draw(label, context);
            }
            else
            {
                ImGui.TextDisabled("Type not implemented");
            }
        }
        catch (Exception ex)
        {
            string error = ex.Message;
            ImGui.TextDisabled(error);
        }

        if (_tempActions != null && _tempActions.Count > 0)
        {
            _atomicActionBuilder.Do(new BatchAction(_tempActions));
            _tempActions = null;
        }

        if (context.ApplyEdits)
        {
            _atomicActionBuilder.Commit();
            _atomicActionBuilder = null;
            Refresh();
        }

        if (property.IsReadOnly)
            ImGui.EndDisabled();

        return result;
    }
}