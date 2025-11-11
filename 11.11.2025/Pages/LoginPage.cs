using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;

namespace Tracking.Tests.Pages
{
    // Simple Page Object for the login screen.
    public class LoginPage
    {
        private readonly IWebDriver _driver;
        private readonly WebDriverWait _wait;

        public LoginPage(IWebDriver driver)
        {
            _driver = driver ?? throw new ArgumentNullException(nameof(driver));
            _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(8));
        }

        // Locators chosen per task.
        private By LoginForm => By.CssSelector("#login");
        private By UsernameInput => By.CssSelector("#username");
        private By PasswordInput => By.CssSelector("#password");
        private By SubmitButton => By.CssSelector("#submit");
        private By LoggedInMarker => By.CssSelector("#logout"); // element present when logged in

        // Wait for login form to appear
        public void WaitForLoginScreen()
        {
            _wait.Until(d => d.FindElement(LoginForm).Displayed);
        }

        public bool IsLoginScreenDisplayed()
        {
            try
            {
                return _driver.FindElement(LoginForm).Displayed;
            }
            catch (NoSuchElementException)
            {
                return false;
            }
        }

        public void EnterCredentials(string username, string password)
        {
            var u = _wait.Until(d => d.FindElement(UsernameInput));
            u.Clear();
            u.SendKeys(username);

            var p = _driver.FindElement(PasswordInput);
            p.Clear();
            p.SendKeys(password);
        }

        public void Submit()
        {
            var btn = _driver.FindElement(SubmitButton);
            btn.Click();
        }

        public bool IsLoggedIn()
        {
            try
            {
                return _wait.Until(d => d.FindElement(LoggedInMarker).Displayed);
            }
            catch
            {
                return false;
            }
        }
    }
}