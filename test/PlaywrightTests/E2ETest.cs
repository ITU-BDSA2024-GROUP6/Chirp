using Microsoft.Playwright;
using Xunit;


namespace test.PlaywrightTests
{
    [TestFixture, NonParallelizable]
    public class E2ETest : PageTest, IClassFixture<CustomWebApplicationFactory>, IDisposable
    {
        private IBrowserContext? _context;
        private IBrowser? _browser;
        private CustomWebApplicationFactory _factory = null!;
        private string _serverAddress = null!;
        private IPlaywright _playwright = null!;

        [SetUp]
        public async Task SetUp()
        {
            _factory = new CustomWebApplicationFactory();
            _serverAddress = _factory.TestServerAddress;

            await InitializeBrowserAndCreateBrowserContextAsync();

            var test = TestContext.CurrentContext.Test;

        }

        [Test]
        public async Task CompleteE2ETest()
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

            await _page.GetByRole(AriaRole.Button, new() { Name = "Logout" }).ClickAsync();
            var expectedLogoutURL = _serverAddress + "Identity/Account/Logout";
            await Expect(_page).ToHaveURLAsync(expectedLogoutURL);

            var loginButton = _page.GetByRole(AriaRole.Button, new() { Name = "Login" });
            await Expect(loginButton).ToBeVisibleAsync();

            await _page.GetByRole(AriaRole.Button, new() { Name = "Login" }).ClickAsync();
            await _page.GetByPlaceholder("name@example.com").ClickAsync();
            await _page.GetByPlaceholder("name@example.com").FillAsync("TestUser");
            await Expect(_page.GetByPlaceholder("name@example.com")).ToHaveValueAsync("TestUser");
            await _page.GetByPlaceholder("Password").ClickAsync();
            await _page.GetByPlaceholder("Password").FillAsync("Password1!");
            await Expect(_page.GetByPlaceholder("Password")).ToHaveValueAsync("Password1!");

            await _page.GetByRole(AriaRole.Button, new() { Name = "Log in" }).ClickAsync();
            await Expect(_page).ToHaveURLAsync(_serverAddress);

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

            await _page.Locator("#cheepInput").ClickAsync();
            await Expect(_page.Locator("#cheepInput")).ToBeFocusedAsync();

            await _page.Locator("#cheepInput").FillAsync("Public Test Cheep");
            await Expect(_page.Locator("#cheepInput")).ToHaveValueAsync("Public Test Cheep");

            await _page.GetByRole(AriaRole.Button, new() { Name = "Share" }).ClickAsync();

            var publicCheep = _page.GetByText("Public Test Cheep");
            await Expect(publicCheep).ToBeVisibleAsync();

            await Expect(_page).ToHaveURLAsync(_serverAddress);

            await _page.GetByRole(AriaRole.Button, new() { Name = "Your Timeline" }).ClickAsync();
            var expectedPrivateURL = _serverAddress + "TestUser";
            await Expect(_page).ToHaveURLAsync(expectedPrivateURL);

            await _page.Locator("#cheepInput").ClickAsync();
            await Expect(_page.Locator("#cheepInput")).ToBeFocusedAsync();

            await _page.Locator("#cheepInput").FillAsync("Private Test Cheep");
            await Expect(_page.Locator("#cheepInput")).ToHaveValueAsync("Private Test Cheep");

            await _page.GetByRole(AriaRole.Button, new() { Name = "Share" }).ClickAsync();

            var privateCheep = _page.GetByText("Private Test Cheep");
            await Expect(privateCheep).ToBeVisibleAsync();

            await Expect(_page).ToHaveURLAsync(expectedPrivateURL);

            await _page.GetByRole(AriaRole.Button, new() { Name = "Public Timeline" }).ClickAsync();
            await Expect(_page).ToHaveURLAsync(_serverAddress);

            var publicFollowButton = _page.Locator("li").Filter(new() { HasText = "Starbuck now is what we hear the worst." }).GetByRole(AriaRole.Button);

            //follow author
            await Expect(publicFollowButton).ToHaveTextAsync("Follow");
            await publicFollowButton.ClickAsync();
            await Expect(publicFollowButton).ToHaveTextAsync("Unfollow");

            //unfollow author
            await publicFollowButton.ClickAsync();
            await Expect(publicFollowButton).ToHaveTextAsync("Follow");

            //Check to see if we are still on the same page
            await Expect(_page).ToHaveURLAsync(_serverAddress);

            await _page.Locator("li").Filter(new() { HasText = "Jacqualine Gilcoine Follow Starbuck now is what we hear the worst. — 2023-08-01" }).GetByRole(AriaRole.Link).ClickAsync();
            var expectedUserURL = _serverAddress + "Jacqualine%20Gilcoine";
            await Expect(_page).ToHaveURLAsync(expectedUserURL);


            var usertimelineFollowButton = _page.Locator("li").Filter(new() { HasText = "Starbuck now is what we hear the worst." }).GetByRole(AriaRole.Button);

            //follow author
            await Expect(usertimelineFollowButton).ToHaveTextAsync("Follow");
            await usertimelineFollowButton.ClickAsync();
            await Expect(usertimelineFollowButton).ToHaveTextAsync("Unfollow");


            //unfollow author
            await usertimelineFollowButton.ClickAsync();
            await Expect(usertimelineFollowButton).ToHaveTextAsync("Follow");

            //Check to see if we are still on the same page
            await Expect(_page).ToHaveURLAsync(expectedUserURL);

            await _page.GetByRole(AriaRole.Button, new() { Name = "Public Timeline" }).ClickAsync();
            await Expect(_page).ToHaveURLAsync(_serverAddress);

            //Follow user
            await _page.Locator("li").Filter(new() { HasText = "Jacqualine Gilcoine Follow Starbuck now is what we hear the worst. — 2023-08-01" }).GetByRole(AriaRole.Button).ClickAsync();

            //Go to Private Timeline
            await _page.GetByRole(AriaRole.Button, new() { Name = "Your Timeline" }).ClickAsync();
            await Expect(_page).ToHaveURLAsync(expectedPrivateURL);

            //Unfollow user
            var privateUnFollowButton = _page.Locator("li").Filter(new() { HasText = "Starbuck now is what we hear the worst." }).GetByRole(AriaRole.Button);
             var privateTargetCheep = _page.GetByText("Starbuck now is what we hear the worst.");
            await Expect(privateUnFollowButton).ToHaveTextAsync("Unfollow");
            await privateUnFollowButton.ClickAsync();

            //Check to see if the cheep no longer gets shown on Private Timeline
            await Expect(privateTargetCheep).Not.ToBeVisibleAsync();

            await _page.GetByRole(AriaRole.Button, new() { Name = "Public Timeline" }).ClickAsync();
            await Expect(_page).ToHaveURLAsync(_serverAddress);

            await _page.Locator("li").Filter(new() { HasText = "Jacqualine Gilcoine Follow Starbuck now is what we hear the worst. — 2023-08-01" }).GetByRole(AriaRole.Button).ClickAsync();

            await _page.GetByRole(AriaRole.Button, new() { Name = "About me" }).ClickAsync();
            var expectedAboutMeURL = _serverAddress + "Identity/Account/Manage";
            await Expect(_page).ToHaveURLAsync(expectedAboutMeURL);

            await _page.GetByRole(AriaRole.Link, new() { Name = "Following" }).ClickAsync();
            var expectedFollowingUrl = _serverAddress + "Identity/Account/Manage/Following";
            await Expect(_page).ToHaveURLAsync(expectedFollowingUrl);

            var followingUserName = _page.Locator("li").Filter(new() { HasText = "Jacqualine Gilcoine" });
            await Expect(followingUserName).ToBeVisibleAsync();

            var followingUserUnFollowButton = _page.Locator("li").Filter(new() { HasText = "Jacqualine Gilcoine" }).GetByRole(AriaRole.Button);
            await Expect(followingUserUnFollowButton).ToHaveTextAsync("Unfollow");
            await followingUserUnFollowButton.ClickAsync();

            var unfollowConfirmationText = _page.GetByText("You are not following any");
            await Expect(unfollowConfirmationText).ToBeVisibleAsync();

            await _page.GetByRole(AriaRole.Link, new() { Name = "Password" }).ClickAsync();
            var expectedChangePasswordUrl = _serverAddress + "Identity/Account/Manage/ChangePassword";
            await Expect(_page).ToHaveURLAsync(expectedChangePasswordUrl);

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

            var confirmationText = _page.GetByText("Your password has been");
            await Expect(confirmationText).ToBeVisibleAsync();

            await _page.GetByRole(AriaRole.Link, new() { Name = "Profile" }).ClickAsync();
            await Expect(_page).ToHaveURLAsync(expectedAboutMeURL);

            await Expect(_page.GetByPlaceholder("Please enter your phone")).ToBeEmptyAsync();
            await _page.GetByPlaceholder("Please enter your phone").ClickAsync();
            await _page.GetByPlaceholder("Please enter your phone").FillAsync("00000000");
            await Expect(_page.GetByPlaceholder("Please enter your phone")).ToHaveValueAsync("00000000");
            await _page.GetByRole(AriaRole.Button, new() { Name = "Save" }).ClickAsync();
            await Expect(_page.GetByPlaceholder("Please enter your phone")).ToHaveValueAsync("00000000");


            await Expect(_page).ToHaveURLAsync(expectedAboutMeURL);

            await _page.GetByRole(AriaRole.Link, new() { Name = "Your Cheeps" }).ClickAsync();
            var expectedPersonalCheepsURL = _serverAddress + "Identity/Account/Manage/PersonalCheeps";
            await Expect(_page).ToHaveURLAsync(expectedPersonalCheepsURL);

            
            await Expect(publicCheep).ToBeVisibleAsync();
            await Expect(privateCheep).ToBeVisibleAsync();

            await _page.GetByRole(AriaRole.Link, new() { Name = "Forget me" }).ClickAsync();
            var expectedForgetMeURL = _serverAddress + "Identity/Account/Manage/Delete";
            await Expect(_page).ToHaveURLAsync(expectedForgetMeURL);

            await _page.GetByRole(AriaRole.Button, new() { Name = "Forget me" }).ClickAsync();
            var expectedDeleteURL = _serverAddress + "Identity/Account/Manage/DeletePersonalData";
            await Expect(_page).ToHaveURLAsync(expectedDeleteURL);

            await Expect(_page.GetByPlaceholder("Please enter your password.")).ToBeEmptyAsync();
            await _page.GetByPlaceholder("Please enter your password.").ClickAsync();
            await _page.GetByPlaceholder("Please enter your password.").FillAsync("NewPassword1!");
            await Expect(_page.GetByPlaceholder("Please enter your password.")).ToHaveValueAsync("NewPassword1!");
            await _page.GetByRole(AriaRole.Button, new() { Name = "Delete data and close my" }).ClickAsync();
            await Expect(_page).ToHaveURLAsync(_serverAddress);

            await Expect(publicCheep).Not.ToBeVisibleAsync();
            await Expect(privateCheep).Not.ToBeVisibleAsync();
        }

        [TearDown]
        public void TearDown()
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