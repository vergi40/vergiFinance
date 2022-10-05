namespace vergiCommon.Input
{
    public static class Read
    {
        private const string BashSymbol = "> ";

        public static void Write(string message)
        {
            Console.WriteLine(message);
        }

        public static void LineEnd()
        {
            Console.Write(Environment.NewLine);
        }


        /// <summary>
        /// Read input stream. Either one key, or until ENTER is pressed.
        /// </summary>
        /// <param name="selectionModeOn">User selects one character instead of typing full string and ENTER.</param>
        /// <returns></returns>
        public static IInput ReadInput(bool selectionModeOn)
        {
            if (selectionModeOn)
            {
                var input = ReadInputKey();
                LineEnd();
                return input;
            }
            // User presses ENTER in the end of input
            else return ReadInputString();
        }


        private static IInput ReadInputKey()
        {
            Console.Write(InputEntry());
            var key = Console.ReadKey();
            var input = new InputKey(key);
            return input;
        }

        private static IInput ReadInputString()
        {
            Console.Write(InputEntry());
            var inputString = Console.ReadLine();
            if (inputString == null) throw new ArgumentException("Null input");
            var input = new InputString(inputString);
            return input;
        }


        public static string InputEntry(bool isAdmin = false)
        {
            if (isAdmin) return $"[ADMIN] {BashSymbol}";
            return BashSymbol;
        }
    }
}
