using System.Globalization;
using System.Specs.Pages;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;
using Reqnroll;

namespace System.Specs.Steps;

[Binding]
public class SystemStepDefinitions : IDisposable
{
    private const string Url = "http://nginx:8080/";
    
    private readonly IWebDriver _driver;
    private readonly IndexPage _indexPage;
    private readonly SummaryPage _summaryPage;

    public SystemStepDefinitions()
    {
        _driver = GetRemoteChromeDriver();

        _indexPage = new(_driver);
        _summaryPage = new(_driver);
    }
    
    [Given(@"Пользовать открывает страницу")]
    public void GivenПользоватьОткрываетСтраницу()
    {
        _driver.Navigate().GoToUrl(Url);   
    }

    [When(@"Пользователь вводит и отправляет текст ""(.*)""")]
    public void WhenПользовательВводитИОтправляетТекст(string p0)
    {
        _indexPage.SetTextToArea(p0);
        _indexPage.SubmitText();
    }

    [Then(@"Приложение отображает ранг, равный ""(.*)""")]
    public void ThenПриложениеОтображаетРангРавный(string p0)
    {
        double expectedRank = double.Parse(p0, CultureInfo.InvariantCulture);

        double actualRank = _summaryPage.GetRankText();
        
        Assert.Equal(expectedRank, actualRank);
    }

    public void Dispose()
    {
        _driver.Dispose();
    }

    private static IWebDriver GetRemoteChromeDriver()
    {
        var seleniumHubUrl = new Uri("http://selenium-hub:4444/wd/hub");

        var options = new ChromeOptions();
        options.AddArgument("--headless");
        options.AddArgument("--no-sandbox");
        options.AddArgument("--disable-dev-shm-usage");

        return new RemoteWebDriver(seleniumHubUrl, options.ToCapabilities());
    }
}