namespace ByteAid.Extensions.Serialization.Tests.Models;

public class Person
{
    public string? Name { get; set; }
    public int Age { get; set; }
    public string? Email { get; set; }
    public DateTime BirthDate { get; set; }
    public bool IsVerified { get; set; }
    public Guid Id { get; set; }
    public Status Status { get; set; }
}
