using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Npgsql;

namespace DBHelper.Test
{
    public class SqlServerTest
    {
        private readonly string _connectionString =
            "Data Source=127.0.0.1;Initial Catalog=tempdb;User ID=sa;Password=Password123";


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

        private IDbConnection GetConnection()
        {
            var con = new SqlConnection(_connectionString);
            con.Open();
            return con;
        }

        [Fact]
        public void CreateDropSchema()
        {
            using var con = GetConnection();

            string schemaName = "SchemaTest";

            string createSchemaSQL = new SqlBuilder()
                .CreateSchema(schemaName)
                .UseMsSqlDialect() 
                .Build();
            con.ExecuteScalar(createSchemaSQL);

            string dropSchemaSQL = new SqlBuilder()
                .DropSchema(schemaName)
                .UseMsSqlDialect()
                .Build();
            con.Execute(dropSchemaSQL);
        }

        [Fact]
        public void CreateDropTable()
        {
            using var con = GetConnection();

            string createTableSQL = new SqlBuilder()
                .CreateTable<Role>(true)
                .CreateTable<User>(true)
                .UseMsSqlDialect() 
                .Build();

            con.Execute(createTableSQL);

            var count = con.Execute(@"insert into roles(role_name) values (@name)",
                new[] { new Role(){id = 1,name = "1"}, new Role() { id = 2, name = "2" }, new Role() { id = 3, name = "3" } }
            );
            Assert.Equal(3, count);

            string dropTableSQL = new SqlBuilder()
                .DropTable<User>(true)
                .DropTable<Role>(true)
                .UseMsSqlDialect()
                .Build();

            con.Execute(dropTableSQL);
        }

        [Fact]
        public void CreateDropSequence()
        {
            using var con = GetConnection();

            string createSequenceSQL = new SqlBuilder()
                .CreateSequence("my_sequence", start: 10, minvalue: 0, maxvalue: 100, incrementBy: 5, cycle: true)
                .UseMsSqlDialect() // or UsePostgresqlDialect()
                .Build();
            con.Execute(createSequenceSQL);

            int my_sequence10 = con.QueryFirst<int>("SELECT NEXT VALUE FOR my_sequence");
            int my_sequence15 = con.QueryFirst<int>("SELECT NEXT VALUE FOR my_sequence");
            int my_sequence20 = con.QueryFirst<int>("SELECT NEXT VALUE FOR my_sequence");
            int my_sequence25 = con.QueryFirst<int>("SELECT NEXT VALUE FOR my_sequence");
            int my_sequence30 = con.QueryFirst<int>("SELECT NEXT VALUE FOR my_sequence");
            Assert.Equal(10, my_sequence10);
            Assert.Equal(15, my_sequence15);
            Assert.Equal(20, my_sequence20);
            Assert.Equal(25, my_sequence25);
            Assert.Equal(30, my_sequence30);

            string dropSequenceSQL = new SqlBuilder()
                .DropSequence("my_sequence")
                .UseMsSqlDialect() // or UsePostgresqlDialect()
                .Build();
            con.Execute(dropSequenceSQL);

        }
    }


    public class PostgresTest
    {
        private readonly string _connectionString =
            "User ID=postgres;Password=postgres;Host=localhost;Port=5432;Database=test;";


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

        private IDbConnection GetConnection()
        {
            var con = new NpgsqlConnection(_connectionString);
            con.Open();
            return con;
        }

        [Fact]
        public void CreateDropSchema()
        {
            using var con = GetConnection();

            string schemaName = "SchemaTest";

            string createSchemaSQL = new SqlBuilder()
                .CreateSchema(schemaName)
                .UsePostgresqlDialect()
                .Build();
            con.ExecuteScalar(createSchemaSQL);

            string dropSchemaSQL = new SqlBuilder()
                .DropSchema(schemaName)
                .UsePostgresqlDialect()
                .Build();
            con.Execute(dropSchemaSQL);
        }

        [Fact]
        public void CreateDropTable()
        {
            using var con = GetConnection();

            string createTableSQL = new SqlBuilder()
                .CreateTable<Role>(true)
                .CreateTable<User>(true)
                .UsePostgresqlDialect()
                .Build();

            con.Execute(createTableSQL);

            var count = con.Execute(@"insert into roles(role_name) values (@name)",
                new[] { new Role() { id = 1, name = "1" }, new Role() { id = 2, name = "2" }, new Role() { id = 3, name = "3" } }
            );
            Assert.Equal(3, count);

            string dropTableSQL = new SqlBuilder()
                .DropTable<User>(true)
                .DropTable<Role>(true)
                .UsePostgresqlDialect()
                .Build();

            con.Execute(dropTableSQL);
        }

        [Fact]
        public void CreateDropSequence()
        {
            using var con = GetConnection();

            string createSequenceSQL = new SqlBuilder()
                .CreateSequence("my_sequence", start: 10, minvalue: 0, maxvalue: 100, incrementBy: 5, cycle: true)
                .UsePostgresqlDialect() // or UsePostgresqlDialect()
                .Build();
            con.Execute(createSequenceSQL);

            int my_sequence10 = con.QueryFirst<int>("SELECT nextval('my_sequence');");
            int my_sequence15 = con.QueryFirst<int>("SELECT nextval('my_sequence');");
            int my_sequence20 = con.QueryFirst<int>("SELECT nextval('my_sequence');");
            int my_sequence25 = con.QueryFirst<int>("SELECT nextval('my_sequence');");
            int my_sequence30 = con.QueryFirst<int>("SELECT nextval('my_sequence');");
            Assert.Equal(10, my_sequence10);
            Assert.Equal(15, my_sequence15);
            Assert.Equal(20, my_sequence20);
            Assert.Equal(25, my_sequence25);
            Assert.Equal(30, my_sequence30);

            string dropSequenceSQL = new SqlBuilder()
                .DropSequence("my_sequence")
                .UsePostgresqlDialect() // or UsePostgresqlDialect()
                .Build();
            con.Execute(dropSequenceSQL);

        }
    }
}
