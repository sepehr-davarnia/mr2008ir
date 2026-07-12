public class Tag : Entity
{
    public string Name { get; private set; }
    public string Slug { get; private set; }

    protected Tag()
    {
        Name = "N/A";
        Slug = "N/A";
    }

    public Tag(string name, string slug)
    {
        Name = name;
        Slug = slug;
    }

    public void UpdateName(string name)
    {
        Name = name;
        MarkUpdated();
    }
}
