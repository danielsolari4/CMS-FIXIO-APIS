using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rino.Utils.Backload
{
    public class ChunkResult
    {
        public bool Error { get; set; }
        public string Message { get; set; }
        public string FilePath { get; set; }
    }
}
