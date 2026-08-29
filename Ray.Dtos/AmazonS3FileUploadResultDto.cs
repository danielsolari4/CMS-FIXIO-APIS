using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ray.Dtos
{
    public class AmazonS3FileUploadResultDto
    {
        public bool Error { get; set; }
        public string FileUrl { get; set; }
    }
}
