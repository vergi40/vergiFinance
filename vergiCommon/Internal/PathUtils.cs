using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vergiCommon.Internal
{
    internal class PathUtils
    {
        /// <returns>Return folder path where file was found</returns>
        public static string TravelParentsUntilFileFound(string fileName, string startFolder)
        {
            var current = Directory.GetParent(startFolder);
            for (int i = 0; i < 10; i++)
            {
                if (current == null) break;

                if (current.EnumerateFiles().Any(x => x.Name.Equals(fileName)))
                {
                    return current.FullName;
                }

                current = current.Parent;
            }

            throw new ArgumentException($"Could not find file traveling the folder tree. File name: {fileName}");
        }

        /// <returns>Return folder path where file was found</returns>
        public static string TravelParentsUntilSlnFileFound(string startFolder)
        {
            var current = Directory.GetParent(startFolder);
            for (int i = 0; i < 10; i++)
            {
                if (current == null) break;

                if (current.GetFiles("*.sln").Any())
                {
                    return current.FullName;
                }

                current = current.Parent;
            }

            throw new ArgumentException($"Could not find sln file traveling the folder tree.");
        }
    }
}
