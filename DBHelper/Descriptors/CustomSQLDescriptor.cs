namespace DBHelper.Descriptors;

public class CustomSQLDescriptor : IObjectDescriptor
{
    public string CustomSQL { get; }

    public CustomSQLDescriptor(string customSQL)
    {
        CustomSQL = customSQL;
    }
}