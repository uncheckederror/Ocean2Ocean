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
                else if (results.Count() == 1)
                {
                    return Redirect($"/{results.FirstOrDefault().JourneyName}");
                }

                if (results.Count() > 1)
                {
                    return View("Journeys", new JourneysSearchResult
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
                    return View("Journeys", new JourneysSearchResult
                    {
                        Query = journeyName,
                        Journeys = results
                    });
                }
            }
        }

        [HttpGet]
        [HttpPost]
        [Route("/Journeys/Add")]
        public async Task<IActionResult> AddJourneyAsync([Bind("JourneyName,Bio,GeometryFileName,ImagePath")] Journey? newJourney)
        {
            if (newJourney != null && !string.IsNullOrWhiteSpace(newJourney.JourneyName) && newJourney.JourneyName.Length <= 100
                && !string.IsNullOrWhiteSpace(newJourney.Bio) && newJourney.Bio.Length <= 300
                && !string.IsNullOrWhiteSpace(newJourney.GeometryFileName) && newJourney.GeometryFileName.Length <= 300
                && !string.IsNullOrWhiteSpace(newJourney.ImagePath) && newJourney.ImagePath.Length <= 100)
            {
                var checkCreate = await newJourney.PostAsync(_azureSQL);

                if (checkCreate)
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

        [Route("/Journeys/{journeyId}/Update")]
        public async Task<IActionResult> UpdateJourneyAsync(int journeyId)
        {
            return View();
        }

        [Route("/Journeys/{journeyId}/Delete")]
        public async Task<IActionResult> DeleteJourneyAsync(int journeyId)
        {
            return View();
        }
    }
}
