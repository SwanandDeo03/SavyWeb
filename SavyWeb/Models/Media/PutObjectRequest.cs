
namespace SavyWeb.Models.Media
{
    internal class PutObjectRequest
    {
        public PutObjectRequest()
        {
        }

        public string BucketName { get; set; }
        public string Key { get; set; }
        public Stream InputStream { get; set; }
        public object Metadata { get; internal set; }
        public string ContentType { get; internal set; }
    }
}