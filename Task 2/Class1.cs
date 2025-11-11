using System;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace TestTheTest
{
    // Simple, self-contained NUnit Selenium test.
    public class LoginAutomation
    {
        private IWebDriver _driver = null!;
        private WebDriverWait? _wait;

        [SetUp]
        public void SetUp()
        {
            // Assumes chromedriver is available on PATH or via Selenium Manager.
            var options = new ChromeOptions();
            options.AddArgument("--start-maximized");
            _driver = new ChromeDriver(options);
            _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(8));
        }

        [Test]
        public void ValidUser_IsRedirectedToLoginSuccess()
        {
            // Arrange
            var baseUrl = "https://qa.sorted.com/newtrack";
            var usernameValue = "john_smith@sorted.com";
            var passwordValue = "Pa55w0rd!";
            var expectedUrl = "https://qa.sorted.com/newtrack/loginSuccess";

            // Act
            _driver.Navigate().GoToUrl(baseUrl);

            // Wait for form (avoid Thread.Sleep)
            _wait!.Until(d => d.FindElement(By.CssSelector("form#login")).Displayed);

            var loginForm = _driver.FindElement(By.CssSelector("form#login"));

            // Find elements relative to the form to reduce brittleness
            var usernameInput = loginForm.FindElement(By.CssSelector("input#username, input[name='username']"));
            var passwordInput = loginForm.FindElement(By.CssSelector("input#password, input[name='password']"));
            var submitButton = loginForm.FindElement(By.CssSelector("button#submit, input[type='submit'], button[type='submit']"));

            usernameInput.Clear();
            usernameInput.SendKeys(usernameValue);

            passwordInput.Clear();
            passwordInput.SendKeys(passwordValue);

            submitButton.Click();

            // Wait for post-login marker or URL change
            _wait.Until(d =>
            {
                // prefer explicit element check if there is a logout element on success
                try
                {
                    var loggedInMarker = d.FindElement(By.CssSelector("#logout"));
                    if (loggedInMarker.Displayed) return true;
                }
                catch (NoSuchElementException) { }

                return d.Url.StartsWith(expectedUrl, StringComparison.OrdinalIgnoreCase);
            });

            // Assert
            var actualUrl = _driver.Url;
            Assert.AreEqual(expectedUrl, actualUrl, "User should be redirected to the login success page after valid credentials.");
        }

        [TearDown]
        public void TearDown()
        {
            try
            {
                _driver?.Quit();
                _driver?.Dispose();
            }
            catch
            {
                // swallow cleanup errors
            }
        }
    }
}
