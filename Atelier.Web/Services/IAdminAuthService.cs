using Atelier.Infrastructure.Auth;

namespace Atelier.Web.Services;

public interface IAdminAuthService
{
    Task<AdminUser?> AuthenticateAsync(string username, string password, CancellationToken cancellationToken = default);
}
