using English.Website.Api.Dtos.TurnstileDtos;

namespace English.Website.Application.Services.IServices
{
    public interface ITurnstileService
    {
        Task<bool> VerifyTokenAsync(string? token, string? remoteIp = null);
    }
}
