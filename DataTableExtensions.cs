﻿using AddAppAPI.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace AddAppAPI.Extensions
{
    public static class DataTableExtensions
    {
        public static DataTable Fill<T>(this DataTable dt, params T[] collection) where T : class, new()
        {
            Type type = typeof(T);
            dt.TableName = type.UnderlyingSystemType.Name;
            Dictionary<int, PropertyInfo> columnIndexAndProperties = null;
            dt.AddColumns(type, ref columnIndexAndProperties);

            if (collection == null)
            {
                return dt;
            }

            foreach (T obj in collection)
            {
                dt.AddRow(obj, columnIndexAndProperties);
            }

            return dt;
        }

        private static void AddColumns(this DataTable dt, Type type, ref Dictionary<int, PropertyInfo> columnIndexAndProperties)
        {
            var properties = type.GetProperties();
            columnIndexAndProperties = new Dictionary<int, PropertyInfo>(properties.Count());
            int index = 0;
            foreach (var p in properties)
            {
                DataColumn column = new DataColumn();
                column.ColumnName = p.Name;
                column.DataType = CommonFunctions.GetType(p.PropertyType);
                dt.Columns.Add(column);
                columnIndexAndProperties.Add(index++, p);
            }
        }

        private static void AddRow(this DataTable dt, object obj, Dictionary<int, PropertyInfo> columnIndexAndProperties)
        {
            DataRow row = dt.NewRow();
            foreach (var kv in columnIndexAndProperties)
            {
                var value = kv.Value.GetValue(obj, null);
                if (value == null)
                {
                    row[kv.Key] = DBNull.Value;
                }
                else
                {
                    row[kv.Key] = value;
                }
            }

            dt.Rows.Add(row);
        }
    }
}