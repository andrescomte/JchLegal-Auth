using MediatR;
using AuthService.Domain.Repository;

namespace AuthService.ApplicationApi.Application.Query.UsersQuery
{
    public class GetUserByIdHandler : IRequestHandler<GetUserByIdRequest, GetUsersListResponse?>
    {
        private readonly IUsersRepository _usersRepository;

        public GetUserByIdHandler(IUsersRepository usersRepository)
        {
            _usersRepository = usersRepository;
        }

        public async Task<GetUsersListResponse?> Handle(GetUserByIdRequest request, CancellationToken cancellationToken)
        {
            var user = await _usersRepository.GetByIdAsync(request.Id);
            if (user == null) return null;

            return new GetUsersListResponse
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                CurrentStatus = user.UserStatusHistory.FirstOrDefault()?.Status?.Code,
                Roles = user.UserRoles.Select(ur => ur.Role.Code)
            };
        }
    }
}
