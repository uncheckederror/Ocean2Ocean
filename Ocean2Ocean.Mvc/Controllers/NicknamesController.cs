using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

using Ocean2Ocean.DataAccess;

namespace Ocean2Ocean.Controllers
{
    public class NicknamesController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly string _azureSQL;

        public NicknamesController(IConfiguration configuration)
        {
            _configuration = configuration;
            _azureSQL = _configuration.GetConnectionString("AzureSQL");
        }

        [Route("/Nicknames/")]
        public async Task<IActionResult> IndexAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                var results = await Nickname.GetAllAsync(_azureSQL);

                return View("Nicknames", new NicknameSearchResult
                {
                    Query = name,
                    Nicknames = results
                });
            }
            else
            {
                var results = await Nickname.GetByNicknameAsync(name, _azureSQL);

                if (!results.Any())
                {
                    results = await Nickname.SearchByNicknameAsync(name, _azureSQL);
                }

                if (results.Count() > 1)
                {
                    return View("Nicknames", new NicknameSearchResult
                    {
                        Query = name,
                        Nicknames = results
                    });
                }
                else
                {
                    return View("Nicknames", new NicknameSearchResult
                    {
                        Query = name,
                        Nicknames = results
                    });
                }
            }
        }

        [Route("/Nicknames/{name}")]
        public async Task<IActionResult> ViewName(string name)
        {
            var nicknames = await Nickname.GetByNicknameAsync(name, _azureSQL);
            var nickname = nicknames.FirstOrDefault();

            var entries = await Step.GetByExactNicknameAsync(nickname?.Name, _azureSQL);

            var uniqueJourneyNames = entries.Select(x => x.JourneyName).Distinct();

            var journeys = new List<Journey>();
            foreach (var journey in uniqueJourneyNames)
            {
                var results = await Journey.GetByJourneyNameAsync(journey, _azureSQL);
                if (!(results is null))
                {
                    journeys.Add(results.FirstOrDefault());
                }
            }

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

            return View("Nickname", new NicknameResult
            {
                Nickname = nickname,
                Journeys = journeys,
                Teams = teams,
                Steps = entries
            });
        }

        [Route("/Nicknames/Add/")]
        public async Task<IActionResult> AddNameAsync([Bind("Name,Bio")] Nickname newNickname)
        {
            if (newNickname != null && !string.IsNullOrWhiteSpace(newNickname.Name) && newNickname.Name.Length <= 100
                && !string.IsNullOrWhiteSpace(newNickname.Bio) && newNickname.Bio.Length <= 300)
            {
                newNickname.Active = true;

                var checkCreate = await newNickname.PostAsync(_azureSQL);

                if (checkCreate)
                {
                    return Redirect($"/Nicknames/{newNickname.Name}");
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

        [Route("/Nicknames/Update")]
        public async Task<IActionResult> UpdateNameAsync([Bind("NicknameId,Name,Bio")] Nickname newNickname)
        {
            if (newNickname != null
                && !string.IsNullOrWhiteSpace(newNickname.Name)
                && newNickname.Name.Length <= 100
                && !string.IsNullOrWhiteSpace(newNickname.Bio)
                && newNickname.Bio.Length <= 300)
            {
                newNickname.Active = true;

                var nickname = await Nickname.GetByIdAsync(newNickname.NicknameId, _azureSQL);

                nickname.Bio = newNickname.Bio.Trim();
                // Without implementing cascading changes in the Steps table for all entries with this Nickname there's no value in allow the user to change their nickname. They are better off just making another.
                //nickname.Name = newNickname.Name.Trim();

                var checkCreate = await nickname.PutAsync(_azureSQL);

                if (checkCreate)
                {
                    return Redirect($"/Nicknames/{nickname.Name}");
                }
            }

            return Redirect($"/Nicknames/{newNickname.Name}");
        }

        [Route("/Nicknames/Delete")]
        public async Task<IActionResult> DeleteNameAsync(int nicknameId)
        {
            return View();
        }
    }
}
