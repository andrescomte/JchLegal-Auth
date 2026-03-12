namespace AuthService.Domain.SeedWork
{
    public static class PasswordPolicy
    {
        public static void Validate(string password)
        {
            if (password.Length < 8)
                throw new DomainException("La contraseña debe tener al menos 8 caracteres.");

            if (!password.Any(char.IsUpper))
                throw new DomainException("La contraseña debe contener al menos una letra mayúscula.");

            if (!password.Any(char.IsLower))
                throw new DomainException("La contraseña debe contener al menos una letra minúscula.");
        }
    }
}
