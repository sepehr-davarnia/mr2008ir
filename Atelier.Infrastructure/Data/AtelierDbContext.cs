using Atelier.Infrastructure.Auth;
using Atelier.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Atelier.Infrastructure.Data;

public class AtelierDbContext : DbContext
{
    public AtelierDbContext(DbContextOptions<AtelierDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Media> Media => Set<Media>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Page> Pages => Set<Page>();
    public DbSet<Lead> Leads => Set<Lead>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<SiteSetting> SiteSettings => Set<SiteSetting>();
    public DbSet<AdminUser> AdminUsers => Set<AdminUser>();
    public DbSet<MediaContent> MediaContents => Set<MediaContent>();
    public DbSet<BlogPost> BlogPosts => Set<BlogPost>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AtelierDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
