using Atelier.Domain.Enums;
using Atelier.Infrastructure.Data;
using Atelier.Web.Areas.Admin.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Atelier.Web.Areas.Admin.Controllers;

public class OrdersController : AdminController
{
    private readonly AtelierDbContext _dbContext;
    public OrdersController(AtelierDbContext dbContext) => _dbContext = dbContext;

    [HttpGet]
    public async Task<IActionResult> Index(OrderStatus? status)
    {
        var query = _dbContext.Orders.AsNoTracking();
        if (status.HasValue) query = query.Where(order => order.Status == status.Value);

        var orders = await query.OrderByDescending(order => order.CreatedAt).Take(250)
            .Select(order => new OrderListItemViewModel
            {
                Id = order.Id, Number = order.Number, CustomerName = order.CustomerName, Phone = order.Phone,
                Total = order.Total, ItemsCount = order.Items.Sum(item => item.Quantity), Status = order.Status,
                PaymentStatus = order.PaymentStatus,
                CreatedAt = order.CreatedAt
            }).ToListAsync();
        ViewData["FilterStatus"] = status;
        return View(orders);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var order = await _dbContext.Orders.AsNoTracking().Where(item => item.Id == id)
            .Select(item => new OrderDetailsViewModel
            {
                Id = item.Id, Number = item.Number, CustomerName = item.CustomerName, Phone = item.Phone,
                Province = item.Province, City = item.City, Address = item.Address, PostalCode = item.PostalCode,
                CustomerNote = item.CustomerNote, Total = item.Total, Status = item.Status, PaymentStatus = item.PaymentStatus, Carrier=item.Carrier, TrackingNumber=item.TrackingNumber, CreatedAt = item.CreatedAt,
                Items = item.Items.Select(line => new OrderItemViewModel
                {
                    ProductName = line.ProductName, UnitPrice = line.UnitPrice, Quantity = line.Quantity, LineTotal = line.LineTotal
                }).ToList(),
                History = item.StatusHistory.OrderBy(history => history.CreatedAt).Select(history => new OrderHistoryItemViewModel { Status=history.Status, Note=history.Note, CreatedAt=history.CreatedAt }).ToList(),
                Payments = _dbContext.PaymentTransactions.Where(payment => payment.OrderId == item.Id).OrderByDescending(payment => payment.CreatedAt)
                    .Select(payment => new PaymentItemViewModel { Gateway=payment.Gateway, Status=payment.Status, ReferenceId=payment.ReferenceId, GatewayCode=payment.GatewayCode, CreatedAt=payment.CreatedAt }).ToList()
            }).FirstOrDefaultAsync();
        return order is null ? NotFound() : View(order);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, OrderStatus status, string? carrier, string? trackingNumber)
    {
        if (!Enum.IsDefined(status)) return BadRequest();
        var order = await _dbContext.Orders.FirstOrDefaultAsync(item => item.Id == id);
        if (order is null) return NotFound();
        if (status == OrderStatus.Shipped) order.SetShipment(carrier, trackingNumber);
        order.UpdateStatus(status);
        await _dbContext.SaveChangesAsync();
        TempData["SuccessMessage"] = "وضعیت سفارش به‌روزرسانی شد.";
        return RedirectToAction(nameof(Details), new { id });
    }
}
