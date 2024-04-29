using HaroohiePals.Actions;
using HaroohiePals.MarioKart.Actions;
using HaroohiePals.MarioKart.MapData;
using HaroohiePals.NitroKart.Course;
using HaroohiePals.NitroKart.Extensions;
using HaroohiePals.NitroKart.MapData.Intermediate.Sections;
using OpenTK.Mathematics;
using System.Collections.Generic;

namespace HaroohiePals.MarioKartToolbox.Gui.Viewport
{
    internal class CheckPointPathDrawTool : ConnectedPathDrawTool<MkdsCheckPointPath, MkdsCheckPoint>
    {
        public CheckPointPathDrawTool(IMkdsCourse course, MapDataCollection<MkdsCheckPoint> collection) : base(course, collection, course.MapData.CheckPointPaths)
        {
        }

        protected override void UpdateEntry(Vector3d rayStart, Vector3d rayDir)
        {
            if (IsMouseDown)
                _entry.Point1 = _entry.Point2 = new Vector2d(rayStart.X, rayStart.Z);
            else if (IsMouseDrag)
                _entry.Point2 = new Vector2d(rayStart.X, rayStart.Z);
        }

        private void SetStartGotoSection()
        {
            var actions = new List<IAction>();

            void setEntryFields(MkdsCheckPoint entry, bool startSection = false, bool endSection = false)
            {
                if (entry == null)
                    return;

                short startSectionValue = (short)(startSection ? 0 : -1);
                short gotoSectionValue = (short)(endSection ? 0 : -1);

                //Avoid useless actions
                if (entry.StartSection != startSectionValue || entry.GotoSection != gotoSectionValue)
                {
                    actions.Add(entry.SetPropertyAction(o => o.StartSection, startSectionValue));
                    actions.Add(entry.SetPropertyAction(o => o.GotoSection, gotoSectionValue));
                }
            }

            foreach (var path in _paths)
            {
                foreach (var entry in path.Points)
                {
                    if (entry == path.Points[0])
                        setEntryFields(entry, true);
                    else if (entry == path.Points[^1])
                        setEntryFields(entry, false, true);
                    else
                        setEntryFields(entry);
                }
            }

            _atomicActionBuilder.Do(new BatchAction(actions));
        }

        private void SetNearestRespawnPoint()
        {
            if (_course.MapData.RespawnPoints != null && _course.MapData.RespawnPoints.Count > 0)
            {
                var actions = new List<IAction>();

                var entryAvgPos = (_entry.Point1 + _entry.Point2) / 2;

                var nearestRespawn = _course.MapData.RespawnPoints.FindNearest(entryAvgPos);

                actions.Add(new SetMapDataEntryReferenceAction<MkdsRespawnPoint>(_entry, nameof(MkdsCheckPoint.Respawn), null, nearestRespawn));
                _atomicActionBuilder.Do(new BatchAction(actions));
            }
        }

        protected override void PerformFinalizationActions()
        {
            SetStartGotoSection();
            SetNearestRespawnPoint();
        }
    }
}
