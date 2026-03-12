using MediatR;
using AuthService.Domain.Repository;
using AuthService.Domain.SeedWork;

namespace AuthService.ApplicationApi.Application.Query.RolesQuery
{
    public class RolesListHandler : IRequestHandler<RolesListRequest, PagedResponse<RolesListResponse>>
    {
        private readonly IRolesRepository _rolesRepository;

        public RolesListHandler(IRolesRepository rolesRepository)
        {
            _rolesRepository = rolesRepository;
        }

        public async Task<PagedResponse<RolesListResponse>> Handle(RolesListRequest request, CancellationToken cancellationToken)
        {
            var (roles, totalCount) = await _rolesRepository.ReadAll(request.Page, request.PageSize);

            return new PagedResponse<RolesListResponse>
            {
                Data = roles.Select(r => new RolesListResponse { Id = r.Id, Code = r.Code, Name = r.Name }),
                Page = request.Page,
                PageSize = request.PageSize,
                TotalCount = totalCount
            };
        }
    }
}
