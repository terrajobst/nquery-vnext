#nullable enable

using System;
using System.Collections.Generic;

namespace NQuery
{
    internal static class ValueTuple
    {
        public static ValueTuple<T1, T2> Create<T1, T2>(T1 item1, T2 item2)
        {
            return new ValueTuple<T1, T2>(item1, item2);
        }
    }

    internal struct ValueTuple<T1, T2> : IEquatable<ValueTuple<T1, T2>>
    {
        public ValueTuple(T1 item1, T2 item2)
        {
            Item1 = item1;
            Item2 = item2;
        }

        public T1 Item1 { get; set; }
        public T2 Item2 { get; set; }

        public bool Equals(ValueTuple<T1, T2> other)
        {
            return EqualityComparer<T1>.Default.Equals(Item1, other.Item1) &&
                   EqualityComparer<T2>.Default.Equals(Item2, other.Item2);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;

            return obj is ValueTuple<T1, T2> && Equals((ValueTuple<T1, T2>) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (EqualityComparer<T1>.Default.GetHashCode(Item1)*397) ^ EqualityComparer<T2>.Default.GetHashCode(Item2);
            }
        }

        public static bool operator ==(ValueTuple<T1, T2> left, ValueTuple<T1, T2> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ValueTuple<T1, T2> left, ValueTuple<T1, T2> right)
        {
            return !left.Equals(right);
        }
    }
}