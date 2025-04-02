using System.Security.Cryptography.X509Certificates;

namespace SavyWeb.Models.Media
{
    public class Media
    {
        public required string Id { get; set; }
        public required string Name { get; set; }
        public required IFormFile Files { get; set; }
        public required string Description { get; set; }
        public bool IsCompleted { get; set; }
    }
}
