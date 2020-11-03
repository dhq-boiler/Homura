# Homura

Homura is an Object-relational mapping library in C# made to make it easy to access the database.

## Concepts

Homura has 3 basic concepts.

1. Tables in database

2. Entity classes

3. Data access object classes

### Tables in database

A table that exists in a relational database. The table name consists of the entity class name described later and the version number. All table components (table name, columns) are defined in the entity class.

### Entity classes

Define the components of the table as classes. Implement the table columns as properties. For `Column` attribute, specify the column name and data type, and order of the column. You can also set the primary key (`PrimaryKey` attribute) and index (`Index` attribute) at the same time.

### Data access object classes

We will make full use of the Data access object class created by inheriting from Dao\<T\>.

Dao\<T\> has a very complicated structure, so I will publish the introduction at a later date.

Basically, create a data access object class `AlphaDao` that inherits Dao\<`Alpha`\>, that handles the entity class `Alpha`.

## Requirements

* .net Framework v4.6.2 or later

## Installation

1. Select "Manage NuGet Packages..." at project's References in Solution Explorer(Visual Studio 2019).

2. Select "Browse" tab and fill in "Homura" on search textbox.

3. Find "Homura" library and push Install button.

OR

By Powershell

```powershell
PM> Install-Package Homura -Version 1.0.0
```

