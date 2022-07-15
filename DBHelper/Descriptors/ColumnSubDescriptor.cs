using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;

namespace DBHelper.Descriptors
{
    public class ColumnSubDescriptor
    {
        private readonly PropertyInfo m_property;

        public TableDescriptor CreateTableDescriptor { get; }

        public Type PropType => m_property.PropertyType;

        public bool AutoIncrement { get; }

        public string Name { get; }

        public int Order { get; }

        public string TypeName { get; }

        public bool PrimaryKey { get; }

        public bool ForeignKey { get; }

        public Type ForeignKeyReferenceType { get; }

        public string ForeignKeyReferencePropName { get; }

        public bool Index { get; }

        public bool IndexUnique { get; }

        public bool Required { get; }

        public bool Unique { get; }

        public int? MaxLength { get; }

        public string DefaultValue { get; }

        private ColumnSubDescriptor(TableDescriptor createTableDescriptor, PropertyInfo Property, Attribute[] attributes)
        {

            CreateTableDescriptor = createTableDescriptor;
            m_property = Property; 
            Name = m_property.GetColumnName();

            if (attributes.FirstOrDefault(a=>a is ColumnAttribute) is ColumnAttribute columnAttribute)
            {
                Order = columnAttribute.Order;
                TypeName = columnAttribute.TypeName;
            }
            else
            {
                Order = -1;
                TypeName = null;
            }

            PrimaryKey = attributes.Any(a=>a is KeyAttribute);

            if (attributes.FirstOrDefault(a => a is ForeignKeyAttribute) is ForeignKeyAttribute foreignKeyAttribute)
            {
                ForeignKey = true;
                ForeignKeyReferenceType = foreignKeyAttribute.ReferencesType;
                ForeignKeyReferencePropName = foreignKeyAttribute.ReferencesPropName;
            }
            else
            {
                ForeignKey = false;
                ForeignKeyReferenceType = null;
                ForeignKeyReferencePropName = string.Empty;
            }

            if (attributes.FirstOrDefault(a => a is IndexAttribute) is IndexAttribute indexAttribute)
            {
                Index = true;
                IndexUnique = indexAttribute.Unique;
            }
            else
            {
                Index = false;
                IndexUnique = false;
            }


            AutoIncrement = attributes.Any(a => a is AutoIncrementAttribute);

            Required = attributes.Any(a => a is RequiredAttribute);

            Unique = attributes.Any(a => a is UniqueAttribute);

            if (attributes.FirstOrDefault(a => a is MaxLengthAttribute) is MaxLengthAttribute maxLengthAttribute)
            {
                MaxLength = maxLengthAttribute.Length;
            }
            else
            {
                MaxLength =
                    attributes.FirstOrDefault(a => a is StringLengthAttribute) is StringLengthAttribute
                        stringLengthAttribute
                        ? stringLengthAttribute.MaximumLength
                        : null;
            }

            var defaultValueAttribute =
                attributes.FirstOrDefault(a => a is DefaultValueAttribute) as DefaultValueAttribute;//m_property.GetCustomAttribute<DefaultValueAttribute>();
            DefaultValue = defaultValueAttribute?.Value?.ToString();
        }

        public static IEnumerable<ColumnSubDescriptor> GetColumnsDescriptors(TableDescriptor createTableDescriptor)
        {
            List<ColumnSubDescriptor> columns = new();

            foreach (var property in createTableDescriptor.Type.GetProperties())
            {
                var attributes = property.GetCustomAttributes().ToArray();

                //filter NotMappedAttribute
                if (attributes.Any(a=>a is NotMappedAttribute))                
                    continue;

                columns.Add(new ColumnSubDescriptor(createTableDescriptor, property, attributes));
            }

            return Sort(columns);
        }

        private static IEnumerable<ColumnSubDescriptor> Sort(List<ColumnSubDescriptor> columns)
        {
            int countColumns = columns.Count;
            if (countColumns == 0) return Array.Empty<ColumnSubDescriptor>();

            var orderedColumns = columns.Where(c => c.Order != -1).OrderBy(c => c.Order).ToList();
            if (orderedColumns.Count == 0) return columns.ToArray();

            var unorderedColumns = new Stack<ColumnSubDescriptor>(columns.Where(c => c.Order == -1));
            for (int i = 0; i < countColumns; i++)
            {
                if (unorderedColumns.Count == 0) break;

                if (i > orderedColumns.Count - 1)
                {
                    orderedColumns.AddRange(unorderedColumns);
                    break;
                }

                if (orderedColumns[i].Order > i)
                {
                    orderedColumns.Insert(i, unorderedColumns.Pop());
                }
            }

            return orderedColumns.ToArray();
        }

    }
}
