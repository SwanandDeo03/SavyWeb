using Microsoft.AspNetCore.Mvc;

namespace SavyWeb.Models.Media
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileUploadController : ControllerBase
    {
        public FileUploadController()
        {
            if (!Directory.Exists(UploadFolder))
            {
                Directory.CreateDirectory(UploadFolder);
            }
        }

        public string UploadFolder { get; } = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

        [HttpPost("uploadlocal")]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            var filePath = Path.Combine(UploadFolder, file.FileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return Ok(new { Message = "File uploaded successfully", file.FileName, FilePath = filePath });
        }
    }
}
