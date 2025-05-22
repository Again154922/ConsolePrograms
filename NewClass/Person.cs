namespace NewClass;

public class Person(int age, string name, Gender gender, string address)
{
    private int Age { get; } = age;
    private string Name { get; } = name;
    private Gender Gender { get; } = gender;
    private string Address { get; } = address;

    public void Introduction()
    {
        Console.WriteLine($"My name is {Name} and I am {Age} years old. " +
                          $"I am {Gender}, my address is {Address}.");
    }
}

public enum Gender
{
    Male,
    Female
}
