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
                //else if (results.Count() == 1)
                //{
                //    return Redirect($"/{results.FirstOrDefault().JourneyName}");
                //}

                if (results.Count() > 1)
                {
                    return View("Nicknames", new NicknameSearchResult
                    {
                        Query = name,
                        Nicknames = results
                    });
                }
                //else if (results.Count() == 1)
                //{
                //    return Redirect($"/{results.FirstOrDefault().JourneyName}");
                //}
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
            var results = await Nickname.GetByNicknameAsync(name, _azureSQL);

            // Maybe look up all the entries that reference this team?
            return View("Nicknames", new NicknameSearchResult
            {
                Query = name,
                Nicknames = results
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

        [Route("/Nicknames/{name}/Update")]
        public async Task<IActionResult> UpdateNameAsync(int teamId)
        {
            return View();
        }

        [Route("/Nicknames/{name}/Delete")]
        public async Task<IActionResult> DeleteNameAsync(int teamId)
        {
            return View();
        }
    }
}
