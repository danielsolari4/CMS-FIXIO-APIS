//using System;
//using System.Configuration;
//using System.Drawing;
//using System.Drawing.Drawing2D;
//using System.Drawing.Imaging;
//using System.IO;
//using System.Linq;
//using System.Web;

//namespace Rino.BackendApi.ImageStorage
//{
//    public static class ImageStoreHelper
//    {
//        public static string UpdateUserProfileImage(string profileImagePath, HttpPostedFile file)
//        {
//            if (file == null)
//                throw new ArgumentException("file");

//            var storeFolderName = ConfigurationHelper.GetValue<string>("CMS.User.ProfilePicture.Root.Folder.Name");

//            if (string.IsNullOrWhiteSpace(storeFolderName))
//                throw new Utils.Exception.ConfigurationException("Falta nombre en carpeta de imágenes en perfil");//profile picture folder name

//            var relativePath = ConfigurationHelper.GetValue<string>("CMS.User.ProfilePicture.Root.Folder");

//            if (!Directory.Exists(string.Format("{0}{1}", relativePath, storeFolderName)))
//                Directory.CreateDirectory(string.Format("{0}{1}", relativePath, storeFolderName));

//            if (!string.IsNullOrWhiteSpace(file.ContentType) && ConfigurationHelper.GetValue<string>("CMS.User.ProfilePicture.ContentType.Allowed") != null && !ConfigurationHelper.GetValue<string>("CMS.User.ProfilePicture.ContentType.Allowed").Contains(file.ContentType.Replace("image/", string.Empty)))
//                throw new Exception("ISEX_001");//profile picture content type

//            if (ConfigurationHelper.GetValue<double>("CMS.User.ProfilePicture.MaxContentLength.Bytes") < file.ContentLength)
//                throw new Exception("ISEX_002-" + (ConfigurationHelper.GetValue<int>("CMS.User.ProfilePicture.MaxContentLength.Bytes") / 1000000) + "MB");

//            var fileName = string.Format("{0}{1}", Guid.NewGuid(), Path.GetExtension(file.FileName));

//            // stores new profile image
//            file.SaveAs(string.Format("{0}{1}/{2}", relativePath, storeFolderName, fileName));

//            // deletes old profile image path
//            if (!string.IsNullOrWhiteSpace(profileImagePath) && File.Exists(string.Format("{0}{1}", relativePath, profileImagePath)))
//                File.Delete(string.Format("{0}{1}", relativePath, profileImagePath));

//            return string.Format("{0}/{1}", storeFolderName, fileName);
//        }

//        public static string GetMediaSizePath(MediaDto media, int width, int height)
//        {
//            if (media != null && width > 0 && height > 0)
//            {
//                if (media.Size1Path.Contains(string.Format("_{0}x{1}", width, height)))
//                    return media.Size1Path;

//                else if (media.Size2Path.Contains(string.Format("_{0}x{1}", width, height)))
//                    return media.Size2Path;

//                else if (media.Size3Path.Contains(string.Format("_{0}x{1}", width, height)))
//                    return media.Size3Path;

//                else
//                {
//                    var original = media.Size1Path;
//                    media.Size1Path = media.Size1Path.Replace(Path.GetFileNameWithoutExtension(media.Size1Path), string.Format("{0}_{1}x{2}", Path.GetFileNameWithoutExtension(media.Size2Path), ConfigurationHelper.GetValue<int>("CMS.Media.FileUpload.Size1.Width", () => 0), ConfigurationHelper.GetValue<int>("CMS.Media.FileUpload.Size1.Height", () => 0)));
//                    media.Size2Path = media.Size2Path.Replace(Path.GetFileNameWithoutExtension(media.Size2Path), string.Format("{0}_{1}x{2}", Path.GetFileNameWithoutExtension(media.Size2Path), ConfigurationHelper.GetValue<int>("CMS.Media.FileUpload.Size2.Width", () => 0), ConfigurationHelper.GetValue<int>("CMS.Media.FileUpload.Size2.Height", () => 0)));
//                    media.Size3Path = media.Size3Path.Replace(Path.GetFileNameWithoutExtension(media.Size3Path), string.Format("{0}_{1}x{2}", Path.GetFileNameWithoutExtension(media.Size2Path), ConfigurationHelper.GetValue<int>("CMS.Media.FileUpload.Size3.Width", () => 0), ConfigurationHelper.GetValue<int>("CMS.Media.FileUpload.Size3.Height", () => 0)));
//                    media.Size4Path = media.Size4Path.Replace(Path.GetFileNameWithoutExtension(media.Size4Path), string.Format("{0}_{1}x{2}", Path.GetFileNameWithoutExtension(media.Size2Path), ConfigurationHelper.GetValue<int>("CMS.Media.FileUpload.Size4.Width", () => 0), ConfigurationHelper.GetValue<int>("CMS.Media.FileUpload.Size4.Height", () => 0)));
//                    media.Size5Path = media.Size5Path.Replace(Path.GetFileNameWithoutExtension(media.Size5Path), string.Format("{0}_{1}x{2}", Path.GetFileNameWithoutExtension(media.Size2Path), ConfigurationHelper.GetValue<int>("CMS.Media.FileUpload.Size5.Width", () => 0), ConfigurationHelper.GetValue<int>("CMS.Media.FileUpload.Size5.Height", () => 0)));

//                    return original.Replace(Path.GetFileNameWithoutExtension(original), string.Format("{0}_{1}x{2}", Path.GetFileNameWithoutExtension(original), width, height));
//                }
//            }

//            return string.Empty;
//        }


//        public static string CreateMediaSizePath(string fileName, int width, int height)
//        {
//            if (!string.IsNullOrWhiteSpace(fileName) && width > 0 && height > 0)
//            {
//                var dir = string.Format(@"{0}\{1}\{2}\{3}\{4}", ConfigurationHelper.GetValue<string>("CMS.Media.FileUpload.Sizes.Folder"), ConfigurationHelper.GetValue<string>("CMS.Media.FileUpload.Sizes.Folder.Name"), DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day); //Creates path string
//                Directory.CreateDirectory(dir); //Checks if path exists, creates it if it doesn't, ignores it if it does.
//                var filePath = string.Format(@"{0}\{1}", dir, Path.GetFileName(fileName)); //Join path with fileName
//                var newFileName = filePath.Replace(Path.GetFileNameWithoutExtension(filePath), string.Format("{0}_{1}x{2}", Path.GetFileNameWithoutExtension(filePath), width, height)); //Replaces fileName with fileName with dimensions

//                return newFileName;
//            }

//            return string.Empty;
//        }

//        public static bool IsValidFile(object file)
//        {

//            long maxFileSize;

//            try
//            {
//                maxFileSize = Convert.ToInt64(ConfigurationHelper.GetValue<double>("CMS.Media.FileUpload.FileSize.Bytes"));
//            }
//            catch (Exception ex)
//            {
//                throw new ConfigurationErrorsException("Configuration value missing or invalid. FileUploadsMaxFileSize is not defined or can not be converted to Int.", ex);
//            }

//            if (file is Image)
//            {
//                var format = GetImageFormat((Image)file);

//                if (!Equals(format, ImageFormat.Wmf))
//                {
//                    long imageFileSize;
//                    using (var ms = new MemoryStream())
//                    {
//                        ((Image)file).Save(ms, format);
//                        imageFileSize = ms.Length;
//                    }
//                    if (imageFileSize > maxFileSize)
//                    {
//                        throw new HttpException("File exceeds max size.");
//                    }
//                }
//                else
//                {
//                    throw new HttpException("File is not valid format or type.");
//                }
//            }

//            if (file is Backload.Contracts.Status.IFileStatusItem)
//                if ((file as Backload.Contracts.Status.IFileStatusItem).FileSize > maxFileSize)
//                {
//                    throw new HttpException("File exceeds max size.");
//                }

//            string allowedFileExtensions;

//            try
//            {
//                allowedFileExtensions = ConfigurationHelper.GetValue<string>("CMS.Media.FileUpload.ContentType.Allowed");
//            }
//            catch (Exception ex)
//            {
//                throw new ConfigurationErrorsException("Configuration value missing. FileUploadsAllowedExtensions is not defined.", ex);
//            }

//            var extensionList = allowedFileExtensions.Split(',').ToList<string>();

//            if (file is Image)
//            {
//                var format = GetImageFormat((Image)file).ToString();

//                if (!extensionList.Contains(format.ToLower()))
//                {
//                    throw new HttpException("File is not valid format or type.");
//                }

//                return true;
//            }

//            else if (file is Backload.Contracts.Status.IFileStatusItem)
//            {
//                var fileName = (file as Backload.Contracts.Status.IFileStatusItem).FileName;

//                if (!extensionList.Contains(Path.GetExtension(fileName).Replace(".", "").ToLower()))
//                {
//                    throw new HttpException("File is not valid format or type.");
//                }

//                return true;
//            }

//            else
//            {
//                throw new HttpException("File is not valid format or type.");
//            }

//        }

//        public static Image CropFixedSize(Image image, int width, int height)
//        {
//            #region 
//            var needToFill = true;
//            var sourceWidth = image.Width;
//            var sourceHeight = image.Height;
//            var sourceX = 0;
//            var sourceY = 0;
//            double destX = 0;
//            double destY = 0;

//            double nScale = 0;
//            double nScaleW = 0;
//            double nScaleH = 0;

//            if (sourceWidth < width || sourceHeight < height)
//            {
//                needToFill = false;
//            }


//            nScaleW = ((double)width / (double)sourceWidth);
//            nScaleH = ((double)height / (double)sourceHeight);
//            if (!needToFill)
//            {
//                nScale = Math.Min(nScaleH, nScaleW);
//            }
//            else
//            {
//                nScale = Math.Max(nScaleH, nScaleW);
//                destY = (height - sourceHeight * nScale) / 2;
//                destX = (width - sourceWidth * nScale) / 2;
//            }

//            if (nScale > 1)
//                nScale = 1;

//            var destWidth = (int)Math.Round(sourceWidth * nScale);
//            var destHeight = (int)Math.Round(sourceHeight * nScale);
//            #endregion

//            Bitmap bmPhoto;
//            try
//            {
//                bmPhoto = new Bitmap(destWidth + (int)Math.Round(2 * destX), destHeight + (int)Math.Round(2 * destY));
//            }
//            catch (Exception ex)
//            {
//                throw new ApplicationException(string.Format("destWidth:{0}, destX:{1}, destHeight:{2}, desxtY:{3}, Width:{4}, Height:{5}",
//                    destWidth, destX, destHeight, destY, width, height), ex);
//            }
//            using (var grPhoto = Graphics.FromImage(bmPhoto))
//            {
//                grPhoto.InterpolationMode = InterpolationMode.HighQualityBicubic;
//                grPhoto.CompositingQuality = CompositingQuality.HighQuality;
//                grPhoto.SmoothingMode = SmoothingMode.HighQuality;

//                var to = new Rectangle((int)Math.Round(destX), (int)Math.Round(destY), destWidth, destHeight);
//                var from = new Rectangle(sourceX, sourceY, sourceWidth, sourceHeight);
//                grPhoto.DrawImage(image, to, from, GraphicsUnit.Pixel);

//                return bmPhoto;
//            }
//        }

//        public static Image ConvertBase64ToImage(string base64Image)
//        {
//            var bytes = Convert.FromBase64String(base64Image);

//            try
//            {
//                using (var ms = new MemoryStream(bytes))
//                {
//                    var image = Image.FromStream(ms);
//                    return image;
//                }
//            }
//            catch
//            {
//                throw new ArgumentException("base64 string is not an image");
//            }
//        }

//        public static ImageFormat GetImageFormat(this Image img)
//        {
//            if (img.RawFormat.Equals(ImageFormat.Jpeg))
//                return ImageFormat.Jpeg;
//            if (img.RawFormat.Equals(ImageFormat.Bmp))
//                return ImageFormat.Bmp;
//            if (img.RawFormat.Equals(ImageFormat.Png))
//                return ImageFormat.Png;

//            return ImageFormat.Wmf;
//        }

//    }
//}