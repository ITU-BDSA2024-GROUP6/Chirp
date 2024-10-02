using System;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using System.Text.Json;
using System.Collections.Generic;

namespace Chirp.ApiTests
{
    public class ApiTests : IDisposable
    {
        private readonly HttpClient _client;
        private const string BaseUrl = "http://localhost:5273"; // Adjust this URL as needed

        public ApiTests()
        {
            _client = new HttpClient
            {
                BaseAddress = new Uri(BaseUrl)
            };
        }

        public void Dispose()
        {
            _client.Dispose();
        }

        [Fact]
        public async Task GetPublicCheeps_ReturnsOkStatusCode()
        {
            // Arrange
            var response = await _client.GetAsync("/");

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetUserTimeline_ReturnsOkStatusCode()
        {
            // Arrange
            var testUser = "Octavio Wagganer"; // Replace with a known user in your system
            var response = await _client.GetAsync($"/{testUser}");

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        }
    }

    // You might need to define this class to match your API's response
    public class CheepViewModel
    {
        public string Author { get; set; }
        public string Message { get; set; }
        public string Timestamp { get; set; }
    }
}