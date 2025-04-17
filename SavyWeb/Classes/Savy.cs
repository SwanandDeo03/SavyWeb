using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Net.Mime;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

public class Savy
{
    private readonly IAmazonS3 _s3Client;

    private readonly string _bucketName;

    public Savy(IConfiguration configuration)
    {
        _s3Client = new AmazonS3Client(
            configuration["AWS:AccessKey"],
            configuration["AWS:SecretKey"],
            RegionEndpoint.GetBySystemName(configuration["AWS:Region"])
        );

        _bucketName = configuration["AWS:BucketName"] ?? throw new ArgumentNullException(nameof(configuration), "Bucket name cannot be null");
    }

    public async Task UploadFileAsync(Stream fileStream, string fileName, string contentType)
    {
        var putRequest = new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = fileName,
            InputStream = fileStream,
            ContentType = contentType,
            Metadata =
            {
                ["Content-Type"] = contentType,
                ["Content-Disposition"] = "inline"
            },
            CannedACL = S3CannedACL.Private
        };

        await _s3Client.PutObjectAsync(putRequest);
    }

    // Generate a Pre-Signed URL to allow temporary access to the file
    public string GetPreSignedUrl(string fileName, int expiryInOneDay = 1440)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _bucketName,
            Key = fileName,
            Expires = DateTime.UtcNow.AddMinutes(expiryInOneDay), // 1440 minutes = 1 day
            Protocol = Protocol.HTTPS,
            ResponseHeaderOverrides = new ResponseHeaderOverrides() // Fixing the CS1526 error
            {
                ContentDisposition = "inline"

            }
        };

        return _s3Client.GetPreSignedURL(request);
    }


}
