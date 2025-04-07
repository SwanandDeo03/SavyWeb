namespace SavyWeb.Files
{
    public class Upload_Files
    {
        public int Id { get; set; }
        public required IFormFile Files { get; set; }
        public required string Name { get; set; }
    }
}
