using Microsoft.Extensions.Configuration;

using Ocean2Ocean.DataAccess;

using System;
using System.Linq;
using System.Threading.Tasks;

using Xunit;
using Xunit.Abstractions;

namespace Ocean2Ocean.Tests
{
    public class Integration
    {
        private readonly IConfiguration _configuration;
        private readonly ITestOutputHelper _output;
        private readonly string _azureSql;

        public Integration(ITestOutputHelper output)
        {
            this._output = output;

            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddUserSecrets("4cbe49cf-2224-4186-b153-1574495d6b5c")
                .Build();

            _configuration = config;

            _azureSql = _configuration.GetConnectionString("AzureSQL");
        }

        [Fact]
        public async Task SumSteps()
        {
            var results = await Step.GetAllAsync(_azureSql);
            Assert.NotNull(results);
            Assert.True(results.Any());
            _output.WriteLine(results.Sum(x => x.Steps).ToString() + " Steps in Total");
        }

        [Fact]
        public async Task GetJourneys()
        {
            var results = await Journey.GetAllAsync(_azureSql);
            Assert.NotNull(results);
            Assert.True(results.Any());
            foreach (var result in results)
            {
                _output.WriteLine(result.JourneyName);
                _output.WriteLine(result.StepsTaken.ToString());
            }
        }

        [Fact]
        public async Task GetById()
        {
            var results = await Step.GetAllAsync(_azureSql);
            Assert.NotNull(results);
            Assert.True(results.Any());

            var specificEntry = results.FirstOrDefault();
            var checkSpecificEntry = await Step.GetByIdAsync(specificEntry.EntryId, _azureSql);
            Assert.NotNull(checkSpecificEntry);
            Assert.Equal(checkSpecificEntry.EntryId, specificEntry.EntryId);
            Assert.Equal(checkSpecificEntry.Email, specificEntry.Email);
            Assert.Equal(checkSpecificEntry.Created, specificEntry.Created);
            Assert.Equal(checkSpecificEntry.JourneyName, specificEntry.JourneyName);
            Assert.Equal(checkSpecificEntry.Steps, specificEntry.Steps);
        }

        [Fact]
        public async Task GetByEmail()
        {
            var results = await Step.GetByEmailAsync("batman", _azureSql);
            Assert.NotNull(results);
            Assert.True(results.Any());
            _output.WriteLine(results.Sum(x => x.Steps).ToString() + " Steps in Total");
        }

        [Fact]
        public async Task GetByJourneyName()
        {
            var results = await Step.GetByJourneyAsync("test", _azureSql);
            Assert.NotNull(results);
            Assert.True(results.Any());
            _output.WriteLine(results.Sum(x => x.Steps).ToString() + " Steps in Total");
        }

        [Fact]
        public async Task GetRankingsByJourneyName()
        {
            var results = await Step.GetByJourneyAsync("test", _azureSql);
            Assert.NotNull(results);
            Assert.True(results.Any());

            var journey = results.FirstOrDefault();

            var leaders = await Leaderboard.GetRankingsAsync(journey.JourneyName, _azureSql);
            Assert.NotNull(leaders);
            Assert.True(leaders.Any());
        }

        [Fact]
        public async Task GetDailyRankingsByJourneyName()
        {
            var results = await Step.GetByJourneyAsync("test", _azureSql);
            Assert.NotNull(results);
            Assert.True(results.Any());

            var journey = results.FirstOrDefault();

            var leaders = await Leaderboard.GetDailyRankingsAsync(journey.JourneyName, _azureSql);
            Assert.NotNull(leaders);
            Assert.True(leaders.Any());
        }

        [Fact]
        public async Task AddAnEntry()
        {
            var entry = new Step
            {
                Email = "batman@theBestCounty.gov",
                JourneyName = "Test",
                Steps = 100,
                Created = DateTime.Now
            };

            var checkSubmitted = await entry.PostAsync(_azureSql);

            Assert.True(checkSubmitted);
        }

        [Fact]
        public async Task UpdateAnEntry()
        {
            var results = await Step.GetByJourneyAsync("test", _azureSql);
            Assert.NotNull(results);
            Assert.True(results.Any());

            var entry = results.FirstOrDefault();
            entry.Steps = entry.Steps + 100;
            entry.Created = DateTime.Now;

            var checkSubmitted = await entry.PutAsync(_azureSql);

            Assert.True(checkSubmitted);
        }

        [Fact]
        public async Task CheckForDuplicateEntry()
        {
            var entry = new Step
            {
                Email = "batman@theBestCounty.gov",
                JourneyName = "Test",
                Steps = 100,
                Created = DateTime.Now
            };

            var checkSubmitted = await entry.PostAsync(_azureSql);

            Assert.True(checkSubmitted);

            var checkDuplicate = await entry.CheckForDuplicateAsync(_azureSql);

            Assert.True(checkDuplicate);
        }

        [Fact]
        public async Task AddAndDeleteAnEntry()
        {
            var entry = new Step
            {
                Email = "batman@theBestCounty.gov",
                JourneyName = "Test",
                Steps = 100,
                Created = DateTime.Now
            };

            var checkSubmitted = await entry.PostAsync(_azureSql);

            Assert.True(checkSubmitted);

            var results = await Step.GetByEmailAsync("batman@theBestCounty.gov", _azureSql);
            var deleteMe = results.FirstOrDefault();

            var checkDelete = await deleteMe.DeleteAsync(_azureSql);

            Assert.True(checkDelete);
        }
    }
}
