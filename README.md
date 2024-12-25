# ByteAid.Extensions.Serialization

A flexible, dependency injection-friendly serialization library for .NET that simplifies the process of implementing custom serialization rules for different data formats. Currently supports CSV, TSV, and PSV (pipe-separated values) with configurable headers and separators.

## Features

- Strong typing support
- Configurable through dependency injection
- Support for headers and custom column names
- Multiple separator types (comma, tab, pipe)
- Built with nullable reference types
- Support for both .NET Standard 2.1 and .NET 8.0
- Clean separation of abstractions and implementations

## Installation

You can install the package via NuGet:

```bash
# Core package
dotnet add package ByteAid.Extensions.Serialization

# Abstractions only
dotnet add package ByteAid.Extensions.Serialization.Abstractions

# Separated text (CSV, TSV, PSV) support
dotnet add package ByteAid.Extensions.Serialization.SeparatedText
```

## Quick Start

### 1. Define your model

```csharp
public class Person
{
    public string Name { get; set; } = default!;
    public int Age { get; set; }
    public string Email { get; set; } = default!;
}
```

### 2. Configure services

```csharp
services.AddSerialization(builder =>
{
    builder.ConfigureSeparatedText<Person>(rules =>
    {
        rules.HasHeaders();
        rules.Property(p => p.Name, 0, "Full Name");
        rules.Property(p => p.Age, 1, "Years");
        rules.Property(p => p.Email, 2, "Email Address");
    }, ValueSeparator.Comma);
});
```

### 3. Use the serialization helper

```csharp
public class PersonService
{
    private readonly ISerializationHelper _serializationHelper;

    public PersonService(ISerializationHelper serializationHelper)
    {
        _serializationHelper = serializationHelper;
    }

    public string SerializePeople(IEnumerable<Person> people)
    {
        return _serializationHelper.SerializeAsSeparatedText(people);
    }

    public Person DeserializePerson(string input)
    {
        return _serializationHelper.DeserializeFromSeparatedText<Person>(input);
    }
}
```

## Advanced Usage

### Different Separator Types

The library supports three types of separators:

```csharp
// CSV (Comma-Separated Values)
builder.ConfigureSeparatedText<Person>(rules => { ... }, ValueSeparator.Comma);

// TSV (Tab-Separated Values)
builder.ConfigureSeparatedText<Order>(rules => { ... }, ValueSeparator.Tab);

// PSV (Pipe-Separated Values)
builder.ConfigureSeparatedText<Address>(rules => { ... }, ValueSeparator.Pipe);
```

### Custom Header Names

You can specify custom header names for your properties:

```csharp
builder.ConfigureSeparatedText<Person>(rules =>
{
    rules.HasHeaders();
    rules.Property(p => p.Name, 0, "Full Name");
    rules.Property(p => p.BirthDate, 1, "Date of Birth");
    rules.Property(p => p.Email, 2, "Contact Email");
}, ValueSeparator.Comma);
```

### Working with Headers

You can get header information programmatically:

```csharp
// Get header line with separator
string headerLine = _serializationHelper.GetHeaderLine<Person>();

// Get headers as array
string[] headers = _serializationHelper.GetHeadersArray<Person>();
```

## Type Support

The library supports a wide range of .NET types for serialization/deserialization:

- Basic types (string, int, long, decimal, etc.)
- DateTime and DateTimeOffset
- TimeSpan
- Guid
- Enums
- Nullable value types
- Boolean (with support for various formats)
- Uri
- Version
- IPAddress
- Base64-encoded byte arrays

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

### Development Prerequisites

- .NET SDK 8.0 or later
- An IDE that supports .NET (Visual Studio, VS Code, Rider)

### Building the Project

```bash
dotnet build
```

### Running Tests

```bash
dotnet test
```

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE.md) file for details.

## Support

If you encounter any problems or have suggestions, please [open an issue](https://github.com/byteaid/ByteAid.Extensions.Serialization/issues/new) on GitHub.