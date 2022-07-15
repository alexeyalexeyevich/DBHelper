using System;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace DBHelper;

public static class SqlBuilderCache
{
    private static readonly ConcurrentDictionary<PropertyInfo, string> columnNames = new();
    public static string GetColumnName(this PropertyInfo prop)
    {
        if (!columnNames.ContainsKey(prop))
        {
            var c = prop.GetCustomAttribute<ColumnAttribute>();
            var columnName = c != null && !string.IsNullOrWhiteSpace(c.Name) ? c.Name : prop.Name;
            columnNames.TryAdd(prop, columnName);
        }
        return columnNames[prop];
    }

    private static readonly ConcurrentDictionary<Type, (string name, string schema)> tablesNames = new();
    public static (string name, string schema) GetTableName(this Type type)
    {
        if (!tablesNames.ContainsKey(type))
        {
            string name, schema;
            var tableAttribute = type.GetCustomAttribute<TableAttribute>();
            if (tableAttribute != null)
            {
                name = tableAttribute.Name;
                schema = tableAttribute.Schema;
            }
            else
            {
                name = type.Name;
                schema = String.Empty;
            }
            tablesNames.TryAdd(type, (name, schema));
        }

        return tablesNames[type];
    }  
}