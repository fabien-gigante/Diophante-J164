using System.Collections;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;

//using number = System.Numerics.BigInteger;
using number = System.Int64;

namespace Maths;

using Permutation = ImmutableArray<number>;
using bits = System.UInt64;

public static class BinaryExt {
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int BitsSetCount(this ulong i) => (int)UInt64.PopCount(i);
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int BitsSetCount(this UInt128 i) => (int)UInt128.PopCount(i);
  public static IEnumerable<uint> BitsSet(this ulong c) {
    for (uint v = 0; c != 0; v++, c >>= 1) if ((c & 1) != 0) yield return v;
  }
  public static IEnumerable<uint> BitsSet(this UInt128 c) {
    for (uint v = 0; c != 0; v++, c >>= 1) if ((c & 1) != 0) yield return v;
  }
}

public readonly struct Range : IReadOnlyList<number> {
  public readonly number Start;
  public readonly number End;
  public Range(number start, number end) { Start = start; End = end; }
  number Count => End - Start + 1;
  int IReadOnlyCollection<number>.Count => (int)Count;
  public number this[int index] => Start + (number)index;
  public IEnumerator<number> GetEnumerator() { for (var i = Start; i <= End; i++) yield return i; }
  IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

public readonly struct FastSet : IImmutableSet<number> {
  private readonly bits code;
  public static readonly FastSet Empty = new();
  public static readonly int MaxSize = bits.MaxValue.BitsSetCount();

  private FastSet(bits code) => this.code = code;
  public bits Code => code;
  public bool IsEmpty() => code == 0;
  public FastSet(FastSet other) : this(other.code) { }
  public FastSet(IEnumerable<number> other) : this(other.Aggregate(Empty, (aggr, value) => (FastSet)aggr.Add(value))) { }
  public static IImmutableSet<number> Create(number size) => size <= MaxSize ? new FastSet() : ImmutableSortedSet.Create<number>();
  public static IImmutableSet<number> Create(number start, number count) {
    var range = new Range(start, count); 
    return range.End > FastSet.MaxSize ? range.ToImmutableSortedSet() : new FastSet(range);
  }
  public int Count => (int)code.BitsSetCount();
  public IImmutableSet<number> Add(number value) => new FastSet(code | (bits)1ul << (int)value);
  public IImmutableSet<number> Remove(number value) => new FastSet(code & ~((bits)1ul << (int)value));
  public IImmutableSet<number> Clear() => Empty;
  public bool Contains(number value) => (code & ((bits)1ul << (int)value)) != 0;
  public bool TryGetValue(number equalValue, out number actualValue) { actualValue = equalValue; return Contains(equalValue); }

  public IEnumerator<number> GetEnumerator() => code.BitsSet().Cast<number>().GetEnumerator();
  IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

  public bool SetEquals(FastSet other) => code == other.code;
  public bool SetEquals(IEnumerable<number> other) => SetEquals(other as FastSet? ?? new FastSet(other));
  public IImmutableSet<number> Union(FastSet other) => new FastSet(code | other.code);
  public IImmutableSet<number> Union(IEnumerable<number> other) => Union(other as FastSet? ?? new FastSet(other));
  public IImmutableSet<number> Intersect(FastSet other) => new FastSet(code & other.code);
  public IImmutableSet<number> Intersect(IEnumerable<number> other) => Intersect(other as FastSet? ?? new FastSet(other));
  public IImmutableSet<number> Except(FastSet other) => new FastSet(code & ~other.code); 
  public IImmutableSet<number> Except(IEnumerable<number> other) => Except(other as FastSet? ?? new FastSet(other));
  public IImmutableSet<number> SymmetricExcept(FastSet other) => new FastSet(code ^ other.code); 
  public IImmutableSet<number> SymmetricExcept(IEnumerable<number> other) => SymmetricExcept(other as FastSet? ?? new FastSet(other));
  public bool IsSubsetOf(FastSet other) => (code | other.code) == other.code; 
  public bool IsSubsetOf(IEnumerable<number> other) => IsSubsetOf(other as FastSet? ?? new FastSet(other));
  public bool IsSupersetOf(FastSet other) => (code | other.code) == code; 
  public bool IsSupersetOf(IEnumerable<number> other) => IsSupersetOf(other as FastSet? ?? new FastSet(other));
  public bool IsProperSubsetOf(FastSet other) => IsSubsetOf(other) && code != other.code; 
  public bool IsProperSubsetOf(IEnumerable<number> other) => IsProperSubsetOf(other as FastSet? ?? new FastSet(other));
  public bool IsProperSupersetOf(FastSet other) => IsProperSupersetOf(other) && code != other.code;
  public bool IsProperSupersetOf(IEnumerable<number> other) => IsProperSupersetOf(other as FastSet? ?? new FastSet(other));
  public bool Overlaps(FastSet other) => (code & other.code) != 0;
  public bool Overlaps(IEnumerable<number> other) => Overlaps(other as FastSet? ?? new FastSet(other));
}

public static class Combinatronis {
  static IEnumerable<IImmutableList<number>> Choose(number p, number n, number m)
    => p == 0 ? new[] { ImmutableList<number>.Empty } : from a in new Range(m, n - p) from s in Choose(p - 1, n, a + 1) select s.Insert(0, a);
  public static IEnumerable<IImmutableList<number>> Choose(number p, number n) => Choose(p, n, 0);
  public static IEnumerable<IImmutableList<T>> Choose<T>(number p, IReadOnlyList<T> c)
    => from s in Choose(p, (number)c.Count) select (from a in s select c[(int)a]).ToImmutableList();
  public static IEnumerable<(T x, T y)> Pairs<T>(IReadOnlyList<T> c) => from s in Choose(2, c) select (s[0], s[1]);
}

public static class Permutations {
  public static IEnumerable<ImmutableArray<T>> Enumerate<T>(IImmutableSet<T> set) {
    if (set.Count == 0) { yield return ImmutableArray<T>.Empty; yield break; }
    foreach (T a in set)
      foreach (var p in Enumerate(set.Remove(a)))
        yield return p.Insert(0, a);
  }
  private static IImmutableSet<number> CreateSet(number start, number count) {
    var range = new Range(start, count); number last = start + count - 1;
    return last > FastSet.MaxSize ? range.ToImmutableSortedSet() : new FastSet(range);
  }
  public static Permutation Identity(number n) => new Range(1, n).ToImmutableArray();
  public static Permutation Circular(number n) => new Range(1, n).Select(x => x % n + 1).ToImmutableArray();
  public static Permutation Reverse(number n) => new Range(1, n).Select(x => n + 1 - x).ToImmutableArray();
  public static IEnumerable<ImmutableArray<T>> Enumerate<T>(IEnumerable<T> set) => Enumerate(set.ToImmutableSortedSet());
  public static IEnumerable<Permutation> Enumerate(number start, number count) => Enumerate(CreateSet(start, count));
  public static IEnumerable<Permutation> Enumerate(number n) => Enumerate(1, n);
  public static Permutation Compose(this Permutation a, Permutation b) {
    var c = ImmutableArray.CreateBuilder<number>(a.Length);
    foreach (var x in b) c.Add(a[(int)(x - 1)]);
    return c.MoveToImmutable();
  }
  public static Permutation Power(this Permutation a, int k) => k > 0 ? a.Compose(Power(a, k - 1)) : k < 0 ? Power(a, -k).Inverse() : Identity((number)a.Length);
  public static Permutation Inverse(this Permutation a) => new Range(1, (number)a.Length).Select(x => (number)(a.IndexOf(x) + 1)).ToImmutableArray();
}

