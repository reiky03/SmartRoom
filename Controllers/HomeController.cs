using Microsoft.AspNetCore.Mvc;
using SmartRoom.Services;

namespace SmartRoom.Controllers;

public class HomeController : Controller
{
    private readonly IBookingService _bookingService;

    public HomeController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    public async Task<IActionResult> Index()
    {
        var stats = await _bookingService.GetDashboardStatsAsync();
        return View(stats);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View();
    }
}
