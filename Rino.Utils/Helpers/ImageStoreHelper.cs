using System;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Http;
using Rino.Dtos;
using Rino.Dtos.Configuration;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using Rino.Utils.Exception;
using System.Threading.Tasks;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using Microsoft.Extensions.FileProviders;


namespace Rino.Utils.Helpers
{
    public static class ImageStoreHelper
    {
        public static async Task<string> UpdateUserProfileImage(string profileImagePath, IFormFile file, MediaSettings mediaSettings)
        {
            if (file == null)
                throw new System.Exception("PIEX_001");

            var storeFolderName = mediaSettings.Profile.FolderName;

            if (string.IsNullOrWhiteSpace(storeFolderName))
                throw new ConfigurationException("PIEX_002");//profile picture folder name

            var relativePath = mediaSettings.Profile.FolderPath;
            var profileDir = Path.Combine(relativePath, storeFolderName);

            if (!Directory.Exists(profileDir))
                Directory.CreateDirectory(profileDir);

            if (!string.IsNullOrWhiteSpace(file.ContentType) && mediaSettings.Profile.ContentTypeAllowed != null && !mediaSettings.Profile.ContentTypeAllowed.Contains(file.ContentType.Replace("image/", string.Empty)))
                throw new System.Exception("PIEX_003");//profile picture content type

            if (mediaSettings.Profile.MaxContentLength < file.Length)
                throw new System.Exception("PIEX_004-" + ((int)mediaSettings.Profile.MaxContentLength / 1000000) + "MB"); //profile picture content size

            var fileName = string.Format("{0}{1}", Guid.NewGuid(), Path.GetExtension(file.FileName));
            string filePath = Path.Combine(relativePath, storeFolderName, fileName);

            using (Stream fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }


            // deletes old profile image path
            if (!string.IsNullOrWhiteSpace(profileImagePath) && File.Exists(Path.Combine(relativePath, profileImagePath)))
                File.Delete(Path.Combine(relativePath, profileImagePath));

            return string.Format("{0}/{1}", storeFolderName, fileName);
        }

        public static bool IsImage(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return false;

            var extension = Path.GetExtension(filePath).TrimStart('.').ToLowerInvariant();
            return extension == "png" || extension == "jpeg" || extension == "jpg"
                || extension == "gif" || extension == "tiff" || extension == "tif" || extension == "bmp";
        }

        public static bool IsValidFile(IFormFile file, MediaSettings mediaSettings)
        {
            if (file == null || file.Length == 0)
                return false;

            if (mediaSettings.FileUpload.FileSizeBytes > 0 && file.Length > mediaSettings.FileUpload.FileSizeBytes)
                return false;

            var allowed = mediaSettings.FileUpload.ContentTypeAllowed;
            if (string.IsNullOrWhiteSpace(allowed))
                return false;

            var extension = Path.GetExtension(file.FileName).TrimStart('.').ToLowerInvariant();
            return allowed
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim().TrimStart('.').ToLowerInvariant())
                .Contains(extension);
        }

        public static string GenerateThumbnail(string path, MediaSettings mediaSettings)
        {
            try
            {
                var thumbsFolderPath = Path.Combine(mediaSettings.Sizes.FolderPath, mediaSettings.Sizes.ThumbsFileFolder);
                var thumbFinalPath = Path.Combine(thumbsFolderPath, Path.GetFileName(path));

                using (SixLabors.ImageSharp.Image image = SixLabors.ImageSharp.Image.Load(path))
                {
                    image.Mutate(x => x.Resize(80, 60));
                    Directory.CreateDirectory(thumbsFolderPath);
                    image.Save(thumbFinalPath);
                }
                return thumbFinalPath;
            }
            catch (System.Exception ex)
            {
                return string.Empty;
            }
        }
        public static string UpdateUserProfileImage(string profileImagePath, Stream stream, string fileName, MediaSettings mediaSettings)
        {
            LogImage("Pruebo que tenga datos.", mediaSettings);

            if (stream == null || stream.Length == 0)
                throw new System.Exception("No se encontró ningún archivo subido");

            LogImage("Tomo el nombre de la carpeta final", mediaSettings);

            var storeFolderName = mediaSettings.Profile.FolderName;

            LogImage("Chequeo el nombre de la carpeta", mediaSettings);

            if (string.IsNullOrWhiteSpace(storeFolderName))
                throw new ConfigurationException("Nombre de carpeta de imagenes de perfil");//profile picture folder name

            LogImage("Toma el path relativo", mediaSettings);

            var relativePath = mediaSettings.Profile.FolderPath;
            var profileDir = Path.Combine(relativePath, storeFolderName);

            LogImage("Chequea la existencia del directorio", mediaSettings);

            if (!Directory.Exists(profileDir))
                Directory.CreateDirectory(profileDir);

            LogImage("Chequea la extensión del archivo", mediaSettings);

            if (!string.IsNullOrWhiteSpace(fileName.Split('.').ToList().Last()) && mediaSettings.Profile.ContentTypeAllowed != null && !mediaSettings.Profile.ContentTypeAllowed.Contains(fileName.Split('.').ToList().Last()))
                throw new System.Exception("El tipo de imágen no está permitido");//profile picture content type

            LogImage("Chequea el tamaño de la imagen cargada", mediaSettings);

            if (mediaSettings.Profile.MaxContentLength < stream.Length)
                throw new System.Exception("El tamaño de la imágen no puede superar los " + ((int)mediaSettings.Profile.MaxContentLength / 1000000) + "MB"); //profile picture content size

            LogImage("Genera el guid de la imagen y toma el path final de la misma", mediaSettings);

            var new_fileName = string.Format("{0}{1}", Guid.NewGuid(), Path.GetExtension(fileName));

            // stores new profile image
            try
            {
                LogImage("Copia el archivo de la imagen y lo genera en el directorio final", mediaSettings);

                using (var fileStream = File.Create(Path.Combine(relativePath, storeFolderName, new_fileName)))
                {
                    stream.Seek(0, SeekOrigin.Begin);
                    stream.CopyTo(fileStream);
                }

            }
            catch (System.Exception e)
            {
                LogImage("Error mensaje: " + e.Message, mediaSettings);
                LogImage("Error Stack Trace: " + e.StackTrace, mediaSettings);
                Console.WriteLine(e);
                throw;
            }

            // deletes old profile image path
            if (!string.IsNullOrWhiteSpace(profileImagePath) && File.Exists(Path.Combine(relativePath, profileImagePath)))
                File.Delete(Path.Combine(relativePath, profileImagePath));

            return string.Format("{0}/{1}", storeFolderName, new_fileName);
        }

        private static void LogImage(string message, MediaSettings mediaSettings)
        {
            var relativePath = mediaSettings.Profile.FolderPath;
            var storeFolderName = mediaSettings.Profile.FolderName;
            var new_fileName = "logImage.txt";
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(message);
            // flush every 20 seconds as you do it
            File.AppendAllText(Path.Combine(relativePath, storeFolderName, new_fileName), sb.ToString());
            sb.Clear();
        }

        public static SixLabors.ImageSharp.Image CropFixedSize(SixLabors.ImageSharp.Image image, int width, int height)
        {
            #region 
            var needToFill = true;
            var sourceWidth = image.Width;
            var sourceHeight = image.Height;
            var sourceX = 0;
            var sourceY = 0;
            double destX = 0;
            double destY = 0;

            double nScale = 0;
            double nScaleW = 0;
            double nScaleH = 0;

            if (sourceWidth < width || sourceHeight < height)
            {
                needToFill = false;
            }


            nScaleW = ((double)width / (double)sourceWidth);
            nScaleH = ((double)height / (double)sourceHeight);
            if (!needToFill)
            {
                nScale = Math.Min(nScaleH, nScaleW);
            }
            else
            {
                nScale = Math.Max(nScaleH, nScaleW);
                destY = (height - sourceHeight * nScale) / 2;
                destX = (width - sourceWidth * nScale) / 2;
            }

            if (nScale > 1)
                nScale = 1;

            var destWidth = (int)Math.Round(sourceWidth * nScale);
            var destHeight = (int)Math.Round(sourceHeight * nScale);
            #endregion

            //using var image = Image.Load("original.jpg");
            //image.Mutate(x => x.Resize(image.Width / 2, image.Height / 2));

            if (width > sourceWidth)
                width = sourceWidth;
            if (height > sourceHeight)
                height = sourceHeight;

            try
            {
                var to = new Rectangle((int)Math.Round(destX), (int)Math.Round(destY), destWidth, destHeight);
                var from = new Rectangle(sourceX, sourceY, sourceWidth, sourceHeight);

                var imageClone = image.Clone(x => x
                .Resize(new ResizeOptions
                {
                    Mode = ResizeMode.Crop,
                    Position = AnchorPositionMode.Center,
                    Size = new Size(width, height)
                }));
                //.Crop(new Rectangle(sourceX, sourceY, width, height)));
                return imageClone;
            }
            catch (System.Exception ex)
            {
                throw new ApplicationException(string.Format("destWidth:{0}, destX:{1}, destHeight:{2}, desxtY:{3}, Width:{4}, Height:{5}",
                    destWidth, destX, destHeight, destY, width, height), ex);
            }
            //System.Drawing.Image
            //using (var grPhoto = Image.FromImage(bmPhoto))
            //{

            //    grPhoto.InterpolationMode = InterpolationMode.HighQualityBicubic;
            //    grPhoto.CompositingQuality = CompositingQuality.HighQuality;
            //    grPhoto.SmoothingMode = SmoothingMode.HighQuality;

            //    var to = new Rectangle((int)Math.Round(destX), (int)Math.Round(destY), destWidth, destHeight);
            //    var from = new Rectangle(sourceX, sourceY, sourceWidth, sourceHeight);
            //    grPhoto.DrawImage(image, to, from, GraphicsUnit.Pixel);

            //    return bmPhoto;
            //}

            return image;
        }

        public static Image ResizeImage(Image image, int width, int height)
        {
            var porcentajeResize = width * 100 / image.Width;
            height = (porcentajeResize * image.Height) / 100;

            image.Mutate(x => x.Resize(width, height));

            return image;
        }
        public static SixLabors.ImageSharp.Image ConvertBase64ToImage(string base64Image)
        {
            var bytes = Convert.FromBase64String(base64Image);

            try
            {
                using (var ms = new MemoryStream(bytes))
                {
                    var image = SixLabors.ImageSharp.Image.Load(ms);
                    return image;
                }
            }
            catch
            {
                throw new ArgumentException("base64 string is not an image");
            }
        }

        //public static ImageFormat GetImageFormat(this Image img)
        //{
        //    img.currentImage
        //    //img.currentImageFormat
        //    if (img.RawFormat.Equals(ImageFormat.Jpeg))
        //        return ImageFormat.Jpeg;
        //    if (img.RawFormat.Equals(ImageFormat.Bmp))
        //        return ImageFormat.Bmp;
        //    if (img.RawFormat.Equals(ImageFormat.Png))
        //        return ImageFormat.Png;

        //    return ImageFormat.Wmf;
        //}

        public static string CreateMediaSizePath(string fileName, int width, int height, MediaSettings mediaSettings)
        {
            if (!string.IsNullOrWhiteSpace(fileName) && width > 0 && height > 0)
            {
                var path = ImageStoreHelper.CreateYearMonthDayFolderByPath(Path.Combine(mediaSettings.Sizes.FolderPath, mediaSettings.Sizes.FolderName));
                return $@"{path}/{Path.GetFileNameWithoutExtension(fileName)}_{width}x{height}{Path.GetExtension(fileName)}";
            }

            return string.Empty;
        }

        //public static bool IsValidFile(object file, FileUpload fileUploadConfig)
        //{
        //    long maxFileSize = fileUploadConfig.FileSizeBytes;

        //    if (file is Image)
        //    {
        //        var format = GetImageFormat((System.Drawing.Image)file);

        //        if (!Equals(format, ImageFormat.Wmf))
        //        {
        //            long imageFileSize;
        //            using (var ms = new MemoryStream())
        //            {
        //                ((Image)file).Save(ms, format);
        //                imageFileSize = ms.Length;
        //            }
        //            if (imageFileSize > maxFileSize)
        //                return false;
        //        }
        //        else
        //        {
        //            return false;
        //        }

        //        return true;
        //    }

        //    return false;
        //}

        public static string ResizeAndSaveImage(string filePath, int w, int h, MediaSettings mediaSettings)
        {
            var stringToReplace = Directory.GetCurrentDirectory();

            if (!string.IsNullOrWhiteSpace(filePath) && w > 0 && h > 0)
            {
                //IImageFormat imageFormat;

                using (var image = Image.Load(filePath))
                {
                    var encoder = image.DetectEncoder(filePath);
                    var extension = Path.GetExtension(filePath);

                    if (extension.Contains("jpg") || extension.Contains("jpeg"))
                    {
                        var jpegEncoder = new JpegEncoder
                        {
                            Quality = mediaSettings.FileUpload.ImageQuality
                        };
                        encoder = jpegEncoder;
                    }


                    //image.Mutate(c => c.Resize(30, 30));
                    var resizedImage = ImageStoreHelper.CropFixedSize(image, w, h); //Resizes images
                    var newFileName = ImageStoreHelper.CreateMediaSizePath(Path.GetFileName(filePath), w, h, mediaSettings);

                    //var format = Path.GetExtension(newFileName);
                    //image.DetectEncoder(filePath);
                    //resizedImage.DetectEncoder
                    //var encoder = ImageCodecInfo.GetImageEncoders().First(c => c.FormatID == format.Guid);
                    //var encParams = new EncoderParameters(1);
                    //encParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, mediaSettings.FileUpload.ImageQuality);

                    resizedImage.Save(newFileName, encoder); //Saves image foreach Size
                    return newFileName.Replace(stringToReplace, "")
                        .Replace(mediaSettings.Sizes.FolderPath + "\\", "")
                        .Replace(mediaSettings.Sizes.FolderPath + "/", "")
                        .Replace("\\" + mediaSettings.FileUpload.FolderPath, "")
                        .Replace("/" + mediaSettings.FileUpload.FolderPath, "")
                        .Replace("\\", "/").Replace("//", "/");
                }


            }

            return string.Empty;
        }

        public static string CreateSizeBodyContentNew(string filePath, int w, int h, MediaSettings mediaSettings)
        {
            var stringToReplace = Directory.GetCurrentDirectory();
            if (!string.IsNullOrWhiteSpace(filePath) && w > 0 && h > 0)
            {
                //IImageFormat imageFormat;

                using (var image = Image.Load(filePath))
                {
                    var encoder = image.DetectEncoder(filePath);
                    var extension = Path.GetExtension(filePath);

                    if (extension.Contains("jpg") || extension.Contains("jpeg"))
                    {
                        var jpegEncoder = new JpegEncoder
                        {
                            Quality = mediaSettings.FileUpload.ImageQuality
                        };
                        encoder = jpegEncoder;
                    }               
                    var resizedImage = ImageStoreHelper.ResizeImage(image, w, h); //Resizes images
                    var newFileName = ImageStoreHelper.CreateMediaSizePath(Path.GetFileName(filePath), w, h, mediaSettings);
               

                    resizedImage.Save(newFileName, encoder); //Saves image foreach Size
                    return newFileName.Replace(stringToReplace, "")
                        .Replace(mediaSettings.Sizes.FolderPath + "\\", "")
                        .Replace(mediaSettings.Sizes.FolderPath + "/", "")
                        .Replace("\\" + mediaSettings.FileUpload.FolderPath, "")
                        .Replace("/" + mediaSettings.FileUpload.FolderPath, "")
                        .Replace("\\", "/").Replace("//", "/");
                }


            }

            return string.Empty;
        }

        public static string CreateYearMonthDayFolderByPath(string path)
            => Directory.CreateDirectory(Path.Combine(path, DateTime.UtcNow.Year.ToString(), DateTime.UtcNow.Month.ToString(), DateTime.UtcNow.Day.ToString())).FullName;

        public static string GetMediaSizePath(MediaDto media, int width, int height, MediaSettings mediaSettings)
        {
            if (media != null && width > 0 && height > 0)
            {
                if (media.Size1Path.Contains(string.Format("_{0}x{1}", width, height)))
                    return media.Size1Path;

                else if (media.Size2Path.Contains(string.Format("_{0}x{1}", width, height)))
                    return media.Size2Path;

                else if (media.Size3Path.Contains(string.Format("_{0}x{1}", width, height)))
                    return media.Size3Path;

                else
                {
                    var original = media.Size1Path;
                    media.Size1Path = media.Size1Path.Replace(Path.GetFileNameWithoutExtension(media.Size1Path), string.Format("{0}_{1}x{2}", Path.GetFileNameWithoutExtension(media.Size2Path), mediaSettings.Sizes.Size1.Width, mediaSettings.Sizes.Size1.Height));
                    media.Size2Path = media.Size2Path.Replace(Path.GetFileNameWithoutExtension(media.Size2Path), string.Format("{0}_{1}x{2}", Path.GetFileNameWithoutExtension(media.Size2Path), mediaSettings.Sizes.Size1.Width, mediaSettings.Sizes.Size2.Height));
                    media.Size3Path = media.Size3Path.Replace(Path.GetFileNameWithoutExtension(media.Size3Path), string.Format("{0}_{1}x{2}", Path.GetFileNameWithoutExtension(media.Size2Path), mediaSettings.Sizes.Size1.Width, mediaSettings.Sizes.Size3.Height));
                    media.Size4Path = media.Size4Path.Replace(Path.GetFileNameWithoutExtension(media.Size4Path), string.Format("{0}_{1}x{2}", Path.GetFileNameWithoutExtension(media.Size2Path), mediaSettings.Sizes.Size1.Width, mediaSettings.Sizes.Size4.Height));
                    media.Size5Path = media.Size5Path.Replace(Path.GetFileNameWithoutExtension(media.Size5Path), string.Format("{0}_{1}x{2}", Path.GetFileNameWithoutExtension(media.Size2Path), mediaSettings.Sizes.Size1.Width, mediaSettings.Sizes.Size5.Height));

                    return original.Replace(Path.GetFileNameWithoutExtension(original), string.Format("{0}_{1}x{2}", Path.GetFileNameWithoutExtension(original), width, height));
                }
            }

            return string.Empty;
        }

        public static void DeleteFiles(IFileProvider physicalFileProvider)
        {
            if (physicalFileProvider is PhysicalFileProvider)
            {
                var directory = physicalFileProvider.GetDirectoryContents(string.Empty);
                foreach (var file in directory)
                {
                    if (!file.IsDirectory)
                    {
                        var fileInfo = new System.IO.FileInfo(file.PhysicalPath);
                        fileInfo.Delete();

                    }
                }
            }
        }
    }
}
