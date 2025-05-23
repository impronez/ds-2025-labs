using System.Text.Json;

namespace RankCalculator.Tests;

public class RankCalculatorTests
{
    private const string TestFileName = "rank_calculator_test_data.json";

    private class TestData
    {
        public string? Input { get; set; }
        public double Expected { get; set; }
    }
    
    public static IEnumerable<object?[]> JsonData
    {
        get
        {
            var jsonContent = File.ReadAllText(TestFileName);
            var data = JsonSerializer.Deserialize<List<TestData>>(jsonContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            })!;

            foreach (var item in data)
            {
                yield return [ item.Input, item.Expected ];
            }
        }
    }

    [Theory]
    [MemberData(nameof(JsonData))]
    public void CalculateRank_ReturnsExpectedValue(string? input, double expected)
    {
        // Arrange
        // Act
        double actual = RankCalculatorService.CalculateRank(input);

        // Assert
        Assert.Equal(expected, actual, precision: 3);
    }
}