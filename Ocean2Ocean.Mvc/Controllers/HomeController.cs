using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Ocean2Ocean.DataAccess;
using Ocean2Ocean.Models;

namespace Ocean2Ocean.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _azureSQL;

        public HomeController(ILogger<HomeController> logger, IConfiguration configuration)
        {
            _logger = logger; _configuration = configuration;
            _azureSQL = _configuration.GetConnectionString("AzureSQL");
        }

        [Route("/")]
        public async Task<IActionResult> IndexAsync()
        {
            var results = await Journey.GetAllAsync(_azureSQL);

            return View("Index", results);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
