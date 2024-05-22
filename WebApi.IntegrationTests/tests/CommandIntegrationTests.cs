using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Application.DTO;
using DataModel.Repository;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using WebApi.IntegrationTests.Helpers;
using Xunit;

namespace WebApi.IntegrationTests.Tests
{
    public class CommandIntegrationTests : IClassFixture<IntegrationTestsWebAppFactory<Program>>
    {
        private readonly IntegrationTestsWebAppFactory<Program> _factory;
        private readonly HttpClient _client;

        public CommandIntegrationTests(IntegrationTestsWebAppFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
        }

        [Fact]
        public async Task PostAssociation_ReturnsAccepted_WhenValidAssociation()
        {

            // Arrange
            var client = _factory.CreateClient();
            // Arrange
            using (var scope = _factory.Services.CreateScope())
            {
                var scopedServices = scope.ServiceProvider;
                var db = scopedServices.GetRequiredService<AbsanteeContext>();

                Utilities.ReinitializeDbForTests(db);
            }
            var associationDTO = new
            {
                ColaboratorId = 1,
                ProjectId = 1,
                StartDate = DateOnly.FromDateTime(DateTime.Now),
                EndDate = DateOnly.FromDateTime(DateTime.Now.AddDays(10)),
                Fundamental = true
            };

            var content = new StringContent(JsonConvert.SerializeObject(associationDTO), Encoding.UTF8, "application/json");

            // Act
            var response = await client.PostAsync("/api/Association", content);

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(System.Net.HttpStatusCode.Accepted, response.StatusCode);

            var responseString = await response.Content.ReadAsStringAsync();
            var returnedDto = JsonConvert.DeserializeObject<AssociationDTO>(responseString);

            Assert.NotNull(returnedDto);
        }

        [Fact]
        public async Task PostAssociation_ReturnsBadRequest_WhenInvalidAssociation()
        {
            // Arrange
            var client = _factory.CreateClient();
            using (var scope = _factory.Services.CreateScope())
            {
                var scopedServices = scope.ServiceProvider;
                var db = scopedServices.GetRequiredService<AbsanteeContext>();

                Utilities.ReinitializeDbForTests(db);
            }
            var invalidAssociationDTO = new
            {
                ColaboratorId = 1,
                ProjectId = 1,
                StartDate = DateOnly.FromDateTime(DateTime.Now),
                EndDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-1)), // End date before start date
                Fundamental = true,
            };

            var content = new StringContent(JsonConvert.SerializeObject(invalidAssociationDTO), Encoding.UTF8, "application/json");

            // Act
            var response = await client.PostAsync("/api/Association", content);

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}
