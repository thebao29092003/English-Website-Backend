using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using English.Website.Api.Dtos.CloudinaryDtos;
using English.Website.Api.Extensions.Helpers;
using English.Website.Application.Services.IServices;

namespace English.Website.Application.Services
{
    public class CloudinaryService : ICloudinaryService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryService(Cloudinary cloudinary)
        {
            _cloudinary = cloudinary;
        }

        public async Task<UploadResponseDto> UploadFileAsync(UploadRequestDto requestDto)
        {
            var file = requestDto.File;
            if (file == null || file.Length == 0)
            {
                throw new BadRequestException("Invalid file");
            }

            var allowedExtensions = new[] { ".mp3", ".wav", ".m4a", ".aac", ".wma" };
            var extension = Path.GetExtension(file.FileName)?.ToLowerInvariant();
            if (string.IsNullOrEmpty(extension) || !allowedExtensions.Contains(extension))
            {
                throw new BadRequestException("Unsupported file format");
            }

            using var stream = file.OpenReadStream();

            // Khởi tạo tham số Upload với ResourceType = "auto" để hỗ trợ tự động phát hiện loại file
            var uploadParams = new VideoUploadParams
            {
                File = new FileDescription(file.FileName, stream),

                // Các thông số cấu hình theo yêu cầu:
                Overwrite = false,
                UseFilename = false,
                UniqueFilename = true,
                UseFilenameAsDisplayName = true,
                UseAssetFolderAsPublicIdPrefix = false,
                Type = "upload",
                AssetFolder = "Audio-Test",
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.Error != null)
            {
                throw new BadRequestException($"Error from Cloudinary: {uploadResult.Error.Message}");
            }

            return new UploadResponseDto
            {
                PublicId = uploadResult.PublicId,
                SecureUrl = uploadResult.SecureUrl?.ToString(),
                DisplayName = uploadResult.DisplayName,
                AssetFolder = uploadResult.AssetFolder,
                ResourceType = uploadResult.ResourceType
            };
        }
    }
}
