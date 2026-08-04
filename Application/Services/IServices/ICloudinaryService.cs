using English.Website.Api.Dtos.AIDtos.DeepSeekDto;
using English.Website.Api.Dtos.CloudinaryDtos;

namespace English.Website.Application.Services.IServices
{

    public interface ICloudinaryService
    {
        Task<Guid?> UploadFileAsync(UploadRequestDto requestDto);
        Task<bool> DeleteFileAsync(string publicId);
    }
}
