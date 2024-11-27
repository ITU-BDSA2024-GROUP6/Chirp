using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using System;
using System.Threading.Tasks;
using System.Data.Common;
using Chirp.Infrastructure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace testing.PlaywrightTests
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class Tests : PageTest, IDisposable
    {
        private IPlaywright _playwright;
        private IBrowserContext? _context;
        private IBrowser _browser;
        private TestServerFactory _factory;
        private HttpClient _client;
        private string _serverAddress;

        [SetUp]
        public async Task SetUp()
        {
            // Initialize the custom test server factory
            _factory = new TestServerFactory();
            _serverAddress = _factory.BaseServerUrl;

            // Create an HTTP client for interacting with the server
            _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = true,
                HandleCookies = true,
            });

            // Set up Playwright browser
            _playwright = await Microsoft.Playwright.Playwright.CreateAsync();
            _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = false, // Set to false to see the browser during tests
            });

            _context = await _browser.NewContextAsync(new BrowserNewContextOptions());


            // Possibly add arbitrary Register/Login logic for all test that check if authenticated users can do as intended. This should be able to be skipped if the test has something to do with Register or Login
        }

        [TearDown] 
        public async Task TearDown() 
        { 
            Dispose();
        }



        [Test]
        public async Task HomepageHasPlaywrightInTitleAndGetStartedLinkLinkingtoTheIntroPage()
        {
            var _page = await _context!.NewPageAsync();
            await _page.GotoAsync(_serverAddress);

            
            await _page.GetByRole(AriaRole.Link, new() { NameString = "Register" }).ClickAsync();
            await _page.GetByPlaceholder("Username").ClickAsync();
            await _page.GetByPlaceholder("Username").FillAsync("BennyMedDetHenny");
            await _page.GetByPlaceholder("name@example.com").ClickAsync();
            await _page.GetByPlaceholder("name@example.com").FillAsync("beor@itu.dk");
            await _page.GetByLabel("Password", new() { Exact = true }).ClickAsync();
            await _page.GetByLabel("Password", new() { Exact = true }).FillAsync("Password1!");
            await _page.GetByLabel("Confirm Password").ClickAsync();
            await _page.GetByLabel("Confirm Password").FillAsync("Password1!");
            await _page.GetByRole(AriaRole.Button, new() { NameString = "Register" }).ClickAsync();
            await _page.GetByRole(AriaRole.Button, new() { NameString = "Logout" }).ClickAsync();
            await _page.GetByRole(AriaRole.Link, new() { NameString = "Login" }).ClickAsync();
            await _page.GetByPlaceholder("name@example.com").ClickAsync();
            await _page.GetByPlaceholder("name@example.com").FillAsync("BennyMedDetHenny");
            await _page.GetByPlaceholder("Password").ClickAsync();
            await _page.GetByPlaceholder("Password").FillAsync("Password1!");
            await _page.GetByRole(AriaRole.Button, new() { NameString = "Log in" }).ClickAsync();
            await _page.GetByRole(AriaRole.Button, new() { NameString = "Logout" }).ClickAsync();
            await _page.GetByRole(AriaRole.Link, new() { NameString = "Login" }).ClickAsync();
            await _page.GetByPlaceholder("name@example.com").ClickAsync();
            await _page.GetByPlaceholder("name@example.com").FillAsync("beor@itu.dk");
            await _page.GetByPlaceholder("Password").ClickAsync();
            await _page.GetByPlaceholder("Password").FillAsync("Password1!");
            await _page.GetByRole(AriaRole.Button, new() { NameString = "Log in" }).ClickAsync();
            await _page.Locator("#cheepInput").ClickAsync();
            await _page.Locator("#cheepInput").FillAsync("Hello everybody!");
            await _page.GetByRole(AriaRole.Button, new() { NameString = "Share" }).ClickAsync();
            await _page.GetByRole(AriaRole.Link, new() { NameString = "BennyMedDetHenny", Exact = true }).ClickAsync();
            await _page.Locator("#cheepInput").ClickAsync();
            await _page.Locator("#cheepInput").FillAsync("I can write on my own timeline aswell! Cool!");
            await _page.GetByRole(AriaRole.Button, new() { NameString = "Share" }).ClickAsync();


        }
         //dispose browser and context after each test
        public void Dispose()
        {
        _context?.DisposeAsync().GetAwaiter().GetResult();
        _browser?.DisposeAsync().GetAwaiter().GetResult();
        _factory?.DisposeAsync().GetAwaiter().GetResult();
        _playwright = null;
        _serverAddress = null;
        _client = null;
        }
    }
}
