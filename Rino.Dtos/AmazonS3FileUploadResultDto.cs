using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rino.Dtos
{
    public class AmazonS3FileUploadResultDto
    {
        public bool Error { get; set; }
        public string FileUrl { get; set; }
        public string Message { get; set; }
    }
}
