using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using ConsoleAppFramework;
using CsUtils.Models;

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
    /// <param name="outputType">--type, Control the information to print. Full|RelativePath|FullPath|Name</param>
    /// <param name="onlyTests">--tests, Include only test projects.</param>
    /// <param name="targetFramework">--tfm, Filter result to e.g. only contain "net8" projects.</param>
    /// <param name="outputFile">-o, Write output to file.</param>
    /// <param name="verbose">-v, </param>
    [Command("print")]
    public int PrintPaths([Argument]string slnPath, OutputType outputType = OutputType.Full, bool onlyTests = false, 
        string? targetFramework = null, string? outputFile = null, bool verbose = false)
    {
        var parser = new SlnParser(slnPath);
        if (!parser.Validate())
        {
            return 1;
        }

        var projectInfos = parser.Parse();
        if(verbose) Console.WriteLine($"Solution parsed. Found {projectInfos.Count} projects.");

        var filter = new ProjectFilters();
        projectInfos = filter.Filter(projectInfos, onlyTests, targetFramework);
        if(verbose && (onlyTests || targetFramework != null))
        {
            Console.WriteLine($"Filtered projects with (onlyTests={onlyTests}, targetFramework={targetFramework}). Remaining: {projectInfos.Count}");
        }

        if(verbose) Console.WriteLine();
        PrintBasedOnType(projectInfos, outputType);
        return 0;
    }

    private static void PrintBasedOnType(IReadOnlyList<ProjectInfo> projectInfos, OutputType outputType)
    {
        switch (outputType)
        {
            case OutputType.Full:
                foreach (var info in projectInfos.OrderBy(i => i.FullPath))
                {
                    Console.WriteLine(info);
                }
                break;
            case OutputType.RelativePath:
                foreach (var info in projectInfos.OrderBy(i => i.RelativePath))
                {
                    Console.WriteLine(info.RelativePath);
                }
                break;
            case OutputType.FullPath:
                foreach (var info in projectInfos.OrderBy(i => i.FullPath))
                {
                    Console.WriteLine(info.FullPath);
                }
                break;
            case OutputType.Name:
                foreach (var info in projectInfos.OrderBy(i => i.Name))
                {
                    Console.WriteLine(info.Name);
                }
                break;
        }
    }
}

public enum OutputType
{
    /// <summary>
    /// Print records with all information
    /// </summary>
    Full,
    /// <summary>
    /// Print relative path to projects
    /// </summary>
    RelativePath,
    /// <summary>
    /// Print full path to projects
    /// </summary>
    FullPath,
    /// <summary>
    /// Print project names
    /// </summary>
    Name
}