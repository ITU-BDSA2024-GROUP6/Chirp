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
                await SetUpRegisterAndLogin();
            }
        }

        [Test, Category("SkipSetUp")]
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

        [Test]
        public async Task UserCanLogout()
        {
            await _page.GetByRole(AriaRole.Button, new() { Name = "Logout" }).ClickAsync();
            var expectedURL = _serverAddress + "Identity/Account/Logout";
            await Expect(_page).ToHaveURLAsync(expectedURL);
            var loginButton = _page.GetByRole(AriaRole.Button, new() { Name = "Login" });
            await Expect(loginButton).ToBeVisibleAsync();
        }

        [Test]
        public async Task UserCanLoginWithUsername()
        {
            await _page.GetByRole(AriaRole.Button, new() { Name = "Logout" }).ClickAsync();

            await _page.GetByRole(AriaRole.Button, new() { Name = "Login" }).ClickAsync();
            await _page.GetByPlaceholder("name@example.com").ClickAsync();
            await _page.GetByPlaceholder("name@example.com").FillAsync("TestUser");
            await Expect(_page.GetByPlaceholder("name@example.com")).ToHaveValueAsync("TestUser");
            await _page.GetByPlaceholder("Password").ClickAsync();
            await _page.GetByPlaceholder("Password").FillAsync("Password1!");
            await Expect(_page.GetByPlaceholder("Password")).ToHaveValueAsync("Password1!");

            await _page.GetByRole(AriaRole.Button, new() { Name = "Log in" }).ClickAsync();
            await Expect(_page).ToHaveURLAsync(_serverAddress);
        }

        [Test]
        public async Task UserCanLoginWithEmail()
        {
            await _page.GetByRole(AriaRole.Button, new() { Name = "Logout" }).ClickAsync();

            await _page.GetByRole(AriaRole.Button, new() { Name = "Login" }).ClickAsync();
            await _page.GetByPlaceholder("name@example.com").ClickAsync();
            await _page.GetByPlaceholder("name@example.com").FillAsync("TestUser@Test.Test");
            await Expect(_page.GetByPlaceholder("name@example.com")).ToHaveValueAsync("TestUser@Test.Test");
            await _page.GetByPlaceholder("Password").ClickAsync();
            await _page.GetByPlaceholder("Password").FillAsync("Password1!");
            await Expect(_page.GetByPlaceholder("Password")).ToHaveValueAsync("Password1!");

            await _page.GetByRole(AriaRole.Button, new() { Name = "Log in" }).ClickAsync();
            await Expect(_page).ToHaveURLAsync(_serverAddress);
        }

        [Test]
        public async Task UserCanGoToPrivateTimelineByClickingOnYourTimeline()
        {
            await _page.GetByRole(AriaRole.Button, new() { Name = "Your Timeline" }).ClickAsync();
            var expectedURL = _serverAddress + "TestUser";
            await Expect(_page).ToHaveURLAsync(expectedURL);
        }

        [Test]
        public async Task UserCanGoToPublicTimelineByClickingOnPublicTimeline()
        {
            await _page.GetByRole(AriaRole.Button, new() { Name = "Public Timeline" }).ClickAsync();
            await Expect(_page).ToHaveURLAsync(_serverAddress);
        }

        [Test]
        public async Task UserCanGoToAboutMePageByClickingOnAboutMe()
        {
            await _page.GetByRole(AriaRole.Button, new() { Name = "About me" }).ClickAsync();
            var expectedURL = _serverAddress + "Identity/Account/Manage";
            await Expect(_page).ToHaveURLAsync(expectedURL);
        }

        [Test]
        public async Task UserCanPostCheepsOnPublicTimeline()
        {
            await _page.Locator("#cheepInput").ClickAsync();
            await Expect(_page.Locator("#cheepInput")).ToBeFocusedAsync();

            await _page.Locator("#cheepInput").FillAsync("Test Cheep");
            await Expect(_page.Locator("#cheepInput")).ToHaveValueAsync("Test Cheep");

            await _page.GetByRole(AriaRole.Button, new() { Name = "Share" }).ClickAsync();

            var cheep = _page.GetByText("Test Cheep");
            await cheep.HighlightAsync();
            await Expect(cheep).ToBeVisibleAsync();

            await Expect(_page).ToHaveURLAsync(_serverAddress);
        }

        [Test]
        public async Task UserCanPostCheepsOnPrivateTimeline()
        {
            await _page.GetByRole(AriaRole.Button, new() { Name = "Your Timeline" }).ClickAsync();
            var expectedURL = _serverAddress + "TestUser";
            await Expect(_page).ToHaveURLAsync(expectedURL);

            await _page.Locator("#cheepInput").ClickAsync();
            await Expect(_page.Locator("#cheepInput")).ToBeFocusedAsync();

            await _page.Locator("#cheepInput").FillAsync("Test Cheep");
            await Expect(_page.Locator("#cheepInput")).ToHaveValueAsync("Test Cheep");

            await _page.GetByRole(AriaRole.Button, new() { Name = "Share" }).ClickAsync();

            var cheep = _page.GetByText("Test Cheep");
            await cheep.HighlightAsync();
            await Expect(cheep).ToBeVisibleAsync();

            await Expect(_page).ToHaveURLAsync(expectedURL);
        }

        [Test]
        public async Task UserCanAddPhoneNumberToAccount()
        {
            await _page.GetByRole(AriaRole.Button, new() { Name = "About me" }).ClickAsync();
            var expectedURL = _serverAddress + "Identity/Account/Manage";
            await Expect(_page).ToHaveURLAsync(expectedURL);

            await Expect(_page.GetByPlaceholder("Please enter your phone")).ToBeEmptyAsync();
            await _page.GetByPlaceholder("Please enter your phone").ClickAsync();
            await _page.GetByPlaceholder("Please enter your phone").FillAsync("00000000");
            await Expect(_page.GetByPlaceholder("Please enter your phone")).ToHaveValueAsync("00000000");
            await _page.GetByRole(AriaRole.Button, new() { Name = "Save" }).ClickAsync();
            await Expect(_page.GetByPlaceholder("Please enter your phone")).ToHaveValueAsync("00000000");
            await Expect(_page).ToHaveURLAsync(expectedURL);
        }

        [Test]
        public async Task UserCanChangePassword()
        {
            await _page.GetByRole(AriaRole.Button, new() { Name = "About me" }).ClickAsync();
            var expectedURL = _serverAddress + "Identity/Account/Manage";
            await Expect(_page).ToHaveURLAsync(expectedURL);

            await _page.GetByRole(AriaRole.Link, new() { Name = "Password" }).ClickAsync();
            await Expect(_page.GetByPlaceholder("Please enter your old")).ToBeEmptyAsync();
            await _page.GetByPlaceholder("Please enter your old").ClickAsync();
            await _page.GetByPlaceholder("Please enter your old").FillAsync("Password1!");
            await Expect(_page.GetByPlaceholder("Please enter your old")).ToHaveValueAsync("Password1!");
            await Expect(_page.GetByPlaceholder("Please enter your new")).ToBeEmptyAsync();
            await _page.GetByPlaceholder("Please enter your new").ClickAsync();
            await _page.GetByPlaceholder("Please enter your new").FillAsync("NewPassword1!");
            await Expect(_page.GetByPlaceholder("Please enter your new")).ToHaveValueAsync("NewPassword1!");
            await Expect(_page.GetByPlaceholder("Please confirm your new")).ToBeEmptyAsync();
            await _page.GetByPlaceholder("Please confirm your new").ClickAsync();
            await _page.GetByPlaceholder("Please confirm your new").FillAsync("NewPassword1!");
            await Expect(_page.GetByPlaceholder("Please confirm your new")).ToHaveValueAsync("NewPassword1!");
            await _page.GetByRole(AriaRole.Button, new() { Name = "Update password" }).ClickAsync();


        }

        [TearDown]
        public async Task TearDown()
        {
            Console.WriteLine("Tearing down");
            Dispose();
        }

        private async Task SetUpRegisterAndLogin()
        {
            _page = await _context!.NewPageAsync();
            await _page.GotoAsync(_serverAddress);
            //first register user, because a new in memory database is created for each test.
            await _page.GetByRole(AriaRole.Button, new() { Name = "Register" }).ClickAsync();
            await _page.WaitForURLAsync(new Regex("/Identity/Account/Register$"));
            await _page.GetByPlaceholder("Username").ClickAsync();
            await _page.GetByPlaceholder("Username").FillAsync("TestUser");
            await _page.GetByPlaceholder("name@example.com").ClickAsync();
            await _page.GetByPlaceholder("name@example.com").FillAsync("TestUser@Test.Test");
            await _page.GetByLabel("Password", new() { Exact = true }).ClickAsync();
            await _page.GetByLabel("Password", new() { Exact = true }).FillAsync("Password1!");
            await _page.GetByLabel("Confirm Password").ClickAsync();
            await _page.GetByLabel("Confirm Password").FillAsync("Password1!");
            await _page.Locator("#registerSubmit").ClickAsync();
            await _page.WaitForURLAsync(_serverAddress);
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
