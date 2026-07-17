using English.Website.Api.Dtos.AudioDtos;
using English.Website.Api.Extensions.Helpers;
using English.Website.Application.Services.IServices;
using English.Website.Domain.DatabaseContext;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Nodes;
using Hangfire;

namespace English.Website.Application.Services
{
    public class AudioService
    {
        private readonly EnglishDBContext _dbContext;
        private readonly IUserContextService _userContextService;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly IBackgroundJobClient _backgroundJobClient;

        public AudioService(
            EnglishDBContext dbContext,
            IUserContextService userContextService,
            ICloudinaryService cloudinaryService,
            IBackgroundJobClient backgroundJobClient)
        {
            _dbContext = dbContext;
            _userContextService = userContextService;
            _cloudinaryService = cloudinaryService;
            _backgroundJobClient = backgroundJobClient;
        }

        public async Task<List<AudioRecordingDto>> GetUserRecordingsAsync()
        {
            // Lấy thông tin user từ UserContext
            var userDetail = await _userContextService.GetUserDetail();
            var userId = userDetail.UserId;

            // Truy vấn database lấy dữ liệu từ 3 bảng và map sang DTO
            var recordings = await _dbContext.Recording
                .Where(r => r.UserId == userId && !r.IsDeleted)
                .Select(r => new AudioRecordingDto
                {
                    RecodingId = r.RecordingId,
                    FileName = r.FileName,
                    FileSize = r.FileSize,
                    FileType = r.FileType,
                    FileUrl = r.Url,
                    Duration = r.Duration,
                    CreatedAt = r.CreatedAt,
                    SpeechToText = r.AISpeechToText == null ? null : new AISpeechToTextDto
                    {
                        AITranscript = r.AISpeechToText.AITranscript,
                        OverallConfidence = r.AISpeechToText.OverallConfidence,
                        FluencyScore = r.AISpeechToText.FluencyScore,
                        PronunciationScore = r.AISpeechToText.PronunciationScore != null
                            ? r.AISpeechToText.PronunciationScore * 100
                            : null
                    },
                    Analysis = (r.AISpeechToText == null || r.AISpeechToText.AIAnalysis == null) ? null : new AIAnalysisDto
                    {
                        OverallGrammarScore = r.AISpeechToText.AIAnalysis.OverallGrammarScore,
                        OverallVocabScore = r.AISpeechToText.AIAnalysis.OverallVocabScore
                    }
                })
                .ToListAsync();

            return recordings;
        }

        public async Task<AudioDetailDto> GetAudioDetailAsync(string recordingId)
        {
            // Lấy thông tin user từ UserContext
            var userDetail = await _userContextService.GetUserDetail();
            var userId = userDetail.UserId;

            var recording = await _dbContext.Recording
                .Include(r => r.AISpeechToText)
                .FirstOrDefaultAsync(r => r.UserId == userId && r.RecordingId == Guid.Parse(recordingId) && !r.IsDeleted)
                ?? throw new BadRequestException("Recording not found.");

            var result = new AudioDetailDto
            {
                RecodingId = recording.RecordingId,
                FileName = recording.FileName,
                FileSize = recording.FileSize,
                FileType = recording.FileType,
                FileUrl = recording.Url,
                Duration = recording.Duration,
                CreatedAt = recording.CreatedAt
            };

            if (recording.AISpeechToText != null)
            {
                var AIAnalysis = await _dbContext.AIAnalysis
                    .FirstOrDefaultAsync(aiAnalysis => aiAnalysis.AISpeechToText == recording.AISpeechToText);

                if (AIAnalysis != null)
                {
                    result.OverallGrammarScore = AIAnalysis.OverallGrammarScore;
                    result.OverallVocabScore = AIAnalysis.OverallVocabScore;
                    if (!string.IsNullOrEmpty(AIAnalysis.AnalysisContentJson))
                    {
                        try
                        {
                            result.AnalysisContentJson = JsonNode.Parse(AIAnalysis.AnalysisContentJson);
                        }
                        catch
                        {
                            result.AnalysisContentJson = null;
                        }
                    }
                }
                result.AITranscript = recording.AISpeechToText.AITranscript;
                result.OverallConfidence = recording.AISpeechToText.OverallConfidence;
                result.FluencyScore = recording.AISpeechToText.FluencyScore;
                result.PronunciationScore = recording.AISpeechToText.PronunciationScore;
                result.WordPerMinute = recording.AISpeechToText.WordPerMinute;
                result.TypeAnalyse = recording.AISpeechToText.TypeAnalyse;

                if (!string.IsNullOrEmpty(recording.AISpeechToText.FluencyErrorsJson))
                {
                    try
                    {
                        result.FluencyErrors = JsonNode.Parse(recording.AISpeechToText.FluencyErrorsJson);
                    }
                    catch
                    {
                        result.FluencyErrors = null;
                    }
                }

                if (!string.IsNullOrEmpty(recording.AISpeechToText.WordsJson))
                {
                    try
                    {
                        result.WordsJson = JsonNode.Parse(recording.AISpeechToText.WordsJson);
                    }
                    catch
                    {
                        result.WordsJson = null;
                    }
                }

                if (!string.IsNullOrEmpty(recording.AISpeechToText.WordsPronunciationScore))
                {
                    try
                    {
                        result.WordsPronunciationScore = JsonNode.Parse(recording.AISpeechToText.WordsPronunciationScore);
                    }
                    catch
                    {
                        result.WordsPronunciationScore = null;
                    }
                }
            }

            return result;
        }

        public async Task SoftDeleteRecordingAsync(string recordingId)
        {
            var userDetail = await _userContextService.GetUserDetail();
            var userId = userDetail.UserId;

            if (!Guid.TryParse(recordingId, out var parsedRecordingId))
            {
                throw new BadRequestException("Invalid recording ID format.");
            }

            var recording = await _dbContext.Recording
                .FirstOrDefaultAsync(r => r.UserId == userId && r.RecordingId == parsedRecordingId && !r.IsDeleted)
                ?? throw new BadRequestException("Recording not found or already deleted.");

            // 1. Delete physical file from Cloudinary in background via Hangfire
            if (!string.IsNullOrEmpty(recording.CloudinaryPublicId))
            {
                _backgroundJobClient.Enqueue<ICloudinaryService>(service =>
                    service.DeleteFileAsync(recording.CloudinaryPublicId));
            }

            // 2. Perform soft delete by flagging IsDeleted and clearing references
            recording.IsDeleted = true;
            recording.DeletedAt = DateTime.UtcNow;
            recording.Url = null;
            recording.CloudinaryPublicId = null;

            await _dbContext.SaveChangesAsync();
        }
    }
}
