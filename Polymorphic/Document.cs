namespace Polymorphic;

public abstract class Document(string fileName)
{
    private string FileName => fileName;

    public abstract void Show();
}
