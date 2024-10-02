using System;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using System.Text.Json;
using System.Collections.Generic;

namespace Chirp.E2ETest
{
    public class E2ETest : IDisposable
    {
        private readonly HttpClient _client;
        private const string BaseUrl = "http://localhost:5273"; // Adjust this URL as needed

        public E2ETest()
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
        public async Task PublicTimeline_ShouldContainCheepsAndCheep()
        {
            // Arrange: URL for the public timeline page
            var url = "/";
            // Act: Send GET request to public timeline
            var response = await _client.GetAsync(url);
            
            // Assert: Ensure response was successful (status code 200)
            response.EnsureSuccessStatusCode();
            
            // Act: Read HTML content
            var content = await response.Content.ReadAsStringAsync();
            
            // Assert: Check that some chirps are rendered
            Assert.Contains("cheeps", content);  // Should match the Class of the chirps list
            Assert.Contains("cheep", content);   // Should match the Class of a single cheep
        }

        [Fact]
        public async Task UserTimeline_ShouldContainCheepsAndCheepFromUser()
        {
            var username = "Jacqualine Gilcoine";
            // Arrange: URL for a user's timeline page (use an example author name like 'john')
            var url = $"/{username}";  // Assuming 'john' is an existing user
            
            // Act: Send GET request to user timeline
            var response = await _client.GetAsync(url);
            
            // Assert: Ensure response was successful (status code 200)
            response.EnsureSuccessStatusCode();
            
            // Act: Read HTML content
            var content = await response.Content.ReadAsStringAsync();
            
            // Assert: Check that user chirps are rendered
            Assert.Contains(username, content);  // Check that the correct author's name appears in the page
            Assert.DoesNotContain("Octavio Wagganer", content); // Check that a false author's name does not appear in the page
            Assert.Contains("cheep", content);   // Should match the Class of a single cheep
        }
    }
}