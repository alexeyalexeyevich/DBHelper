using DBHelper.Descriptors;
using System;
using System.Collections.Generic;

namespace DBHelper.Dialects;

public class PostgresqlDialect : BaseDialect
{
    public override string DefaultSchema { get; } = "public";

    private readonly Dictionary<Type, string> dbAutoIncrementTypes = new()
    {
        {typeof(int), "SERIAL"},
        {typeof(long), "BIGSERIAL"},
    };

    private readonly Dictionary<Type, string> dbTypesDictionary = new()
    {
        {typeof(short), "smallint"},
        {typeof(short?), "smallint"},
        {typeof(int), "integer"},
        {typeof(int?), "integer"},
        {typeof(string), "varchar"},
        {typeof(char[]), "varchar"},
        {typeof(byte[]), "bytea"},
        {typeof(byte), "smallint"},
        {typeof(byte?), "smallint"},
        {typeof(Guid), "uuid"},
        {typeof(Guid?), "uuid"},
        {typeof(TimeSpan), "time without time zone"},
        {typeof(TimeSpan?), "time without time zone"},
        {typeof(decimal), "numeric"},
        {typeof(decimal?), "numeric"},
        {typeof(bool), "boolean"},
        {typeof(bool?), "boolean"},
        {typeof(DateTime), "timestamp"},
        {typeof(DateTime?), "timestamp"},
        {typeof(double), "double precision"},
        {typeof(double?), "double precision"},
        {typeof(float), "real"},
        {typeof(float?), "real"},
        {typeof(long), "bigint"},
        {typeof(long?), "bigint"},
    };

    protected override string GetForeignKeyConstraintName(string tableName, string columnName, string tableNameRef, string columnNameRef)
    {
        return $"{tableName}{columnName}_to_{tableNameRef}{columnNameRef}_fkey";
    }

    protected override string GetPrimaryKeyConstraintName(string tableName)
    {
        return $"{tableName}_pkey";
    }

    protected override string GetPrimaryUniqueName(string tableName, string columnName)
    {
        return $"{tableName}{columnName}_unique";
    }

    protected override string GetIndexName(string tableName, string columnName)
    {
        return $"{tableName}{columnName}_inx";
    }

    protected override string GetAutoIncrementForColumnsSQL(ColumnSubDescriptor columnSubDescriptor)
    {
        //AutoIncrement set in GetDBType
        return String.Empty;
    }

    protected override string GetDBType(ColumnSubDescriptor columnSubDescriptor)
    {
        string result = base.GetDBType(columnSubDescriptor);
        if (!string.IsNullOrEmpty(result)) return result;

        Type propType;
        if (columnSubDescriptor.PropType.IsEnum) //(Enum)
        {
            propType = Enum.GetUnderlyingType(columnSubDescriptor.PropType);
        }
        else if (Nullable.GetUnderlyingType(columnSubDescriptor.PropType) != null && Nullable.GetUnderlyingType(columnSubDescriptor.PropType)!.IsEnum) //(Enum?)
        {
            propType = Enum.GetUnderlyingType(Nullable.GetUnderlyingType(columnSubDescriptor.PropType)!);
        }
        else
        {
            propType = columnSubDescriptor.PropType;
        }

        //AutoIncrement
        if (columnSubDescriptor.AutoIncrement)
        {
            if (dbAutoIncrementTypes.ContainsKey(propType))
            {
                return dbAutoIncrementTypes[propType];
            }
        }

        if (dbTypesDictionary.ContainsKey(propType))
        {
            result = dbTypesDictionary[propType];
            if (result.Equals("varchar"))
            {
                result += columnSubDescriptor.MaxLength != null ? $"({columnSubDescriptor.MaxLength})" : String.Empty;
            }
            return result;
        }
        else
            throw new Exception($"Database does not support type '{columnSubDescriptor.PropType.Name}'");
    }

}