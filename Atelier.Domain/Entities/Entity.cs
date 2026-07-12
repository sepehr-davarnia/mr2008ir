public abstract class Entity
{
    public int Id { get; protected set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    protected Entity()
    {
        CreatedAt = DateTime.UtcNow;
    }

    protected void MarkUpdated()
    {
        UpdatedAt = DateTime.UtcNow;
    }
}
