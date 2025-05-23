using System.Globalization;
using OpenQA.Selenium;
using SeleniumExtras.WaitHelpers;
using OpenQA.Selenium.Support.UI;

namespace System.Specs.Pages;

public class SummaryPage
{
    private const string ZeroRankResult = "Оценка содержания: 0";
    private const int MaxWaitSeconds = 5;
    
    private readonly IWebDriver _webDriver;

    private static readonly By RankTextXPath = By.XPath("//span[@id='rank']"); 
    private static readonly By SimilarityTextXPath = By.XPath("//span[@id='similarity']");

    public SummaryPage(IWebDriver webDriver)
    {
        _webDriver = webDriver;
    }

    public double GetRankText()
    {
        IWebElement element = GetRankWebElement();

        return double.Parse(element.Text, CultureInfo.InvariantCulture);
    }

    public double GetSimilarityText()
    {
        IWebElement element = GetSimilarityWebElement();
        
        return double.Parse(element.Text, CultureInfo.InvariantCulture);
    }

    private IWebElement GetRankWebElement()
    { 
        var wait = new WebDriverWait(_webDriver, TimeSpan.FromSeconds(MaxWaitSeconds));
        return wait.Until(ExpectedConditions.ElementIsVisible(RankTextXPath));
    }
    
    private IWebElement GetSimilarityWebElement()
    {
        var wait = new WebDriverWait(_webDriver, TimeSpan.FromSeconds(MaxWaitSeconds));
        return wait.Until(ExpectedConditions.ElementIsVisible(SimilarityTextXPath));
    }
}