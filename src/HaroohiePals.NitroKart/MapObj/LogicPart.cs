using System;
using System.Collections.Generic;

namespace HaroohiePals.NitroKart.MapObj
{
    public abstract class LogicPart
    {
        public abstract void AddInstance(MObjInstance instance);
        public void Initialize() => GlobalInit();
        public abstract void Execute();

        protected virtual void GlobalInit() { }
        protected virtual void GlobalPreUpdate() { }
        protected virtual void GlobalPostUpdate() { }
    }

    public abstract class LogicPart<T> : LogicPart where T : MObjInstance
    {
        public enum LogicPartType
        {
            Type0 = 0,
            Type1 = 1,
            Type2 = 2,
            Type3 = 3
        }

        protected readonly LogicPartType _type;

        protected readonly MkdsContext _context;
        protected readonly List<T>     _instances = new();

        protected LogicPart(MkdsContext context, LogicPartType type)
        {
            _context = context;
            _type    = type;
        }

        public override void AddInstance(MObjInstance instance)
        {
            if (instance is not T tInst)
                throw new ArgumentException(nameof(instance));

            _instances.Add(tInst);
        }

        protected virtual void Update(T instance) { }

        public override void Execute()
        {
            try
            {
                GlobalPreUpdate();
            }
            catch
            {

            }
            
            foreach (var instance in _instances)
            {
                if ((instance.Flags & MObjInstance.InstanceFlags.Suspended) != 0)
                    continue;

                var oldPos = instance.Position;
                try
                {
                    Update(instance);
                }
                catch
                {

                }
                if (_type == LogicPartType.Type2)
                    instance.Velocity = instance.Position - oldPos;
            }

            try
            {
                GlobalPostUpdate();
            }
            catch
            {

            }
        }

        public T AcquireFreeObject()
        {
            foreach (var instance in _instances)
            {
                if ((instance.Flags & MObjInstance.InstanceFlags.Free) != 0)
                {
                    instance.SetVisible();
                    return instance;
                }
            }

            return null;
        }
    }
}