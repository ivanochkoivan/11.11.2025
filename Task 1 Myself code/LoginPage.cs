using OpenQA.Selenium;

namespace Task_1_Myself_code
{
    public class LoginPage
    {
        public IWebDriver driver;
        public LoginPage(IWebDriver _driver) {
            driver = _driver;
        }

        private By usernameFieldSelector = By.XPath("//input[@id=\"username\"]");
        private By passwordFieldSelector = By.XPath("//input[@id=\"password\"]");
        private By loginButtonSelector = By.XPath("//input[@id=\"Login\"]");
        private By logoutButtonSelector = By.XPath("//input[@id=\"Logout\"]");

        public void fillLoginForm(string username, string password) {
            var usernameField = driver.FindElement(usernameFieldSelector);
            var passwordField = driver.FindElement(passwordFieldSelector);
            var loginButton = driver.FindElement(loginButtonSelector);

            usernameField.SendKeys(username);
            passwordField.SendKeys(password);
            loginButton.Click();
        }

        public bool isLoginSuccessful()
        {
            try
            {
                driver.FindElement(logoutButtonSelector);
                return true;
            }
            catch { 
                return false;
            }
        }

        public bool isLoginDisplayed()
        {
            try
            {
                driver.FindElement(loginButtonSelector);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
