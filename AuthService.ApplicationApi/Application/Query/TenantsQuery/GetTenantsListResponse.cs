namespace AuthService.ApplicationApi.Application.Query.TenantsQuery
{
    public class GetTenantsListResponse
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
