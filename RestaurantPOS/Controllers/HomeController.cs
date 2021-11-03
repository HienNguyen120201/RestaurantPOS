using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RestaurantPOS.Models;
using RestaurantPOS.Services;
using System.Security.Claims;
using RestaurantManagement.Models;

namespace RestaurantPOS.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpGet("/Payment")]
        public async Task<IActionResult> Payment()
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Login", "Home");
            var payment = await _customerService.GetBillToPayAsync(User);
            return View(payment);
        }

        [HttpPost("/Payment")]
        public async Task<IActionResult> Payment(PaymentViewModel billPaymentVM)
        {
            await _customerService.UpdatePaymentMethodAsync(User, billPaymentVM);
            return RedirectToAction("MenuFood", "Menu");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
