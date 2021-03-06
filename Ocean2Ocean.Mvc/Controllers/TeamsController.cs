﻿using System;
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

                if (results.Count() > 1)
                {
                    return View("Teams", new TeamsSearchResult
                    {
                        Query = teamName,
                        Teams = results
                    });
                }
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
            var teams = await Team.GetByTeamNameAsync(teamName, _azureSQL);
            var team = teams.FirstOrDefault();

            var entries = await Step.GetByTeamAsync(team?.TeamName, _azureSQL);

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

            return View("Team", new TeamResult
            {
                Team = team,
                Nicknames = nicknames,
                Steps = entries
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

        [Route("/Teams/Update")]
        public async Task<IActionResult> UpdateTeamAsync([Bind("TeamId,TeamName,Bio,TeamWebsite")] Team newTeam)
        {
            if (newTeam != null && !string.IsNullOrWhiteSpace(newTeam.TeamName) && newTeam.TeamName.Length <= 100
                && !string.IsNullOrWhiteSpace(newTeam.Bio) && newTeam.Bio.Length <= 300
                && !string.IsNullOrWhiteSpace(newTeam.TeamWebsite) && newTeam.TeamWebsite.Length <= 300)
            {
                var team = await Team.GetByIdAsync(newTeam.TeamId, _azureSQL);

                team.TeamWebsite = newTeam.TeamWebsite;
                team.Bio = newTeam.Bio;

                var checkCreate = await team.PutAsync(_azureSQL);

                if (checkCreate)
                {
                    return Redirect($"/Teams/{team.TeamName}");
                }
            }

            return Redirect($"/Teams/{newTeam.TeamName}");
        }

        [Route("/Teams/{teamName}/Delete")]
        public async Task<IActionResult> DeleteTeamAsync(int teamId)
        {
            return View();
        }
    }
}
