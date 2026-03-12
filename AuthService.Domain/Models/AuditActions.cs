namespace AuthService.Domain.Models
{
    public static class AuditActions
    {
        public const string LoginSuccess  = "LOGIN_SUCCESS";
        public const string LoginFailed   = "LOGIN_FAILED";
        public const string UserRegistered = "USER_REGISTERED";
        public const string Logout        = "LOGOUT";
        public const string RoleUpdated      = "ROLE_UPDATED";
        public const string PasswordChanged        = "PASSWORD_CHANGED";
        public const string PasswordResetRequested = "PASSWORD_RESET_REQUESTED";
        public const string PasswordResetCompleted = "PASSWORD_RESET_COMPLETED";
    }
}
