namespace NewClass;

public class Person(int age, string name, Gender gender, string address)
{
    private int Age => age;
    private string Name => name;
    private Gender Gender => gender;
    private string Address => address;

    // public Person(int age, string name, Gender gender, string address)
    // {
    //     this.Age = age;
    //     this.Name = name;
    //     this.Gender = gender;
    //     this.Address = address;
    // }

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
