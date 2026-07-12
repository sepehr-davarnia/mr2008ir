using Atelier.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Atelier.Infrastructure.Auth;

public class AdminAuthService
{
    private readonly AtelierDbContext _dbContext;
    private readonly PasswordHasher<AdminUser> _passwordHasher;

    public AdminAuthService(AtelierDbContext dbContext)
    {
        _dbContext = dbContext;
        _passwordHasher = new PasswordHasher<AdminUser>();
    }

    public async Task<AdminUser?> AuthenticateAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            return null;
        }

        var adminUser = await _dbContext.AdminUsers
            .SingleOrDefaultAsync(
                user => user.Username == username && user.IsActive,
                cancellationToken);

        if (adminUser is null)
        {
            return null;
        }

        var result = _passwordHasher.VerifyHashedPassword(adminUser, adminUser.PasswordHash, password);
        return result == PasswordVerificationResult.Success || result == PasswordVerificationResult.SuccessRehashNeeded
            ? adminUser
            : null;
    }
}
