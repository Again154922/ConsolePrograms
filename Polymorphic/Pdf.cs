namespace Polymorphic;

public class Pdf(string fileName) : Document(fileName)
{
    public override void Show()
    {
        Console.WriteLine("Show PDF contents...");
    }
}
