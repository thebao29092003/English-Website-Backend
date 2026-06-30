using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using English.Website.Api.Dtos.AIDtos.AzureSpeechDto;
using English.Website.Api.Dtos.BackendPythonDtos;
using English.Website.Api.Dtos.CloudinaryDtos;
using English.Website.Api.Extensions.Helpers;
using English.Website.Application.Services.IServices;
using English.Website.Domain.DatabaseContext;
using English.Website.Domain.Entities;
using English.Website.Domain.Entities.AI;

namespace English.Website.Application.Services
{
    public class CloudinaryService : ICloudinaryService
    {
        private readonly Cloudinary _cloudinary;
        private readonly EnglishDBContext _dbContext;
        private readonly IUserContextService _userContextService;
        private readonly IAssemblyAIService _assemblyAIService;
        private readonly IBackendPythonService _backendPythonService;

        public CloudinaryService(
            Cloudinary cloudinary,
            EnglishDBContext dbContext,
            IAssemblyAIService assemblyAIService,
            IBackendPythonService backendPythonService,
            IUserContextService userContextService)
        {
            _cloudinary = cloudinary;
            _dbContext = dbContext;
            _userContextService = userContextService;
            _assemblyAIService = assemblyAIService;
            _backendPythonService = backendPythonService;
        }

        public async Task<string?> UploadFileAsync(UploadRequestDto requestDto)
        {
            #region upload file
            const long MaxFileSize = 5 * 1024 * 1024; // 5 MB

            var user = await _userContextService.GetUserDetail();
            var file = requestDto.File;
            if (file == null || file.Length == 0)
            {
                throw new BadRequestException("Invalid file");
            }
            if (file.Length > MaxFileSize)
            {
                throw new BadRequestException("File must smaller 5MB");
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
            #endregion

            if (uploadResult.Error != null)
            {
                throw new BadRequestException($"Error from Cloudinary: {uploadResult.Error.Message}");
            }

            var secureUrl = uploadResult.SecureUrl?.ToString();

            if (string.IsNullOrEmpty(secureUrl))
            {
                throw new BadRequestException("Invalid SecureUrl");
            }

            var assemblyAIRequestDto = new AssemblyAIRequestDto
            {
                AudioUrl = secureUrl,
            };

            var recordingId = Guid.NewGuid();

            // gọi api Assembly AI
            var transcriptId = await _assemblyAIService.SubmitAudioAssemblyAI(assemblyAIRequestDto);

            await _dbContext.AISpeechToText.AddAsync(new AISpeechToText
            {
                RecordingId = recordingId,
                UserId = user.UserId,
                AssemblyAIId = transcriptId,
                TypeAnalyse = requestDto.TypeAnalyse
            });

            // gọi api wav2vec2 python param RecordingId
            await _backendPythonService.ConvertAudioToPhonetic(new RequestConvertAudioPhoneticDto
            {
                AudioPath  = secureUrl,
                RecordingId = recordingId,
                CallbackUrl = "https://localhost:7025/api/backend-python/phonetic-webhook",
                TranscriptId = transcriptId
            });


            // lưu database
            await _dbContext.Recording.AddAsync(new Recording
            {
                RecordingId = recordingId,
                UserId = user.UserId,
                CloudinaryPublicId = uploadResult.PublicId,
                Url = secureUrl,
                FileName = uploadResult.DisplayName,
                FileSize = uploadResult.Bytes,
                FileType = uploadResult.Format,
                Duration = uploadResult.Duration,
            });
            await _dbContext.SaveChangesAsync();

            return transcriptId;

        }
    }
}
