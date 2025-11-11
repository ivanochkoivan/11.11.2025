using System.IO;
using Microsoft.Extensions.Configuration;
using OpenQA.Selenium;
using TechTalk.SpecFlow;
using Tracking.Tests.Support;
using Microsoft.Extensions.Configuration.Json;

namespace Tracking.Tests.Hooks
{
    [Binding]
    public class TestHooks
    {
        private readonly ScenarioContext _scenarioContext;
        private readonly IConfiguration _configuration;

        public TestHooks(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;

            // Load configuration (appsettings.json) so tests can read base URL and credentials.
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .Build();
            _configuration = config;
            _scenarioContext["Configuration"] = _configuration;
        }

        [BeforeScenario]
        public void BeforeScenario()
        {
            var driver = DriverFactory.Create(_configuration);
            _scenarioContext["Driver"] = driver;
        }

        [AfterScenario]
        public void AfterScenario()
        {
            if (_scenarioContext.TryGetValue("Driver", out object? obj) && obj is IWebDriver driver)
            {
                try
                {
                    driver.Quit();
                    driver.Dispose();
                }
                catch
                {
                    
                }
            }
        }
    }
}