using Atelier.Infrastructure.Data;
using Atelier.Web.Areas.Admin.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Atelier.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize]
public class LeadsController : Controller
{
    private readonly AtelierDbContext _dbContext;

    public LeadsController(AtelierDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var leads = await _dbContext.Leads
            .AsNoTracking()
            .OrderByDescending(lead => lead.CreatedAt)
            .Select(lead => new LeadListItemViewModel
            {
                Id = lead.Id,
                Name = lead.Name,
                Email = lead.Email,
                CreatedAt = lead.CreatedAt
            })
            .ToListAsync();

        return View(leads);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var lead = await _dbContext.Leads.AsNoTracking().FirstOrDefaultAsync(item => item.Id == id);
        if (lead is null)
        {
            return NotFound();
        }

        var model = new LeadDetailsViewModel
        {
            Id = lead.Id,
            Name = lead.Name,
            Email = lead.Email,
            Message = lead.Message,
            CreatedAt = lead.CreatedAt,
            Status = lead.Status
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var lead = await _dbContext.Leads.FirstOrDefaultAsync(item => item.Id == id);
        if (lead is null)
        {
            return NotFound();
        }

        _dbContext.Leads.Remove(lead);
        await _dbContext.SaveChangesAsync();

        TempData["SuccessMessage"] = "درخواست با موفقیت حذف شد.";
        return RedirectToAction("Index", new { area = "Admin" });
    }
}
