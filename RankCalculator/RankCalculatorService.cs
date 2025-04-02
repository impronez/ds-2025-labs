using System.Globalization;
using Services;

namespace RankCalculator;

public class RankCalculatorService
{
    private const string RankPrefix = "RANK-";
    private const string TextPrefix = "TEXT-";
    
    private IStorageService _storageService;

    public RankCalculatorService(IStorageService storageService)
    {
        _storageService = storageService;
    }
    
    public void Process(string id)
    {
        var text = _storageService.GetValue(TextPrefix + id);
        
        var rank = CalculateRank(text);
        var key = RankPrefix + id;

        SaveRank(key, rank);
    }

    private void SaveRank(string key, double value)
    {
        _storageService.Save(key, value.ToString(CultureInfo.InvariantCulture));
    }
    
    private static double CalculateRank(string? text)
    {
        if (string.IsNullOrEmpty(text)) return 0;
        
        return text.Count(ch => !char.IsLetter(ch)) / (double)text.Length;
    }
}