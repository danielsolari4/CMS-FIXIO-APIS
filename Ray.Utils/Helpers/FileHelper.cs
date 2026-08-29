using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ray.Utils.Helpers
{
    public static class FileHelper
    {
        //public static Bitmap GetImageInfo(string path)
        //{
        //    Bitmap img = new Bitmap(path);
        //    return img;
        //}

        public static long GetFileSize(string path)
        {
            FileInfo info = new FileInfo(path);
            return info.Length;
        }
    }
}
