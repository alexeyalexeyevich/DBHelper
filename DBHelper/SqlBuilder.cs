using DBHelper.Descriptors;
using DBHelper.Dialects;
using System.Collections.Generic;

namespace DBHelper;

public class SqlBuilder : ISqlBuilder
{
    IDBDialect m_dialect = new MsSqlDialect();
    List<IObjectDescriptor> m_objects = new();

    public string Build()
    {
        return m_dialect.Build(m_objects);
    }

    public ISqlBuilder CreateTable<T>()
    {
        m_objects.Add(new CreateTableDescriptor(typeof(T)));
        return this;
    }

    public ISqlBuilder DropTable<T>()
    {
        m_objects.Add(new DropTableDescriptor(typeof(T)));
        return this;
    }

    public ISqlBuilder TruncateTable<T>()
    {
        m_objects.Add(new TruncateTableDescriptor(typeof(T)));
        return this;
    }

    public ISqlBuilder CreateSchema(string name)
    {
        m_objects.Add(new CreateSchemaDescriptor(name));
        return this;
    }

    public ISqlBuilder DropSchema(string name)
    {
        m_objects.Add(new DropSchemaDescriptor(name));
        return this;
    }


    public ISqlBuilder UseDialect<T>() where T : IDBDialect, new()
    {
        m_dialect = new T();
        return this;
    }

    public ISqlBuilder CustomSql(string sql)
    {
        m_objects.Add(new CustomSQLDescriptor(sql));
        return this;
    }

    public ISqlBuilder UsePostgresqlDialect()
    {
        return UseDialect<PostgresqlDialect>();
    }

    public ISqlBuilder UseMsSqlDialect()
    {
        return UseDialect<MsSqlDialect>();
    }

    public ISqlBuilder CreateSequence(string name, string schema = null, string dbType = null, int? start = null, int? incrementBy = null, int? minvalue = null, int? maxvalue = null, bool? cycle = null)
    {
        m_objects.Add(new CreateSequenceDescriptor(name, schema, dbType, start, incrementBy, minvalue, maxvalue, cycle));
        return this;
    }

    public ISqlBuilder DropSequence(string name, string schema = null)
    {
        m_objects.Add(new DropSequenceDescriptor(name, schema));
        return this;
    }

    public ISqlBuilder Clear()
    {
        m_objects = new();
        return this;
    }

}