using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace integration
{
    public class IntegrationTests
    {
        private readonly HttpClient _client;

        public IntegrationTests()
        {
            _client = new HttpClient
            {
                BaseAddress = new Uri("http://localhost:5273") 
            };
        }

        [Fact]
        public async Task GetHomePage_ReturnsSuccessAndCorrectContentType()
        {
            var response = await _client.GetAsync("/");

            response.EnsureSuccessStatusCode(); 
            Assert.Equal("text/html; charset=utf-8", 
                response.Content.Headers.ContentType.ToString());
        }

        [Fact]
        public async Task GetUserTimeline_ReturnsSuccessAndCorrectContentType()
        {
            var testUser = "TestUser"; 

            var response = await _client.GetAsync($"/{testUser}");

            response.EnsureSuccessStatusCode();
            Assert.Equal("text/html; charset=utf-8", 
                response.Content.Headers.ContentType.ToString());
        }
    }
}
