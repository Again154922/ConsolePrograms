namespace Generic
{
    public static class Generic
    {
        public static void Main(string[] args)
        {
            Texts<int> textInt = new Texts<int>(1000);
            Texts<string> textString = new Texts<string>("hello world");
            textInt.ShowText();
            textInt.SetText(2000);
            Console.WriteLine(textInt.GetText());
        }
    }

    class Texts<T>(T text)
    {
        private T Text { get; set; } = text;

        public void ShowText()
        {
            Console.WriteLine(Text);
        }

        public void SetText(T text)
        {
            Text = text;
        }

        public T GetText()
        {
            return Text;
        }
    }
}

