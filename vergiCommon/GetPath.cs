using System.Reflection;
using vergiCommon.Internal;

namespace vergiCommon
{
    /// <summary>
    /// General filesystem paths
    /// </summary>
    public static class GetPath
    {
        /// <summary>
        /// Folder path to my documents
        /// </summary>
        /// <returns></returns>
        public static string MyDocuments()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        }

        /// <summary>
        /// Folder path to given subfolder in my documents
        /// </summary>
        /// <param name="folderName"></param>
        /// <returns></returns>
        public static string MyDocumentsSubFolder(string folderName)
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), folderName);
        }

        /// <summary>
        /// Folder path to desktop
        /// </summary>
        /// <returns></returns>
        public static string Desktop()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        }

        /// <summary>
        /// Path to folder where this code is executed, e.g. /bin/Release/net6.0/
        /// </summary>
        /// <returns>Bin folder path of the calling program</returns>
        public static string ThisAssembly()
        {
            return Environment.CurrentDirectory;
            //return Assembly.GetExecutingAssembly().
        }

        /// <summary>
        /// Path to folder where this code's project file (.csproj) exists.
        /// Travels parent folders until file found.
        /// </summary>
        /// <returns>Project folder path of the calling program</returns>
        public static string ThisProject()
        {
            var callingAssembly = Assembly.GetCallingAssembly();
            var projectName = callingAssembly.GetName().Name + ".csproj";

            var projectFolderPath = PathUtils.TravelParentsUntilFileFound(projectName, ThisAssembly());
            return projectFolderPath;
        }

        /// <summary>
        /// Path to folder where this code's solution file (.sln) exists.
        /// Travels parent folders until file found.
        /// </summary>
        /// <returns>Solution folder path of the calling program</returns>
        public static string ThisSolution()
        {
            var callingAssembly = Assembly.GetCallingAssembly();
            var projectName = callingAssembly.GetName().Name + ".csproj";

            var projectFolderPath = PathUtils.TravelParentsUntilFileFound(projectName, ThisAssembly());
            var solutionFolderPath = PathUtils.TravelParentsUntilSlnFileFound(projectFolderPath);

            return solutionFolderPath;
        }
    }
}
