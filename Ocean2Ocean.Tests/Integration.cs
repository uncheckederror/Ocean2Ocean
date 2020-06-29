using Microsoft.AspNetCore.Routing;
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
        public async Task GetAllSteps()
        {
            var results = await Step.GetAllAsync(_azureSql);
            Assert.NotNull(results);
            Assert.True(results.Any());
        }

        [Fact]
        public async Task GetById()
        {
            var results = await Step.GetAllAsync(_azureSql);
            Assert.NotNull(results);
            Assert.True(results.Any());

            var specificEntry = results.FirstOrDefault();
            var checkSpecificEntry = await Step.GetByIdAsync(specificEntry.StepId, _azureSql);
            Assert.NotNull(checkSpecificEntry);
            Assert.Equal(checkSpecificEntry.StepId, specificEntry.StepId);
            Assert.Equal(checkSpecificEntry.Nickname, specificEntry.Nickname);
            Assert.Equal(checkSpecificEntry.Created, specificEntry.Created);
            Assert.Equal(checkSpecificEntry.JourneyName, specificEntry.JourneyName);
            Assert.Equal(checkSpecificEntry.Steps, specificEntry.Steps);
        }

        [Fact]
        public async Task GetByEmail()
        {
            var results = await Step.GetByNicknameAsync("batman", _azureSql);
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
        public async Task GetTeamRankingsByJourneyName()
        {
            var results = await Step.GetByJourneyAsync("test", _azureSql);
            Assert.NotNull(results);
            Assert.True(results.Any());

            var journey = results.FirstOrDefault();

            var leaders = await Leaderboard.GetTeamRankingsAsync(journey.JourneyName, _azureSql);
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
        public async Task GetDailyTeamRankingsByJourneyName()
        {
            var results = await Step.GetByJourneyAsync("test", _azureSql);
            Assert.NotNull(results);
            Assert.True(results.Any());

            var journey = results.FirstOrDefault();

            var leaders = await Leaderboard.GetDailyTeamRankingsAsync(journey.JourneyName, _azureSql);
            Assert.NotNull(leaders);
            Assert.True(leaders.Any());
        }

        [Fact]
        public async Task GetTotalStepsByDay()
        {
            var results = await Step.GetByJourneyAsync("test", _azureSql);
            Assert.NotNull(results);
            Assert.True(results.Any());

            var journey = results.FirstOrDefault();

            var leaders = await Leaderboard.GetTotalStepsByDayAsync(journey.JourneyName, _azureSql);
            Assert.NotNull(leaders);
            Assert.True(leaders.Any());
        }

        [Fact]
        public async Task GetAllNicknames()
        {
            var results = await Nickname.GetAllAsync(_azureSql);
            Assert.NotNull(results);
            Assert.True(results.Any());
        }

        [Fact]
        public async Task AddAnEntry()
        {
            var entry = new Step
            {
                Nickname = "batman",
                JourneyName = "Test",
                TeamName = "Test Team",
                Steps = 100,
                Created = DateTime.Now
            };

            var checkSubmitted = await entry.PostAsync(_azureSql);

            Assert.True(checkSubmitted);

            var fromDb = await Step.GetByNicknameAsync(entry.Nickname, _azureSql);
            var entryFromDb = fromDb.FirstOrDefault();
            Assert.NotNull(entryFromDb);

            var checkDelete = await entryFromDb.DeleteAsync(_azureSql);
            Assert.True(checkDelete);
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
                Nickname = "batman",
                JourneyName = "Test",
                TeamName = "Test Team",
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
                Nickname = "batman@theBestCounty.gov",
                JourneyName = "Test",
                TeamName = "Test Team",
                Steps = 100,
                Created = DateTime.Now
            };

            var checkSubmitted = await entry.PostAsync(_azureSql);

            Assert.True(checkSubmitted);

            var results = await Step.GetByNicknameAsync("batman@theBestCounty.gov", _azureSql);
            var deleteMe = results.FirstOrDefault();

            var checkDelete = await deleteMe.DeleteAsync(_azureSql);

            Assert.True(checkDelete);
        }

        [Fact]
        public async Task CreateAndDeleteAJourney()
        {
            var journey = new Journey
            {
                JourneyName = "AddDeleteTest",
                GeometryFileName = "/route.geojson",
                Bio = $"This is a test Journey created on {DateTime.Now}",
                Active = true,
                Created = DateTime.Now,
                ImagePath = "/images/DSC06102small.jpg"
            };

            var result = await journey.PostAsync(_azureSql);

            Assert.True(result);

            var getFromDb = await Journey.GetByJourneyNameAsync(journey.JourneyName, _azureSql);

            Assert.True(getFromDb.Any());

            var created = getFromDb.FirstOrDefault();
            var checkDelete = await created.DeleteAsync(_azureSql);

            Assert.True(checkDelete);
        }

        [Fact]
        public async Task UpdateAnExistingJourney()
        {
            var results = await Journey.GetAllAsync(_azureSql);
            Assert.True(results.Any());
            var selected = results.FirstOrDefault();

            var checkid = await Journey.GetByIdAsync(selected.JourneyId, _azureSql);
            Assert.Equal(selected.JourneyId, checkid.JourneyId);

            var newBio = $"This is a test Journey created on {DateTime.Now}";
            checkid.Bio = newBio;
            var checkUpdate = await checkid.PutAsync(_azureSql);
            Assert.True(checkUpdate);
        }

        [Fact]
        public async Task GetAllJourneys()
        {
            var results = await Journey.GetAllAsync(_azureSql);
            Assert.True(results.Any());
        }

        [Fact]
        public async Task GetJourneyById()
        {
            var results = await Journey.GetAllAsync(_azureSql);
            Assert.True(results.Any());
            var selected = results.FirstOrDefault();

            var checkid = await Journey.GetByIdAsync(selected.JourneyId, _azureSql);
            Assert.Equal(selected.JourneyId, checkid.JourneyId);
        }

        [Fact]
        public async Task GetJourneyByName()
        {
            var results = await Journey.GetAllAsync(_azureSql);
            Assert.True(results.Any());
            var selected = results.FirstOrDefault();

            var checkid = await Journey.GetByJourneyNameAsync(selected.JourneyName, _azureSql);
            var selectedChecked = checkid.FirstOrDefault();
            Assert.Equal(selected.JourneyName, selectedChecked.JourneyName);
        }

        [Fact]
        public async Task SearchByJourneyByName()
        {
            var results = await Journey.GetAllAsync(_azureSql);
            Assert.True(results.Any());
            var selected = results.FirstOrDefault();

            var checkid = await Journey.SearchByJourneyNameAsync(selected.JourneyName, _azureSql);
            var selectedChecked = checkid.FirstOrDefault();
            Assert.Equal(selected.JourneyName, selectedChecked.JourneyName);
        }

        [Fact]
        public async Task CreateAndDeleteTeam()
        {
            var team = new Team
            {
                TeamName = "TestCreateTeam",
                TeamWebsite = "https://thomasryan.xyz/",
                Bio = "This is a team created to test the process of team creation and deletion.",
                Active = true,
                Created = DateTime.Now
            };

            var checkSubmit = await team.PostAsync(_azureSql);

            Assert.True(checkSubmit);

            var fromDb = await Team.GetByTeamNameAsync(team.TeamName, _azureSql);
            var thisTeam = fromDb.FirstOrDefault();

            Assert.Equal(team.TeamName, thisTeam.TeamName);

            var checkDelete = await thisTeam.DeleteAsync(_azureSql);
            Assert.True(checkDelete);
        }

        [Fact]
        public async Task CreateAndDeleteNickname()
        {
            var nickname = new Nickname
            {
                Name = "TestNickname",
                Bio = "This is a team created to test the process of team creation and deletion.",
                Active = true,
                Created = DateTime.Now
            };

            var checkSubmit = await nickname.PostAsync(_azureSql);

            Assert.True(checkSubmit);

            var fromDb = await Nickname.GetByNicknameAsync(nickname.Name, _azureSql);
            var thisTeam = fromDb.FirstOrDefault();

            Assert.Equal(nickname.Name, thisTeam.Name);

            var checkDelete = await thisTeam.DeleteAsync(_azureSql);
            Assert.True(checkDelete);
        }
    }
}
