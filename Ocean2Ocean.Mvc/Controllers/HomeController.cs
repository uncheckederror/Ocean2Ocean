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
            _logger = logger;
            _configuration = configuration;
            _azureSQL = _configuration.GetConnectionString("AzureSQL");
        }

        /// <summary>
        /// Render the route for a journey.
        /// </summary>
        /// <param name="Date"></param>
        /// <param name="steps"></param>
        /// <param name="journeyName"></param>
        /// <returns></returns>
        [Route("/{journeyName}")]
        public async Task<IActionResult> IndexAsync(DateTime? Date, int steps, string journeyName)
        {
            // Using today's date get the sum of all steps up to this point.
            var currentDate = Date ?? DateTime.Now;

            // Return early if this is a demo.
            if (steps > 1)
            {
                return View("ESRI", new Journey
                {
                    // Can't be less than 1 or greater than the maximum number of steps in the route.
                    StepsTaken = steps > 1 ? steps : 1,
                    Date = currentDate,
                    Participants = 10,
                    ErrorMessage = "This is a demo."
                });
            }

            // Lookup a specific journey.
            if (!string.IsNullOrWhiteSpace(journeyName) && (journeyName.Length < 50))
            {
                journeyName = journeyName.Trim();
                // Make a query to the db.
                var results = await Step.GetByJourneyAsync(journeyName, _azureSQL);

                if (results.Any())
                {
                    steps = results.Sum(x => x.Steps);

                    return View("ESRI", new Journey
                    {
                        // Can't be less than 1 or greater than the maximum number of steps in the route.
                        StepsTaken = steps > 1 ? steps : 1,
                        JourneyName = results.FirstOrDefault().JourneyName,
                        Date = currentDate,
                        Participants = 10
                    });
                }
                else
                {
                    // The the journey has no entries.
                    return View("ESRI", new Journey
                    {
                        // Can't be less than 1 or greater than the maximum number of steps in the route.
                        StepsTaken = steps > 1 ? steps : 1,
                        JourneyName = results.FirstOrDefault().JourneyName,
                        Date = currentDate,
                        Participants = 0,
                        ErrorMessage = "This Journey has no Entries."
                    });
                }
            }
            else
            {
                // If no journey is supplied.
                return View("ESRI", new Journey
                {
                    // Can't be less than 1 or greater than the maximum number of steps in the route.
                    StepsTaken = steps > 1 ? steps : 1,
                    Date = currentDate,
                    Participants = 0,
                    ErrorMessage = "No Journey was supplied."
                });
            }
        }

        [Route("/Journeys/")]
        public async Task<IActionResult> SearchJournys(string journeyName)
        {
            if (string.IsNullOrWhiteSpace(journeyName))
            {
                var results = await Journey.GetAllAsync(_azureSQL);

                return View("SearchJourneys", new JourneysSearchResult
                {
                    Query = journeyName,
                    Journeys = results
                });
            }
            else
            {
                var results = await Journey.GetByJourneyNameAsync(journeyName, _azureSQL);

                if (!results.Any())
                {
                    results = await Journey.SearchByJourneyNameAsync(journeyName, _azureSQL);
                }
                else if (results.Count() == 1)
                {
                    return Redirect($"/{results.FirstOrDefault().JourneyName}");
                }

                if (results.Count() > 1)
                {
                    return View("SearchJourneys", new JourneysSearchResult
                    {
                        Query = journeyName,
                        Journeys = results
                    });
                }
                else if (results.Count() == 1)
                {
                    return Redirect($"/{results.FirstOrDefault().JourneyName}");
                }
                else
                {
                    return View("SearchJourneys", new JourneysSearchResult
                    {
                        Query = journeyName,
                        Journeys = results
                    });
                }
            }
        }

        /// <summary>
        /// Add steps to a Journey.
        /// </summary>
        /// <param name="step"></param>
        /// <returns></returns>
        [Route("/Home/AddSteps/{journeyName}/")]
        [Route("/Home/AddSteps/")]
        public async Task<IActionResult> AddSteps([Bind("Nickname,JourneyName,TeamName,Steps,StepsTaken,StepsInRoute")] Step step)
        {
            if (step != null && !string.IsNullOrWhiteSpace(step.JourneyName) && (step.JourneyName.Length < 50) && !string.IsNullOrWhiteSpace(step.Nickname) && (step.Nickname.Length < 50) && step.Steps > 1)
            {
                step.Nickname = step.Nickname.Trim();
                step.JourneyName = step.JourneyName.Trim();

                // Set the team name if one is not supplied.
                if (string.IsNullOrWhiteSpace(step.TeamName))
                {
                    step.TeamName = $"{step.JourneyName} Team";
                }

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
                    var results = await Step.GetByNicknameAsync(step.Nickname, _azureSQL);

                    if (results.Any())
                    {
                        return View("AddSteps", results);
                    }

                    results = await Step.GetByExactNicknameAsync(step.Nickname, _azureSQL);

                    return View("AddSteps", results);
                }

                // Save this entry to the db.
                var checkSubmitted = await step.PostAsync(_azureSQL);

                if (checkSubmitted)
                {
                    var results = await Step.GetByNicknameAsync(step.Nickname, _azureSQL);

                    if (results.Any())
                    {
                        return View("AddSteps", results);
                    }

                    results = await Step.GetByExactNicknameAsync(step.Nickname, _azureSQL);

                    return View("AddSteps", results);
                }
            }
            // Submit steps anonymously.
            else if (step != null && !string.IsNullOrWhiteSpace(step.JourneyName) && (step.JourneyName.Length < 50) && string.IsNullOrWhiteSpace(step.Nickname) && step.Steps > 1)
            {
                step.Nickname = $"{step.JourneyName} Team";
                step.JourneyName = step.JourneyName.Trim();

                // Set the team name if one is not supplied.
                if (string.IsNullOrWhiteSpace(step.TeamName))
                {
                    step.TeamName = $"{step.JourneyName} Team";
                }

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
                    var results = await Step.GetByNicknameAsync(step.Nickname, _azureSQL);

                    if (results.Any())
                    {
                        return View("AddSteps", results);
                    }

                    results = await Step.GetByExactNicknameAsync(step.Nickname, _azureSQL);

                    return View("AddSteps", results);
                }
            }
            // Show the page for the user.
            else if (step != null && step.Steps == 0 && !string.IsNullOrWhiteSpace(step.JourneyName) && (step.JourneyName.Length < 50) && !string.IsNullOrWhiteSpace(step.Nickname) && (step.Nickname.Length < 50))
            {
                step.Nickname = step.Nickname.Trim();
                step.JourneyName = step.JourneyName.Trim();

                var results = await Step.GetByNicknameAsync(step.Nickname, _azureSQL);

                if (results.Any())
                {
                    return View("AddSteps", results);
                }

                results = await Step.GetByExactNicknameAsync(step.Nickname, _azureSQL);

                return View("AddSteps", results);
            }
            // Show the group leader board.
            else if (step?.Steps == 0 && !string.IsNullOrWhiteSpace(step.JourneyName) && (step.JourneyName.Length < 50))
            {
                step.JourneyName = step.JourneyName.Trim();

                var results = await Leaderboard.GetRankingsAsync(step.JourneyName, _azureSQL);
                var daily = await Leaderboard.GetDailyRankingsAsync(step.JourneyName, _azureSQL);

                if (results.Any())
                {
                    return View("Leaderboard", new LeaderboardDetail
                    {
                        JourneyRankings = results,
                        DailyRankings = daily
                    });
                }
            }

            return Redirect($"/{step?.JourneyName}");
        }

        /// <summary>
        /// Show the leaderboard for a specific journey.
        /// </summary>
        /// <param name="journeyName"></param>
        /// <returns></returns>
        [Route("/Home/Leaderboard/{journeyName}/")]
        public async Task<IActionResult> JourneyLeaderboardAsync(string journeyName)
        {
            if (!string.IsNullOrWhiteSpace(journeyName) && (journeyName.Length < 50))
            {
                journeyName = journeyName.Trim();

                var results = await Leaderboard.GetRankingsAsync(journeyName, _azureSQL);
                var daily = await Leaderboard.GetDailyRankingsAsync(journeyName, _azureSQL);
                var byDay = await Leaderboard.GetTotalStepsByDayAsync(journeyName, _azureSQL);

                if (results.Any())
                {
                    return View("Leaderboard", new LeaderboardDetail
                    {
                        JourneyRankings = results,
                        DailyRankings = daily,
                        SumByDay = byDay
                    });
                }
            }

            return Redirect($"/{journeyName}");
        }

        /// <summary>
        /// See all of the Journeys in the Db.
        /// </summary>
        /// <returns></returns>
        [Route("/Home/Journeys/")]
        [Route("/")]
        public async Task<IActionResult> JourneysAsync()
        {
            var results = await Journey.GetAllAsync(_azureSQL);

            return View("Index", results);
        }

        /// <summary>
        /// Delete a steps entry from a journey.
        /// </summary>
        /// <param name="entryId"></param>
        /// <param name="journeyName"></param>
        /// <returns></returns>
        [Route("/Home/RemoveSteps/{journeyName}/")]
        public async Task<IActionResult> RemoveSteps(int stepId, string journeyName)
        {
            if (!(stepId > 1) || string.IsNullOrWhiteSpace(journeyName) || (journeyName.Length > 50))
            {
                // Let them know this was bad input maybe?
                return RedirectToAction("Index");
            }

            journeyName = journeyName.Trim();

            var deleteMe = await Step.GetByIdAsync(stepId, _azureSQL);

            if (deleteMe is null)
            {
                // Let them know this was bad input maybe?
                return RedirectToAction("Index", new
                {
                    journeyName = journeyName
                });
            }

            var checkDelete = await deleteMe.DeleteAsync(_azureSQL);

            var results = await Step.GetByNicknameAsync(deleteMe.Nickname, _azureSQL);

            if (!results.Any())
            {
                results = await Step.GetByExactNicknameAsync(deleteMe.Nickname, _azureSQL);
            }

            if (results != null && results.Any() && checkDelete)
            {
                return View("AddSteps", results);
            }
            else
            {
                // No entries for this email remain or it didn't work.
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
