using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetaParser
{
    public static class Extensions
    {
        //public static string ToString<T>(this Array collection, string separator = ",")
        //{
        //    if (collection == null)
        //        return "<null>";

        //    List<string> values = new List<string>();
        //    foreach (T item in collection)
        //        values.Add(item.ToString());
        //    return String.Join(separator, values.ToArray());
        //}

        //public static string ToString<T>(this T[] collection, string separator = ",")
        //{
        //    if (collection == null)
        //        return "<null>";

        //    List<string> values = new List<string>();
        //    foreach (T item in collection)
        //        values.Add(item.ToString());
        //    return String.Join(separator, values.ToArray());
        //}
        
        public static string ToString<T>(this IEnumerable<T> collection, string separator = ",")
        {
            if (collection == null)
                return "<null>";

            List<string> values = new List<string>();
            foreach (T item in collection)
                values.Add(item.ToString());
            return String.Join(separator, values.ToArray());
        }

        public static string RemoveNullChars(this string s)
        {
            return s.Replace("\0", "");
        }

        public static string UppercaseFirst(this string s)
        {
            if (string.IsNullOrEmpty(s))
                return string.Empty;

            char[] a = s.ToCharArray();
            a[0] = char.ToUpper(a[0]);
            return new string(a);
        }
    }
}
