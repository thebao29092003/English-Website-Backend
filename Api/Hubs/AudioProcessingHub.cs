using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace English.Website.Api.Hubs
{
    [Authorize]
    public class AudioProcessingHub : Hub
    {
        /// <summary>
        /// Khi client kết nối thành công (đã xác thực JWT),
        /// tự động thêm vào group theo UserId từ token claims.
        /// Frontend không cần gọi thêm method nào. override tính đa hình
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst("UserId")?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, userId.ToLowerInvariant());
            }

            await base.OnConnectedAsync();
        }
    }
}
