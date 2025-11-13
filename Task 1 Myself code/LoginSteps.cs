using NUnit.Framework;
using OpenQA.Selenium;
using TechTalk.SpecFlow;

namespace Task_1_Myself_code.Steps
{
    [Binding]
    public class LoginSteps
    {
        private ScenarioContext _scenarioContext;
        private IWebDriver Driver => (IWebDriver)_scenarioContext["Driver"]!;

        public LoginSteps(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
        }

        [Given("I am not logged in")]
        public void GivenIAmNotLoggedIn()
        {
            Driver.Manage().Cookies.DeleteAllCookies();
        }

        [When(@"I navigate to ""(.*)""")]
        public void WhenINavigateTo(string relative)
        {
            var url = TestsSetup.Config.BaseUrl.TrimEnd('/') + "/" + relative.TrimStart('/');
            Driver.Navigate().GoToUrl(url);
        }

        [Then("I should see the login screen")]
        public void ThenIShouldSeeTheLoginScreen()
        {
            var page = new LoginPage(Driver);
            Assert.IsTrue(page.isLoginDisplayed(), "Expected login controls to be visible for anonymous user.");
        }

        [Given("I'm on the login screen")]
        public void GivenImOnTheLoginScreen()
        {           
            var loginUrl = TestsSetup.Config.BaseUrl.TrimEnd('/') + "/login";
            Driver.Navigate().GoToUrl(loginUrl);
            var page = new LoginPage(Driver);
            Assert.IsTrue(page.isLoginDisplayed(), "Login screen did not show up.");
        }

        [When("I enter a valid username and password and submit")]
        public void WhenIEnterAValidUsernameAndPasswordAndSubmit()
        {
            var page = new LoginPage(Driver);
            page.fillLoginForm(TestsSetup.Config.Username, TestsSetup.Config.Password);
        }

        [Then("I am logged in successfully")]
        public void ThenIAmLoggedInSuccessfully()
        {
            var page = new LoginPage(Driver);
            Assert.IsTrue(page.isLoginSuccessful(), "Expected user to be logged in after submitting valid credentials.");
        }
    }
}