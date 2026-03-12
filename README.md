# Homura

[![NuGet](https://img.shields.io/nuget/v/Homura.svg)](https://www.nuget.org/packages/Homura/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

Homura is a C# ORM (Object-Relational Mapping) library for SQLite, featuring built-in migration support and Source Generator integration.

## Features

- **Entity-based table definition** — Define tables as C# classes with attribute-driven column mapping
- **DAO (Data Access Object) pattern** — Type-safe database access via `Dao<T>` base class
- **Migration system** — Version-controlled schema evolution with `ChangePlan` and automatic table upgrades
- **Source Generator** — Auto-generate DAO and ChangePlan classes with `[GenerateDao]` and `[GenerateChangePlan]` attributes
- **Query Builder** — Fluent SQL query construction with ISO DML support

## Requirements

- .NET 9.0 or .NET 10.0

## Installation

```shell
dotnet add package Homura
```

Or via Package Manager Console:

```powershell
PM> Install-Package Homura
```

## Quick Start

### 1. Define an Entity Class

Define your table schema as a class inheriting from `EntityBaseObject`. Use attributes to specify columns, primary keys, and indexes.

```csharp
using Homura.ORM;
using Homura.ORM.Mapping;

[DefaultVersion(typeof(VersionOrigin))]
public class Book : EntityBaseObject
{
    [Column("Id", "NUMERIC", 0), PrimaryKey, Index]
    public Guid Id { get; set; }

    [Column("Title", "TEXT", 1)]
    public string Title { get; set; }

    [Column("Author", "TEXT", 2)]
    public string Author { get; set; }
}
```

### 2. Create a DAO Class

Create a data access object by inheriting from `Dao<T>` (or `SQLiteBaseDao<T>` for SQLite).

```csharp
public class BookDao : Dao<Book>
{
    public BookDao() : base() { }

    public BookDao(Type entityVersionType) : base(entityVersionType) { }
}
```

### 3. Use Source Generator (Optional)

Instead of manually writing DAO and ChangePlan classes, use attributes to auto-generate them:

```csharp
[GenerateDao]
[GenerateChangePlan(typeof(VersionOrigin))]
[DefaultVersion(typeof(VersionOrigin))]
public class Book : EntityBaseObject
{
    [Column("Id", "NUMERIC", 0), PrimaryKey, Index]
    public Guid Id { get; set; }

    [Column("Title", "TEXT", 1)]
    [Since(typeof(VersionOrigin))]
    public string Title { get; set; }
}
```

### 4. Migration

Define a `ChangePlan` to manage schema versions:

```csharp
public class BookChangePlan_VersionOrigin : ChangePlan<Book, VersionOrigin>
{
    public BookChangePlan_VersionOrigin(VersioningMode mode)
        : base("Book", PostMigrationVerification.TableExists, mode) { }

    public override void UpgradeToTargetVersion(IConnection connection)
    {
        CreateTable(connection);
    }
}
```

## Key Attributes

| Attribute | Description |
|---|---|
| `[Column(name, type, order)]` | Define a column with name, SQL type, and ordinal position |
| `[PrimaryKey]` | Mark a property as the primary key |
| `[Index]` | Create an index on the column |
| `[NotNull]` | Add a NOT NULL constraint |
| `[DefaultVersion(type)]` | Specify the default version for the entity |
| `[Since(type)]` | Indicate which version introduced the column |
| `[Until(type)]` | Indicate which version removed the column |
| `[GenerateDao]` | Auto-generate a DAO class via Source Generator |
| `[GenerateChangePlan]` | Auto-generate a ChangePlan class via Source Generator |

## Project Structure

```
Homura/                    # Main ORM library
Homura.SourceGenerator/    # Source Generator for DAO and ChangePlan auto-generation
Homura.Test/               # Unit tests
```

## License

[MIT License](LICENSE)

