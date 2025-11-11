using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Microsoft.Extensions.Configuration;

namespace Tracking.Tests.Support
{
    // Factory that creates IWebDriver instances. Read headless option from appsettings.json.
    public static class DriverFactory
    {
        public static IWebDriver Create(IConfiguration? configuration = null)
        {
            // Minimal safe defaults; headless by default for CI friendliness.
            var headless = true;
            if (configuration != null)
            {
                var headlessValue = configuration.GetSection("Browser")?["Headless"];
                if (bool.TryParse(headlessValue, out var parsed)) headless = parsed;
            }

            var options = new ChromeOptions();
            if (headless)
            {
                options.AddArgument("--headless=new");
                options.AddArgument("--no-sandbox");
                options.AddArgument("--disable-gpu");
            }
            options.AddArgument("--disable-dev-shm-usage");
            options.AddArgument("--window-size=1920,1080");

            // Return ChromeDriver; package provides the driver binary for build/runtime.
            return new ChromeDriver(options);
        }
    }
}