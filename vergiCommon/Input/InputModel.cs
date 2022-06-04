namespace vergiCommon.Input
{
    public interface IInput
    {
        /// <summary>
        /// Given input means user wants to exit current loop
        /// </summary>
        bool Exit { get; set; }
        bool ElevateToAdmin { get; set; }
        string InputAsString { get; set; }
        bool Equals(string text);
    }

    class InputKey : IInput
    {
        public bool Exit { get; set; }
        public bool ElevateToAdmin { get; set; }
        public string InputAsString { get; set; }
        public bool Equals(string text)
        {
            return text.Equals(InputAsString, StringComparison.CurrentCultureIgnoreCase);
        }

        public InputKey(ConsoleKeyInfo input)
        {
            Exit = IsExitInput(input);
            InputAsString = input.KeyChar.ToString();
            bool success = int.TryParse(input.KeyChar.ToString(), out var intValue);
            if (success) InputAsString = intValue.ToString();
        }

        private bool IsExitInput(ConsoleKeyInfo input)
        {
            if (input.Key == ConsoleKey.Escape
                || input.Key == ConsoleKey.Q
                || input.Key == ConsoleKey.E)
            {
                return true;
            }

            return false;
        }
    }

    class InputString : IInput
    {
        public bool Exit { get; set; }
        public bool ElevateToAdmin { get; set; }
        public string InputAsString { get; set; }
        public bool Equals(string text)
        {
            return text.Equals(InputAsString, StringComparison.CurrentCultureIgnoreCase);
        }

        public InputString(string input)
        {
            Exit = IsExitInput(input);
            ElevateToAdmin = IsAdminCommand(input);
            InputAsString = input;
        }

        private bool IsAdminCommand(string input)
        {
            if (input.Equals("admin", StringComparison.CurrentCulture)) return true;
            return false;
        }

        private bool IsExitInput(string input)
        {
            if (input.Equals("q", StringComparison.CurrentCultureIgnoreCase)) return true;
            if (input.Equals("exit", StringComparison.CurrentCultureIgnoreCase)) return true;
            if (input.Equals("e", StringComparison.CurrentCultureIgnoreCase)) return true;
            if (input.Equals("quit", StringComparison.CurrentCultureIgnoreCase)) return true;
            return false;
        }
    }
}
