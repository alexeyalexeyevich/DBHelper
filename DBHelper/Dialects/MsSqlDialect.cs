using DBHelper.Descriptors;
using System;
using System.Collections.Generic;
using System.Xml;

namespace DBHelper.Dialects;

public class MsSqlDialect : BaseDialect
{
    public override string DefaultSchema { get; } = "dbo";

    private readonly Dictionary<Type, string> dbTypesDictionary = new()
    {
        {typeof(short), "smallint"},
        {typeof(short?), "smallint"},
        {typeof(int), "int"},
        {typeof(int?), "int"},
        {typeof(string), "nvarchar"},
        {typeof(char[]), "nvarchar"},
        {typeof(byte[]), "varbinary"},
        {typeof(byte), "tinyint"},
        {typeof(byte?), "tinyint"},
        {typeof(Guid), "uniqueidentifier"},
        {typeof(Guid?), "uniqueidentifier"},
        {typeof(TimeSpan), "time"},
        {typeof(TimeSpan?), "time"},
        {typeof(decimal), "smallmoney"},
        {typeof(decimal?), "smallmoney"},
        {typeof(bool), "bit"},
        {typeof(bool?), "bit"},
        {typeof(DateTime), "datetime"},
        {typeof(DateTime?), "datetime"},
        {typeof(double), "float"},
        {typeof(double?), "float"},
        {typeof(float), "real"},
        {typeof(float?), "real"},
        {typeof(long), "bigint"},
        {typeof(long?), "bigint"},
        {typeof(XmlDocument), "xml"},
        {typeof(XmlNode), "xml"},
        {typeof(XmlElement), "xml"},
    };

    protected override string GetForeignKeyConstraintName(string tableName, string columnName, string tableNameRef, string columnNameRef)
    {
        return $"FK_{tableName}{columnName}_TO_{tableNameRef}{columnNameRef}";
    }

    protected override string GetPrimaryKeyConstraintName(string tableName)
    {
        return $"PK_{tableName}";
    }

    protected override string GetPrimaryUniqueName(string tableName, string columnName)
    {
        return $"UC_{tableName}{columnName}";
    }

    protected override string GetIndexName(string tableName, string columnName)
    {
        return $"INX_{tableName}{columnName}";
    }

    protected override string GetAutoIncrementForColumnsSQL(ColumnSubDescriptor columnSubDescriptor)
    {
        return "IDENTITY(1,1)";
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

        if (dbTypesDictionary.ContainsKey(propType))
        {
            result = dbTypesDictionary[propType];
            if (result.Equals("nvarchar") || result.Equals("varbinary"))
            {
                result += columnSubDescriptor.MaxLength != null ? $"({columnSubDescriptor.MaxLength})" : "(max)";
            }
            return result;
        }
        else
            throw new Exception($"Database does not support type '{columnSubDescriptor.PropType.Name}'[{columnSubDescriptor.Name}]");
    }

    protected override string GetDefaultValueSQL(ColumnSubDescriptor defaultValue)
    {
        TableDescriptor createTableDescriptor = defaultValue.CreateTableDescriptor;
        (string tableName, string schema) = GetTableName(createTableDescriptor);
        //ALTER TABLE <table_name> ADD DEFAULT '<default_value>' FOR <column_name>;
        return $"ALTER TABLE {schema}.{tableName} ADD DEFAULT '{defaultValue.DefaultValue}' FOR {defaultValue.Name} ;";
    }
}