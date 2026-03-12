namespace AuthService.ApplicationApi.Application.Query.UsersQuery
{
    public class GetUsersListResponse
    {
        public long Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CurrentStatus { get; set; }
        public IEnumerable<string> Roles { get; set; } = [];
    }
}
