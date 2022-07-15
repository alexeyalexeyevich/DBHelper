using System.ComponentModel;
using DBHelper;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

internal class Program
{
    static void Main(string[] args)
    {

        var cb = new SqlBuilder();
        string sqlPostgres = cb.CreateSchema("SchemaTest")
        .CreateSequence("SequenceTest", "SchemaTest", cycle: true)
            .CreateTable<Role>()
            .CreateTable<User>()
            .DropTable<User>()
            .DropTable<Role>()
            .DropSchema("SchemaTest")
            .UsePostgresqlDialect()
            .Build();

        Console.WriteLine("----------------------------------------Postgres----------------------------------------");
        Console.WriteLine(sqlPostgres);
        Console.WriteLine();
        Console.WriteLine();

        string mssqlPostgres = cb.CreateSchema("SchemaTest")
            .CreateSequence("SequenceTest", "SchemaTest", cycle: true)
            .CreateTable<Role>()
            .CreateTable<User>()
            .DropTable<User>()
            .DropTable<Role>()
            .DropSchema("SchemaTest")
            .UseMsSqlDialect()
            .Build();
        Console.WriteLine("----------------------------------------MsSql----------------------------------------");
        Console.WriteLine(mssqlPostgres);
        Console.ReadKey();
    }
}


public struct aa
{
    public int a;
}

[Table("UserTable", Schema = "SchemaTest")]
public class User
{
    [Key]
    [AutoIncrement]
    [Required]
    [Column("id", Order = 0)]
    public int Id { get; set; }
    [Column("user_name", Order = 3)]
    public string Name { get; set; }

    [Column("user_surname", Order = 2)]
    public string surname { get; set; }

    public long column_long { get; set; }

    [DefaultValue("10")]
    public float column_float { get; set; }

    [Index(true)]
    public int column_int_unique_index { get; set; }

    [Unique]
    public int column_int_unique { get; set; }

    [MaxLength(50)]
    public string column_string_max50 { get; set; }

    public Guid column_guid { get; set; }

    public DateTime column_datetime { get; set; }

    [DBHelper.ForeignKey(typeof(Role), nameof(Role.Id))]
    public int role_id { get; set; }

    [NotMapped]
    public Role? role { get; set; }

}

[Table("Role", Schema = "SchemaTest")]
public class Role
{
    [Key]
    [AutoIncrement]
    [Required]
    public int Id { get; set; }

    public string? Name { get; set; }

}