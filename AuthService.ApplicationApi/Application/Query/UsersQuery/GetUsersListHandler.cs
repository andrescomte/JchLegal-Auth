using MediatR;
using AuthService.Domain.Repository;
using AuthService.Domain.SeedWork;

namespace AuthService.ApplicationApi.Application.Query.UsersQuery
{
    public class GetUsersListHandler : IRequestHandler<GetUsersListRequest, PagedResponse<GetUsersListResponse>>
    {
        private readonly IUsersRepository _usersRepository;

        public GetUsersListHandler(IUsersRepository usersRepository)
        {
            _usersRepository = usersRepository;
        }

        public async Task<PagedResponse<GetUsersListResponse>> Handle(GetUsersListRequest request, CancellationToken cancellationToken)
        {
            var (users, totalCount) = await _usersRepository.GetAllAsync(request.Page, request.PageSize);

            return new PagedResponse<GetUsersListResponse>
            {
                Data = users.Select(u => new GetUsersListResponse
                {
                    Id = u.Id,
                    Username = u.Username,
                    Email = u.Email,
                    IsActive = u.IsActive,
                    CreatedAt = u.CreatedAt,
                    CurrentStatus = u.UserStatusHistory.FirstOrDefault()?.Status?.Code,
                    Roles = u.UserRoles.Select(ur => ur.Role.Code)
                }),
                Page = request.Page,
                PageSize = request.PageSize,
                TotalCount = totalCount
            };
        }
    }
}
