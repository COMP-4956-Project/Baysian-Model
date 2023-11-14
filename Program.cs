using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using MongoDB.Driver;
using MongoDB.Bson;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .Build();

host.Run();

// public class Program
// {
//     static string CONNECTION_STRING = "mongodb+srv://admin:Wk2hn25gmfv1JpFh@comp4956.9hedzlx.mongodb.net/";
//     static void Main(string[] args)
//     {
//         Console.WriteLine(testDB());
//         Console.WriteLine(Run());
//     }

//     static string testDB(){

//         MongoClient dbClient = new MongoClient(CONNECTION_STRING);
//         var dbList = dbClient.ListDatabases().ToList();

//         var database = dbClient.GetDatabase("CodeCraft");
//         var challenges = database.GetCollection<BsonDocument>("challenges");

//         var filter = Builders<BsonDocument>.Filter.Eq("difficulty", "Easy");

//         var result = challenges.Find(filter).ToList();

//         Console.WriteLine(result.Count);

//         foreach(var doc in result){
//             Console.WriteLine(doc);
//         }
//         return dbList[0].ToString();
//     }

//     public static string Run()
//     {
//         var data = ProcessData("test-data.json");
//         string userLevel = "easy"; // TODO: Replace this with the user level from DB
//         return GenerateChellenge(data[userLevel]);
//     }

//     static dynamic ProcessData(string fileName)
//     {
//         string json = File.ReadAllText(fileName);
//         dynamic data = JsonConvert.DeserializeObject(json)!;
//         return data!;
//     }

//     static string GenerateChellenge(dynamic data)
//     {
//         List<double> posteriors = new List<double>();
//         double totalNumberOfChallenges = 0;

//         foreach (var category in data)
//         {
//             double numberOfChallenges = category.Value["numberOfChallenges"];
//             totalNumberOfChallenges += numberOfChallenges;
//         }

//         foreach (var category in data)
//         {
//             double numberOfChallenges = category.Value["numberOfChallenges"];
//             double successRate = CalculateSuccessRate(category);

//             double posterior = successRate * (numberOfChallenges / totalNumberOfChallenges);
//             posteriors.Add(posterior);
//         }
//         double normalizedConstant = CalculateNormalizedConstant(posteriors);

//         Dictionary<string, double> results = new Dictionary<string, double>();
//         foreach (var category in data)
//         {
//             double numberOfChallenges = category.Value["numberOfChallenges"];
//             double successRate = CalculateSuccessRate(category);

//             double result = CalculateBayesianTheorom(successRate, numberOfChallenges, normalizedConstant);
//             results.Add(category.Name, result);
//         }

//         string categoryWithLowestSuccessRate = CompareProbability(results);
//         string challenge = PickChallenge(data, categoryWithLowestSuccessRate);

//         return challenge;
//     }
//     static string CompareProbability(Dictionary<string, double> results)
//     {
//         if (results == null || results.Count == 0)
//         {
//             throw new ArgumentException("The dictionary is empty or null.");
//         }
//         var minPair = results.Aggregate((x, y) => x.Value < y.Value ? x : y);
//         return minPair.Key;
//     }

//     static double CalculateNormalizedConstant(List<double> posterior)
//     {
//         double result = 0;
//         foreach (var value in posterior)
//         {
//             result += value;
//         }
//         return result;
//     }

//     static double CalculateBayesianTheorom(double successRate, double numberOfChallenges, double normalizedConstant)
//     {
//         return successRate * numberOfChallenges / normalizedConstant;
//     }

//     static double CalculateSuccessRate(dynamic category)
//     {
//         double attempted = category.Value["attempted"];
//         double passed = category.Value["passed"];
//         return passed / attempted;
//     }

//     static string PickChallenge(dynamic data, string category)
//     {
//         var challenges = data[category]["challenges"];
//         Random random = new Random();
//         int randomIndex = random.Next(0, challenges.Count);
//         return challenges[randomIndex];
//     }
// }
