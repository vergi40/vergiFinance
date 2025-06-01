namespace CsUtils;

public class SlnParser
{
    private readonly string _slnFilePath;

    public SlnParser(string slnFilePath)
    {
        _slnFilePath = slnFilePath;
    }


    public bool Validate()
    {
        if (!File.Exists(_slnFilePath))
        {
            Console.WriteLine("File does not exist in given path. Remember to add hyphens \"<full path>\".");
            return false;
        }

        if (Path.GetExtension(_slnFilePath) != ".sln")
        {
            Console.WriteLine("Given file is not of type '.sln'");
            return false;
        }

        return true;
    }

    public string Parse()
    {
        if (!File.Exists(_slnFilePath))
        {
            throw new FileNotFoundException($"Solution file not found: {_slnFilePath}");
        }

        var lines = File.ReadAllLines(_slnFilePath).ToList();

        // Microsoft Visual Studio Solution File, Format Version 12.00
        // # Visual Studio Version 17
        // VisualStudioVersion = 17.2.32519.379
        // MinimumVisualStudioVersion = 10.0.40219.1
        // Project("{9A19103F-16F7-4668-BE54-9A1E7A4F7556}") = "vergiFinance", "vergiFinance\vergiFinance.csproj", "{C1CD129C-27BE-4FA1-BEBA-493292174B61}"
        // EndProject
        // Project("{9A19103F-16F7-4668-BE54-9A1E7A4F7556}") = "Terminal", "Terminal\Terminal.csproj", "{73A84E27-75DA-4719-9528-D92E74C01FFB}"
        // EndProject
        // Project("{9A19103F-16F7-4668-BE54-9A1E7A4F7556}") = "vergiCommon", "vergiCommon\vergiCommon.csproj", "{30690911-23AE-4033-A0D1-D72811DE4DC9}"
        // EndProject
        // Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "vergiFinance.UnitTests", "vergiFinance.UnitTests\vergiFinance.UnitTests.csproj", "{BE3581D1-F937-41BD-84F0-7FD3E94AAA46}"
        // EndProject
        // Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "CsUtils", "CsUtils\CsUtils.csproj", "{DDC0AEE4-F464-4BD4-A2CF-2D5F2E762CCB}"
        // EndProject
        // Global

        var projectListing = ParseProjects(lines);
        var infos = new List<ProjectInfo>();
        var directory = Path.GetDirectoryName(_slnFilePath) ?? throw new ArgumentException();
        foreach (var (name, relativePath) in projectListing)
        {
            var fullPath = Path.Combine(directory, relativePath);
            infos.Add(new ProjectInfo(name, relativePath, fullPath));
        }

        return string.Join("\n", infos);
    }

    private static List<ProjectNameAndPath> ParseProjects(List<string> lines)
    {
        var projects = new List<ProjectNameAndPath>();
        var projectLineRegex = new System.Text.RegularExpressions.Regex(
            @"^Project\(""\{[^""]+\}""\)\s*=\s*""([^""]+)"",\s*""([^""]+)""",
            System.Text.RegularExpressions.RegexOptions.Compiled);

        foreach (var line in lines)
        {
            var match = projectLineRegex.Match(line);
            if (match.Success)
            {
                var name = match.Groups[1].Value;
                var path = match.Groups[2].Value;
                projects.Add(new ProjectNameAndPath(name, path));
            }
        }
        return projects;
    }

    private sealed record ProjectNameAndPath(string Name, string Path);
    private sealed record ProjectInfo(string Name, string RelativePath, string FullPath);
}