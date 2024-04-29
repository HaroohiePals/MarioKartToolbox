using HaroohiePals.NitroKart.MapData;
using System;

namespace HaroohiePals.NitroKart.MapObj
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public sealed class MapObjAttribute : Attribute
    {
        public readonly MkdsMapObjectId Id;
        public readonly Type[]      RenderPartTypes;
        public readonly Type        LogicPartType;
        public readonly object      Arg;

        public MapObjAttribute(MkdsMapObjectId id, Type[] renderPartTypes, Type logicPartType = null, object arg = null)
        {
            Id              = id;
            RenderPartTypes = renderPartTypes;
            LogicPartType   = logicPartType;
            Arg             = arg;
        }
    }
}