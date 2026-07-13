using System.Text.Json;

namespace Atelier.Web.Services;

public interface ICartService
{
    IReadOnlyDictionary<int, int> GetItems();
    int Count { get; }
    void Add(int productId, int quantity = 1);
    void Update(int productId, int quantity);
    void Remove(int productId);
    void Clear();
}

public sealed class CartService : ICartService
{
    private const string SessionKey = "mr2008.cart";
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CartService(IHttpContextAccessor httpContextAccessor) => _httpContextAccessor = httpContextAccessor;

    public int Count => GetMutableItems().Values.Sum();

    public IReadOnlyDictionary<int, int> GetItems() => GetMutableItems();

    public void Add(int productId, int quantity = 1)
    {
        if (productId <= 0) return;
        var items = GetMutableItems();
        items[productId] = Math.Clamp(items.GetValueOrDefault(productId) + quantity, 1, 20);
        Save(items);
    }

    public void Update(int productId, int quantity)
    {
        if (quantity <= 0) { Remove(productId); return; }
        var items = GetMutableItems();
        if (items.ContainsKey(productId)) items[productId] = Math.Clamp(quantity, 1, 20);
        Save(items);
    }

    public void Remove(int productId)
    {
        var items = GetMutableItems();
        items.Remove(productId);
        Save(items);
    }

    public void Clear() => Session.Remove(SessionKey);

    private ISession Session => _httpContextAccessor.HttpContext?.Session
        ?? throw new InvalidOperationException("Cart requires an active HTTP session.");

    private Dictionary<int, int> GetMutableItems()
    {
        var json = Session.GetString(SessionKey);
        if (string.IsNullOrWhiteSpace(json)) return new Dictionary<int, int>();

        try
        {
            return JsonSerializer.Deserialize<Dictionary<int, int>>(json) ?? new Dictionary<int, int>();
        }
        catch (JsonException)
        {
            Session.Remove(SessionKey);
            return new Dictionary<int, int>();
        }
    }

    private void Save(Dictionary<int, int> items) =>
        Session.SetString(SessionKey, JsonSerializer.Serialize(items));
}
