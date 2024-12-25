using ByteAid.Extensions.Serialization.Tests.Models;
using ByteAid.Extensions.Serialization;
using ByteAid.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace ByteAid.Extensions.Serialization.Tests;

[TestClass]
public class SeparatedTextTests
{
    private Person _testPerson = default!;
    private Guid _testGuid;

    [TestInitialize]
    public void Setup()
    {
        _testGuid = Guid.NewGuid();
        _testPerson = new Person
        {
            Name = "John Doe",
            Age = 30,
            Email = "john.doe@example.com",
            BirthDate = new DateTime(1990, 1, 1),
            IsVerified = true,
            Id = _testGuid,
            Status = Status.Active
        };
    }

    [TestMethod]
    public void TestSeparatedTextWithDifferentSeparators()
    {
        var services = new ServiceCollection();
        services.AddSerialization(builder =>
        {
            // CSV Config
            builder.ConfigureSeparatedText<Person>(rules =>
            {
                rules.HasHeaders();
                rules.Property(p => p.Name, 0, "Full Name");
                rules.Property(p => p.Age, 1, "Years Old");
                rules.Property(p => p.Email, 2);
            }, ValueSeparator.Comma);

            // PSV Config
            builder.ConfigureSeparatedText<Address>(rules =>
            {
                rules.HasHeaders();
                rules.Property(p => p.Street, 0, "Street Name");
                rules.Property(p => p.City, 1, "City Name");
                rules.Property(p => p.Country, 2);
            }, ValueSeparator.Pipe);

            // TSV Config
            builder.ConfigureSeparatedText<Order>(rules =>
            {
                rules.HasHeaders();
                rules.Property(p => p.OrderId, 0, "Order Number");
                rules.Property(p => p.Amount, 1, "Total Amount");
                rules.Property(p => p.Date, 2);
            }, ValueSeparator.Tab);
        });

        var serviceProvider = services.BuildServiceProvider();
        var helper = serviceProvider.GetRequiredService<ISerializationHelper>();

        // Test CSV format
        var csvResult = helper.SerializeAsSeparatedText(_testPerson);
        var csvLines = csvResult.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        Assert.AreEqual("Full Name,Years Old,Email", csvLines[0]);
        Assert.IsTrue(csvLines[1].StartsWith("John Doe,30,"));

        // Test PSV format
        var address = new Address { Street = "Main St", City = "Springfield", Country = "USA" };
        var psvResult = helper.SerializeAsSeparatedText(address);
        var psvLines = psvResult.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        Assert.AreEqual("Street Name|City Name|Country", psvLines[0]);
        Assert.AreEqual("Main St|Springfield|USA", psvLines[1]);

        // Test TSV format
        var order = new Order { OrderId = "123", Amount = 99.99m, Date = DateTime.Today };
        var tsvResult = helper.SerializeAsSeparatedText(order);
        var tsvLines = tsvResult.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        Assert.AreEqual("Order Number\tTotal Amount\tDate", tsvLines[0]);
        Assert.IsTrue(tsvLines[1].StartsWith("123\t99.99\t"));
    }

    [TestMethod]
    public void TestSeparatedTextCollectionSerialization()
    {
        var services = new ServiceCollection();
        services.AddSerialization(builder =>
        {
            builder.ConfigureSeparatedText<Person>(rules =>
            {
                rules.HasHeaders();
                rules.Property(p => p.Name, 0, "Full Name");
                rules.Property(p => p.Age, 1, "Years Old");
                rules.Property(p => p.Email, 2, "E-mail");
            }, ValueSeparator.Comma);
        });

        var people = new List<Person>
    {
        _testPerson,
        new() {
            Name = "Jane Smith",
            Age = 25,
            Email = "jane@example.com"
        }
    };

        var serviceProvider = services.BuildServiceProvider();
        var helper = serviceProvider.GetRequiredService<ISerializationHelper>();

        var result = helper.SerializeAsSeparatedText(people);
        var lines = result.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

        Assert.AreEqual(3, lines.Length); // Header + 2 records
        Assert.AreEqual("Full Name,Years Old,E-mail", lines[0]);
        Assert.IsTrue(lines[1].StartsWith("John Doe,30,"));
        Assert.IsTrue(lines[2].StartsWith("Jane Smith,25,"));
    }

    [TestMethod]
    public void TestSeparatedTextDeserialization()
    {
        var services = new ServiceCollection();
        services.AddSerialization(builder =>
        {
            builder.ConfigureSeparatedText<Person>(rules =>
            {
                rules.HasHeaders();
                rules.Property(p => p.Name, 0);
                rules.Property(p => p.Age, 1);
                rules.Property(p => p.Email, 2);
            }, ValueSeparator.Comma);
        });

        var serviceProvider = services.BuildServiceProvider();
        var helper = serviceProvider.GetRequiredService<ISerializationHelper>();

        var input = "Name,Age,Email\nJohn Doe,30,john@example.com";
        var person = helper.DeserializeFromSeparatedText<Person>(input);

        Assert.IsNotNull(person);
        Assert.AreEqual("John Doe", person.Name);
        Assert.AreEqual(30, person.Age);
        Assert.AreEqual("john@example.com", person.Email);
    }

    [TestMethod]
    public void TestSeparatedTextCollectionDeserialization()
    {
        var services = new ServiceCollection();
        services.AddSerialization(builder =>
        {
            builder.ConfigureSeparatedText<Person>(rules =>
            {
                rules.HasHeaders();
                rules.Property(p => p.Name, 0);
                rules.Property(p => p.Age, 1);
                rules.Property(p => p.Email, 2);
            }, ValueSeparator.Comma);
        });

        var serviceProvider = services.BuildServiceProvider();
        var helper = serviceProvider.GetRequiredService<ISerializationHelper>();

        var input = "Name,Age,Email\nJohn Doe,30,john@example.com\nJane Smith,25,jane@example.com";
        var lines = input.Split('\n');
        var people = helper.DeserializeFromSeparatedText<Person>(lines);

        Assert.IsNotNull(people);
        Assert.AreEqual(2, people.Count);

        Assert.AreEqual("John Doe", people[0].Name);
        Assert.AreEqual(30, people[0].Age);
        Assert.AreEqual("john@example.com", people[0].Email);

        Assert.AreEqual("Jane Smith", people[1].Name);
        Assert.AreEqual(25, people[1].Age);
        Assert.AreEqual("jane@example.com", people[1].Email);
    }

    [TestMethod]
    public void TestGetHeaderLine()
    {
        var services = new ServiceCollection();
        services.AddSerialization(builder =>
        {
            // CSV Config
            builder.ConfigureSeparatedText<Person>(rules =>
            {
                rules.HasHeaders();
                rules.Property(p => p.Name, 0, "Full Name");
                rules.Property(p => p.Age, 1, "Years Old");
                rules.Property(p => p.Email, 2, "E-mail");
            }, ValueSeparator.Comma);

            // TSV Config
            builder.ConfigureSeparatedText<Order>(rules =>
            {
                rules.HasHeaders();
                rules.Property(p => p.OrderId, 0, "Order Number");
                rules.Property(p => p.Amount, 1, "Total Amount");
                rules.Property(p => p.Date, 2, "Order Date");
            }, ValueSeparator.Tab);
        });

        var serviceProvider = services.BuildServiceProvider();
        var helper = serviceProvider.GetRequiredService<ISerializationHelper>();

        // Test CSV format
        var csvHeaders = helper.GetHeaderLine<Person>();
        Assert.AreEqual("Full Name,Years Old,E-mail", csvHeaders);

        // Test TSV format
        var tsvHeaders = helper.GetHeaderLine<Order>();
        Assert.AreEqual("Order Number\tTotal Amount\tOrder Date", tsvHeaders);
    }


    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void TestGetHeaderLine_ThrowsWhenTypeNotConfigured()
    {
        var services = new ServiceCollection();
        services.AddSerialization(builder =>
        {
            // No configuramos ningún tipo
        });

        var serviceProvider = services.BuildServiceProvider();
        var helper = serviceProvider.GetRequiredService<ISerializationHelper>();

        helper.GetHeaderLine<Person>();
    }
}