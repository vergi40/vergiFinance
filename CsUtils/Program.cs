using ConsoleAppFramework;

namespace CsUtils;

internal class Program
{
    static void Main(string[] args)
    {
        // https://github.com/Cysharp/ConsoleAppFramework
        var app = ConsoleApp.Create();
        app.Add<Commands>();
        app.Run(args);
    }
}

public class Commands
{
    /// <summary>Display message.</summary>
    /// <param name="msg">Message to show.</param>
    public void Echo(string msg) => Console.WriteLine(msg);
}