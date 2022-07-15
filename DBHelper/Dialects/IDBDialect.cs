using DBHelper.Descriptors;
using System.Collections.Generic;

namespace DBHelper.Dialects
{
    public interface IDBDialect
    {
        public string DefaultSchema { get; }
        string Build(List<IObjectDescriptor> objectsDescriptor);

    }
}