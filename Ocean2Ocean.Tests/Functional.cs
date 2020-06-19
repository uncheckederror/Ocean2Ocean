using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

using System.Net.Http;
using System.Threading.Tasks;

using Xunit;
using Xunit.Abstractions;

namespace Ocean2Ocean.Tests
{
    public class Functional : IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly ITestOutputHelper _output;
        private readonly IConfiguration _configuration;
        protected readonly HttpClient _client;

        public Functional(WebApplicationFactory<Startup> factory, ITestOutputHelper output)
        {
            _output = output;

            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddUserSecrets("40f816f3-0a65-4523-a9be-4bbef0716720")
                .Build();

            _configuration = config;

            _client = factory.CreateClient();
        }

        [Theory]
        [InlineData("/")]
        public async Task GetStaticPagesAsync(string url)
        {
            // Arrange
            var response = await _client.GetAsync(url);

            // Act
            response.EnsureSuccessStatusCode();
            var stringResponse = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Contains("Ocean", stringResponse);
        }
    }
}
