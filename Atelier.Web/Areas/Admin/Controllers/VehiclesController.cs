using Atelier.Infrastructure.Data;
using Atelier.Web.Areas.Admin.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Atelier.Web.Areas.Admin.Controllers;

[Area("Admin"), Authorize]
public sealed class VehiclesController : Controller
{
    private readonly AtelierDbContext _db;
    public VehiclesController(AtelierDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> Index() => View(await _db.Vehicles.AsNoTracking()
        .OrderBy(x => x.Make).ThenBy(x => x.Model).ThenBy(x => x.YearFrom)
        .Select(x => new VehicleListItemViewModel
        {
            Id = x.Id, Slug = x.Slug, IsActive = x.IsActive,
            Label = x.Make + " " + x.Model + " | " + x.YearFrom + "-" + (x.YearTo ?? x.YearFrom) + " | " + x.Engine + " | " + x.Trim,
            ProductCount = _db.ProductCompatibilities.Count(c => c.VehicleId == x.Id)
        }).ToListAsync());

    [HttpGet]
    public IActionResult Create() => View(new VehicleEditViewModel { YearFrom = DateTime.UtcNow.Year });

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(VehicleEditViewModel model)
    {
        if (model.YearTo < model.YearFrom) ModelState.AddModelError(nameof(model.YearTo), "سال پایان نباید قبل از سال شروع باشد.");
        if (await _db.Vehicles.AnyAsync(x => x.Slug == model.Slug)) ModelState.AddModelError(nameof(model.Slug), "این اسلاگ قبلاً استفاده شده است.");
        if (!ModelState.IsValid) return View(model);
        var vehicle = new Vehicle(model.Make, model.Model, model.YearFrom, model.YearTo, model.Engine, model.Trim, model.Slug.Trim().ToLowerInvariant());
        vehicle.SetActive(model.IsActive);
        _db.Vehicles.Add(vehicle);
        await _db.SaveChangesAsync();
        TempData["SuccessMessage"] = "خودرو ذخیره شد.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var x = await _db.Vehicles.AsNoTracking().FirstOrDefaultAsync(v => v.Id == id);
        return x is null ? NotFound() : View(new VehicleEditViewModel { Id=x.Id, Make=x.Make, Model=x.Model, YearFrom=x.YearFrom, YearTo=x.YearTo, Engine=x.Engine, Trim=x.Trim, Slug=x.Slug, IsActive=x.IsActive });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(VehicleEditViewModel model)
    {
        if (!model.Id.HasValue) return NotFound();
        var vehicle = await _db.Vehicles.FirstOrDefaultAsync(x => x.Id == model.Id);
        if (vehicle is null) return NotFound();
        if (model.YearTo < model.YearFrom) ModelState.AddModelError(nameof(model.YearTo), "سال پایان نباید قبل از سال شروع باشد.");
        if (await _db.Vehicles.AnyAsync(x => x.Slug == model.Slug && x.Id != model.Id)) ModelState.AddModelError(nameof(model.Slug), "این اسلاگ قبلاً استفاده شده است.");
        if (!ModelState.IsValid) return View(model);
        vehicle.Update(model.Make, model.Model, model.YearFrom, model.YearTo, model.Engine, model.Trim);
        vehicle.SetActive(model.IsActive);
        _db.Entry(vehicle).Property("Slug").CurrentValue = model.Slug.Trim().ToLowerInvariant();
        await _db.SaveChangesAsync();
        TempData["SuccessMessage"] = "خودرو به‌روزرسانی شد.";
        return RedirectToAction(nameof(Index));
    }
}
