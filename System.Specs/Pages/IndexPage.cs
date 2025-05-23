using OpenQA.Selenium;

namespace System.Specs.Pages;

public class IndexPage
{
    private readonly IWebDriver _webDriver;

    private static readonly By TextAreaXPath = By.XPath("//textarea[@name='text']");
    private static readonly By SubmitButtonXPath = By.XPath("//input[@type='submit']");

    public IndexPage(IWebDriver webDriver)
    {
        _webDriver = webDriver;
    }

    public IWebElement GetTextArea()
    {
        return _webDriver.FindElement(TextAreaXPath);
    }
    
    public IWebElement GetSubmitButton()
    {
        return _webDriver.FindElement(SubmitButtonXPath);
    }

    public void SetTextToArea(string text)
    {
        GetTextArea().SendKeys(text);
    }
    
    public void SubmitText()
    {
        GetSubmitButton().Click();
    }
}