using Microsoft.Playwright;
using NUnit.Framework;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;


namespace test.PlaywrightTests
{
    [TestFixture, NonParallelizable]
    public class UITest : PageTest, IClassFixture<CustomWebApplicationFactory>, IDisposable
    {
        private IBrowserContext? _context;
        private IBrowser? _browser;
        private CustomWebApplicationFactory _factory;
        private string _serverAddress;
        private IPlaywright _playwright;
        private HttpClient _client;
        private IPage _page;

        [SetUp]
        public async Task SetUp()
        {            
            _factory = new CustomWebApplicationFactory();
            _serverAddress = _factory.TestServerAddress;
            _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = true,
                HandleCookies = true,
            });

            await InitializeBrowserAndCreateBrowserContextAsync();
            var test = TestContext.CurrentContext.Test;

            // Check if the test is marked with the "SkipSetUp" category
            if (!test.Properties["Category"].Contains("SkipSetUp"))
            {
                // await SetUpRegisterAndLogin();
            }
        }

        [Test]
        public async Task Test_LoginFunctionality()
        {
            var _page = await _context!.NewPageAsync();
            await _page.GotoAsync(_serverAddress);

            await _page.GetByRole(AriaRole.Link, new() { NameString = "Register" }).ClickAsync();
            await _page.GetByPlaceholder("Username").ClickAsync();
            await _page.GetByPlaceholder("Username").FillAsync("TestUser");
            await _page.GetByPlaceholder("name@example.com").ClickAsync();
            await _page.GetByPlaceholder("name@example.com").FillAsync("Test@Test.Test");
            await _page.GetByLabel("Password", new() { Exact = true }).ClickAsync();
            await _page.GetByLabel("Password", new() { Exact = true }).FillAsync("Password1!");
            await _page.GetByLabel("Confirm Password").ClickAsync();
            await _page.GetByLabel("Confirm Password").FillAsync("Password1!");
            await _page.GetByRole(AriaRole.Button, new() { NameString = "Register" }).ClickAsync();


        }

        [TearDown] 
        public async Task TearDown() 
        { 
            Console.WriteLine("Tearing down");
            Dispose();
        }

        private async Task InitializeBrowserAndCreateBrowserContextAsync() 
        {
            _playwright = await Microsoft.Playwright.Playwright.CreateAsync();
            _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true, //Set to false if you want to see the browser
            });
                
            _context = await _browser.NewContextAsync(new BrowserNewContextOptions());
        }

        public void Dispose()
        {
            _context?.DisposeAsync().GetAwaiter().GetResult();
            _browser?.DisposeAsync().GetAwaiter().GetResult();
        }

        
/*
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
        */
    }
}
