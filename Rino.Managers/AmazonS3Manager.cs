using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Rino.Dtos;
using Rino.Dtos.Configuration;
using Rino.Utils.Backload;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Rino.Managers
{
    public interface IAmazonS3Manager
    {
        Task<AmazonS3FileUploadResultDto> UploadFromFileAsync(FileUploadResult backloadUploadResult, bool removeAfterUpload);
        Task<AmazonS3FileUploadResultDto> UploadOneFromFileAsync(string sizePath, string filePath, bool removeAfterUpload);
        Task<AmazonS3FileUploadResultDto> UploadOneFromStreamAsync(string sizePath, Stream stream, string contentType = null);
        Task DeleteOneAsync(string key);
    }

    public class AmazonS3Manager : IAmazonS3Manager
    {
        private readonly AppSettings _appSettings;
        private IAmazonS3 _amazonS3Client;
        public AmazonS3Manager(AppSettings appSettings)
        {
            _appSettings = appSettings;
        }

        private IAmazonS3 AmazonS3Client => _amazonS3Client ??= GetAmazonS3Client();

        private bool UsePublicReadAcl => _appSettings.AmazonS3?.UsePublicReadAcl ?? true;
        private bool DisablePayloadSigning => _appSettings.AmazonS3?.DisablePayloadSigning ?? false;

        private IAmazonS3 GetAmazonS3Client()
        {
            if (_appSettings.AmazonS3 == null ||
                string.IsNullOrWhiteSpace(_appSettings.AmazonS3.BucketName) ||
                string.IsNullOrWhiteSpace(_appSettings.AmazonS3.AccessKeyId) ||
                string.IsNullOrWhiteSpace(_appSettings.AmazonS3.SecretAccessKey) ||
                string.IsNullOrWhiteSpace(_appSettings.AmazonS3.S3ProductionUrl))
            {
                throw new InvalidOperationException("AmazonS3 settings are incomplete.");
            }

            var config = new AmazonS3Config
            {
                ServiceURL = _appSettings.AmazonS3.S3ProductionUrl,
                ForcePathStyle = _appSettings.AmazonS3.ForcePathStyle,
                RequestChecksumCalculation = RequestChecksumCalculation.WHEN_REQUIRED
            };

            if (!string.IsNullOrWhiteSpace(_appSettings.AmazonS3.Region))
                config.AuthenticationRegion = _appSettings.AmazonS3.Region;

            return new AmazonS3Client(
                new BasicAWSCredentials(_appSettings.AmazonS3.AccessKeyId, _appSettings.AmazonS3.SecretAccessKey),
                config);
        }

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
                    await AmazonS3Client.PutObjectAsync(putRequest);

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
                return new AmazonS3FileUploadResultDto() { Error = true, Message = e.Message };
            }
            catch (Exception e)
            {
                return new AmazonS3FileUploadResultDto() { Error = true, Message = e.Message };
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
                    Key = sizePath
                        .Replace(_appSettings.Media.Sizes.FolderPath + "\\", "")
                        .Replace(_appSettings.Media.Sizes.FolderPath + "/", "")
                        .Replace("\\", "/").TrimStart('/'),
                    FilePath = filePath
                };

                ApplyAcl(putRequest);

                await AmazonS3Client.PutObjectAsync(putRequest);

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
                return new AmazonS3FileUploadResultDto() { Error = true, Message = e.Message };
            }
            catch (Exception e)
            {
                return new AmazonS3FileUploadResultDto() { Error = true, Message = e.Message };
            }
        }

        public async Task<AmazonS3FileUploadResultDto> UploadOneFromStreamAsync(string sizePath, Stream stream, string contentType = null)
        {
            try
            {
                //Sizes
                var putRequest = new PutObjectRequest()
                {
                    BucketName = _appSettings.AmazonS3.BucketName,
                    Key = sizePath
                        .Replace(_appSettings.Media.Sizes.FolderPath + "\\", "")
                        .Replace(_appSettings.Media.Sizes.FolderPath + "/", "")
                        .Replace("\\", "/").TrimStart('/'),
                    InputStream = stream
                };

                if (!string.IsNullOrWhiteSpace(contentType))
                    putRequest.ContentType = contentType;

                ApplyAcl(putRequest);

                await AmazonS3Client.PutObjectAsync(putRequest);


                return new AmazonS3FileUploadResultDto()
                {
                    Error = false
                    // FileUrl = $"{_appSettings.AmazonS3.S3ProductionUrl}/{_appSettings.AmazonS3.BucketName}/{uploadPath}"
                };
            }
            catch (AmazonS3Exception e)
            {
                return new AmazonS3FileUploadResultDto() { Error = true, Message = e.Message };
            }
            catch (Exception e)
            {
                return new AmazonS3FileUploadResultDto() { Error = true, Message = e.Message };
            }
        }

        public async Task DeleteOneAsync(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return;

            await AmazonS3Client.DeleteObjectAsync(new DeleteObjectRequest
            {
                BucketName = _appSettings.AmazonS3.BucketName,
                Key = key.Replace("\\", "/").TrimStart('/')
            });
        }

        private List<PutObjectRequest> GetFilesToPost(FileUploadResult backloadUploadResult)
        {
            List<PutObjectRequest> putObjectRequests = new List<PutObjectRequest>();
            AddPutRequest(putObjectRequests, backloadUploadResult.Url, backloadUploadResult.Url);
            AddPutRequest(putObjectRequests, backloadUploadResult.ThumbnailUrl, Path.Combine(Directory.GetCurrentDirectory(), _appSettings.Media.Sizes.FolderPath, backloadUploadResult.ThumbnailUrl));
            AddPutRequest(putObjectRequests, backloadUploadResult.Size1Path, Path.Combine(Directory.GetCurrentDirectory(), _appSettings.Media.Sizes.FolderPath, backloadUploadResult.Size1Path));
            AddPutRequest(putObjectRequests, backloadUploadResult.Size2Path, Path.Combine(Directory.GetCurrentDirectory(), _appSettings.Media.Sizes.FolderPath, backloadUploadResult.Size2Path));
            AddPutRequest(putObjectRequests, backloadUploadResult.Size3Path, Path.Combine(Directory.GetCurrentDirectory(), _appSettings.Media.Sizes.FolderPath, backloadUploadResult.Size3Path));
            AddPutRequest(putObjectRequests, backloadUploadResult.Size4Path, Path.Combine(Directory.GetCurrentDirectory(), _appSettings.Media.Sizes.FolderPath, backloadUploadResult.Size4Path));
            AddPutRequest(putObjectRequests, backloadUploadResult.Size5Path, Path.Combine(Directory.GetCurrentDirectory(), _appSettings.Media.Sizes.FolderPath, backloadUploadResult.Size5Path));
            AddPutRequest(putObjectRequests, backloadUploadResult.Size6Path, Path.Combine(Directory.GetCurrentDirectory(), _appSettings.Media.Sizes.FolderPath, backloadUploadResult.Size6Path));

            return putObjectRequests;
        }

        private void AddPutRequest(List<PutObjectRequest> putObjectRequests, string keyPath, string filePath)
        {
            if (string.IsNullOrWhiteSpace(keyPath) || string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                return;

            var putRequest = new PutObjectRequest()
            {
                BucketName = _appSettings.AmazonS3.BucketName,
                Key = keyPath.Replace(Directory.GetCurrentDirectory(), string.Empty)
                    .Replace(_appSettings.Media.Sizes.FolderPath, string.Empty)
                    .Replace("\\", "/")
                    .TrimStart('/'),
                FilePath = filePath
            };

            ApplyAcl(putRequest);
            putObjectRequests.Add(putRequest);
        }

        private void ApplyAcl(PutObjectRequest putRequest)
        {
            if (UsePublicReadAcl)
                putRequest.CannedACL = S3CannedACL.PublicRead;

            putRequest.DisablePayloadSigning = DisablePayloadSigning;
        }
    }
}
