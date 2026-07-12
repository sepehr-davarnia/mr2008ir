using Atelier.Infrastructure.Auth;

namespace Atelier.Web.Services;

public class AdminAuthServiceAdapter : IAdminAuthService
{
    private readonly AdminAuthService _adminAuthService;

    public AdminAuthServiceAdapter(AdminAuthService adminAuthService)
    {
        _adminAuthService = adminAuthService;
    }

    public Task<AdminUser?> AuthenticateAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        return _adminAuthService.AuthenticateAsync(username, password, cancellationToken);
    }
}
