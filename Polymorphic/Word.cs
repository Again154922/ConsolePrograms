namespace Polymorphic;

public class Word(string fileName) : Document(fileName)
{
    public override void Show()
    {
        Console.WriteLine("Show Word contents...");
    }
}
