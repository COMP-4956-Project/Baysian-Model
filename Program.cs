using System;
using System.Collections.Generic;
using Newtonsoft.Json;


public class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine(Run());
    }

    public static string Run()
    {
        var data = ProcessData("test-data.json");
        string userLevel = "easy"; // TODO: Replace this with the user level from DB
        return GenerateChellenge(data[userLevel]);
    }

    static dynamic ProcessData(string fileName)
    {
        string json = File.ReadAllText(fileName);
        dynamic data = JsonConvert.DeserializeObject(json)!;
        return data!;
    }

    static string GenerateChellenge(dynamic data)
    {
        List<double> posteriors = new List<double>();
        double totalNumberOfChallenges = 0;

        foreach (var category in data)
        {
            double numberOfChallenges = category.Value["numberOfChallenges"];
            totalNumberOfChallenges += numberOfChallenges;
        }

        foreach (var category in data)
        {
            double numberOfChallenges = category.Value["numberOfChallenges"];
            double successRate = CalculateSuccessRate(category);

            double posterior = successRate * (numberOfChallenges / totalNumberOfChallenges);
            posteriors.Add(posterior);
        }
        double normalizedConstant = CalculateNormalizedConstant(posteriors);

        Dictionary<string, double> results = new Dictionary<string, double>();
        foreach (var category in data)
        {
            double numberOfChallenges = category.Value["numberOfChallenges"];
            double successRate = CalculateSuccessRate(category);

            double result = CalculateBayesianTheorom(successRate, numberOfChallenges, normalizedConstant);
            results.Add(category.Name, result);
        }

        string categoryWithLowestSuccessRate = CompareProbability(results);
        string challenge = PickChallenge(data, categoryWithLowestSuccessRate);

        return challenge;
    }
    static string CompareProbability(Dictionary<string, double> results)
    {
        if (results == null || results.Count == 0)
        {
            throw new ArgumentException("The dictionary is empty or null.");
        }
        var minPair = results.Aggregate((x, y) => x.Value < y.Value ? x : y);
        return minPair.Key;
    }

    static double CalculateNormalizedConstant(List<double> posterior)
    {
        double result = 0;
        foreach (var value in posterior)
        {
            result += value;
        }
        return result;
    }

    static double CalculateBayesianTheorom(double successRate, double numberOfChallenges, double normalizedConstant)
    {
        return successRate * numberOfChallenges / normalizedConstant;
    }

    static double CalculateSuccessRate(dynamic category)
    {
        double attempted = category.Value["attempted"];
        double passed = category.Value["passed"];
        return passed / attempted;
    }

    static string PickChallenge(dynamic data, string category)
    {
        var challenges = data[category]["challenges"];
        Random random = new Random();
        int randomIndex = random.Next(0, challenges.Count);
        return challenges[randomIndex];
    }
}
