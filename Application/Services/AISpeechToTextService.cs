using English.Website.Api.Dtos.BackendPythonDtos;
using English.Website.Api.Extensions.Helpers;
using English.Website.Domain.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace English.Website.Application.Services
{
    public class AISpeechToTextService
    {
        private readonly EnglishDBContext _dbContext;

        public AISpeechToTextService (EnglishDBContext dBContext)
        {
            _dbContext = dBContext;
        }

        public async Task Update(PythonPhonemeWebhookDto webhookData)
        {
            var sttRecord = _dbContext.AISpeechToText.FirstOrDefault(s => s.RecordingId == Guid.Parse(webhookData.RecordingId))
                ?? throw new BadRequestException("Not found AISpeechToText by RecordingId");

            // Cập nhật chuỗi âm vị thu được từ Wav2Vec2 vào DB
            sttRecord.PhoneticTranscript = webhookData.Phonemes;
            await _dbContext.SaveChangesAsync();
        }
    }
}
