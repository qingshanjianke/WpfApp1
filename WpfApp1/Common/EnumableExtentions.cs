using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1.Common
{

    public static class EnumableExtentions
    {
        public static void Remove<T>(this IList<T> obj, Predicate<T> predicate)
        {
            var removeItems = new List<T>();
            foreach (var item in obj)
            {
                if (predicate(item))
                {
                    removeItems.Add(item);
                }
            }
            foreach (var item in removeItems)
            {
                obj.Remove(item);
            }
        }

        public static void ForEach<T>(this IEnumerable<T> obj, Action<T> action)
        {
            foreach (var item in obj)
            {
                action(item);
            }
        }

        public static string Join<T>(this IEnumerable<T> obj, string split)
        {
            return string.Join(split, obj);
        }

        public static DataTable ToDataTable<T>(this IEnumerable<T> list)
        {
            var dt = new DataTable(typeof(T).Name);
            foreach (var item in typeof(T).GetProperties()) dt.Columns.Add(item.Name);
            var value = dt.NewRow();
            foreach (var item in list)
            {
                value = dt.NewRow();
                foreach (DataColumn dtColumn in dt.Columns) value[dt.Columns.IndexOf(dtColumn)] = typeof(T).GetProperty(dtColumn.ColumnName).GetValue(item);
                dt.Rows.Add(value);
            }

            return dt;
        }

        public static IEnumerable<List<T>> SplitToSmallListByPageSize<T>(this List<T> list, int size = 10)
        {
            for (var i = 0; i < list.Count; i += size) yield return list.GetRange(i, Math.Min(size, list.Count - i));
        }

        public static IEnumerable<List<T>> SplitToSmallListByPage<T>(this List<T> list, int page = 10)
        {
            var size = (int)Math.Ceiling(list.Count / (double)page);
            for (var i = 0; i < list.Count; i += size) yield return list.GetRange(i, Math.Min(size, list.Count - i));
        }

        public static ConcurrentDictionary<TKey, TElement> ToConcurrentDictionary<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector)
        {
            var dict = new ConcurrentDictionary<TKey, TElement>();
            foreach (var item in source) dict.TryAdd(keySelector(item), elementSelector(item));
            return dict;
        }

        public static IEnumerable<TSource> Distinct<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            HashSet<TKey> keys = new HashSet<TKey>();
            foreach (TSource element in source)
                if (keys.Add(keySelector(element)))
                    yield return element;
        }

        public static IEnumerable<TSource> Except<TSource, TKey>(this IEnumerable<TSource> first, IEnumerable<TSource> second, Func<TSource, TKey> keySelector)
        {
            HashSet<TKey> keys = second.Select(keySelector).ToHashSet();
            foreach (TSource element in first)
                if (keys.Add(keySelector(element)))
                    yield return element;
        }
    }
}
