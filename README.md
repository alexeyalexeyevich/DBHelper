# DBHelper
## Features
`DBHelper` contains the `SqlBuilder` class, which helps to build SQL statements for creating/deleting schemas, tables, sequence generators. When constructing SQL expressions, descriptions of classes of database models are used.
Released Postgresql and SQL Server dialects.
## Create Table
```csharp
ISqlBuilder CreateTable<T>(bool ifNotExists = false);
```
Example basic usage:
```csharp
public class User
{
    public int id { get; set; }
    public string name { get; set; }
    public int age { get; set; }
}

string createTableSQL = new SqlBuilder()
                        .CreateTable<User>()
                        .UseMsSqlDialect() // or UsePostgresqlDialect()
                        .Build();
Console.WriteLine(createTableSQL);
```
Result for SQL Server:
```sql
CREATE TABLE dbo.User (
       id int null,
       name nvarchar(max) null,
       age int null
);
```
Result for postgresql:
```sql
CREATE TABLE public.User (
       id integer null,
       name varchar null,
       age integer null
);
```
More complex example:
```csharp
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


string createTableSQL = new SqlBuilder()
                            .CreateTable<Role>()
                            .CreateTable<User>()
                            .UseMsSqlDialect() // or UsePostgresqlDialect()
                            .Build();
Console.WriteLine(createTableSQL);
```
Result for SQL Server:
```sql
CREATE TABLE dbo.roles (
       id int IDENTITY(1,1) not null,
       role_name nvarchar(max) null
);
ALTER TABLE dbo.roles ADD CONSTRAINT PK_roles PRIMARY KEY (id);

CREATE TABLE dbo.users (
       id int IDENTITY(1,1) not null,
       user_name nvarchar(max) null,
       role_id int null,
       age int null,
       email nvarchar(50) null
);
ALTER TABLE dbo.users ADD CONSTRAINT PK_users PRIMARY KEY (id);
ALTER TABLE dbo.users ADD CONSTRAINT FK_usersrole_id_TO_rolesid FOREIGN KEY(role_id) REFERENCES dbo.roles(id);
ALTER TABLE dbo.users ADD CONSTRAINT UC_usersemail UNIQUE (email);
ALTER TABLE dbo.users ADD DEFAULT '0' FOR age ;
```
Result for postgresql:
```sql
CREATE TABLE public.roles (
       id SERIAL  not null,
       role_name varchar null
);
ALTER TABLE public.roles ADD CONSTRAINT roles_pkey PRIMARY KEY (id);

CREATE TABLE public.users (
       id SERIAL  not null,
       user_name varchar null,
       role_id integer null,
       age integer null,
       email varchar(50) null
);
ALTER TABLE public.users ADD CONSTRAINT users_pkey PRIMARY KEY (id);
ALTER TABLE public.users ADD CONSTRAINT usersrole_id_to_rolesid_fkey FOREIGN KEY(role_id) REFERENCES public.roles(id);
ALTER TABLE public.users ADD CONSTRAINT usersemail_unique UNIQUE (email);
ALTER TABLE public.users ALTER COLUMN age SET DEFAULT '0';
```
## Drop Table
```csharp
ISqlBuilder DropTable<T>(bool ifExists = false);
```
Example basic usage:
```csharp
string dropTableSQL = new SqlBuilder()
                            .DropTable<User>()
                            .UseMsSqlDialect() // or UsePostgresqlDialect()
                            .Build();
Console.WriteLine(dropTableSQL);
```
Result for SQL Server:
```sql
DROP TABLE dbo.users;
```
Result for postgresql:
```sql
DROP TABLE public.users;
```

## Create/Drop Schema
```csharp
public ISqlBuilder CreateSchema(string name);
public ISqlBuilder DropSchema(string name);
```
Example basic usage:
```csharp
string schemaSQL = new SqlBuilder()
                        .CreateSchema("SchemaTest")
                        .DropSchema("SchemaTest")
                        .UseMsSqlDialect() // or UsePostgresqlDialect()
                        .Build();

Console.WriteLine(schemaSQL);
```
Result:
```sql
CREATE SCHEMA SchemaTest;
DROP SCHEMA SchemaTest;
```
## Create/Drop Sequence
```csharp
ISqlBuilder CreateSequence(string name, string schema = null, string dbType = null, int? start = null, int? incrementBy = null, int? minvalue = null, int? maxvalue = null, bool? cycle = null);
ISqlBuilder DropSequence(string name, string schema = null);
```
Example basic usage:
```csharp
string createSequenceSQL = new SqlBuilder()
            .CreateSequence("my_sequence", schema: "my_schema", start: 10, minvalue: 0, maxvalue: 100, incrementBy: 5, cycle: true)
            .DropSequence("my_sequence", schema: "my_schema")
            .UseMsSqlDialect() // or UsePostgresqlDialect()
            .Build();
 Console.WriteLine(createSequenceSQL);
```
Result:
```sql
CREATE SEQUENCE my_schema.my_sequence START WITH 10 INCREMENT BY 5 MINVALUE 0 MAXVALUE 100 CYCLE;
DROP SEQUENCE my_schema.my_sequence;
```
