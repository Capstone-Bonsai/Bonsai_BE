using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace WebAPI.Tests
{
    public class ProgramTests
    {
        private readonly HttpClient _httpClient;
        private readonly WebApplicationFactory<Program> _application;
        public ProgramTests()
        {
            _application = new WebApplicationFactory<Program>();
            //.WithWebHostBuilder(builder =>
            //{
            //    builder.ConfigureServices(services =>
            //    {
            //        services.AddHealthChecks();
            //    });
            //});

            _httpClient = _application.CreateClient();
        }

        [Fact]
        public async Task Program_HealthCheck_ShouldReturnHealthResult()
        {

            var response = await _httpClient.GetStringAsync("/healthchecks");

            response.Should().Be("Healthy");
        }
    }
}
