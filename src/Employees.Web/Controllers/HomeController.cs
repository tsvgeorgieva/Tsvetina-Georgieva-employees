using Employees.Core;
using Microsoft.AspNetCore.Mvc;

namespace Employees.Web.Controllers;

public class HomeController(IPairReportService reportService) : Controller
{
    [HttpGet]
    public IActionResult Index() => View();

    [HttpPost]
    public IActionResult Index(IFormFile? file)
    {
        if (file is null)
        {
            return View();
        }

        using var reader = new StreamReader(file.OpenReadStream());

        return View(reportService.Run(reader));
    }
}
