using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Atelier.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize]
public abstract class AdminController : Controller
{
}
