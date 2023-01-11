using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vergiCommon.Public
{
    public interface ICsvFile : IFile
    {
        /// <summary>
        /// Has header that defines each column
        /// </summary>
        bool HasHeaderDefinition { get; }
        IReadOnlyList<string> Headers { get; }
        IReadOnlyList<IReadOnlyList<string>> Data { get; }

        string Separator { get; }
    }
}
