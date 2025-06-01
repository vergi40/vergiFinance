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
    /// <summary>Display message - test.</summary>
    /// <param name="msg">-m, Message to show.</param>
    public void Echo(string msg) => Console.WriteLine(msg);

    /// <summary>
    /// Print all project file (.csproj) paths from solution file (.sln).
    /// </summary>
    /// <param name="slnPath">Full path to solution file (.sln)</param>
    /// <param name="onlyTestProjects">-t, Only include test project paths.</param>
    /// <param name="outputFile">-o, Write output to file.</param>
    [Command("print")]
    public int PrintPaths([Argument]string slnPath, bool onlyTestProjects = false, string? outputFile = null)
    {
        var parser = new SlnParser(slnPath);
        if (!parser.Validate())
        {
            return 1;
        }
        Console.WriteLine("All good for now");

        var result = parser.Parse();
        Console.WriteLine(result);

        return 0;
    }
}