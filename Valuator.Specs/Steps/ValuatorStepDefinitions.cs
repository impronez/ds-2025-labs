using System.Net;
using Moq;
using Reqnroll;
using StackExchange.Redis;
using Valuator.Specs.Fixture;

namespace Valuator.Specs.Steps;

[Binding]
public class ValuatorStepDefinitions
{
    private readonly ITestServerFixture _provider;
    private readonly IDatabase _database;
    private HttpResponseMessage _response;
    private string _submittedText;
    private string _generatedId;

    public ValuatorStepDefinitions()
    {
        _provider = new TestServerFixture();
        
        IConnectionMultiplexer redis = ConnectionMultiplexer.Connect(_provider.Container.GetConnectionString());
        _database = redis.GetDatabase();
    }

    [When(@"пользователь отправляет текст ""(.*)""")]
    public async Task WhenПользовательОтправляетТекст(string text)
    {
        _submittedText = text;

        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "Text", text }
        });
        
        _response = await _provider.HttpClient.PostAsync("/", content);

        Assert.True(false, $"Status code: {_response.StatusCode}");

        if (_response.StatusCode == HttpStatusCode.Redirect)
        {
            var redirectUri = _response.Headers.Location?.ToString();
            var query = System.Web.HttpUtility.ParseQueryString(redirectUri?.Split('?').Last());
            _generatedId = query["id"];
        }
    }

    [Then(@"система сохраняет similarity-key")]
    public async Task ThenСистемаСохраняетSimilarityKey()
    {
        var key = $"SIMILARITY-{_generatedId}";
        var value = await _database.StringGetAsync(key);
        Assert.False(value.IsNullOrEmpty, $"Similarity key {key} не найден в Redis");
    }

    [Then(@"система сохраняет text-key")]
    public async Task ThenСистемаСохраняетTextKey()
    {
        var key = $"TEXT-{_generatedId}";
        var value = await _database.StringGetAsync(key);
        Assert.Equal(_submittedText, value);
    }

    [Then(@"система отправляет сообщение в очередь RankCalculator")]
    public void ThenСистемаОтправляетСообщениеВОчередьRankCalculator()
    {
        _provider.MessageBrokerServiceMock.Verify(mb =>
            mb.SendMessageAsync(It.IsAny<string>(), It.Is<string>(rk => rk == "rank-calculator-queue"), _generatedId),
            Times.Once);
    }

    [Then(@"система отправляет лог в очередь логирования")]
    public void ThenСистемаОтправляетЛогВОчередьЛогирования()
    {
        _provider.MessageBrokerServiceMock.Verify(mb =>
            mb.SendMessageAsync(
                It.Is<string>(exchange => exchange == "logger-exchange"),
                It.Is<string>(rk => rk == "similarity-routing-key"),
                It.Is<string>(msg => msg.Contains(_generatedId))),
            Times.Once);
    }

    [Then(@"пользователь перенаправляется на страницу summary с параметром id")]
    public void ThenПользовательПеренаправляетсяНаСтраницуSummaryСПараметромId()
    {
        Assert.True(_response.StatusCode == HttpStatusCode.Redirect);
        Assert.Contains("/summary?id=", _response.Headers.Location?.ToString());
        Assert.False(string.IsNullOrEmpty(_generatedId), "id не был извлечён из редиректа");
    }
}
