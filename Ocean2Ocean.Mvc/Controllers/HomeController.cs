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

        [Route("/{journeyName}")]
        [Route("/")]
        public async Task<IActionResult> IndexAsync(DateTime? Date, int steps, string journeyName)
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

            // Lookup a specific journey.
            if (!string.IsNullOrWhiteSpace(journeyName))
            {
                // Make a query to the db.
                var results = await Step.GetByJourneyAsync(journeyName, _azureSQL);

                if (results.Any())
                {
                    steps = results.Sum(x => x.Steps);

                    return View("Index", new Journey
                    {
                        // Can't be less than 1 or greater than the maximum number of steps in the route.
                        StepsTaken = steps > 1 ? steps : 1,
                        JourneyName = results.FirstOrDefault().JourneyName,
                        Date = currentDate,
                        Participants = 10,
                        MapboxAccessToken = _mapboxAccessToken
                    });
                }
                else
                {
                    // The the journey has no entries.
                    return View("Index", new Journey
                    {
                        // Can't be less than 1 or greater than the maximum number of steps in the route.
                        StepsTaken = steps > 1 ? steps : 1,
                        JourneyName = results.FirstOrDefault().JourneyName,
                        Date = currentDate,
                        Participants = 0,
                        MapboxAccessToken = _mapboxAccessToken
                    });
                }
            }
            else
            {
                // If no journey is supplied.
                return View("Index", new Journey
                {
                    // Can't be less than 1 or greater than the maximum number of steps in the route.
                    StepsTaken = steps > 1 ? steps : 1,
                    Date = currentDate,
                    Participants = 0,
                    MapboxAccessToken = _mapboxAccessToken
                });
            }
        }

        [Route("/Home/AddSteps/{journeyName}/")]
        [Route("/Home/AddSteps/")]
        public async Task<IActionResult> AddSteps([Bind("Email,JourneyName,Steps,StepsTaken,StepsInRoute")] Step step)
        {
            if (step != null && !string.IsNullOrWhiteSpace(step.JourneyName) && !string.IsNullOrWhiteSpace(step.Email) && step.Steps > 1)
            {
                // Block step values that are too large.
                if (step.StepsInRoute - (step.StepsTaken + step.Steps) < 0)
                {
                    // Let them know this was bad input maybe?
                    return RedirectToAction("Index");
                }

                // Prevent the submission of duplicate entries on the same day.
                var checkDuplicate = await step.CheckForDuplicateAsync(_azureSQL);

                if (checkDuplicate)
                {
                    var results = await Step.GetByEmailAsync(step.Email, _azureSQL);
                    return View("AddSteps", results);
                }

                // Save this entry to the db.
                var checkSubmitted = await step.PostAsync(_azureSQL);

                if (checkSubmitted)
                {
                    var results = await Step.GetByEmailAsync(step.Email, _azureSQL);
                    return View("AddSteps", results);
                }
            }
            else if (step != null && step.Steps == 0 && !string.IsNullOrWhiteSpace(step.JourneyName) && !string.IsNullOrWhiteSpace(step.Email))
            {
                var results = await Step.GetByEmailAsync(step.Email, _azureSQL);
                return View("AddSteps", results);
            }
            else if (step?.Steps == 0 && !string.IsNullOrWhiteSpace(step.JourneyName))
            {
                var results = await Step.GetRankingsAsync(step.JourneyName, _azureSQL);
                return View("Leaderboard", results);
            }

            return Redirect($"/{step?.JourneyName}");
        }

        [Route("/Home/Leaderboard/{journeyName}/")]
        public async Task<IActionResult> Leaderboard(string journeyName)
        {
            if (!string.IsNullOrWhiteSpace(journeyName))
            {
                var results = await Step.GetRankingsAsync(journeyName, _azureSQL);

                if (results.Any())
                {
                    return View("Leaderboard", results);
                }
            }

            return Redirect($"/{journeyName}");
        }

        [Route("/Home/RemoveSteps/{journeyName}/")]
        public async Task<IActionResult> RemoveSteps(int entryId, string journeyName)
        {
            if (!(entryId > 1))
            {
                // Let them know this was bad input maybe?
                return RedirectToAction("Index");
            }

            var deleteMe = await Step.GetByIdAsync(entryId, _azureSQL);

            if (deleteMe is null)
            {
                // Let them know this was bad input maybe?
                return RedirectToAction("Index", new
                {
                    journeyName = journeyName
                });
            }

            var checkDelete = await deleteMe.DeleteAsync(_azureSQL);

            if (checkDelete)
            {
                var results = await Step.GetByEmailAsync(deleteMe.Email, _azureSQL);
                return View("AddSteps", results);
            }
            else
            {
                // No entries for this email remain.
                return RedirectToAction("Index", new
                {
                    journeyName = journeyName
                });
            }
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
