using Maths;
using EnumerableExtensions;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using number = System.Double;
using Point = Maths.Vector2D;

new J164().Run();

readonly struct Segment : IEquatable<Segment> {
  public readonly Point a, b;
  public Segment(Point a, Point b) { this.a = a; this.b = b; }
  public static implicit operator Segment((Point a, Point b) s) => new(s.a, s.b);
  public static implicit operator (Point a, Point b)(Segment s) => (s.a, s.b);
  public Point Interpolate(number t) => a * (1 - t) + b * t;
  public static bool IntersectLineSegment(Segment /*Line*/ s1, Segment s2) {
    const number eplison = 1e-12;
    number deno = Point.Determinant(s1.b - s1.a, s2.b - s2.a);
    number t2 = Point.Determinant(s2.a - s1.a, s1.b - s1.a) / deno;
    return eplison < t2 && t2 < 1 - eplison;
  }
  public static bool IntersectSegmentSegment(Segment s1, Segment s2) {
    const number eplison = 1e-12;
    number deno = Point.Determinant(s1.b - s1.a, s2.b - s2.a);
    number t1 = Point.Determinant(s2.a - s1.a, s2.b - s2.a) / deno;
    number t2 = Point.Determinant(s2.a - s1.a, s1.b - s1.a) / deno;
    return eplison < t1 && t1 < 1 - eplison && eplison < t2 && t2 < 1 - eplison;
  }
  public override int GetHashCode() => (a, b).GetHashCode();
  public bool Equals(Segment other) => (a,b).Equals(other);
  public override bool Equals(object? obj) => obj is Segment other && Equals(other);
}

readonly struct Square : IComparable<Square>, IEquatable<Square> {
  readonly Point origin;
  public readonly int Id { get; }
  public Square(number x, number y, int id) { origin = (x, y); Id = id; }
  public IEnumerable<Segment> Edges() {
    Point a = origin, b = origin + (1, 0), c = origin + (1, 1), d = origin + (0, 1);
    return new Segment[] { (a, b), (b, c), (c, d), (d, a) };
  }
  public bool IntersectLine(Segment /*Line*/ line) => Edges().Any(s => Segment.IntersectLineSegment(line, s));
  public override int GetHashCode() => Id;
  public int CompareTo(Square other) => Id.CompareTo(other.Id);
  public bool Equals(Square other) => Id == other.Id;
  public override bool Equals(object? obj) => obj is Square other && Equals(other);
}

class Grid {
  readonly int size;
  readonly List<Square> squares;
  List<List<ulong>>? stripes;
  public Grid(int size) {
    Console.WriteLine($"Grid : {size}x{size}");
    this.size = size; squares = new(size);
    for (int id = 0, y = 0; y < size; y++) for (int x = 0; x < size; x++, id++) squares.Add(new Square(x, y, id));
  }
  IEnumerable<Segment> Secants() {
    const number epsilon = 1e-8;
    Point[] delta = new Point[] { (0,0), (0, -epsilon), (0, +epsilon), (-epsilon, 0), (+epsilon, 0) };
    for (int i = 0; i < (size + 1) * (size + 1) - 1; i++)
      for (int j = i + 1; j < (size + 1) * (size + 1); j++) {
        Point a = (i % (size + 1), i / (size + 1)), b = (j % (size + 1), j / (size + 1));
        foreach (var d1 in delta) foreach (var d2 in delta) yield return (a + d1, b + d2);
      }
  }
  IEnumerable<long> Intersect(Segment line) => squares.Where(sq => sq.IntersectLine(line)).Select(sq => (long)sq.Id);
  ulong Stripe(Segment line) => new FastSet(Intersect(line)).Code;
  IEnumerable<ulong> AllStripes() => Secants().AsParallel().Select(Stripe).Distinct();
  string CoveringToString(IImmutableList<ulong> l, char sep = '\n') {
    string str = "" + sep;
    for (int i = 0, y = 0; y < size; y++) {
      for (int x = 0; x < size; x++, i++) {
        int n = l.IndexOf(s => (s >> i & 1) == 1);
        str += (n == -1) ? "· " : $"\u001b[{31+n}m■ \u001b[0m";
      }
      str += sep;
    }
    return str;
  }
  [MemberNotNull(nameof(stripes))]
  void ComputeStripes() {
    if (stripes != null) return;
    Console.WriteLine("- Compute stripes");
    if (size * size > FastSet.MaxSize) throw new OverflowException(nameof(size));
    List<ulong> all = AllStripes().ToList(); List<ulong> redondant = new();
    foreach (var a in all) foreach (var b in all) if (a != b && (a | b) == b) redondant.Add(a);
    all.RemoveAll(redondant.Contains);
    int m = all.Max(s => s.BitsSetCount()); stripes = new(m + 1);
    for (int c = 0; c <= m; c++) stripes.Add(all.Where(s => s.BitsSetCount() == c).ToList());
    Console.WriteLine($"    Stripes {all.Count} : {string.Join("", stripes.Select((s, i) => s.Count == 0 ? "" : $"{s.Count} ({i}) "))}");
  }
  IEnumerable<IImmutableList<ulong>> Choose(int n, int max, ulong left) {
    if (left == 0) { yield return ImmutableList<ulong>.Empty; yield break; }
    if (n == 0) yield break;
    int b = (int)ulong.PopCount(left);  int min = (b + (n - 1)) / n;
    for (int c = max; c >= min; c--) {
      foreach (var x in stripes![c].Select(s => (s, l: left & ~s)).Where(x => x.l != left))
        foreach (var l in Choose(n - 1, c, x.l)) yield return l.Add(x.s);
    }
  }
  public void SearchCovering(int n) {
    ComputeStripes();
    Console.WriteLine($"- Search covering with {n} stripes");
    ulong full = (ulong.MaxValue) >> (64 - size * size);
    foreach (var l in Choose(n, stripes.Count - 1, full)) {
      Console.WriteLine("    Found : " + CoveringToString(l));
      break;
    }
  }
  public void SearchCoverings() { SearchCovering(size - 2); SearchCovering(size - 1); }
}

class J164 {
  public void Run() {
    Console.OutputEncoding = System.Text.Encoding.UTF8;
    Stopwatch s = new(); s.Start();
    for (int n = 3; n <= 8; n++) new Grid(n).SearchCoverings();
    s.Stop(); Console.WriteLine($"{s.ElapsedMilliseconds}ms elapsed");
  }
}
