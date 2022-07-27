using System;
using System.Collections.Generic;

namespace DBHelper.Descriptors;

public class TableDescriptor : IObjectDescriptor
{
    public Type Type { get; }

    public string Name { get; }

    public string Schema { get; }

    public TableDescriptor(Type type)
    {
        Type = type;

        (Name, Schema) = Type.GetTableName();
    }
}

public class CreateTableDescriptor : TableDescriptor
{
    public IEnumerable<ColumnSubDescriptor> Columns { get; }
    public bool IfNotExists { get; }

    public CreateTableDescriptor(Type type, bool ifNotExists = false) : base(type)
    {
        Columns = ColumnSubDescriptor.GetColumnsDescriptors(this);
        IfNotExists = ifNotExists;
    }
}

public class TruncateTableDescriptor : TableDescriptor
{
    public TruncateTableDescriptor(Type type) : base(type)
    {
    }
}


public class DropTableDescriptor : TableDescriptor
{
    public bool IfExists { get; }
    public DropTableDescriptor(Type type, bool ifExists) : base(type)
    {
        IfExists = ifExists;
    }
}