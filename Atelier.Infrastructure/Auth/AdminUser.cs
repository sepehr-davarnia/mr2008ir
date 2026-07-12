namespace Atelier.Infrastructure.Auth;

public class AdminUser
{
    protected AdminUser()
    {
    }

    public AdminUser(string username, string passwordHash, bool isActive, DateTime createdAt)
    {
        Username = username;
        PasswordHash = passwordHash;
        IsActive = isActive;
        CreatedAt = createdAt;
    }

    public int Id { get; private set; }
    public string Username { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
}
