//using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;

namespace SavyWeb.Models.Media
{
    [Route("api/[controller]")]
    [ApiController]
    public class UploadController : ControllerBase
    {
        private readonly S3Service _s3Service;
        private readonly string _bucketName = "savymedia";
        private readonly string _region = "ap-south-1";

        public UploadController(S3Service s3Service)
        {
            _s3Service = s3Service;
        }

        //[HttpPost]
        [Route("S3Upload")]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                //stream.Position = 0;

                await _s3Service.UploadFileAsync(stream, file.FileName);

                string presignedUrl = _s3Service.GetPreSignedUrl(file.FileName, 120);

                return Ok(new
                {
                    Message = "File uploaded successfully.",
                    FileName = file.FileName,
                    ObjectUrl = presignedUrl
                });
            }
        }
    }
}
