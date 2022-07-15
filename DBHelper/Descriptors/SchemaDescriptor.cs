using System;
using System.Collections.Generic;
using System.Text;

namespace DBHelper.Descriptors
{
    public class SchemaDescriptor : IObjectDescriptor
    {
        public string Name { get; }
        public SchemaDescriptor(string name)
        {
            Name = name;
        }
    }

    public class CreateSchemaDescriptor : SchemaDescriptor
    {        
        public CreateSchemaDescriptor(string name):base(name)
        {            
        }
    }

    public class DropSchemaDescriptor : SchemaDescriptor
    {
        public DropSchemaDescriptor(string name) : base(name)
        {
        }
    }
}
