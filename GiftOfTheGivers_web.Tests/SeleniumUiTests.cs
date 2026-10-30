using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using Xunit;

namespace GiftOfTheGivers_web.Tests;

public class SeleniumUiTests
{
    private static readonly TimeSpan WaitTimeout = TimeSpan.FromSeconds(10);

    [SeleniumFact]
    public void EmployeeLoginPageShowsSignInFields()
    {
        var baseAddress = GetBaseAddress();
        using var driver = CreateDriver();

        driver.Navigate().GoToUrl(new Uri(baseAddress, "/Employee/Login"));

        Assert.Equal("Employee / Admin Login", WaitForElement(driver, By.TagName("h1")).Text);
        Assert.Equal("email", driver.FindElement(By.Name("Email")).GetDomAttribute("type"));
        Assert.Equal("password", driver.FindElement(By.Name("Password")).GetDomAttribute("type"));
        Assert.Equal("Sign In", driver.FindElement(By.CssSelector("button[type='submit']")).Text);
    }

    [SeleniumFact]
    public void GuestCanReviewAnAnonymousDonation()
    {
        var baseAddress = GetBaseAddress();
        using var driver = CreateDriver();

        driver.Navigate().GoToUrl(new Uri(baseAddress, "/Donation/Index"));
        Assert.Contains("You are donating as a guest", driver.FindElement(By.ClassName("alert-info")).Text);

        driver.FindElement(By.CssSelector(".amount-btn[data-amount='250']")).Click();
        var eftOption = driver.FindElement(By.CssSelector("input[name='PaymentMethod'][value='EFT']"));
        ScrollIntoView(driver, eftOption);
        eftOption.Click();
        var continueButton = driver.FindElement(By.CssSelector(".donation-action button[type='submit']"));
        ScrollIntoView(driver, continueButton);
        continueButton.Click();

        WaitForUrl(driver, "/Donation/Details");

        Assert.Equal("Donation Details", WaitForElement(driver, By.TagName("h1")).Text);
        Assert.Equal("250", driver.FindElement(By.Name("Amount")).GetDomAttribute("value"));
        Assert.True(bool.Parse(driver.FindElement(By.Name("IsAnonymous")).GetDomAttribute("value")!));
        Assert.Contains("Anonymous", driver.FindElement(By.ClassName("donation-summary")).Text);
        Assert.Contains("EFT", driver.FindElement(By.ClassName("donation-summary")).Text);
    }

    private static Uri GetBaseAddress()
    {
        var configuredAddress = Environment.GetEnvironmentVariable("SELENIUM_BASE_URL");
        if (string.IsNullOrWhiteSpace(configuredAddress))
        {
            throw new InvalidOperationException(
                "SELENIUM_BASE_URL must be set to run Selenium UI tests.");
        }

        if (!Uri.TryCreate(configuredAddress, UriKind.Absolute, out var address) ||
            (address.Scheme != Uri.UriSchemeHttp && address.Scheme != Uri.UriSchemeHttps))
        {
            throw new InvalidOperationException(
                "SELENIUM_BASE_URL must be an absolute HTTP or HTTPS URL.");
        }

        return new Uri(address.GetLeftPart(UriPartial.Authority) + "/");
    }

    private static IWebDriver CreateDriver()
    {
        var options = new ChromeOptions();
        options.AddArgument("--headless=new");
        options.AddArgument("--window-size=1440,1600");
        options.AcceptInsecureCertificates = true;

        return new ChromeDriver(options);
    }

    private static IWebElement WaitForElement(IWebDriver driver, By selector)
    {
        return new WebDriverWait(driver, WaitTimeout)
            .Until(currentDriver => currentDriver.FindElement(selector));
    }

    private static void ScrollIntoView(IWebDriver driver, IWebElement element)
    {
        ((IJavaScriptExecutor)driver).ExecuteScript(
            "document.documentElement.style.scrollBehavior = 'auto';" +
            "arguments[0].scrollIntoView({ block: 'center', behavior: 'instant' });",
            element);
    }

    private static void WaitForUrl(IWebDriver driver, string path)
    {
        new WebDriverWait(driver, WaitTimeout)
            .Until(currentDriver => new Uri(currentDriver.Url).AbsolutePath.Equals(
                path,
                StringComparison.OrdinalIgnoreCase));
    }
}

public sealed class SeleniumFactAttribute : FactAttribute
{
    public SeleniumFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("SELENIUM_BASE_URL")))
        {
            Skip = "Set SELENIUM_BASE_URL to the running Gift of the Givers website to run Selenium UI tests.";
        }
    }
}
