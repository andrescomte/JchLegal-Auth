namespace AuthService.ApplicationApi.Application.Query.RolesQuery
{
    public class RolesListResponse
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
}
