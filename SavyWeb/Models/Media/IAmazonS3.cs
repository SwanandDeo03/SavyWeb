
namespace SavyWeb.Models.Media
{
    internal interface IAmazonS3
    {
        Task PutObjectAsync(PutObjectRequest request);
    }
}