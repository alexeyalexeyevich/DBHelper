using DBHelper.Dialects;

namespace DBHelper
{
    public interface ISqlBuilder
    {
        string Build();

        public ISqlBuilder CreateSchema(string name);

        public ISqlBuilder DropSchema(string name);

        ISqlBuilder CreateTable<T>(bool ifNotExists = false);

        ISqlBuilder DropTable<T>(bool ifExists = false);

        ISqlBuilder TruncateTable<T>();

        ISqlBuilder CreateSequence(string name, string schema = null, string dbType = null, int? start = null, int? incrementBy = null, int? minvalue = null, int? maxvalue = null, bool? cycle = null);

        ISqlBuilder DropSequence(string name, string schema = null);

        ISqlBuilder CustomSql(string sql);

        ISqlBuilder UseDialect<T>() where T : IDBDialect, new();

        ISqlBuilder UsePostgresqlDialect();

        ISqlBuilder UseMsSqlDialect();

        ISqlBuilder Clear();
    }


}