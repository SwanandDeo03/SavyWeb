using Amazon.S3;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace SavyWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SavyController(IAmazonS3 s3Client, IConfiguration configuration, Savy s3Service) : ControllerBase
    {
        private readonly IAmazonS3 _s3Client = s3Client;
        private readonly string _bucketName = configuration["AWS:BucketName"] ?? "savymedia";
        private readonly Savy _s3Service = s3Service;

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
