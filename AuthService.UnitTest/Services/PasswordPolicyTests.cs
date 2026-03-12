using AuthService.Domain.SeedWork;

namespace AuthService.UnitTest.Services
{
    public class PasswordPolicyTests
    {
        [Fact]
        public void Validate_Passes_With_Valid_Password()
        {
            var ex = Record.Exception(() => PasswordPolicy.Validate("Passw0rd"));
            Assert.Null(ex);
        }

        [Fact]
        public void Validate_Throws_When_Too_Short()
        {
            var ex = Assert.Throws<DomainException>(() => PasswordPolicy.Validate("Ab1"));
            Assert.Contains("8 caracteres", ex.Message);
        }

        [Fact]
        public void Validate_Throws_When_No_Uppercase()
        {
            var ex = Assert.Throws<DomainException>(() => PasswordPolicy.Validate("password123"));
            Assert.Contains("mayúscula", ex.Message);
        }

        [Fact]
        public void Validate_Throws_When_No_Lowercase()
        {
            var ex = Assert.Throws<DomainException>(() => PasswordPolicy.Validate("PASSWORD123"));
            Assert.Contains("minúscula", ex.Message);
        }

        [Theory]
        [InlineData("Abcdefgh")]
        [InlineData("Password1")]
        [InlineData("MiClaveSegura")]
        public void Validate_Passes_With_Various_Valid_Passwords(string password)
        {
            var ex = Record.Exception(() => PasswordPolicy.Validate(password));
            Assert.Null(ex);
        }
    }
}
