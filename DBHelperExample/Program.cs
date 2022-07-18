using DBHelper;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

internal class Program
{
    static void Main(string[] args)
    {
        //Create/Drop schema
        string schemaSQL = new SqlBuilder()
            .CreateSchema("SchemaTest")
            .DropSchema("SchemaTest")
            .UseMsSqlDialect() // or UsePostgresqlDialect()
            .Build();
        Console.WriteLine(schemaSQL);

        //Create Table
        string createTableSQL = new SqlBuilder()
            .CreateTable<Role>()
            .CreateTable<User>()
            .UseMsSqlDialect() // or UsePostgresqlDialect()
            .Build();
        Console.WriteLine(createTableSQL);

        //Drop Table
        string dropTableSQL = new SqlBuilder()
            .DropTable<User>()
            .UseMsSqlDialect() // or UsePostgresqlDialect()
            .Build();
        Console.WriteLine(dropTableSQL);

        //Create/Drop sequence 
        string createSequenceSQL = new SqlBuilder()
            .CreateSequence("my_sequence", schema: "my_schema", start: 10, minvalue: 0, maxvalue: 100, incrementBy: 5, cycle: true)
            .DropSequence("my_sequence", schema: "my_schema")
            .UseMsSqlDialect() // or UsePostgresqlDialect()
            .Build();
        Console.WriteLine(createSequenceSQL);
    }
}



[Table("users")]
public class User
{
    [Key]
    [AutoIncrement]
    [Required]
    [Column(Order = 0)]
    public int id { get; set; }

    [Column("user_name", Order = 1)]
    public string name { get; set; }

    [MaxLength(50)]
    [Unique]
    public string email { get; set; }

    [DefaultValue(0)]
    public int age { get; set; }

    [DBHelper.ForeignKey(typeof(Role), nameof(Role.id))]
    public int role_id { get; set; }

    [NotMapped]
    public Role? role { get; set; }
}

[Table("roles")]
public class Role
{
    [Key]
    [AutoIncrement]
    [Required]
    public int id { get; set; }

    [Column("role_name")]
    public string? name { get; set; }

}


