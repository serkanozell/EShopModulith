namespace BuildingBlocks.Common.Services
{
    public interface ICurrentUser
    {
        string? UserId { get; }
        string? Username { get; }
        string? Email { get; }
        bool IsAuthenticated { get; }
        IReadOnlyCollection<string> Roles { get; }
    }
}