using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

using Ocean2Ocean.DataAccess;

namespace Ocean2Ocean.Controllers
{
    public class JourneysController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly string _azureSQL;

        public JourneysController(IConfiguration configuration)
        {
            _configuration = configuration;
            _azureSQL = _configuration.GetConnectionString("AzureSQL");
        }

        /// <summary>
        /// List all Journeys and search through them.
        /// </summary>
        /// <param name="journeyName"></param>
        /// <returns></returns>
        [Route("/Journeys/")]
        public async Task<IActionResult> SearchJournys(string journeyName)
        {
            if (string.IsNullOrWhiteSpace(journeyName))
            {
                var results = await Journey.GetAllAsync(_azureSQL);

                return View("Journeys", new JourneysSearchResult
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
                //else if (results.Count() == 1)
                //{
                //    return Redirect($"/{results.FirstOrDefault().JourneyName}");
                //}

                if (results.Count() > 1)
                {
                    return View("Journeys", new JourneysSearchResult
                    {
                        Query = journeyName,
                        Journeys = results
                    });
                }
                //else if (results.Count() == 1)
                //{
                //    return Redirect($"/{results.FirstOrDefault().JourneyName}");
                //}
                else
                {
                    return View("Journeys", new JourneysSearchResult
                    {
                        Query = journeyName,
                        Journeys = results
                    });
                }
            }
        }

        [Route("/Journeys/{journeyName}")]
        public async Task<IActionResult> ViewJourney(string journeyName)
        {
            var journeys = await Journey.GetByJourneyNameAsync(journeyName, _azureSQL);
            var journey = journeys.FirstOrDefault();
            var entries = await Step.GetByJourneyAsync(journey?.JourneyName, _azureSQL);

            var uniqueTeamNames = entries.Select(x => x.TeamName).Distinct();

            var teams = new List<Team>();
            foreach (var team in uniqueTeamNames)
            {
                var results = await Team.GetByTeamNameAsync(team, _azureSQL);
                if (!(results is null))
                {
                    teams.Add(results.FirstOrDefault());
                }
            }

            var uniqueNames = entries.Select(x => x.Nickname).Distinct();

            var nicknames = new List<Nickname>();
            foreach (var name in uniqueNames)
            {
                var results = await Nickname.GetByNicknameAsync(name, _azureSQL);
                if (!(results is null))
                {
                    nicknames.Add(results.FirstOrDefault());
                }
            }

            // Maybe look up all the entries that reference this team?
            return View("Journey", new JourneyResult
            {
                Journey = journey,
                Teams = teams,
                Nicknames = nicknames,
                Steps = entries
            });
        }

        [Route("/Journeys/Add")]
        public async Task<IActionResult> AddJourneyAsync([Bind("JourneyName,Bio,GeometryFileName,ImagePath")] Journey newJourney)
        {
            if (newJourney != null && !string.IsNullOrWhiteSpace(newJourney.JourneyName) && newJourney.JourneyName.Length <= 100
                && !string.IsNullOrWhiteSpace(newJourney.Bio) && newJourney.Bio.Length <= 300
                && !string.IsNullOrWhiteSpace(newJourney.GeometryFileName) && newJourney.GeometryFileName.Length <= 300
                && !string.IsNullOrWhiteSpace(newJourney.ImagePath) && newJourney.ImagePath.Length <= 100)
            {
                newJourney.Active = true;

                var checkCreateJourney = await newJourney.PostAsync(_azureSQL);

                // Create a default team to go with this journey, because every journey need a team to enable anonymous submissions.
                var defaultTeam = new Team
                {
                    TeamName = $"{newJourney.JourneyName} Team",
                    TeamWebsite = "https://thomasryan.xyz/",
                    Bio = $"Every Journey has a Team. This is the default team for {newJourney.JourneyName}.",
                    Active = true,
                    Created = DateTime.Now
                };

                var checkCreateDefaultTeam = await defaultTeam.PostAsync(_azureSQL);

                var defaultNickname = new Nickname
                {
                    Name = $"{newJourney.JourneyName} Team",
                    Bio = $"This is nickname for those that prefer to remain anonymous.",
                    Active = true,
                    Created = DateTime.Now
                };

                var checkCreateDefaultNickname = await defaultNickname.PostAsync(_azureSQL);

                if (checkCreateJourney && checkCreateDefaultTeam && checkCreateDefaultNickname)
                {
                    return Redirect($"/{newJourney.JourneyName}");
                }
                else
                {
                    return View("Add");
                }
            }
            else
            {
                return View("Add");
            }
        }

        [Route("/Journeys/Update")]
        public async Task<IActionResult> UpdateJourneyAsync([Bind("JourneyId,JourneyName,Bio,GeometryFileName,ImagePath")] Journey newJourney)
        {
            if (newJourney != null && !string.IsNullOrWhiteSpace(newJourney.JourneyName) && newJourney.JourneyName.Length <= 100
                && !string.IsNullOrWhiteSpace(newJourney.Bio) && newJourney.Bio.Length <= 300
                && !string.IsNullOrWhiteSpace(newJourney.GeometryFileName) && newJourney.GeometryFileName.Length <= 300
                && !string.IsNullOrWhiteSpace(newJourney.ImagePath) && newJourney.ImagePath.Length <= 100)
            {
                newJourney.Active = true;

                var journey = await Journey.GetByIdAsync(newJourney.JourneyId, _azureSQL);

                journey.ImagePath = newJourney.ImagePath;
                journey.GeometryFileName = newJourney.GeometryFileName;
                journey.Bio = newJourney.Bio;

                var checkUpdateJourney = await journey.PutAsync(_azureSQL);

                if (checkUpdateJourney)
                {
                    return Redirect($"/Journeys/{journey.JourneyName}");
                }
            }

            return Redirect($"/Journeys/{newJourney.JourneyName}");
        }

        [Route("/Journeys/{journeyId}/Delete")]
        public async Task<IActionResult> DeleteJourneyAsync(int journeyId)
        {
            return View();
        }
    }
}
