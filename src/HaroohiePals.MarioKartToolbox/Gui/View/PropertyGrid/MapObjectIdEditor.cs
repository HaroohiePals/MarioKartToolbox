using HaroohiePals.Actions;
using HaroohiePals.Gui.View.PropertyGrid;
using HaroohiePals.MarioKartToolbox.Gui.View.CourseEditor;
using HaroohiePals.NitroKart.MapData;
using HaroohiePals.NitroKart.MapData.Intermediate.Sections;
using HaroohiePals.NitroKart.MapObj;
using ImGuiNET;
using System.Linq;
using System.Reflection;

namespace HaroohiePals.MarioKartToolbox.Gui.View.PropertyGrid;

class MapObjectIdEditor : IPropertyEditor
{
    private readonly IMkdsMapObjDatabase _mobjDatabase;
    private readonly MapObjectIdSelectorModalView _selectorWindow;

    public MapObjectIdEditor(IMkdsMapObjDatabase database)
    {
        _mobjDatabase = database;
        _selectorWindow = new MapObjectIdSelectorModalView(database);
    }

    public bool IsEditorForProperty(PropertyInfo propertyInfo)
        => propertyInfo.PropertyType == typeof(MkdsMapObjectId);

    private MkdsMapObjectId GetMapObjectIdFromObject(object obj)
    {
        if (obj is MkdsMapObject mobj)
            return mobj.ObjectId;

        return 0;
    }


    public bool Draw(string label, PropertyEditorContext context)
    {
        bool result = false;

        MkdsMapObjectId[] values = context.Property.SourceObjects.Select(x => GetMapObjectIdFromObject(x)).ToArray();
        MkdsMapObjectId value = values[0];

        bool sameValues = values.All(x => x == value);
        var mobjIdInfo = _mobjDatabase.GetById(value);
        string formatted = sameValues ? mobjIdInfo is not null ? mobjIdInfo.Name : $"Unused ({value})" : "Multiple values";

        if (ImGui.BeginCombo(label, formatted))
            ImGui.EndCombo();

        if (ImGui.IsItemClicked())
        {
            ImGui.CloseCurrentPopup();
            _selectorWindow.MapObjectId = value;
            _selectorWindow.Reset();
            _selectorWindow.Open();
        }

        _selectorWindow.Draw();

        if (_selectorWindow.Confirm)
        {
            bool isTimeTrialVisible = _mobjDatabase.GetById(_selectorWindow.MapObjectId).IsTimeTrialVisible;

            context.TempActions.Add(new SetPropertyAction(context.Property.SourceObjects, context.Property.Name, _selectorWindow.MapObjectId));

            foreach (var mobj in context.Property.SourceObjects.Where(x => x is MkdsMapObject).Cast<MkdsMapObject>())
            {
                context.TempActions.Add(mobj.SetPropertyAction(x => x.TTVisible, isTimeTrialVisible));
            }

            result = true;
            context.ApplyEdits = true;

            _selectorWindow.Reset();
        }

        return result;
    }
}
