using MediatR;
using AuthService.Domain.Repository;
using AuthService.Domain.SeedWork;

namespace AuthService.ApplicationApi.Application.Query.TenantsQuery
{
    public class GetTenantsListHandler : IRequestHandler<GetTenantsListRequest, PagedResponse<GetTenantsListResponse>>
    {
        private readonly ITenantsRepository _tenantsRepository;

        public GetTenantsListHandler(ITenantsRepository tenantsRepository)
        {
            _tenantsRepository = tenantsRepository;
        }

        public async Task<PagedResponse<GetTenantsListResponse>> Handle(GetTenantsListRequest request, CancellationToken cancellationToken)
        {
            var (tenants, totalCount) = await _tenantsRepository.GetAllAsync(request.Page, request.PageSize);

            return new PagedResponse<GetTenantsListResponse>
            {
                Data = tenants.Select(t => new GetTenantsListResponse
                {
                    Id = t.Id,
                    Code = t.Code,
                    Name = t.Name,
                    IsActive = t.IsActive,
                    CreatedAt = t.CreatedAt
                }),
                Page = request.Page,
                PageSize = request.PageSize,
                TotalCount = totalCount
            };
        }
    }
}
