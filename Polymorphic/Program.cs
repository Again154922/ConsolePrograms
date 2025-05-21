namespace Polymorphic;

public static class Polymorphic2
{
    public static void Main(string[] args)
    {
        Document pdf = new Pdf("Pdf.pdf");
        Document word = new Word("Word.docx");
        
        List<Document> documents = new List<Document>();
        documents.Add(pdf);
        documents.Add(word);

        foreach (Document doc in documents)
            doc.Show();
    }
}
