using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using TechTalk.SpecFlow;

namespace Task_1_Myself_code
{
    [Binding]
    public class TestsSetup
    {
        public static class Config
        {
            public static string BaseUrl { get; } = "https://site.com";
            public static string Username { get; } = "john_smith@sorted.com";
            public static string Password { get; } = "Pa55w0rd!";
        }

        [BeforeScenario]
        public void BeforeScenario(ScenarioContext scenarioContext)
        {
            var options = new ChromeOptions();

            options.AddArgument("--headless=new");
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-gpu");

            var driver = new ChromeDriver(options);
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);

            scenarioContext["Driver"] = driver;
        }

        [AfterScenario]
        public void AfterScenario(ScenarioContext scenarioContext)
        {
            IWebDriver driver = (IWebDriver)scenarioContext["Driver"];
            driver.Quit();
            driver.Dispose();
        }
    }
}