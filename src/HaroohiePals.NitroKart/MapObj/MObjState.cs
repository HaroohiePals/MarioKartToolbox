using HaroohiePals.Graphics;
using HaroohiePals.NitroKart.MapData;
using HaroohiePals.NitroKart.MapData.Intermediate.Sections;
using HaroohiePals.NitroKart.MapObj.Water;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace HaroohiePals.NitroKart.MapObj
{
    public class MObjState
    {
        private readonly MkdsContext _context;

        private readonly List<MObjInstance> _instances            = new();
        private readonly List<RenderPart>   _renderParts          = new();
        private readonly List<RenderPart>   _renderPartsNormal    = new();
        private readonly List<RenderPart>   _renderPartsBillboard = new();
        private readonly List<LogicPart>    _logicParts           = new();

        private bool _hasKoopaBlock;
        private bool _hasRotatingCylinder;
        private bool _hasBridge;
        private bool _hasMissionBarrier;

        private byte _minStaticPolygonId;
        private byte _curStaticPolygonId;
        private byte _maxStaticPolygonId;
        private byte _minCyclicPolygonId;
        private byte _curCyclicPolygonId;

        private byte _oldStaticPolygonId;
        private byte _oldCyclicPolygonId;

        public byte GetCyclicPolygonId()
        {
            if (++_curCyclicPolygonId > 62)
                _curCyclicPolygonId = _minCyclicPolygonId;
            return _curCyclicPolygonId;
        }

        public byte GetStaticPolygonId()
        {
            return _curStaticPolygonId++;
        }

        public void BackupPolygonIds()
        {
            _oldStaticPolygonId = _curStaticPolygonId;
            _oldCyclicPolygonId = _curCyclicPolygonId;
        }

        public void RestorePolygonIds()
        {
            _curStaticPolygonId = _oldStaticPolygonId;
            _curCyclicPolygonId = _oldCyclicPolygonId;
        }

        public Water.Water WaterObject;
        public WaterState  WaterState;

        public Random Random = new(0);

        private static readonly IReadOnlyDictionary<MkdsMapObjectId, (MapObjAttribute, Type)> MObjTypes =
            Assembly.GetAssembly(typeof(MObjState)).GetTypes()
                .SelectMany(t => t.GetCustomAttributes<MapObjAttribute>().Select(attr => (attr, t)))
                .ToDictionary(pair => pair.attr.Id);

        public MObjState(MkdsContext context, HashSet<MkdsMapObject> excludeObji)
        {
            _context           = context;
            _context.MObjState = this;

            _minStaticPolygonId = 31;
            _maxStaticPolygonId = (byte)(_minStaticPolygonId + 9);
            _minCyclicPolygonId = (byte)(_maxStaticPolygonId + 1);

            foreach (var obji in context.Course.MapData.MapObjects)
            {
                if (excludeObji.Contains(obji))
                    continue;

                CreateObject(obji, obji.ObjectId);
                _hasKoopaBlock       |= obji.ObjectId == MkdsMapObjectId.KoopaBlock;
                _hasRotatingCylinder |= obji.ObjectId == MkdsMapObjectId.Gear;
                _hasRotatingCylinder |= obji.ObjectId == MkdsMapObjectId.TestCylinder;
                _hasRotatingCylinder |= obji.ObjectId == MkdsMapObjectId.RotaryRoom;
                _hasRotatingCylinder |= obji.ObjectId == MkdsMapObjectId.RotaryBridge;
                _hasBridge           |= obji.ObjectId == MkdsMapObjectId.Bridge;
                _hasMissionBarrier   |= obji.ObjectId == MkdsMapObjectId.MissionBarrier;
            }

            WaterState?.InitModels();

            foreach (var renderPart in _renderParts)
                renderPart.Initialize();

            foreach (var renderPart in _renderParts)
            {
                if (!renderPart.IsShadow)
                    continue;

                if (renderPart.Type == RenderPart.RenderPartType.Normal)
                    _renderPartsNormal.Add(renderPart);
                else if (renderPart.Type == RenderPart.RenderPartType.Billboard)
                    _renderPartsBillboard.Add(renderPart);
            }

            foreach (var renderPart in _renderParts)
            {
                if (renderPart.IsShadow)
                    continue;

                if (renderPart.Type == RenderPart.RenderPartType.Normal)
                    _renderPartsNormal.Add(renderPart);
                else if (renderPart.Type == RenderPart.RenderPartType.Billboard)
                    _renderPartsBillboard.Add(renderPart);
            }

            foreach (var logicPart in _logicParts)
                logicPart.Initialize();
        }

        public LogicPart GetLogicPartForObjectId(MkdsMapObjectId objectId)
        {
            if (!MObjTypes.TryGetValue(objectId, out var type))
                return null;

            if (type.Item1.LogicPartType == null)
                return null;

            return _logicParts.FirstOrDefault(lp => lp.GetType() == type.Item1.LogicPartType);
        }

        public MObjInstance CreateSubObject(MObjInstance parentMObj, MkdsMapObjectId objectId)
        {
            return CreateObject(null, objectId, parentMObj);
        }

        public void CreateSubObjects(MkdsMapObjectId objectId, int count, MObjInstance parentMObj)
        {
            for (int i = 0; i < count; i++)
            {
                var obj = CreateSubObject(parentMObj, objectId);
                if ((obj.Flags & MObjInstance.InstanceFlags.Free) == 0)
                    obj.SetInvisible();
            }
        }

        public MObjInstance CreateObject(MkdsMapObject obji, MkdsMapObjectId objectId, MObjInstance parentMObj = null)
        {
            if (!MObjTypes.TryGetValue(objectId, out var type))
                return null;

            var renderParts = new RenderPart[3];

            if (type.Item1.RenderPartTypes != null)
            {
                for (int i = 0; i < 3; i++)
                {
                    if (type.Item1.RenderPartTypes.Length == i)
                        break;

                    if (type.Item1.RenderPartTypes[i] is null)
                        continue;

                    renderParts[i] = _renderParts.FirstOrDefault(rp => rp.GetType() == type.Item1.RenderPartTypes[i]);
                    if (renderParts[i] == null)
                    {
                        renderParts[i] = (RenderPart)Activator.CreateInstance(type.Item1.RenderPartTypes[i], _context);
                        _renderParts.Add(renderParts[i]);
                    }
                }
            }

            LogicPart logicPart = null;
            if (type.Item1.LogicPartType != null)
            {
                logicPart = _logicParts.FirstOrDefault(lp => lp.GetType() == type.Item1.LogicPartType);
                if (logicPart == null)
                {
                    logicPart = (LogicPart)Activator.CreateInstance(type.Item1.LogicPartType, _context);
                    _logicParts.Add(logicPart);

                    //Try initializing the newly added logic part, if it fails don't add it
                    try
                    {
                        logicPart.Initialize();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Couldn't initialize LogicPart {logicPart}: {ex.Message}");
                        return null;
                    }
                }
            }

            var instance = (MObjInstance)Activator.CreateInstance(type.Item2, _context, renderParts, logicPart);
            if (obji != null)
                instance.ObjiEntry = obji;
            else if (parentMObj != null)
                instance.ObjiEntry = parentMObj.ObjiEntry;
            instance.ObjectId = objectId;
            if (obji != null)
                instance.Position = obji.Position;
            else if (parentMObj != null)
                instance.Position = parentMObj.Position;
            if (obji != null)
            {
                instance.RotY = (ushort)(obji.Rotation.Y * (1 << 16) / 360.0);
                instance.Mtx = MObjUtil.EulerAnglesToMtx43(obji.Rotation);
            }
            else if (parentMObj != null)
            {
                instance.RotY = parentMObj.RotY;
                instance.Mtx = parentMObj.Mtx;
            }

            if (obji != null)
                instance.Scale = obji.Scale;
            else if (parentMObj != null)
                instance.Scale = parentMObj.Scale;

            // if (mobjConfig->colType)
            //     mobj_unhide(instance);
            // else
            //     mobj_hide(instance);

            if (renderParts[0] != null || renderParts[1] != null || renderParts[2] != null)
            {
                instance.Flags &= ~MObjInstance.InstanceFlags.DisableVisibilityUpdates;
                instance.Flags &= ~MObjInstance.InstanceFlags.Bit3;
                instance.Alpha = 31;
            }
            else
            {
                instance.Flags |= MObjInstance.InstanceFlags.DisableVisibilityUpdates;
                instance.Flags |= MObjInstance.InstanceFlags.Bit3;
            }

            // if (parentMObj != null)
            //     instance.Flags |= parentMObj.Flags & MObjInstance.InstanceFlags.DisabledBeforeStart;
            // else if(obji != null && obji.)

            if (logicPart != null)
                instance.Flags &= ~MObjInstance.InstanceFlags.Suspended;
            else
                instance.Flags |= MObjInstance.InstanceFlags.Suspended;

            //If an object fails to initialize, don't add it to the instances
            try
            {
                if (obji != null)
                    instance.Init(obji, type.Item1.Arg);
                else if (parentMObj != null)
                    instance.Init(null, parentMObj);
            }
            catch
            {
                return null;
            }

            foreach (var renderPart in renderParts)
                renderPart?.AddInstance(instance);
            logicPart?.AddInstance(instance);

            _instances.Add(instance);
            return instance;
        }

        public void Update()
        {
            foreach (var logicPart in _logicParts)
                logicPart.Execute();

            WaterState?.Update();
        }

        public void Render(in Matrix4x3d camMtx)
        {
            // mobj_20D4D44(camMtx);
            WaterState?.Render();
            foreach (var renderPart in _renderPartsNormal)
            {
                _context.RenderContext.GlobalState.LightColors[0] = Rgb555.White;
                _context.RenderContext.GlobalState.LightColors[1] = Rgb555.White;
                renderPart.Render(camMtx);
            }

            //bbm_restoreCameraMtx();
            foreach (var renderPart in _renderPartsBillboard)
                renderPart.Render(camMtx);
        }
    }
}