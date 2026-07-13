using Atelier.Domain.Enums;
using Atelier.Infrastructure.Data;
using Atelier.Web.Areas.Admin.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Atelier.Web.Areas.Admin.Controllers;

public class DashboardController : AdminController
{
    private readonly AtelierDbContext _dbContext;

    public DashboardController(AtelierDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IActionResult> Index()
    {
        var model = new DashboardViewModel
        {
            ProductsCount = await _dbContext.Products.CountAsync(),
            PublishedProductsCount = await _dbContext.Products.CountAsync(product => product.Status == ProductStatus.Published),
            CategoriesCount = await _dbContext.Categories.CountAsync(),
            ProjectsCount = await _dbContext.Projects.CountAsync(),
            PublishedProjectsCount = await _dbContext.Projects.CountAsync(project => project.IsPublished),
            LeadsCount = await _dbContext.Leads.CountAsync(),
            BlogPostsCount = await _dbContext.BlogPosts.CountAsync(post => post.PublishedAt != null),
            OrdersCount = await _dbContext.Orders.CountAsync(),
            AwaitingOrdersCount = await _dbContext.Orders.CountAsync(order => order.Status == OrderStatus.AwaitingReview)
        };

        return View(model);
    }
}
