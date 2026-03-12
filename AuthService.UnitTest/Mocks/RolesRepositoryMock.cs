using System.Diagnostics.CodeAnalysis;
using AuthService.Domain.Models;
using AuthService.Domain.Repository;

namespace AuthService.UnitTest.Mocks
{
    [ExcludeFromCodeCoverage]
    public class RolesRepositoryMock : IRolesRepository
    {
        private readonly List<Roles> _roles = new()
        {
            new Roles { Id = 1, Code = "ADMIN", Name = "Administrador" },
            new Roles { Id = 2, Code = "USER",  Name = "Usuario" },
            new Roles { Id = 3, Code = "GUEST", Name = "Invitado" }
        };

        public Roles Create(Roles rolesObject) => rolesObject;

        public async Task<(List<Roles> Items, int TotalCount)> ReadAll(int page, int pageSize)
        {
            await Task.CompletedTask;
            var items = _roles.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            return (items, _roles.Count);
        }

        public async Task<Roles?> GetByIdAsync(int id)
        {
            await Task.CompletedTask;
            return _roles.FirstOrDefault(r => r.Id == id);
        }

        public async Task<Roles?> GetByCodeAsync(string code)
        {
            await Task.CompletedTask;
            return _roles.FirstOrDefault(r => r.Code == code);
        }

        public async Task<Roles> CreateRoleAsync(Roles role)
        {
            await Task.CompletedTask;
            role.Id = _roles.Max(r => r.Id) + 1;
            _roles.Add(role);
            return role;
        }

        public async Task<Roles?> UpdateRoleAsync(int id, string name)
        {
            await Task.CompletedTask;
            var role = _roles.FirstOrDefault(r => r.Id == id);
            if (role == null)
                return null;

            role.Name = name;
            return role;
        }

        public async Task<bool> ExistsAsync(string code)
        {
            await Task.CompletedTask;
            return _roles.Any(r => r.Code == code);
        }
    }
}
