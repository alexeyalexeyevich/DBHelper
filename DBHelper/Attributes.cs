using System;

namespace DBHelper;

[AttributeUsage(AttributeTargets.Property)]
public class IndexAttribute : Attribute
{
    public bool Unique { get; }

    public IndexAttribute(bool unique = false)
    {
        Unique = unique;
    }
}


[AttributeUsage(AttributeTargets.Property)]
public class UniqueAttribute : Attribute { }


[AttributeUsage(AttributeTargets.Property)]
public class ForeignKeyAttribute : Attribute
{
    public Type ReferencesType { get; }
    public string ReferencesPropName { get; }

    public ForeignKeyAttribute(Type referencesType, string referencesPropName)
    {
        ReferencesType = referencesType;
        ReferencesPropName = referencesPropName;
    }

}

[AttributeUsage(AttributeTargets.Property)]
public class AutoIncrementAttribute : Attribute { }

[AttributeUsage(AttributeTargets.Property)]
public class ComputedAttribute : Attribute
{
    public string Expression { get;}

    public ComputedAttribute(string expression)
    {
        Expression = expression;
    }
}