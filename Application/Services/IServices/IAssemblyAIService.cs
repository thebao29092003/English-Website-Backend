using English.Website.Api.Dtos.AIDtos.DeepSeekDto;

namespace English.Website.Application.Services.IServices
{

    public interface IAssemblyAIService
    {

        /// <summary>
        /// Gọi API DeepSeek với cấu trúc JSON Object và tự động ép kiểu kết quả trả về dạng T.
        /// </summary>
        /// <typeparam name="T">Kiểu dữ liệu mong muốn nhận về (đã lọc theo Schema)</typeparam>
        Task<DeepSeekResult<T>> CallApiAsync<T>(string systemPrompt, string userPrompt, Guid userId);
        Task<ReceiveDataFromDeepseekDto> CallDeepSeekApi(TranscriptRequestDto deepSeekRequest);

    }
}
