using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace HaroohiePals.IO.Serialization
{
    public static class SerializationUtil
    {
        public static FieldType TypeToFieldType(Type type)
        {
            if (type == typeof(byte))
                return FieldType.U8;
            if (type == typeof(sbyte))
                return FieldType.S8;
            if (type == typeof(ushort))
                return FieldType.U16;
            if (type == typeof(short))
                return FieldType.S16;
            if (type == typeof(uint))
                return FieldType.U32;
            if (type == typeof(int))
                return FieldType.S32;
            if (type == typeof(ulong))
                return FieldType.U64;
            if (type == typeof(long))
                return FieldType.S64;

            throw new Exception("Unexpected primitive field type " + type.Name);
        }

        public static Type FieldTypeToType(FieldType type) => type switch
        {
            FieldType.U8     => typeof(byte),
            FieldType.S8     => typeof(sbyte),
            FieldType.U16    => typeof(ushort),
            FieldType.S16    => typeof(short),
            FieldType.U32    => typeof(uint),
            FieldType.S32    => typeof(int),
            FieldType.U64    => typeof(ulong),
            FieldType.S64    => typeof(long),
            FieldType.Fx16   => typeof(double),
            FieldType.Fx32   => typeof(double),
            FieldType.Float  => typeof(float),
            FieldType.Double => typeof(double),
            _                => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };

        public static int GetTypeSize(FieldType type) => type switch
        {
            FieldType.U8     => 1,
            FieldType.S8     => 1,
            FieldType.U16    => 2,
            FieldType.S16    => 2,
            FieldType.U32    => 4,
            FieldType.S32    => 4,
            FieldType.U64    => 8,
            FieldType.S64    => 8,
            FieldType.Fx16   => 2,
            FieldType.Fx32   => 4,
            FieldType.Float  => 4,
            FieldType.Double => 8,
            _                => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };

        public static bool HasPrimitiveType(FieldInfo field)
        {
            return field.FieldType.IsPrimitive ||
                   field.FieldType.IsEnum ||
                   field.GetCustomAttribute<TypeAttribute>() != null ||
                   field.GetCustomAttribute<Fx32Attribute>() != null ||
                   field.GetCustomAttribute<Fx16Attribute>() != null;
        }

        public static bool HasPrimitiveArrayType(FieldInfo field)
        {
            if (!field.FieldType.IsArray)
                return false;

            var type = field.FieldType.GetElementType();

            return type.IsPrimitive ||
                   field.GetCustomAttribute<TypeAttribute>() != null ||
                   field.GetCustomAttribute<Fx32Attribute>() != null ||
                   field.GetCustomAttribute<Fx16Attribute>() != null;
        }

        public static FieldType GetFieldPrimitiveType(FieldInfo field)
        {
            bool isFx32       = field.GetCustomAttribute<Fx32Attribute>() != null;
            bool isFx16       = field.GetCustomAttribute<Fx16Attribute>() != null;
            bool hasFieldType = field.GetCustomAttribute<TypeAttribute>() != null;
            int  count        = (isFx32 ? 1 : 0) + (isFx16 ? 1 : 0) + (hasFieldType ? 1 : 0);
            if (count > 1)
                throw new Exception("More than one field type specified for field " + field.Name + " in type " +
                                    field.DeclaringType?.Name);

            if (isFx32)
                return FieldType.Fx32;
            if (isFx16)
                return FieldType.Fx16;
            if (hasFieldType)
                return field.GetCustomAttribute<TypeAttribute>().Type;
            if (field.FieldType.IsArray)
            {
                var elemType = field.FieldType.GetElementType();
                if (elemType.IsEnum)
                    return TypeToFieldType(elemType.GetEnumUnderlyingType());
                return TypeToFieldType(elemType);
            }

            if (field.FieldType.IsEnum)
                return TypeToFieldType(field.FieldType.GetEnumUnderlyingType());

            return TypeToFieldType(field.FieldType);
        }

        public static FieldType GetVectorPrimitiveType(FieldInfo field)
        {
            bool isFx32       = field.GetCustomAttribute<Fx32Attribute>() != null;
            bool isFx16       = field.GetCustomAttribute<Fx16Attribute>() != null;
            bool hasFieldType = field.GetCustomAttribute<TypeAttribute>() != null;
            int  count        = (isFx32 ? 1 : 0) + (isFx16 ? 1 : 0) + (hasFieldType ? 1 : 0);
            if (count > 1)
                throw new Exception("More than one field type specified for field " + field.Name + " in type " +
                                    field.DeclaringType?.Name);

            if (isFx32)
                return FieldType.Fx32;
            if (isFx16)
                return FieldType.Fx16;
            if (hasFieldType)
                return field.GetCustomAttribute<TypeAttribute>().Type;
            return FieldType.Float;
        }

        public static IEnumerable<FieldInfo> GetFieldsInOrder<T>()
            => GetFieldsInOrder(typeof(T));

        public static IEnumerable<FieldInfo> GetFieldsInOrder(Type type)
        {
            //Sorting by MetadataToken works, but may not be future proof
            return type.GetFields()
                .Where(f => !f.IsStatic && !f.IsLiteral && f.GetCustomAttribute<IgnoreAttribute>() == null)
                .OrderBy(f => f.MetadataToken);
        }

        public static FieldAlignment GetFieldAlignment<T>()
            => GetFieldAlignment(typeof(T));

        public static FieldAlignment GetFieldAlignment(Type type)
            => type.GetCustomAttribute<FieldAlignmentAttribute>()?.Alignment ?? FieldAlignment.Packed;

        private static readonly ConcurrentDictionary<(Type, Type), Delegate> CastCache = new();

        public static T Cast<T>(object data)
            => (T)Cast(data, typeof(T));

        public static object Cast(object data, Type type)
        {
            var inType = data.GetType();

            if (inType == type)
                return data;

            if (CastCache.TryGetValue((inType, type), out var func))
                return func.DynamicInvoke(data);

            var dataParam = Expression.Parameter(data.GetType());
            var run       = Expression.Lambda(Expression.Convert(dataParam, type), dataParam).Compile();

            CastCache.TryAdd((inType, type), run);

            return run.DynamicInvoke(data);
        }
    }
}