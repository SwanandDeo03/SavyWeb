using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Net.Mime;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

public class S3Service
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;
    private string contentType;

    public S3Service(IConfiguration configuration)
    {
        _s3Client = new AmazonS3Client(
            configuration["AWS:AccessKey"],
            configuration["AWS:SecretKey"],
            RegionEndpoint.GetBySystemName(configuration["AWS:Region"])
        );

        _bucketName = configuration["AWS:BucketName"] ?? throw new ArgumentNullException(nameof(configuration), "Bucket name cannot be null");
    }

    public async Task UploadFileAsync(Stream fileStream, string fileName)
    {
        string contentType;

        var putRequest = new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = fileName,
            InputStream = fileStream,
            ContentType = "image/jpeg";
            CannedACL = S3CannedACL.Private // Keep the file private
        };

        await _s3Client.PutObjectAsync(putRequest);
    }

    // Generate a Pre-Signed URL to allow temporary access to the file
    public string GetPreSignedUrl(string fileName, int expiryInMinutes = 1440)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _bucketName,
            Key = fileName,
            Expires = DateTime.UtcNow.AddMinutes(expiryInMinutes), // Set expiration time
            Protocol = Protocol.HTTPS
        };

        return _s3Client.GetPreSignedURL(request);
    }
}
