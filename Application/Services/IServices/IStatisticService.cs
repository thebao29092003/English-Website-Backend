using English.Website.Api.Dtos.StatisticDtos;

namespace English.Website.Application.Services.IServices
{
    public interface IStatisticService
    {
        Task<UserAverageScoreDto> GetUserAverageScores();
    }
}
