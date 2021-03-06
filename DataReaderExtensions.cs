﻿using AddAppAPI.Helpers;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;

namespace AddAppAPI.Extensions
{
    public static class DataReaderExtensions
    {
        public static T ToObject<T>(this SqlDataReader reader) where T : class, new()
        {
            var matchedProperties = GetMatchedProperties<T>(reader);
            if (reader.Read())
            {
                T t = new T();
                foreach (var kv in matchedProperties)
                {
                    if (!reader.IsDBNull(kv.Key))
                    {
                        var tuple = kv.Value;
                        tuple.Item1.SetValue(t, Convert.ChangeType(reader.GetValue(kv.Key), tuple.Item2), null);
                    }
                }

                return t;
            }

            return null;
        }

        public static List<T> ToList<T>(this SqlDataReader reader) where T : class, new()
        {
            var matchedProperties = GetMatchedProperties<T>(reader);
            var list = new List<T>();
            while (reader.Read())
            {
                var t = new T();
                foreach (var kv in matchedProperties)
                {
                    if (!reader.IsDBNull(kv.Key))
                    {
                        var tuple = kv.Value;
                        tuple.Item1.SetValue(t, Convert.ChangeType(reader.GetValue(kv.Key), tuple.Item2), null);
                    }
                }
                list.Add(t);
            }

            return list;
        }

        public static List<T> ToValueTypeList<T>(this SqlDataReader reader) where T : struct
        {
            var list = new List<T>();
            while (reader.Read())
            {
                list.Add((T)reader.GetValue(0));
            }

            return list;
        }

        public static List<KeyValuePair<I, N>> ToKeyValuePairList<I, N>(this SqlDataReader reader)
        {
            var dictionary = new List<KeyValuePair<I, N>>();
            while (reader.Read())
            {
                dictionary.Add(new KeyValuePair<I, N>((I)reader.GetValue(0), (N)reader.GetValue(1)));
            }

            return dictionary;
        }

        public static Dictionary<I, N> ToDictionary<I, N>(this SqlDataReader reader)
        {
            var dictionary = new Dictionary<I, N>();
            while (reader.Read())
            {
                dictionary.Add((I)reader.GetValue(0), (N)reader.GetValue(1));
            }

            return dictionary;
        }

        private static Dictionary<int, Tuple<PropertyInfo, Type>> GetMatchedProperties<T>(SqlDataReader reader)
        {
            Type type = typeof(T);
            var properties = type.GetProperties();
            Dictionary<int, Tuple<PropertyInfo, Type>> matchedProperties = new Dictionary<int, Tuple<PropertyInfo, Type>>(properties.Count());
            for (int i = 0; i < reader.FieldCount; i++)
            {
                //if C# property name is EmpId, db column can be [emp id] or emp_id, or empid
                var dbColumnName = string.Join(string.Empty, reader.GetName(i).Split(" _".ToCharArray()));
                var property = properties.FirstOrDefault(m => m.Name.Equals(dbColumnName, StringComparison.OrdinalIgnoreCase));
                if (property != null)
                {
                    //CommonFunctions.GetType is used to get type from Nullable type. ex: Nullable<int>
                    matchedProperties.Add(i, Tuple.Create(property, CommonFunctions.GetType(property.PropertyType)));
                }
            }

            return matchedProperties;
        }
    }
}