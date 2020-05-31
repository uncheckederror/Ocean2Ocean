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
        private readonly string _mapboxAccessToken;
        private readonly string _azureSQL;

        public HomeController(ILogger<HomeController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            _mapboxAccessToken = _configuration.GetConnectionString("Mapbox");
            _azureSQL = _configuration.GetConnectionString("AzureSQL");
        }

        public async Task<IActionResult> IndexAsync(DateTime? Date, int steps)
        {
            // Using today's date get the sum of all steps up to this point.
            var currentDate = Date ?? DateTime.Now;

            // Return early if this is a demo.
            if (steps > 1)
            {
                return View("Index", new Journey
                {
                    // Can't be less than 1 or greater than the maximum number of steps in the route.
                    StepsTaken = steps > 1 ? steps : 1,
                    Date = currentDate,
                    Participants = 10,
                    MapboxAccessToken = _mapboxAccessToken
                });
            }

            var results = await Step.GetAllAsync(_azureSQL);
            steps = results.Sum(x => x.Steps);

            return View("Index", new Journey
            {
                // Can't be less than 1 or greater than the maximum number of steps in the route.
                StepsTaken = steps > 1 ? steps : 1,
                Date = currentDate,
                Participants = 10,
                MapboxAccessToken = _mapboxAccessToken
            });
        }

        public async Task<IActionResult> AddSteps([Bind("Email,Steps,StepsTaken,StepsInRoute")] Step step)
        {
            if (step != null && !string.IsNullOrWhiteSpace(step.Email) && step.Steps > 1)
            {
                // Block step values that are too large.
                if (step.StepsInRoute - (step.StepsTaken + step.Steps) < 0)
                {
                    // Let them know this was bad input maybe?
                    return RedirectToAction("Index");
                }

                // Save this entry to the db.
                var checkSubmitted = await step.PostAsync(_azureSQL);

                if (checkSubmitted)
                {
                    var results = await Step.GetByEmail(step.Email, _azureSQL);
                    return View("AddSteps", results);
                }
            }

            return RedirectToAction("Index");
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
}
