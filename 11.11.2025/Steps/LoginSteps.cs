using System;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using OpenQA.Selenium;
using TechTalk.SpecFlow;
using Tracking.Tests.Pages;
using NUnit.Framework;
using System.Linq;

namespace Tracking.Tests.Steps
{
    [Binding]
    public class LoginSteps
    {
        private readonly ScenarioContext _scenarioContext;
        private readonly IConfiguration _configuration;
        private IWebDriver Driver => (IWebDriver)_scenarioContext["Driver"];
        private string BaseUrl => _configuration["BaseUrl"] ?? "https://site.com";

        public LoginSteps(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
            _configuration = (IConfiguration)_scenarioContext["Configuration"]!;
        }

        [Given(@"the application base URL is ""(.*)""")]
        public void GivenTheApplicationBaseURLIs(string url)
        {
            // override base url in config for the scenario
            (_configuration as IConfigurationRoot)?.Providers.FirstOrDefault();
            // store explicitly for test usage
            _scenarioContext["BaseUrl"] = url;
        }

        [Given(@"I am not logged in")]
        public void GivenIAmNotLoggedIn()
        {
            // Ensure no session cookie by clearing browser state.
            Driver.Manage().Cookies.DeleteAllCookies();
        }

        [When(@"I navigate to ""(.*)""")]
        public void WhenINavigateTo(string relativePath)
        {
            var baseUrl = _scenarioContext.ContainsKey("BaseUrl") ? (string)_scenarioContext["BaseUrl"]! : BaseUrl;
            var full = new Uri(new Uri(baseUrl), relativePath).ToString();
            Driver.Navigate().GoToUrl(full);
        }

        [Then(@"I should be presented with a login screen")]
        public void ThenIShouldBePresentedWithALoginScreen()
        {
            var login = new LoginPage(Driver);
            // This will wait until login form appears (or time out)
            try
            {
                login.WaitForLoginScreen();
                login.IsLoginScreenDisplayed().Should().BeTrue("anonymous users must see the login screen");
            }
            catch (WebDriverTimeoutException)
            {
                // If no page exists (site.com is not real) the check still compiles.
                Assert.Fail("Login screen was not displayed within timeout.");
            }
        }

        [Given(@"valid user credentials are registered")]
        public void GivenValidUserCredentialsAreRegistered()
        {
            // In a real test it would ensure the user exists (API/DB).
            // For this task it read expected credentials from appsettings.json.
            // This keeps the test independent of an external identity provider.
            var u = _configuration.GetSection("ValidUser")["Username"];
            var p = _configuration.GetSection("ValidUser")["Password"];
            _scenarioContext["ValidUser"] = (u, p);
        }

        [Given(@"I'm on the login screen")]
        public void GivenImOnTheLoginScreen()
        {
            // Navigate to login page explicitly
            var baseUrl = _scenarioContext.ContainsKey("BaseUrl") ? (string)_scenarioContext["BaseUrl"]! : BaseUrl;
            var loginUrl = new Uri(new Uri(baseUrl), "/login").ToString();
            Driver.Navigate().GoToUrl(loginUrl);
            var login = new LoginPage(Driver);
            login.WaitForLoginScreen();
        }

        [When(@"I enter a valid username and password and submit")]
        public void WhenIEnterAValidUsernameAndPasswordAndSubmit()
        {
            var (username, password) = ((string, string))_scenarioContext["ValidUser"];
            var login = new LoginPage(Driver);
            login.EnterCredentials(username, password);
            login.Submit();
        }

        [Then(@"I am logged in successfully")]
        public void ThenIAmLoggedInSuccessfully()
        {
            var login = new LoginPage(Driver);
            // In a real app, the login result would be visible. We assert on a logged-in marker.
            try
            {
                var loggedIn = login.IsLoggedIn();
                loggedIn.Should().BeTrue("after submitting valid credentials the user must be logged in");
            }
            catch (WebDriverTimeoutException)
            {
                Assert.Fail("User was not confirmed as logged in within timeout.");
            }
        }
    }
}