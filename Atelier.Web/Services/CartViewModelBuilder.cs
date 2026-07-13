using Atelier.Domain.Enums;
using Atelier.Infrastructure.Data;
using Atelier.Web.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Atelier.Web.Services;

public interface ICartViewModelBuilder
{
    Task<CartViewModel> BuildAsync();
}

public sealed class CartViewModelBuilder : ICartViewModelBuilder
{
    private readonly AtelierDbContext _dbContext;
    private readonly ICartService _cart;

    public CartViewModelBuilder(AtelierDbContext dbContext, ICartService cart)
    {
        _dbContext = dbContext;
        _cart = cart;
    }

    public async Task<CartViewModel> BuildAsync()
    {
        var stored = _cart.GetItems();
        if (stored.Count == 0) return new CartViewModel();

        var productIds = stored.Keys.ToArray();
        var categories = await _dbContext.Categories.AsNoTracking().Select(category => new CatalogCategoryNode
        {
            Id = category.Id,
            Name = category.Name,
            Slug = category.Slug,
            ParentId = EF.Property<int?>(category, "ParentId")
        }).ToListAsync();
        var products = await _dbContext.Products.AsNoTracking()
            .Where(product => productIds.Contains(product.Id) && product.Status == ProductStatus.Published &&
                              product.PriceType == PriceType.Fixed && product.Price > 0)
            .Select(product => new
            {
                product.Id, product.Name, product.Slug, Price = product.Price!.Value,
                PrimaryCategoryId = product.Categories.OrderBy(category => category.Id).Select(category => (int?)category.Id).FirstOrDefault(),
                ImageUrl = product.Gallery.OrderBy(media => media.Id).Select(media => media.Url).FirstOrDefault()
            }).ToListAsync();

        foreach (var missingId in productIds.Except(products.Select(product => product.Id))) _cart.Remove(missingId);

        var items = products.Select(product => new CartItemViewModel
        {
            ProductId = product.Id,
            Name = product.Name,
            Url = BuildProductUrl(product.Slug, product.PrimaryCategoryId, categories),
            ImageUrl = product.ImageUrl,
            UnitPrice = product.Price,
            Quantity = stored[product.Id]
        }).ToList();

        return new CartViewModel { Items = items, Total = items.Sum(item => item.LineTotal) };
    }

    private static string BuildProductUrl(string productSlug, int? categoryId, IReadOnlyList<CatalogCategoryNode> categories)
    {
        if (!categoryId.HasValue) return "/categories";
        var category = categories.FirstOrDefault(item => item.Id == categoryId.Value);
        if (category is null) return "/categories";
        return CatalogRoutingHelper.BuildProductPath(CatalogRoutingHelper.BuildCategoryChain(categories, category), productSlug);
    }
}
