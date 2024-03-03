using Microsoft.AspNetCore.Mvc;

namespace TicketsPro.Controllers
{
    public class ReportsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
