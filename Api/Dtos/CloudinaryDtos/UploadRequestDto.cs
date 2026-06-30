
using English.Website.Domain.Constants;

namespace English.Website.Api.Dtos.CloudinaryDtos
{
    public class UploadRequestDto
    {
        public IFormFile File { get; set; } = null!;
        public TypeAnalyse TypeAnalyse { get; set; }
    }
}
