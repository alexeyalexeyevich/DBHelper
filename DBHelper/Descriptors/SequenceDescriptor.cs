using System;
using System.Collections.Generic;
using System.Text;

namespace DBHelper.Descriptors
{
    public class SequenceDescriptor : IObjectDescriptor
    {
        public string Name { get; }

        public string Schema { get; }

        public SequenceDescriptor(string name, string schema)
        {
            Name = name;
            Schema = schema;    
        }
    }

    public class CreateSequenceDescriptor : SequenceDescriptor
    {
        public string DbType { get; }
        public int? Start { get; }
        public int? IncrementBy { get; }
        public int? MinValue { get; }
        public int? MaxValue { get; }
        public bool? Cycle { get; }



        public CreateSequenceDescriptor(string name, string schema, string dbType, int? start, int? incrementBy, int? minvalue, int? maxvalue, bool? cycle) : base(name, schema)
        {
            DbType = dbType;
            Start = start;
            IncrementBy = incrementBy;
            MinValue = minvalue;
            MaxValue = maxvalue;
            Cycle = cycle;
        }
    }

    public class DropSequenceDescriptor : SequenceDescriptor
    {
        public DropSequenceDescriptor(string name, string schema) : base(name, schema)
        {
        }
    }
}
