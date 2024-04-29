using System;
using System.Runtime.CompilerServices;

namespace HaroohiePals.IO
{
    public readonly struct Pointer<T> : IEquatable<Pointer<T>>
    {
        public readonly T[] Array;
        public readonly int Index;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Pointer(T[] array, int index = 0)
        {
            Array = array;
            Index = index;
        }

        public ref T this[int offset]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref Array[Index + offset];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Pointer<T> operator ++(Pointer<T> ptr)
            => new(ptr.Array, ptr.Index + 1);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Pointer<T> operator --(Pointer<T> ptr)
            => new(ptr.Array, ptr.Index - 1);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Pointer<T> operator +(Pointer<T> ptr, int offset)
            => new(ptr.Array, ptr.Index + offset);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Pointer<T> operator -(Pointer<T> ptr, int offset)
            => new(ptr.Array, ptr.Index - offset);

        public bool Equals(Pointer<T> other)
            => Array == other.Array && Index == other.Index;

        public override bool Equals(object obj)
            => obj == null && Array == null || obj is Pointer<T> other && Equals(other);

        public override int GetHashCode()
            => HashCode.Combine(Array, Index);

        public static bool operator ==(Pointer<T> left, Pointer<T> right)
            => left.Equals(right);

        public static bool operator !=(Pointer<T> left, Pointer<T> right)
            => !left.Equals(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Pointer<T>(T[] array) => new(array);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Span<T>(Pointer<T> ptr) => ptr.Array.AsSpan(ptr.Index);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ReadOnlySpan<T>(Pointer<T> ptr) => ptr.Array.AsSpan(ptr.Index);
    }
}