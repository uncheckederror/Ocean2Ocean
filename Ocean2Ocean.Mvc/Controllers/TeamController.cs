using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

using Ocean2Ocean.DataAccess;

namespace Ocean2Ocean.Controllers
{
    public class TeamsController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly string _azureSQL;

        public TeamsController(IConfiguration configuration)
        {
            _configuration = configuration;
            _azureSQL = _configuration.GetConnectionString("AzureSQL");
        }

        [Route("/Teams/")]
        public async Task<IActionResult> IndexAsync(string teamName)
        {
            if (string.IsNullOrWhiteSpace(teamName))
            {
                var results = await Team.GetAllAsync(_azureSQL);

                return View("Teams", new TeamsSearchResult
                {
                    Query = teamName,
                    Teams = results
                });
            }
            else
            {
                var results = await Team.GetByTeamNameAsync(teamName, _azureSQL);

                if (!results.Any())
                {
                    results = await Team.SearchByTeamNameAsync(teamName, _azureSQL);
                }
                //else if (results.Count() == 1)
                //{
                //    return Redirect($"/{results.FirstOrDefault().JourneyName}");
                //}

                if (results.Count() > 1)
                {
                    return View("Teams", new TeamsSearchResult
                    {
                        Query = teamName,
                        Teams = results
                    });
                }
                //else if (results.Count() == 1)
                //{
                //    return Redirect($"/{results.FirstOrDefault().JourneyName}");
                //}
                else
                {
                    return View("Teams", new TeamsSearchResult
                    {
                        Query = teamName,
                        Teams = results
                    });
                }
            }
        }

        [Route("/Teams/{teamName}")]
        public async Task<IActionResult> ViewTeam(string teamName)
        {
            var results = await Team.GetByTeamNameAsync(teamName, _azureSQL);

            // Maybe look up all the entries that reference this team?
            return View("Teams", new TeamsSearchResult
            {
                Query = teamName,
                Teams = results
            });
        }

        [Route("/Teams/Add/")]
        public async Task<IActionResult> AddTeamAsync([Bind("TeamName,Bio,TeamWebsite")] Team newTeam)
        {
            if (newTeam != null && !string.IsNullOrWhiteSpace(newTeam.TeamName) && newTeam.TeamName.Length <= 100
                && !string.IsNullOrWhiteSpace(newTeam.Bio) && newTeam.Bio.Length <= 300
                && !string.IsNullOrWhiteSpace(newTeam.TeamWebsite) && newTeam.TeamWebsite.Length <= 300)
            {
                newTeam.Active = true;

                var checkCreate = await newTeam.PostAsync(_azureSQL);

                if (checkCreate)
                {
                    return Redirect($"/Teams/{newTeam.TeamName}");
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

        [Route("/Teams/{teamName}/Update")]
        public async Task<IActionResult> UpdateTeamAsync(int teamId)
        {
            return View();
        }

        [Route("/Teams/{teamName}/Delete")]
        public async Task<IActionResult> DeleteTeamAsync(int teamId)
        {
            return View();
        }
    }
}
