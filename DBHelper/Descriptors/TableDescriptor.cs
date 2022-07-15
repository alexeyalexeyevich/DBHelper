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

    public CreateTableDescriptor(Type type) : base(type)
    {
        Columns = ColumnSubDescriptor.GetColumnsDescriptors(this);
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
    public DropTableDescriptor(Type type) : base(type)
    {
    }
}