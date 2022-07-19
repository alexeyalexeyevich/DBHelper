using DBHelper.Descriptors;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DBHelper.Dialects;

public abstract class BaseDialect : IDBDialect
{
    public abstract string DefaultSchema { get; }

    public string Build(List<IObjectDescriptor> objectsDescriptor)
    {
        string result = String.Empty;
        foreach (var obj in objectsDescriptor)
        {
            switch (obj)
            {
                case CreateTableDescriptor tableDescriptor:
                    result += CreateTable(tableDescriptor) + Environment.NewLine;
                    break;

                case DropTableDescriptor dropTableDescriptor:
                    result += DropTable(dropTableDescriptor) + Environment.NewLine;
                    break;

                case TruncateTableDescriptor truncateTableDescriptor:
                    result += TruncateTable(truncateTableDescriptor) + Environment.NewLine;
                    break;

                case CustomSQLDescriptor customSQL:
                    result += customSQL.CustomSQL + Environment.NewLine;
                    break;

                case CreateSchemaDescriptor createSchemaDescriptor:
                    result += CreateSchema(createSchemaDescriptor) + Environment.NewLine;
                    break;

                case DropSchemaDescriptor dropSchemaDescriptor:
                    result += DropSchema(dropSchemaDescriptor) + Environment.NewLine;
                    break;
                case CreateSequenceDescriptor createSequenceDescriptor:
                    result += CreateSequence(createSequenceDescriptor) + Environment.NewLine;
                    break;

                case DropSequenceDescriptor dropSequenceDescriptor:
                    result += DropSequence(dropSequenceDescriptor) + Environment.NewLine;
                    break;                                }
        }
        return result;
    }

    private string DropSequence(DropSequenceDescriptor dropSequenceDescriptor)
    {
        string schema = String.IsNullOrWhiteSpace(dropSequenceDescriptor.Schema) ? DefaultSchema : dropSequenceDescriptor.Schema;
        //DROP SEQUENCE IF EXISTS <sequence_name>
        return $"DROP SCHEMA IF EXISTS {schema}.{dropSequenceDescriptor.Name};";
    }

    private string CreateSequence(CreateSequenceDescriptor createSequenceDescriptor)
    {
        string schema = String.IsNullOrWhiteSpace(createSequenceDescriptor.Schema) ? DefaultSchema : createSequenceDescriptor.Schema;

        // CREATE SEQUENCE <sequence_name>
        // [AS <database_type>]
        // [START WITH <start>]
        // [INCREMENT BY <increment>]
        // { MINVALUE <min_value> } | { NO MINVALUE }
        // { MAXVALUE <max_value> } | { NO MAXVALUE }
        // [{CYCLE} | { NO CYCLE }];
        string result = string.Empty;
        result += $"CREATE SEQUENCE {schema}.{createSequenceDescriptor.Name} ";
        if (!string.IsNullOrWhiteSpace(createSequenceDescriptor.DbType))        
            result += $"AS {createSequenceDescriptor.DbType} ";
        if(createSequenceDescriptor.Start!=null)
            result += $"START WITH {createSequenceDescriptor.Start} ";
        if(createSequenceDescriptor.IncrementBy!=null)
            result += $"INCREMENT BY {createSequenceDescriptor.IncrementBy} ";
        result += createSequenceDescriptor.MinValue == null ? "NO MINVALUE " : $"MINVALUE {createSequenceDescriptor.MinValue} ";
        result += createSequenceDescriptor.MaxValue == null ? "NO MAXVALUE " : $"MAXVALUE {createSequenceDescriptor.MaxValue} ";
        if (createSequenceDescriptor.Cycle != null)
            result += (bool)createSequenceDescriptor.Cycle ? "CYCLE;" : "NO CYCLE;";
        else
            result += ";";
        return result;
    }

    protected virtual string DropSchema(DropSchemaDescriptor dropSchemaDescriptor)
    {
        //DROP SCHEMA IF EXISTS <schema_name>;
        return $"DROP SCHEMA IF EXISTS {dropSchemaDescriptor.Name};";
    }

    protected virtual string CreateSchema(CreateSchemaDescriptor createSchemaDescriptor)
    {
        //CREATE SCHEMA IF NOT EXISTS <schema_name>;
        return $"CREATE SCHEMA IF NOT EXISTS {createSchemaDescriptor.Name};";
    }

    protected virtual string TruncateTable(TruncateTableDescriptor truncateTableDescriptor)
    {
        (string tableName, string schema) = GetTableName(truncateTableDescriptor);
        //TRUNCATE TABLE <table_name>;
        return $"TRUNCATE TABLE {schema}.{tableName};";
    }

    protected virtual string DropTable(DropTableDescriptor tableDescriptor)
    {
        (string tableName, string schema) = GetTableName(tableDescriptor);
        //DROP TABLE <table_name>;
        return $"DROP TABLE {schema}.{tableName};";
    }

    protected virtual string CreateTable(CreateTableDescriptor createTableDescriptor)
    {
        string result = String.Empty;
        (string tableName, string schema) = GetTableName(createTableDescriptor);

        var columns = GetColumnsSQL(createTableDescriptor.Columns);
        var alterTableConstraints = GetConstraintsSQL(createTableDescriptor, createTableDescriptor.Columns);
        IEnumerable<string> alterIndexes = GetIndexesSQL(createTableDescriptor.Columns);

        result += $"CREATE TABLE {schema}.{tableName} (" + Environment.NewLine +
                  string.Join(", " + Environment.NewLine, columns) + Environment.NewLine +
                  ");" + Environment.NewLine;

        if (alterTableConstraints.Any())
            result += string.Join(Environment.NewLine, alterTableConstraints) + Environment.NewLine;

        if (alterIndexes.Any())
            result += string.Join(Environment.NewLine, alterIndexes) + Environment.NewLine;

        string beforCreateTableSQL = GetBeforTableSQL(createTableDescriptor);
        string afterCreateTableSQL = GetAfterTableSQL(createTableDescriptor);

        if (!string.IsNullOrWhiteSpace(beforCreateTableSQL))
        {
            result = beforCreateTableSQL + Environment.NewLine + result;
        }
        if (!string.IsNullOrWhiteSpace(afterCreateTableSQL))
        {
            result += afterCreateTableSQL + Environment.NewLine;
        }

        return result;
    }

    protected virtual IEnumerable<string> GetIndexesSQL(IEnumerable<ColumnSubDescriptor> indexDescriptorColumns)
    {
        foreach (var indexDescriptor in indexDescriptorColumns.Where(c=>c.Index))
        {
            yield return GetCreateIndexSQL(indexDescriptor);
        }
    }

    private string GetCreateIndexSQL(ColumnSubDescriptor indexSubDescriptor)
    {
        TableDescriptor createTableDescriptor = indexSubDescriptor.CreateTableDescriptor;
        (string tableName, string schema) = GetTableName(createTableDescriptor);
        //CREATE [ UNIQUE ] INDEX  <index_name>  ON <table_name> (<column_name>);
        return $"CREATE {(indexSubDescriptor.IndexUnique? "UNIQUE ": String.Empty)}INDEX {GetIndexName(tableName, indexSubDescriptor.Name)} ON {schema}.{tableName}({indexSubDescriptor.Name});";
    }

    protected virtual string GetAfterTableSQL(CreateTableDescriptor createTableDescriptor) => string.Empty;

    protected virtual string GetBeforTableSQL(CreateTableDescriptor createTableDescriptor) => string.Empty;

    protected virtual IEnumerable<string> GetConstraintsSQL(CreateTableDescriptor createTableDescriptor, IEnumerable<ColumnSubDescriptor> tableDescriptorColumns)
    {
        var constraintPrimaryKey = tableDescriptorColumns.Where(c => c.PrimaryKey);
        if (constraintPrimaryKey.Any())
            yield return GetAlterTablePrimaryKeySQL(constraintPrimaryKey);


        var constraintsForeignKey = tableDescriptorColumns.Where(c => c.ForeignKey);

        foreach (var foreignKey in constraintsForeignKey)
        {
            yield return GetAlterTableForeignKeySQL(foreignKey);
        }

        var constraintUnique = tableDescriptorColumns.Where(c => c.Unique);
        foreach (var unique in constraintUnique)
        {
            yield return GetUniqueSQL(unique);
        }

        var defaultValues = tableDescriptorColumns.Where(c => !string.IsNullOrWhiteSpace(c.DefaultValue));
        foreach (var defaultValue in defaultValues)
        {
            yield return GetDefaultValueSQL(defaultValue);
        }

    }

    protected virtual string GetDefaultValueSQL(ColumnSubDescriptor defaultValue)
    {
        TableDescriptor createTableDescriptor = defaultValue.CreateTableDescriptor;
        (string tableName, string schema) = GetTableName(createTableDescriptor);
        //ALTER TABLE <table_name> ALTER COLUMN <column_name> SET DEFAULT '<default_value>';
        return $"ALTER TABLE {schema}.{tableName} ALTER COLUMN {defaultValue.Name} SET DEFAULT '{defaultValue.DefaultValue}';";
    }

    protected virtual string GetUniqueSQL(ColumnSubDescriptor constraintUnique)
    {
        TableDescriptor createTableDescriptor = constraintUnique.CreateTableDescriptor;
        (string tableName, string schema) = GetTableName(createTableDescriptor);
        //ALTER TABLE <table_name> ADD CONSTRAINT <constraint_name> UNIQUE(<column_name>);
        return $"ALTER TABLE {schema}.{tableName} ADD CONSTRAINT {GetPrimaryUniqueName(tableName, constraintUnique.Name)} UNIQUE ({constraintUnique.Name});";
    }

    protected virtual string GetAlterTableForeignKeySQL(ColumnSubDescriptor foreignKey)
    {
        TableDescriptor createTableDescriptor = foreignKey.CreateTableDescriptor;
        (string tableName, string schema) = GetTableName(createTableDescriptor);

        (string tableNameRef, string schemaRef) = foreignKey.ForeignKeyReferenceType.GetTableName();
        if (string.IsNullOrWhiteSpace(schemaRef)) schemaRef = DefaultSchema;

        string columnRef = foreignKey.ForeignKeyReferenceType
            .GetProperty(foreignKey.ForeignKeyReferencePropName).GetColumnName();
        //ALTER TABLE <table_name> ADD CONSTRAINT <constraint_name> FOREIGN KEY(<column_name>) REFERENCES <ref_table_name>(<ref_column_name>);
        return $"ALTER TABLE {schema}.{tableName} ADD CONSTRAINT {GetForeignKeyConstraintName(tableName, foreignKey.Name, tableNameRef, columnRef)} FOREIGN KEY({foreignKey.Name}) REFERENCES {schemaRef}.{tableNameRef}({columnRef});";
    }

    protected virtual string GetAlterTablePrimaryKeySQL(IEnumerable<ColumnSubDescriptor> constraintPrimaryKey)
    {
        TableDescriptor createTableDescriptor = constraintPrimaryKey.FirstOrDefault().CreateTableDescriptor;
        (string tableName, string schema) = GetTableName(createTableDescriptor);
        //ALTER TABLE <table_name> ADD CONSTRAINT <constraint_name> PRIMARY KEY(<column_name> [, ...]);
        return $"ALTER TABLE {schema}.{tableName} ADD CONSTRAINT {GetPrimaryKeyConstraintName(tableName)} PRIMARY KEY ({(String.Join(", ", constraintPrimaryKey.Select(pk => pk.Name)))});";
    }

    protected (string tableName, string schema) GetTableName(TableDescriptor tableDescriptor)
    {
        string schema = String.IsNullOrWhiteSpace(tableDescriptor.Schema) ? DefaultSchema : tableDescriptor.Schema;
        var tableName = tableDescriptor.Name;
        return (tableName, schema);
    }

    protected virtual IEnumerable<string> GetColumnsSQL(IEnumerable<ColumnSubDescriptor> tableDescriptorColumns)
    {
        foreach (ColumnSubDescriptor columnDescriptor in tableDescriptorColumns)
        {
            if (string.IsNullOrWhiteSpace(columnDescriptor.ComputedExpression))
            {
                yield return GetColumnSQL(columnDescriptor);
            }
            else
            {
                yield return GetComputedColumnSQL(columnDescriptor);
            }
        }
    }

    protected virtual string GetColumnSQL(ColumnSubDescriptor columnDescriptor)
    {
        //<column_definition> ::= <column_name> <data_type> [AUTOINCREMENT] [null | not null] 
        var result =
            $"       {columnDescriptor.Name} {GetDBType(columnDescriptor)} ";

        if (columnDescriptor.AutoIncrement)
            result += GetAutoIncrementForColumnsSQL(columnDescriptor) + " ";

        result += columnDescriptor.Required ? "not null" : "null";
        return result;
    }

    protected virtual string GetComputedColumnSQL(ColumnSubDescriptor columnDescriptor)
    {
        //<column_definition> ::= <column_name> <data_type> GENERATED ALWAYS AS (<expression>) STORED
        var result =
            $"       {columnDescriptor.Name} {GetDBType(columnDescriptor)}  GENERATED ALWAYS AS ({columnDescriptor.ComputedExpression}) STORED";
        return result;
    }

    protected virtual string GetDBType(ColumnSubDescriptor columnSubDescriptor)
    {
        return columnSubDescriptor.TypeName;
    }

    protected abstract string GetForeignKeyConstraintName(string tableName, string columnName, string tableNameRef, string columnNameRef);

    protected abstract string GetPrimaryKeyConstraintName(string tableName);

    protected abstract string GetPrimaryUniqueName(string tableName, string columnName);

    protected abstract string GetIndexName(string tableName, string columnName);

    protected abstract string GetAutoIncrementForColumnsSQL(ColumnSubDescriptor columnSubDescriptor);

}