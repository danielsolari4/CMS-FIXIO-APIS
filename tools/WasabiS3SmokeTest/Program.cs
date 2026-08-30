using System.Text.Json;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

var appsettingsPath = args.Length > 0 ? args[0] : Path.Combine("Ray.BackendApi", "appsettings.json");

using var json = JsonDocument.Parse(File.ReadAllText(appsettingsPath));
var appSettings = json.RootElement.TryGetProperty("AppSettings", out var pascalAppSettings)
    ? pascalAppSettings
    : json.RootElement.GetProperty("appSettings");
var cfg = appSettings.GetProperty("AmazonS3");

var bucket = cfg.GetProperty("BucketName").GetString();
var accessKey = cfg.GetProperty("AccessKeyId").GetString();
var secretKey = cfg.GetProperty("SecretAccessKey").GetString();
var serviceUrl = cfg.GetProperty("S3ProductionUrl").GetString();
var region = cfg.TryGetProperty("Region", out var regionEl) ? regionEl.GetString() : "auto";
var forcePathStyle = !cfg.TryGetProperty("ForcePathStyle", out var fpsEl) || fpsEl.GetBoolean();
var usePublicReadAcl = !cfg.TryGetProperty("UsePublicReadAcl", out var aclEl) || aclEl.GetBoolean();
var disablePayloadSigning = cfg.TryGetProperty("DisablePayloadSigning", out var dpsEl) && dpsEl.GetBoolean();

var s3Config = new AmazonS3Config
{
    ServiceURL = serviceUrl,
    AuthenticationRegion = region,
    ForcePathStyle = forcePathStyle,
    RequestChecksumCalculation = RequestChecksumCalculation.WHEN_REQUIRED
};

using var client = new AmazonS3Client(new BasicAWSCredentials(accessKey, secretKey), s3Config);
var key = $"codex-test/wasabi-config-{Guid.NewGuid():N}.txt";

Console.WriteLine($"Testing bucket={bucket}, endpoint={serviceUrl}, region={region}, forcePathStyle={forcePathStyle}, usePublicReadAcl={usePublicReadAcl}, disablePayloadSigning={disablePayloadSigning}");
Console.WriteLine($"Key={key}");

var putRequest = new PutObjectRequest
{
    BucketName = bucket,
    Key = key,
    ContentBody = "codex storage test"
};
ApplyUploadOptions(putRequest, usePublicReadAcl, disablePayloadSigning);
await client.PutObjectAsync(putRequest);
Console.WriteLine("PutObject OK");
await CheckPublicReadAsync(serviceUrl!, bucket!, key);

var metadata = await client.GetObjectMetadataAsync(bucket, key);
Console.WriteLine($"GetObjectMetadata OK: status={(int)metadata.HttpStatusCode}, length={metadata.ContentLength}");

await client.DeleteObjectAsync(bucket, key);
Console.WriteLine("DeleteObject OK");

var imagePrefix = $"codex-test/image-flow-{Guid.NewGuid():N}";
var imageKeys = new[]
{
    $"{imagePrefix}/Files/Original/sample.jpg",
    $"{imagePrefix}/Files/_thumbs/sample.jpg",
    $"{imagePrefix}/Files/Sizes/sample_380x260.jpg",
    $"{imagePrefix}/Files/Sizes/sample_760x520.jpg",
    $"{imagePrefix}/Files/Sizes/sample_1140x520.jpg",
    $"{imagePrefix}/Files/Sizes/sample_22x10.jpg",
    $"{imagePrefix}/Files/Sizes/sample_15x10.jpg"
};

using var image = new Image<Rgba32>(1280, 720, new Rgba32(34, 90, 160));

await PutImage(client, bucket!, imageKeys[0], image, image.Width, image.Height, crop: false, usePublicReadAcl, disablePayloadSigning);
await PutImage(client, bucket!, imageKeys[1], image, 80, 60, crop: true, usePublicReadAcl, disablePayloadSigning);
await PutImage(client, bucket!, imageKeys[2], image, 380, 260, crop: true, usePublicReadAcl, disablePayloadSigning);
await PutImage(client, bucket!, imageKeys[3], image, 760, 520, crop: true, usePublicReadAcl, disablePayloadSigning);
await PutImage(client, bucket!, imageKeys[4], image, 1140, 520, crop: true, usePublicReadAcl, disablePayloadSigning);
await PutImage(client, bucket!, imageKeys[5], image, 22, 10, crop: true, usePublicReadAcl, disablePayloadSigning);
await PutImage(client, bucket!, imageKeys[6], image, 15, 10, crop: true, usePublicReadAcl, disablePayloadSigning);

foreach (var imageKey in imageKeys)
{
    var imageMetadata = await client.GetObjectMetadataAsync(bucket, imageKey);
    Console.WriteLine($"Image object OK: {imageKey}, status={(int)imageMetadata.HttpStatusCode}, length={imageMetadata.ContentLength}");
}

foreach (var imageKey in imageKeys)
{
    await client.DeleteObjectAsync(bucket, imageKey);
}
Console.WriteLine("Image flow Put/GetMetadata/Delete OK");

static async Task CheckPublicReadAsync(string serviceUrl, string bucket, string key)
{
    using var http = new HttpClient();
    var publicUrl = $"{serviceUrl.TrimEnd('/')}/{bucket}/{key.TrimStart('/')}";
    using var response = await http.GetAsync(publicUrl);

    if (response.IsSuccessStatusCode)
    {
        Console.WriteLine($"Anonymous GET OK: status={(int)response.StatusCode}");
        return;
    }

    var body = await response.Content.ReadAsStringAsync();
    var summary = body.Length > 300 ? body[..300] : body;
    Console.WriteLine($"Anonymous GET failed: status={(int)response.StatusCode}");
    Console.WriteLine(summary.ReplaceLineEndings(" "));
}

static async Task PutImage(IAmazonS3 client, string bucket, string key, Image source, int width, int height, bool crop, bool usePublicReadAcl, bool disablePayloadSigning)
{
    using var clone = source.Clone(x =>
    {
        if (crop)
            x.Resize(new ResizeOptions { Mode = ResizeMode.Crop, Position = AnchorPositionMode.Center, Size = new Size(width, height) });
        else
            x.Resize(width, height);
    });
    await using var stream = new MemoryStream();
    await clone.SaveAsync(stream, new JpegEncoder { Quality = 60 });
    stream.Position = 0;

    var putRequest = new PutObjectRequest
    {
        BucketName = bucket,
        Key = key,
        InputStream = stream,
        ContentType = "image/jpeg"
    };
    ApplyUploadOptions(putRequest, usePublicReadAcl, disablePayloadSigning);
    await client.PutObjectAsync(putRequest);
}

static void ApplyUploadOptions(PutObjectRequest putRequest, bool usePublicReadAcl, bool disablePayloadSigning)
{
    if (usePublicReadAcl)
        putRequest.CannedACL = S3CannedACL.PublicRead;

    putRequest.DisablePayloadSigning = disablePayloadSigning;
}
