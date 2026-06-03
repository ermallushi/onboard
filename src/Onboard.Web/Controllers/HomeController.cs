using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Onboard.Web.Application;
using Onboard.Web.Models;

namespace Onboard.Web.Controllers;

public class HomeController(IOnboardingOrchestrator orchestrator) : Controller
{
    public IActionResult Index()
    {
        var cases = orchestrator.SearchCases(null);
        var vm = new DashboardViewModel
        {
            TotalCases = cases.Count,
            StatusCounts = cases
                .GroupBy(c => c.Status.ToString())
                .OrderBy(g => g.Key)
                .ToDictionary(g => g.Key, g => g.Count()),
            RecentCases = cases.OrderByDescending(c => c.UpdatedAt).Take(10).ToArray()
        };
        return View(vm);
    }

    public IActionResult Guide()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

