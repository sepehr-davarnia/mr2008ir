using Atelier.Domain.Enums;

public class Lead : Entity
{
    public string Name { get; private set; }
    public string Email { get; private set; }
    public string? Message { get; private set; }
    public LeadStatus Status { get; private set; }

    protected Lead()
    {
        Name = "Default Name";
        Email = "default@d.com";
    }


    public Lead(string name, string email, string? message = null)
    {
        Name = name;
        Email = email;
        Message = message;
        Status = LeadStatus.New;
    }

    public void UpdateContactInfo(string name, string email, string? message)
    {
        Name = name;
        Email = email;
        Message = message;
        MarkUpdated();
    }

    public void UpdateStatus(LeadStatus status)
    {
        Status = status;
        MarkUpdated();
    }
}
