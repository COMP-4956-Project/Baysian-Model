using System.Dynamic;
using Xunit;

public class ModelTest
{

    [Fact]
    public void CompareProbabilitySuccessTest()
    {
        // Arrange
        Dictionary<string, double> data = new Dictionary<string, double>
        {
            { "firstValue", 1.2 },
            { "secondValue", 3.2 },
            { "thirdValue", 0.23 },
        };
        string expected = "thirdValue";

        // Act
        string actual = Program.CompareProbability(data);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void CompareProbability_ThrowsArgumentExceptionForEmptyOrNullDictionary()
    {
        // Arrange
        Dictionary<string, double> emptyDictionary = null;

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            Program.CompareProbability(emptyDictionary));
    }

    [Fact]
    public void CompareProbability_ThrowsArgumentExceptionForEmptyDictionary()
    {
        // Arrange
        Dictionary<string, double> emptyDictionary = new Dictionary<string, double>();

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            Program.CompareProbability(emptyDictionary));
    }

    [Fact]
    public void CalculateNormalizedConstantTest()
    {
        int decimalPlaces = 2;

        // Arrange
        List<double> posterior = new List<double>
        {
            12.324432,
            3223.1231,
            445.212
        };
        double expected = 3680.661532;
        double roundedExpected = Math.Round(expected, decimalPlaces, MidpointRounding.AwayFromZero);

        // Act
        double actual = Program.CalculateNormalizedConstant(posterior);
        double roundedActual = Math.Round(actual, decimalPlaces, MidpointRounding.AwayFromZero);

        // Assert
        Assert.Equal(roundedExpected, roundedActual);
    }

    [Fact]
    public void CalculateBayesianTheoromSuccessTest()
    {
        // Arrange
        double successRate = 0.3;
        double numberOfChallenges = 7;
        double normalizedConstant = 0.1233;
        double expected = 17.031630170316301;

        // Act
        double actual = Program.CalculateBayesianTheorom(successRate, numberOfChallenges, normalizedConstant);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void CalculateSuccessRateTest()
    {
        // Arrange
        dynamic data = Program.ProcessData("../../../test-data.json");
        string userLevel = "easy";
        dynamic dataWithLevel = data[userLevel];
        List<double> expected = new List<double>
        {
            0.51000000000000001, 0.30769230769230771, 0.037617554858934171
        };

        // Act
        List<double> actual = new List<double>();
        foreach (var category in dataWithLevel)
        {
            double successRate = Program.CalculateSuccessRate(category);
            actual.Add(successRate);
        }

        // Assert
        Assert.Equal(expected, actual);
    }
}