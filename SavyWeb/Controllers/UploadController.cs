using Amazon.S3;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Amazon.Auth.AccessControlPolicy.ActionIdentifiers;
namespace SavyWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UploadController : ControllerBase
    {
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucketName;
        private readonly S3Service _s3Service;

        public UploadController(IAmazonS3 s3Client, IConfiguration configuration, S3Service s3Service)
        {
            _s3Client = s3Client;
            _bucketName = configuration["AWS:BucketName"] ?? "savymedia";
            _s3Service = s3Service;
        }

        [HttpPost]
        [Route("S3Upload")]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                stream.Position = 0; // Reset the stream position

                // Upload file to S3 and get the file name
                await _s3Service.UploadFileAsync(stream, file.FileName, file.ContentType);

                // Get a Pre-Signed URL with 1-Day expiry
                string presignedUrl = _s3Service.GetPreSignedUrl(file.FileName, 1440);

                return Ok(new
                {
                    Message = "File uploaded successfully.",
                    file.FileName,
                    ObjectUrl = presignedUrl // Use this to access the file
                });
            }
        }
    }
}
