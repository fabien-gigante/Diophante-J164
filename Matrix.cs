namespace Maths;

using System.Numerics;
using number = Double;

public readonly struct Vector2D :
  IEquatable<Vector2D>, IEqualityOperators<Vector2D, Vector2D>,
  IAdditiveIdentity<Vector2D, Vector2D>, IUnaryPlusOperators<Vector2D, Vector2D>, IUnaryNegationOperators<Vector2D, Vector2D>,
  IAdditionOperators<Vector2D, Vector2D, Vector2D>, ISubtractionOperators<Vector2D, Vector2D, Vector2D>, IMultiplyOperators<Vector2D, number, Vector2D>
{
  public readonly number x, y;

  public Vector2D(number x, number y) { this.x = x; this.y = y; }
  public static implicit operator Vector2D((number x, number y) p) => new(p.x, p.y);
  public void Deconstruct(out number x, out number y) { x = this.x; y = this.y; }

  // IEquatable, IEqualityOperators, ...
  public static bool operator ==(Vector2D a, Vector2D b) => (a.x, a.y) == (b.x, b.y);
  public static bool operator !=(Vector2D a, Vector2D b) => !(a == b);
  public bool Equals(Vector2D other) => this == other;
  public override bool Equals(object? obj) => obj is Vector2D m && Equals(m);
  public override int GetHashCode() => (x,y).GetHashCode();
  // IAdditiveIdentity, IAdditionOperators, IUnaryPlusOperators, IUnaryNegationOperators, ISubtractionOperators
  public static readonly Vector2D Zero = (0, 0);
  public static Vector2D AdditiveIdentity => Zero;
  public static Vector2D operator +(Vector2D a) => a;
  public static Vector2D operator +(Vector2D a, Vector2D b) => (a.x + b.x, a.y + b.y);
  public static Vector2D operator -(Vector2D a, Vector2D b) => (a.x - b.x, a.y - b.y);
  public static Vector2D operator -(Vector2D a) => (-a.x, -a.y);
  // IMultiplyOperators, ...
  public static Vector2D operator *(Vector2D a, number k) => (k * a.x, k * a.y);
  public static Vector2D operator *(number k, Vector2D a) => (k * a.x, k * a.y);
  public static number Determinant(Vector2D a, Vector2D b) => a.x * b.y - a.y * b.x;
  public static double Dot(Vector2D a, Vector2D b) => a.x * b.x + a.y * b.y;
  public double SqrMagnitude => Dot(this, this);
}

public readonly struct Matrix :
  IEquatable<Matrix>, IEqualityOperators<Matrix, Matrix>,
  IAdditionOperators<Matrix, Matrix, Matrix>, IUnaryPlusOperators<Matrix, Matrix>,
  IUnaryNegationOperators<Matrix, Matrix>, ISubtractionOperators<Matrix, Matrix, Matrix>,
  IMultiplyOperators<Matrix, Matrix, Matrix>, IMultiplyOperators<Matrix, number, Matrix>
{
  readonly number[,] c;

  public Matrix(int n, int m) => c = new number[n, m];
  public Matrix(int n, int m, Func<int, int, number> f) : this(n, m) {
    for (int i = 0; i < n; i++) for (int j = 0; j < m; j++) c[i, j] = f(i, j);
  }
  public Matrix((int n, int m) d, Func<int, int, number> f) : this(d.n, d.m, f) { }

  public (int n, int m) Dimensions => (c.GetLength(0), c.GetLength(1));
  public number this[int i, int j] { get => c[i, j]; set => c[i, j] = value; }
  public bool IsZero() => c.Cast<number>().All(x => x == (number)0);

  // IEquatable, IEqualityOperators, ...
  public static bool operator ==(Matrix a, Matrix b) => a.Dimensions == b.Dimensions && (a - b).IsZero();
  public static bool operator !=(Matrix a, Matrix b) => !(a == b);
  public bool Equals(Matrix other) => this == other;
  public override bool Equals(object? obj) => obj is Matrix m && Equals(m);
  public override int GetHashCode() => c.GetHashCode();
  // IAdditionOperators, IUnaryPlusOperators, IUnaryNegationOperators, ISubtractionOperators
  public static Matrix operator +(Matrix a, Matrix b) {
    var d = a.Dimensions; if (b.Dimensions != d) throw new ArgumentOutOfRangeException();
    return new Matrix(d, (i, j) => a[i, j] + b[i, j]);
  }
  public static Matrix operator +(Matrix a) => a;
  public static Matrix operator -(Matrix a) => unchecked((number)(-1)) * a;
  public static Matrix operator -(Matrix a, Matrix b) => a + -b;
  // IMultiplyOperators, ...
  public static Matrix operator *(Matrix a, Matrix b) {
    var (n, m) = a.Dimensions; var (m1, p) = b.Dimensions; if (m1 != m) throw new ArgumentOutOfRangeException();
    return new Matrix(n, m, (i, j) => Enumerable.Range(0, m).Sum(k => a[i, k] * b[k, j]));
  }
  public static Matrix operator *(number k, Matrix a) => new(a.Dimensions, (i, j) => k * a[i, j]);
  public static Matrix operator *(Matrix a, number k) => k * a;
}