public class Category : Entity
{
    private readonly List<Category> _children = new();

    public string Name { get; private set; }
    public string Slug { get; private set; }
    public Category? Parent { get; private set; }
    public int? MediaId { get; private set; }
    public IReadOnlyCollection<Category> Children => _children.AsReadOnly();

    protected Category()
    {
        Name = "Default Category";
        Slug = "default-category";
    }

    public Category(string name, string slug, Category? parent = null, int? mediaId = null)
    {
        Name = name;
        Slug = slug;
        SetParent(parent);
        MediaId = mediaId;
    }

    public void UpdateName(string name)
    {
        Name = name;
        MarkUpdated();
    }

    public void SetParent(Category? parent)
    {
        Parent = parent;
        MarkUpdated();
    }

    public void AddChild(Category child)
    {
        if (_children.Contains(child))
        {
            return;
        }

        _children.Add(child);
        child.SetParent(this);
        MarkUpdated();
    }

    public void UpdateMedia(int? mediaId)
    {
        MediaId = mediaId;
        MarkUpdated();
    }

    public void RemoveChild(Category child)
    {
        if (_children.Remove(child))
        {
            child.SetParent(null);
            MarkUpdated();
        }
    }
}
