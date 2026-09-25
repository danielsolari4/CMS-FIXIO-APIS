using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rino.Utils.Backload
{
    public class SessionChunk
    {
        public string ThumbnailFileName { get; set; }
        public string FileName { get; set; }
        public int Total { get; set; }
        public int UploadedBytes { get; set; }
        public string SessionId { get; set; }
        public bool IsChunkedUpload { get; set; }
        public int CurrentPartNumber { get; set; }
        public string FileId { get; set; }

        public string GetPartNumber()
            => $".partNumber-{CurrentPartNumber}";
    }
}
