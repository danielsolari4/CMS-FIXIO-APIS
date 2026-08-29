using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Ray.Dtos;
using Ray.Dtos.Configuration;
using Ray.Utils.Backload;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Ray.Managers
{
    public interface IAmazonS3Manager
    {
        Task<AmazonS3FileUploadResultDto> UploadFromFileAsync(FileUploadResult backloadUploadResult, bool removeAfterUpload);
        Task<AmazonS3FileUploadResultDto> UploadOneFromFileAsync(string sizePath, string filePath, bool removeAfterUpload);
        Task<AmazonS3FileUploadResultDto> UploadOneFromStreamAsync(string sizePath, Stream stream);
        Task DeleteOneAsync(string key);
    }

    public class AmazonS3Manager : IAmazonS3Manager
    {
        private readonly AppSettings _appSettings;
        private IAmazonS3 _amazonS3Client;
        public AmazonS3Manager(AppSettings appSettings)
        {
            _appSettings = appSettings;
            _amazonS3Client = GetAmazonS3Client();
        }

        private IAmazonS3 GetAmazonS3Client()
            => new AmazonS3Client(new BasicAWSCredentials(_appSettings.AmazonS3.AccessKeyId, _appSettings.AmazonS3.SecretAccessKey),
                new AmazonS3Config() { ServiceURL = _appSettings.AmazonS3.S3ProductionUrl });

        /// <summary>
        /// 
        /// </summary>
        /// <param name="uploadPath">year/month/day/file.png</param>
        /// <param name="fileToUploadPath">(i.e) C:/files/file.png</param>
        /// <returns></returns>
        public async Task<AmazonS3FileUploadResultDto> UploadFromFileAsync(FileUploadResult backloadUploadResult, bool removeAfterUpload)
        {
            try
            {
                foreach (var putRequest in GetFilesToPost(backloadUploadResult))
                {
                    await _amazonS3Client.PutObjectAsync(putRequest);

                    if (removeAfterUpload)
                        System.IO.File.Delete(putRequest.FilePath);
                }

                return new AmazonS3FileUploadResultDto()
                {
                    Error = false
                    // FileUrl = $"{_appSettings.AmazonS3.S3ProductionUrl}/{_appSettings.AmazonS3.BucketName}/{uploadPath}"
                };
            }
            catch (AmazonS3Exception e)
            {
                return new AmazonS3FileUploadResultDto() { Error = true };
            }
        }


        public async Task<AmazonS3FileUploadResultDto> UploadOneFromFileAsync(string sizePath, string filePath, bool removeAfterUpload)
        {
            try
            {
                //Sizes
                var putRequest = new PutObjectRequest()
                {
                    BucketName = _appSettings.AmazonS3.BucketName,
                    Key = sizePath.Replace($@"{_appSettings.Media.Sizes.FolderPath}\", "").Replace(@"\", "/").TrimStart('/'),
                    FilePath = filePath,
                    CannedACL = S3CannedACL.PublicRead
                };

                await _amazonS3Client.PutObjectAsync(putRequest);

                if (removeAfterUpload)
                    System.IO.File.Delete(filePath);

                return new AmazonS3FileUploadResultDto()
                {
                    Error = false
                    // FileUrl = $"{_appSettings.AmazonS3.S3ProductionUrl}/{_appSettings.AmazonS3.BucketName}/{uploadPath}"
                };
            }
            catch (AmazonS3Exception e)
            {
                return new AmazonS3FileUploadResultDto() { Error = true };
            }
        }

        public async Task<AmazonS3FileUploadResultDto> UploadOneFromStreamAsync(string sizePath, Stream stream)
        {
            try
            {
                //Sizes
                var putRequest = new PutObjectRequest()
                {
                    BucketName = _appSettings.AmazonS3.BucketName,
                    Key = sizePath.Replace($@"{_appSettings.Media.Sizes.FolderPath}\", "").Replace(@"\", "/").TrimStart('/'),
                    InputStream = stream,
                    CannedACL = S3CannedACL.PublicRead
                };

                await _amazonS3Client.PutObjectAsync(putRequest);


                return new AmazonS3FileUploadResultDto()
                {
                    Error = false
                    // FileUrl = $"{_appSettings.AmazonS3.S3ProductionUrl}/{_appSettings.AmazonS3.BucketName}/{uploadPath}"
                };
            }
            catch (AmazonS3Exception e)
            {
                return new AmazonS3FileUploadResultDto() { Error = true };
            }
        }

        public async Task DeleteOneAsync(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return;

            await _amazonS3Client.DeleteObjectAsync(new DeleteObjectRequest
            {
                BucketName = _appSettings.AmazonS3.BucketName,
                Key = key.Replace("\\", "/").TrimStart('/')
            });
        }

        private List<PutObjectRequest> GetFilesToPost(FileUploadResult backloadUploadResult)
        {
            List<PutObjectRequest> putObjectRequests = new List<PutObjectRequest>();
            var isLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
            var concat = isLinux ? "/" : @"\";
            var pathToReplace = $"{Directory.GetCurrentDirectory()}{concat}";
            AddPutRequest(putObjectRequests, backloadUploadResult.Url, backloadUploadResult.Url);
            AddPutRequest(putObjectRequests, backloadUploadResult.ThumbnailUrl, isLinux ? $"{pathToReplace}{_appSettings.Media.Sizes.FolderPath}{backloadUploadResult.ThumbnailUrl}" : $"{backloadUploadResult.ThumbnailUrl}".Replace("/", "\\"));
            AddPutRequest(putObjectRequests, backloadUploadResult.Size1Path, isLinux ? $"{pathToReplace}{_appSettings.Media.Sizes.FolderPath}{backloadUploadResult.Size1Path}" : $"{_appSettings.Media.Sizes.FolderPath}{backloadUploadResult.Size1Path}".Replace("/", "\\"));
            AddPutRequest(putObjectRequests, backloadUploadResult.Size2Path, isLinux ? $"{pathToReplace}{_appSettings.Media.Sizes.FolderPath}{backloadUploadResult.Size2Path}" : $"{_appSettings.Media.Sizes.FolderPath}{backloadUploadResult.Size2Path}".Replace("/", "\\"));
            AddPutRequest(putObjectRequests, backloadUploadResult.Size3Path, isLinux ? $"{pathToReplace}{_appSettings.Media.Sizes.FolderPath}{backloadUploadResult.Size3Path}" : $"{_appSettings.Media.Sizes.FolderPath}{backloadUploadResult.Size3Path}".Replace("/", "\\"));
            AddPutRequest(putObjectRequests, backloadUploadResult.Size4Path, isLinux ? $"{pathToReplace}{_appSettings.Media.Sizes.FolderPath}{backloadUploadResult.Size4Path}" : $"{_appSettings.Media.Sizes.FolderPath}{backloadUploadResult.Size4Path}".Replace("/", "\\"));
            AddPutRequest(putObjectRequests, backloadUploadResult.Size5Path, isLinux ? $"{pathToReplace}{_appSettings.Media.Sizes.FolderPath}{backloadUploadResult.Size5Path}" : $"{_appSettings.Media.Sizes.FolderPath}{backloadUploadResult.Size5Path}".Replace("/", "\\"));
            AddPutRequest(putObjectRequests, backloadUploadResult.Size6Path, isLinux ? $"{pathToReplace}{_appSettings.Media.Sizes.FolderPath}{backloadUploadResult.Size6Path}" : $"{_appSettings.Media.Sizes.FolderPath}{backloadUploadResult.Size6Path}".Replace("/", "\\"));

            return putObjectRequests;
        }

        private void AddPutRequest(List<PutObjectRequest> putObjectRequests, string keyPath, string filePath)
        {
            if (string.IsNullOrWhiteSpace(keyPath) || string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                return;

            putObjectRequests.Add(new PutObjectRequest()
            {
                BucketName = _appSettings.AmazonS3.BucketName,
                Key = keyPath.Replace(Directory.GetCurrentDirectory(), string.Empty)
                    .Replace(_appSettings.Media.Sizes.FolderPath, string.Empty)
                    .Replace("\\", "/")
                    .TrimStart('/'),
                FilePath = filePath,
                CannedACL = S3CannedACL.PublicRead
            });
        }
    }
}
