using Microsoft.Extensions.Configuration;

using Ocean2Ocean.Models;

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
        public async Task GetByEmail()
        {
            var results = await Step.GetByEmail("batman", _azureSql);
            Assert.NotNull(results);
            Assert.True(results.Any());
            _output.WriteLine(results.Sum(x => x.Steps).ToString() + " Steps in Total");
        }

        [Fact]
        public async Task AddAnEntry()
        {
            var entry = new Step
            {
                Email = "batman@theBestCounty.gov",
                Steps = 100,
                Created = DateTime.Now
            };

            var checkSubmitted = await entry.PostAsync(_azureSql);

            Assert.True(checkSubmitted);
        }
    }
}
