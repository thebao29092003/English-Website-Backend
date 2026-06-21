using English.Website.Api.Dtos.AIDtos.DeepSeekDto;
using English.Website.Api.Dtos.CloudinaryDtos;

namespace English.Website.Application.Services.IServices
{

    public interface ICloudinaryService
    {
        Task<UploadResponseDto> UploadFileAsync(UploadRequestDto requestDto);
    }
}
