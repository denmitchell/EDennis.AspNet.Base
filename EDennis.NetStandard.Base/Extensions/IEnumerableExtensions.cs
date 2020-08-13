using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using static Dapper.SqlMapper;

namespace EDennis.NetStandard.Base {
    public static class IEnumerableExtensions {

		/// <summary>
		/// Packs an IEnumerable<KeyValuePair<string,string>> into a simple string
		/// where 
		/// <list type="bullet">
		/// <item>key value pairs are separated from each other by ;</item>
		/// <item>keys are separated from values by :</item>
		/// <item>value items are separated from each other by ,</item>
		/// </list>
		/// NOTE that by having value items (plural), keys are never repeated,
		/// which keeps the entire string as small as possible.
		/// </summary>
		/// <typeparam name="T">The underlying type having one property as a key and the other as a value</typeparam>
		/// <param name="kvs">The collection to pack into a string</param>
		/// <param name="tupleFunc">A function that resolves a single object into a key-value tuple</param>
		/// <code>
		/// var claimString = claims.PackKeyValues((Claim c) => (c.Type,c.Value));
		/// </code>
		/// <returns>compact string representation of key value pairs</returns>
		/// <see cref="UnpackKeyValues{T}(string, Func{string, string, T})"/>
		public static string PackKeyValues<T>(this IEnumerable<T> kvs, Func<T, ValueTuple<string, string>> tupleFunc) {
			Dictionary<string, List<string>> dict = new Dictionary<string, List<string>>();
			foreach (var kv in kvs) {
				var (k, v) = tupleFunc(kv);
				if (!dict.ContainsKey(k))
					dict.Add(k, new List<string>());
				dict[k].Add(v);
			}
			var list = new List<string>();
			foreach (var key in dict.Keys)
				list.Add($"{key}:{string.Join(",", dict[key])}");
			return string.Join(";", list);
		}


		public static string Pack(this Dictionary<string,string> dict, 
			(char KeyValue, char ValueKey) separators) {
			var list = new List<string>();
			foreach (var key in dict.Keys)
				list.Add($"{key}{separators.KeyValue}{dict[key]}");
			return string.Join(separators.ValueKey,list);
		}

		public static Dictionary<string, string> UnPackDictionary(this string str,
			(char KeyValue, char ValueKey) separators) {
			var list = str.Split(separators.ValueKey);
			var dict = new Dictionary<string, string>();
			foreach(var item in list) {
				var kv = item.Split(separators.KeyValue);
				if (!dict.ContainsKey(kv[0]))
					dict.Add(kv[0], kv[1]);
			}
			return dict;
		}

		/// <summary>
		/// Parses a packed string representation of key value pairs into an
		/// IEnumerable of those key value pairs
		/// </summary>
		/// <typeparam name="T">The underlying type having one property as a key and the other as a value</typeparam>
		/// <param name="str">The source string</param>
		/// <param name="ctor">A "constructor" function that creates a new T out of a key and value</param>
		/// <code>
		/// var claims = claimString.UnpackKeyValues((t,v) => new Claim(t,v));
		/// </code>
		/// <returns></returns>
		/// <see cref="PackKeyValues{T}(IEnumerable{T}, Func{T, (string, string)})"/>
		public static IEnumerable<T> UnpackKeyValues<T>(this string str, Func<string, string, T> ctor) {
			var list = new List<T>();
			var curKey = new StringBuilder();
			var curVal = new StringBuilder();
			var curSb = curKey;
			for (int i = 0; i < str.Length; i++) {
				switch (str[i]) {
					case ':':
						curSb = curVal;
						break;
					case ',':
						list.Add(ctor(curKey.ToString(), curVal.ToString()));
						curVal.Clear();
						break;
					case ';':
						list.Add(ctor(curKey.ToString(), curVal.ToString()));
						curVal.Clear();
						curKey.Clear();
						curSb = curKey;
						break;
					default:
						curSb.Append(str[i]);
						break;
				}
			}
			if (curVal.Length > 0)
				list.Add(ctor(curKey.ToString(), curVal.ToString()));

			return list;
		}


		/// <summary>
		/// Packs an IEnumerable<KeyValuePair<string,string>> into a Dictionary
		/// where 
		/// <list type="bullet">
		/// <item>the key is the first tuple element</item>
		/// <item>the value is a list of the second tuple element</item>
		/// </list>
		/// </summary>
		/// <typeparam name="T">The underlying type having one property as a key and the other as a value</typeparam>
		/// <param name="kvs">The collection to pack into a string</param>
		/// <param name="tupleFunc">A function that resolves a single object into a key-value tuple</param>
		/// <code>
		/// var claimString = claims.ToDictionary((Claim c) => (c.Type,c.Value));
		/// </code>
		/// <returns>Dictionary representation of key value pairs</returns>
		public static Dictionary<string, List<string>> ToDictionary<T>(this IEnumerable<T> kvs, Func<T, ValueTuple<string, string>> tupleFunc) {
			Dictionary<string, List<string>> dict = new Dictionary<string, List<string>>();
			foreach (var kv in kvs) {
				var (k, v) = tupleFunc(kv);
				if (!dict.ContainsKey(k))
					dict.Add(k, new List<string>());
				dict[k].Add(v);
			}
			return dict;
		}



		public static ICustomQueryParameter ToStringTableTypeParameter(this IEnumerable<string> values) {

            var table = new DataTable();
            table.Columns.Add("Value", typeof(string));

            foreach (var value in values) {
                var row = table.NewRow();
                row["Value"] = value;
            }

            return table.AsTableValuedParameter(typeName: "dbo.StringTableType");
        }
    }
}
