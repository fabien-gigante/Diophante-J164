namespace EnumerableExtensions {
  public static class EnumerableExt {
    static List<T> GetRange<T>(this List<T> l, (int o, int l) r) => l.GetRange(r.o, r.l);
    public static List<T> GetRange<T>(this List<T> l, Range r) => l.GetRange(r.GetOffsetAndLength(l.Count));
    public static List<T> GetRanges<T>(this List<T> l, params Range[] r) => r.SelectMany(r => GetRange(l, r)).ToList();
    public static void RemoveLast<T>(this IList<T> l) => l.RemoveAt(l.Count - 1);
    public static bool IsEmpty<T>(this IEnumerable<T> l) => !l.Any();
    public static void Deconstruct<T>(this IEnumerable<T> l, out T first, out IEnumerable<T> rest) => (first, rest) = (l.First(), l.Skip(1));
    public static TValue? GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dic, TKey key, TValue? def) where TValue : struct
      => dic.TryGetValue(key, out TValue value) ? value : def;
    public static void Print<T>(this T obj) => Console.WriteLine(obj);
    public static int IndexOf<T>(this IEnumerable<T> l, Func<T,bool> f) { int i = 0; foreach (var x in l) { if (f(x)) return i; i++; } return -1; }
  }
}
