using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace BuildingBlocks.Common.Services
{
    public sealed class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
    {
        private ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;

        public bool IsAuthenticated => User?.Identity?.IsAuthenticated == true;

        public string? UserId => User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        public string? Username => User?.FindFirst("preferred_username")?.Value;

        public string? Email => User?.FindFirst("email")?.Value;

        public IReadOnlyCollection<string> Roles =>
            User?
                .FindAll(ClaimTypes.Role)
                .Select(x => x.Value)
                .ToArray()
            ?? [];
    }
}