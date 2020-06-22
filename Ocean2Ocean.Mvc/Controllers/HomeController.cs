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

        /// <summary>
        /// Render the route for a journey.
        /// </summary>
        /// <param name="Date"></param>
        /// <param name="steps"></param>
        /// <param name="journeyName"></param>
        /// <returns></returns>
        [Route("/{journeyName}")]
        [Route("/{journeyName}/{teamName}")]
        [Route("/{journeyName}/{teamName}/{name}")]
        public async Task<IActionResult> MapAsync(string journeyName, string teamName, string name)
        {
            var response = new Map();

            if (!string.IsNullOrWhiteSpace(journeyName)
                && (journeyName.Length < 100))
            {
                journeyName = journeyName.Trim();

                var journeys = await Journey.GetByJourneyNameAsync(journeyName, _azureSQL);
                response.Journey = journeys.FirstOrDefault();

                var entries = await Step.GetByJourneyAsync(response.Journey?.JourneyName, _azureSQL);

                foreach (var entry in entries)
                {
                    response.StepsTaken += entry.Steps;
                }
            }

            if (!(string.IsNullOrWhiteSpace(teamName))
                && (teamName.Length < 100))
            {
                teamName = teamName.Trim();

                var teams = await Team.GetByTeamNameAsync(teamName, _azureSQL);
                response.Team = teams.FirstOrDefault();
            }

            if (!(string.IsNullOrWhiteSpace(name))
                && (name.Length < 100))
            {
                name = name.Trim();

                var nicknames = await Nickname.GetByNicknameAsync(name, _azureSQL);
                response.Nickname = nicknames.FirstOrDefault();
            }

            if (response?.Journey is null || string.IsNullOrWhiteSpace(response?.Journey?.JourneyName))
            {
                return Redirect("/");
            }

            return View("ESRI", response);
        }

        /// <summary>
        /// Add steps to a Journey.
        /// </summary>
        /// <param name="step"></param>
        /// <returns></returns>
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

                var teams = await Team.GetByTeamNameAsync(step.TeamName, _azureSQL);
                var team = teams.FirstOrDefault();

                if (team is null)
                {
                    var newTeam = new Team
                    {
                        TeamName = step.TeamName,
                        Bio = "Give me a cool bio.",
                        Active = true,
                        Created = DateTime.Now,
                        TeamWebsite = "https://thomasryan.xyz/",
                    };

                    var checkCreateTeam = await newTeam.PostAsync(_azureSQL);

                    if (!checkCreateTeam)
                    {
                        // Revert to the default if creating a new Team fails.
                        step.TeamName = $"{step.JourneyName} Team";
                    }
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
                    return Redirect($"/{step.JourneyName}/{step.TeamName}/");
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

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
