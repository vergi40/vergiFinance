namespace vergiCommon.Input
{
    internal class Read
    {
        private const string BashSymbol = "> ";

        public static void Write(string message)
        {
            Console.WriteLine(message);
        }

        public void LineEnd()
        {
            Console.Write(Environment.NewLine);
        }

        public IInput ReadInputKey()
        {
            Console.Write(InputEntry());
            var key = Console.ReadKey();
            var input = new InputKey(key);
            return input;
        }

        public IInput ReadInputString()
        {
            Console.Write(InputEntry());
            var inputString = Console.ReadLine();
            if (inputString == null) throw new ArgumentException("Null input");
            var input = new InputString(inputString);
            return input;
        }


        public string InputEntry(bool isAdmin = false)
        {
            if (isAdmin) return $"[ADMIN] {BashSymbol}";
            return BashSymbol;
        }
    }
}
