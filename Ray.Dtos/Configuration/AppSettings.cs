using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ray.Dtos.Configuration
{
    public class AppSettings
    {
        public StructureJson StructureJson { get; set; }
        public Content Content { get; set; }
        public SolrConfig Solr { get; set; }
        public SMTP Smtp { get; set; }
        public Sitemap Sitemap { get; set; }
        public Weather Weather { get; set; }
        public Jwt Jwt { get; set; }
        public MediaSettings Media { get; set; }
        public Admin Admin { get; set; }
        public AmazonS3 AmazonS3 { get; set; }
        public TokenSettings TokenSettings { get; set; }
        public MongoDb MongoDb { get; set; }
        public AddThis AddThis { get; set; }
        public MobileApp MobileApp { get; set; }
        public SyncLayout SyncLayout { get; set; }
        public CacheInvalidation CacheInvalidation { get; set; }
    }

    public class CacheInvalidation
    {
        public List<string> Instances { get; set; }
        public string Url { get; set; }
        public string Token { get; set; }
        public string Endpoint { get; set; }
        public bool? PurgeCdn { get; set; }
        public int? MaxAttempts { get; set; }
        public string SecretCacheKey { get; set; }
        public CacheInvalidationQueue Queue { get; set; }
    }

    public class CacheInvalidationQueue
    {
        public int? MaxAttempts { get; set; }
        public int? StuckMinutes { get; set; }
        public int? RetentionDays { get; set; }
        public int? SchemaRetryMinutes { get; set; }
    }

    public class MobileApp
    {
        public string VersionRequiredAndroid { get; set; }
        public string VersionRequiredIos { get; set; }
    }
    public class Admin
    {
        public string ConfirmUserRequestFEUrl { get; set; }
        public string ConfirmUserRequestUrl { get; set; }
        public string ResetPasswordRequestUrl { get; set; }
    }

    public class Weather
    {
        public string AllowCities { get; set; }
        public string Url { get; set; }
        public string Key { get; set; }
        public string AllowCitiesIds { get; set; }
        public string ImageUrl { get; set; }
    }

    public class StructureJson
    {
        public string Path { get; set; }
    }

    public class Sitemap
    {
        public string FolderPath { get; set; }
    }

    public class SyncLayout
    {
        public string Url { get; set; }
        public int TimeInMinutes { get; set; }
        public string Token { get; set; }
    }

    public class Content
    {
        public string ImageUrl { get; set; }
        public string FrontendUrl { get; set; }
        public string ArchiveYear { get; set; }
        public string AdminUrl { get; set; }
        public string ApiBackendUrl { get; set; }
        public string TimeZone { get; set; }
    }

    public class AmazonS3
    {
        public string BucketName { get; set; }
        public string AccessKeyId { get; set; }
        public string SecretAccessKey { get; set; }
        public string S3ProductionUrl { get; set; }
        public string Region { get; set; }
        public bool ForcePathStyle { get; set; } = true;
        public bool UsePublicReadAcl { get; set; } = true;
        public bool DisablePayloadSigning { get; set; }
    }

    public class SolrConfig
    {
        public string Url { get; set; }
        public SolrCore SolrCore { get; set; }
        public string LayoutInstanceByNodeSecurityToken { get; set; }

    }

    public class SolrCore
    {
        public string Author { get; set; }
        public string Category { get; set; }
        public string Gallery { get; set; }
        public string Keywords { get; set; }
        public string Layout { get; set; }
        public string LayoutInstance { get; set; }
        public string LayoutInstanceByNode { get; set; }
        public string Media { get; set; }
        public string News { get; set; }
        public string Pages { get; set; }
        public string Node { get; set; }
        public string User { get; set; }
        public string ProgrammingGuide { get; set; }
        public string URLRedirect { get; set; }
        public string Channel { get; set; }
        public string Theme { get; set; }
        public string Widget { get; set; }
        public string PrintEdition { get; set; }
        public string SettingsCore { get; set; }
        public string Menu { get; set; }
    }

    public class SMTP
    {
        public int Port { get; set; }
        public string Host { get; set; }
        public bool EnableSsl { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public bool UseDefaultCredentials { get; set; }
        public string FromAddress { get; set; }
        public string FromDisplayName { get; set; }
        public bool EnableEmailNotifications { get; set; }
        public string EnableEmailAddresses { get; set; }
        public string EmailTemplateLocation { get; set; }
    }

    public class MediaSettings
    {
        public bool RemoveAfterUpload { get; set; }
        public Sizes Sizes { get; set; }
        public Profile Profile { get; set; }
        public FileUpload FileUpload { get; set; }
        public Youtube Youtube { get; set; }
        public DailyMotion DailyMotion { get; set; }
        public Kaltura Kaltura { get; set; }
        public SoundCloud SoundCloud { get; set; }
        public Facebook Facebook { get; set; }
    }

    public class Jwt
    {
        public string Key { get; set; }
        public string Issuer { get; set; }
        public string SecretKey { get; set; }
        public int ExpiryDays { get; set; }
    }

    public class Size1
    {
        public int Width { get; set; }
        public int Height { get; set; }
    }

    public class Size2
    {
        public int Width { get; set; }
        public int Height { get; set; }
    }

    public class Size3
    {
        public int Width { get; set; }
        public int Height { get; set; }
    }

    public class Size4
    {
        public int Width { get; set; }
        public int Height { get; set; }
    }

    public class Size5
    {
        public int Width { get; set; }
        public int Height { get; set; }
    }

    public class Sizes
    {
        public string FolderPath { get; set; }
        public string FolderName { get; set; }
        public string OriginalFolderName { get; set; }
        public string ThumbsFileFolder { get; set; }
        public Size1 Size1 { get; set; }
        public Size2 Size2 { get; set; }
        public Size3 Size3 { get; set; }
        public Size4 Size4 { get; set; }
        public Size5 Size5 { get; set; }
        public Size5 Size6 { get; set; }
    }

    public class Profile
    {
        public string FolderPath { get; set; }
        public string FolderName { get; set; }
        public string ContentTypeAllowed { get; set; }
        public double MaxContentLength { get; set; }
    }

    public class FileUpload
    {
        public string FolderPath { get; set; }
        public string FolderName { get; set; }
        public string PrintEditionFolderName { get; set; }
        public int ImageQuality { get; set; }
        public string ContentTypeAllowed { get; set; }
        public int FileSizeBytes { get; set; }
    }

    public class Youtube
    {
        public string ImageUrl { get; set; }
        public string EmbedUrl { get; set; }
    }

    public class DailyMotion
    {
        public string ImageUrl { get; set; }
        public string EmbedUrl { get; set; }
    }

    public class Kaltura
    {
        public string ImageUrl { get; set; }
        public string EmbedUrl { get; set; }
    }

    public class SoundCloud
    {
        public string ImageUrl { get; set; }
        public string ImageSaveUrl { get; set; }
    }

    public class Facebook
    {
        public string EmbedUrl { get; set; }
        public string AppSecret { get; set; }
        public string AccessToken { get; set; }
    }

    public class TokenSettings
    {
        public int UserConfirmationDuration { get; set; }
        public int ResetPasswordDuration { get; set; }
    }
    public class MongoDb
    {
        public string ConnectionString { get; set; }
        public string GetMostReadExclude { get; set; }
        public string DatabaseName { get; set; }
        public string NameCollectionAssetsViewCount { get; set; }
    }

    public class AddThis
    {
        public string Url { get; set; }
        public string Token { get; set; }
        public string XApiKey { get; set; }
    }
}
