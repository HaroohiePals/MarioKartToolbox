using HaroohiePals.Actions;
using HaroohiePals.Gui.View.ImSequencer;
using HaroohiePals.MarioKart.MapData;
using HaroohiePals.NitroKart.MapData;
using HaroohiePals.NitroKart.MapData.Intermediate.Sections;
using System.Collections.Generic;
using System.Linq;

namespace HaroohiePals.MarioKartToolbox.Gui.ViewModel.CourseEditor;

class CameraIntroSequence : ISequence
{
    public bool Focused { get; set; }

    private readonly MapDataCollection<MkdsCamera> _cameras;
    private readonly ActionStack _actionStack;

    private int _lastEditIndex = -1;
    private short _lastDuration;

    public CameraIntroSequence(MapDataCollection<MkdsCamera> cameras, ActionStack actionStack)
    {
        _cameras = cameras;
        _actionStack = actionStack;
    }

    public void Get(int index, out int start, out int end, out int type, out uint color)
    {
        var items = GetIntroCamerasInOrder();
        start = items.Take(index).Sum(x => x.Duration);
        end = start + items[index].Duration - 1;
        type = 0;
        color = 0xFFFFFFFF;
    }

    public void BeginEdit(int index)
    {
        var item = GetIntroCameraByOrderIndex(index);
        _lastEditIndex = index;
        _lastDuration = item.Duration;
    }

    public void Edit(int index, int start, int end)
    {
        var item = GetIntroCameraByOrderIndex(index);
        item.Duration = (short)(end - start);
        _lastEditIndex = index;
    }

    public void EndEdit()
    {
        var item = GetIntroCameraByOrderIndex(_lastEditIndex);

        if (_lastDuration == item.Duration)
            return;

        short targetDuration = item.Duration;
        item.Duration = _lastDuration;
        _actionStack.Add(item.SetPropertyAction(o => o.Duration, targetDuration));
    }

    public int GetItemCount() => GetIntroCamerasInOrder().Length;
    public string GetItemLabel(int index) => $"[Cam {_cameras.IndexOf(GetIntroCamerasInOrder()[index])}] Duration";
    public int GetFrameMin() => 0;
    public int GetFrameMax()
    {
        var items = GetIntroCamerasInOrder();
        return items.Sum(x => x.Duration) + 100;
    }
    public int GetItemTypeCount() => 1;
    public string GetItemTypeName(int typeIndex) => $"Type {typeIndex}";


    private MkdsCamera[] GetIntroCamerasInOrder()
    {
        if (_cameras == null)
            return new MkdsCamera[] { };

        var list = new List<MkdsCamera>();

        var firstTopCamera = _cameras.FirstOrDefault(x => x.FirstIntroCamera == MkdsCameIntroCamera.Top);

        if (firstTopCamera != null)
        {
            list.Add(firstTopCamera);
            var next = firstTopCamera.NextCamera?.Target;

            while (next != null)
            {
                list.Add(next);
                next = next.NextCamera?.Target;

                //Break eventual loops
                if (list.Contains(next))
                    break;
            }
        }

        return list.ToArray();
    }

    public MkdsCamera GetIntroCameraByOrderIndex(int index)
    {
        var items = GetIntroCamerasInOrder();
        return items[index];
    }
}
