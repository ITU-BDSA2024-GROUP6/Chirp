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
        public async Task UserCanRegister()
        {

            var _page = await _context!.NewPageAsync();
            await _page.GotoAsync(_serverAddress);

            await _page.GetByRole(AriaRole.Button, new() { Name = "Register" }).ClickAsync();
            await _page.WaitForURLAsync(new Regex("/Identity/Account/Register$"));

            await _page.GetByPlaceholder("Username").ClickAsync();
            await Expect(_page.GetByPlaceholder("Username")).ToBeFocusedAsync();
            await _page.GetByPlaceholder("Username").FillAsync("TestUser");
            await Expect(_page.GetByPlaceholder("Username")).ToHaveValueAsync("TestUser");


            await _page.GetByPlaceholder("name@example.com").ClickAsync();
            await Expect(_page.GetByPlaceholder("name@example.com")).ToBeFocusedAsync();
            await _page.GetByPlaceholder("name@example.com").FillAsync("TestUser@Test.Test");
            await Expect(_page.GetByPlaceholder("name@example.com")).ToHaveValueAsync("TestUser@Test.Test");


            await _page.GetByLabel("Password", new() { Exact = true }).ClickAsync();
            await _page.GetByLabel("Password", new() { Exact = true }).FillAsync("Password1!");
            await Expect(_page.GetByLabel("Password", new() { Exact = true })).ToHaveValueAsync("Password1!");
            await _page.GetByLabel("Confirm Password").ClickAsync();
            await _page.GetByLabel("Confirm Password").FillAsync("Password1!");
            await Expect(_page.GetByLabel("Confirm Password")).ToHaveValueAsync("Password1!");

            await _page.Locator("#registerSubmit").ClickAsync();
            await Expect(_page).ToHaveURLAsync(_serverAddress);
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


    }
}
