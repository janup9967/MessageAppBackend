using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace MessageApp.Helpers // 👈 Match your namespace
{
    public class CustomUserIdProvider : IUserIdProvider
    {
        public string GetUserId(HubConnectionContext connection)
        {
            return connection.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }
}