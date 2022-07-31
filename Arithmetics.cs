#define numberIsSigned

using System.Numerics;
using System.Runtime.CompilerServices;
using EnumerableExtensions;

using number = System.Int64;
//using number = System.Numerics.BigInteger;

namespace Maths;

public static class Number {
  public static IEnumerable<number> Range(number start, number count) {
    for (number n = start; count > 0; n += 1, count--) yield return n;
  }
  public static IEnumerable<T> Interval<T>(T from, T to) where T : IIncrementOperators<T>, IComparisonOperators<T, T> {
    for (T i = from; i <= to; i++) yield return i;
  }
  public static number Sum(this IEnumerable<number> list) => list.Aggregate((number)0, (a, b) => a + b);
  public static T Sum<T>(this IEnumerable<T> l) where T : IAdditionOperators<T, T, T> => l.Aggregate((x, y) => x + y);
  public static T2 Sum<T1, T2>(this IEnumerable<T1> l, Func<T1,T2> f) where T2 : IAdditionOperators<T2, T2, T2> => l.Select(f).Sum();
  public static T2 Sum<T1, T2>(T1 from, T1 to, Func<T1, T2> f) where T1 : IIncrementOperators<T1>, IComparisonOperators<T1, T1> where T2 : IAdditionOperators<T2, T2, T2>
    => Interval(from, to).Sum(f);

  public static number Min(params number[] list) => list.Min();
  public static number Max(params number[] list) => list.Max();
  public static T Product<T>(this IEnumerable<T> list) where T : IMultiplyOperators<T, T, T> => list.Aggregate((a, b) => a * b);
#if numberIsSigned
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static number Abs(this number a) => a >= 0 ? a : 0 - a;
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int Sign(this number a) => a > 0 ? +1 : a < 0 ? -1 : 0;
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static number Negate(this number a) => -a;
#else
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static number Abs(this number a) => a;
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int Sign(this number a) => a > 0u ? +1 : 0;
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static number Negate(this number a) => a == 0u ? 0u : throw new OverflowException();
#endif

  public static number GCD(IEnumerable<number> numbers) => numbers.Aggregate(GCD);
  public static number GCD(number a, number b) { while (b != 0) (a, b) = (b, a % b); return a; }

  public static number Pow(this number a, number k) {
    number p = (number)1;
    for (; k > 0; k >>= 1, a *= a) if ((k & 1) != 0) p *= a;
    return p;
  }
  public static number PowMod(this number a, number k, number n) {
    number p = (number)1; a %= n;
    for (; k > 0; k >>= 1, a = (a * a) % n) if ((k & 1) != 0) p = (p * a) % n;
    return p;
  }
  public static bool IsSquare(this number n) => Math.Sqrt((double)n) % 1 == 0;
  public static number SqrDist(number x, number y) => x * x + y * y;
  public static number ISqrt(number x) {
    number q = (number)1, r = (number)0, t; 
    while (q <= x) q <<= 2; while (q > 1) { q >>= 2; t = x - r - q; r >>= 1; if (t >= 0) { x = t; r += q; } }
    return r;
  }
  public static number Factorial(this number n) { number f = 1; for (; n > 1; n--) f *= n; return f; }
}

public class Primes {
  readonly List<number> primes = new(new[] { (number)2, (number)3 });
  public IEnumerable<number> All {
    get {
      //foreach (var p in primes) yield return p;  // Can cause "Collection was modified"
      for (int k = 0; k < primes.Count; k++) yield return primes[k];
      for (number p = primes[^1] + 2; ; p += 2)
        if (IsPrime(p)) { primes.Add(p); yield return p; }
    }
  }
  public IEnumerable<number> UpTo(number n) => All.TakeWhile(p => p <= n);
  public bool IsPrime(number n) => All.TakeWhile(p => p * p <= n).All(p => n % p != 0);
  public number Phi(number n) {
    number phi = 1;
    foreach (var p in All) {
      if (n == 1) break;
      while (n % p == 0) { n /= p; phi *= n % p == 0 ? p : p - 1; }
    }
    return phi;
  }
  public IEnumerable<(number p, uint k)> PrimeFactors(number n) {
    foreach (number p in All) {
      uint k = 0;
      while (n % p == 0) { n /= p; k++; }
      if (k > 0) yield return (p, k);
      if (n == 1) yield break;
      if (k == 0 && p * p >= n) { yield return (n, 1); yield break; }
    }
  }
  public int Mobius(number n) { int mu = 1; foreach (var (p, k) in PrimeFactors(n)) if (k > 1) return 0; else mu = -mu; return mu; }
  public IEnumerable<number> Divisors(IEnumerable<(number, uint)> factors) {
    if (factors.IsEmpty()) { yield return 1; yield break; }
    ((number p, uint k), IEnumerable<(number, uint)> other) = factors;
    for (uint i = 0; i <= k; i++) foreach (number d in Divisors(other)) yield return p.Pow((number)i) * d;
  }
  public IEnumerable<number> Divisors(number n) => Divisors(PrimeFactors(n));
  public number OrderMod(number n, number p) => Divisors(Phi(p)).FirstOrDefault(k => n.PowMod(k, p) == 1);
}

public interface ISigned { int Sign(); }
public interface IInversible<T> { bool IsInversible(); T Inverse(); }

public readonly struct Fraction : 
  IEquatable<Fraction>, IComparable<Fraction>, IComparisonOperators<Fraction, Fraction>,
  IAdditiveIdentity<Fraction, Fraction>, IAdditionOperators<Fraction, Fraction, Fraction>, IUnaryPlusOperators<Fraction, Fraction>,
  IUnaryNegationOperators<Fraction, Fraction>, ISubtractionOperators<Fraction, Fraction, Fraction>, ISigned,
  IMultiplicativeIdentity<Fraction, Fraction>, IMultiplyOperators<Fraction, Fraction, Fraction>,
  IInversible<Fraction>, IDivisionOperators<Fraction, Fraction, Fraction>
{
  private readonly number n, d;

  public Fraction() => (n, d) = (0, 1);
  public Fraction(number n, number d) {
    if (d == 0) throw new ArgumentOutOfRangeException(nameof(d));
    number gcd = Number.GCD(n.Abs(), d.Abs()) * d.Sign();
    this.n = n / gcd; this.d = d / gcd;
  }
  public static implicit operator Fraction(number n) => new(n, 1);
  public static implicit operator Fraction((number n, number d) x) => new(x.n, x.d);

  public static readonly Fraction Zero = (number)0;
  public static readonly Fraction One = (number)1;
  public static Fraction AdditiveIdentity => Zero;
  public static Fraction MultiplicativeIdentity => One;
  public static Fraction operator +(Fraction a) => a;
  public static Fraction operator -(Fraction a) => new(a.n.Negate(), a.d);
  public static Fraction operator -(Fraction a, Fraction b) => a + (-b);
  public static bool operator ==(Fraction a, Fraction b) => (a.n, a.d) == (b.n, b.d);
  public static bool operator !=(Fraction a, Fraction b) => !(a == b);
  public static Fraction operator /(Fraction a, Fraction b) => a * b.Inverse();

  public static bool operator >(Fraction a, Fraction b) => (a - b).Sign() > 0;
  public static bool operator <(Fraction a, Fraction b) => (a - b).Sign() < 0;
  public static bool operator >=(Fraction a, Fraction b) => (a - b).Sign() >= 0;
  public static bool operator <=(Fraction a, Fraction b) => (a - b).Sign() <= 0;
  public bool Equals(Fraction other) => this == other;
  public override bool Equals(object? obj) => obj is Fraction f && Equals(f);
  public int CompareTo(Fraction other) => (this - other).Sign();
  public int CompareTo(object? obj) => obj is null ? +1 : CompareTo((Fraction)obj);

  public static Fraction operator +(Fraction a, Fraction b) {
    number gcd = Number.GCD(a.d, b.d), ad = a.d / gcd, bd = b.d / gcd;
    return new(a.n * bd + b.n * ad, a.d * bd);
  }
  public static Fraction operator *(Fraction a, Fraction b) => new(a.n * b.n, a.d * b.d);

  public bool IsInversible() => n != 0;
  public Fraction Inverse() => IsInversible() ? new(d, n) : throw new DivideByZeroException();
  public number Floor() => n / d;
  public override string ToString() => d == 1 ? n.ToString() : $"{n}/{d}";  
  public int Sign() => n.Sign();
  public override int GetHashCode() => (n, d).GetHashCode();
}
