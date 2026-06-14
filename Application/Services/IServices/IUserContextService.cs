using English.Website.Api.Dtos.UserDtos;

namespace English.Website.Application.Services.IServices
{
    public interface IUserContextService
    {
       Task<UserContextDtos> GetUserDetail();
    }
}
