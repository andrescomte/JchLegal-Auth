namespace AuthService.ApplicationApi.Application.Command.Role
{
    public class UpdateRoleResponse
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
}
