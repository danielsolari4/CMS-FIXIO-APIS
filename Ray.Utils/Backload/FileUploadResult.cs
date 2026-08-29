using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ray.Utils.Backload
{
    public class FileUploadResult
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Url { get; set; }
        public Stream Stream { get; set; }
        public string DeleteUrl { get; set; }
        public string ThumbnailUrl { get; set; }
        public string Size1Path { get; set; }
        public string Size2Path { get; set; }
        public string Size3Path { get; set; }
        public string Size4Path { get; set; }
        public string Size5Path { get; set; }
        public string Size6Path { get; set; }
        public bool IsChunkingUploads { get; set; }
        public bool UploadFinished { get; set; }
        public bool Error { get; set; }
        public string Message { get; set; }
        public int Id { get; set; }
    }
}
