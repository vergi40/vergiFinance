using System.Xml.Linq;
using CsUtils.Models;

namespace CsUtils;

internal class ProjectFilters
{
    public ProjectFilters()
    {
    }

    public IReadOnlyList<ProjectInfo> Filter(IReadOnlyList<ProjectInfo> infos, bool onlyTests, string? targetFramework)
    {
        var result = infos.ToList();
        if (onlyTests)
        {
            result = infos.Where(info =>
                info.Name.EndsWith("Tests", StringComparison.OrdinalIgnoreCase) ||
                info.Name.EndsWith("Test", StringComparison.OrdinalIgnoreCase)
            ).ToList();
        }

        if (targetFramework != null)
        {
            var filtered = new List<ProjectInfo>();
            foreach (var projectInfo in result)
            {
                try
                {
                    if (!File.Exists(projectInfo.FullPath))
                        continue;

                    var doc = XDocument.Load(projectInfo.FullPath);
                    var tfElement = doc.Descendants("TargetFramework").FirstOrDefault();
                    if (tfElement != null)
                    {
                        // Match if the TargetFramework contains the targetFramework string (e.g. "net8")
                        if (tfElement.Value.Contains(targetFramework, StringComparison.OrdinalIgnoreCase))
                        {
                            filtered.Add(projectInfo);
                        }
                    }
                    else
                    {
                        // Handle TargetFrameworks (plural, multi-targeting)
                        var tfsElement = doc.Descendants("TargetFrameworks").FirstOrDefault();
                        if (tfsElement != null)
                        {
                            var frameworks = tfsElement.Value.Split(';', StringSplitOptions.RemoveEmptyEntries);
                            if (frameworks.Any(fw => fw.Contains(targetFramework, StringComparison.OrdinalIgnoreCase)))
                            {
                                filtered.Add(projectInfo);
                            }
                        }
                    }
                }
                catch
                {
                    // Ignore projects that can't be loaded or parsed
                }
            }
            result = filtered;
        }

        return result;
    }
}
