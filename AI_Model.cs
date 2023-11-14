using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using Newtonsoft.Json;
using MongoDB.Bson;
using Microsoft.VisualBasic;
using System.Data.Common;


namespace Baysian_Model
{
    public class AI_Model
    {
        
        //static MongoClient dbClient = new MongoClient(CONNECTION_STRING);

        static MongoClient dbClient = new MongoClient(System.Environment.GetEnvironmentVariable("MONGO_DB_CONNECTION_STRING"));

        public static string userDifficulty(int level){
            switch(level){
                case 1:
                    return "Easy";
                case 2:
                    return "Medium";
                case 3: 
                    return "Hard";
                default:
                    return "Easy";
            }
        }
        public static string Run()
        {
            var data = ProcessData("test-data.json");
            string userLevel = "easy"; // TODO: Replace this with the user level from DB
            return GenerateChallenge(data[userLevel]);
        }

        public static string Run(string difficulty){
            return GenerateChallenge(difficulty);
        }


        static dynamic ProcessData(string fileName)
        {
            string json = File.ReadAllText(fileName);
            dynamic data = JsonConvert.DeserializeObject(json)!;
            return data!;
        }

        public static int getUserLevel(string userID){

            dbClient.GetDatabase("CodeCraft");
            var users = dbClient.GetDatabase("CodeCraft").GetCollection<BsonDocument>("users");

            var filter = Builders<BsonDocument>.Filter.Eq("_id", ObjectId.Parse(userID));

            Console.WriteLine(userID);

            var user = users.Find(filter).ToList();

            Console.WriteLine(user.Count);
            Console.WriteLine(user[0]["Level"]);

            // int level = (int)user[0]["Level"];

            // Console.WriteLine(level + "=========");

            return user[0]["Level"].ToInt32();
        }

        // method to add new fields to all documents in database
        public static void updateFields(){
            var database = dbClient.GetDatabase("CodeCraft");
            var collectionUpdate = database.GetCollection<BsonDocument>("challenges_2");
            
            var filter = Builders<BsonDocument>.Filter.Empty;
            var update = Builders<BsonDocument>.Update.Set("passed", 0);
            
            var result = collectionUpdate.UpdateMany(filter, update);

            Console.WriteLine($"Modified {result.ModifiedCount} documents");
        }

        public static int getNumberOfChallenges(string difficulty){
            var database = dbClient.GetDatabase("CodeCraft");
            var collection = database.GetCollection<BsonDocument>("challenges_2");

            var filter = Builders<BsonDocument>.Filter.Eq("difficulty", difficulty);

            var result = collection.Find(filter).ToList();

            return result.Count;
        }

        // first double = attempted #, second double = total # of challenge attempts, third double = # of challenge problems
        public static double[] getChallengesPerCategory(string category, string difficultyLevel){

            double[] res = new double[3];

            var database = dbClient.GetDatabase("CodeCraft");
            var collection = database.GetCollection<BsonDocument>("challenges_2");

            var filter = Builders<BsonDocument>.Filter.Eq("category", category);
            var filterDifficulty = Builders<BsonDocument>.Filter.Eq("difficulty", difficultyLevel);

            var combinedFilter = filter & filterDifficulty;

            var unfiltered = collection.Find(filter).ToList();
            var result = collection.Find(combinedFilter).ToList();

            //Console.WriteLine("Unfiltered: " + unfiltered.Count);
            Console.WriteLine("Filtered: " + result.Count);

            double attempted = 0;
            double passed = 0;
            foreach(var doc in result){
                attempted += doc["attempted"].ToDouble();
                passed += doc["passed"].ToDouble();
            }

            res[0] = attempted;
            res[1] = passed;
            res[2] = (double) result.Count;
            return res;
            // return result.Count;
        }

        
        //static string GenerateChallenge(dynamic data)
        static string GenerateChallenge(string difficultyLevel)
        {
            List<double> posteriors = new List<double>();
            double totalNumberOfChallenges = 0;

            Console.WriteLine("User level: " + difficultyLevel);
            // foreach (var category in data)
            // {
            //     double numberOfChallenges = category.Value["numberOfChallenges"];
            //     totalNumberOfChallenges += numberOfChallenges;
            // }

            // 100 challenges total in database
            // totalNumberOfChallenges = getNumberOfChallenges(difficultyLevel);
            totalNumberOfChallenges = 100;

            //get distinct challenge categories from database
            var database = dbClient.GetDatabase("CodeCraft");
            var collection = database.GetCollection<BsonDocument>("challenges_2");

            var distinctValues = collection.Distinct<string>("category", new BsonDocument()).ToList();

            foreach(var value in distinctValues){
                Console.WriteLine("Category: " + value);
            }

            // calculate success rate for each category
            // foreach (var category in data)
            // {
            //     double numberOfChallenges = category.Value["numberOfChallenges"];
            //     double successRate = CalculateSuccessRate(category);

            //     double posterior = successRate * (numberOfChallenges / totalNumberOfChallenges);
            //     posteriors.Add(posterior);
            // }

            foreach(var category in distinctValues){
                // double numberOfChallenges = getChallengesPerCategory(category).Item2;

                double[] categoryValues = getChallengesPerCategory(category, difficultyLevel);

                double numberOfChallenges = categoryValues[2];
                double successRate = categoryValues[1]/categoryValues[0];

                double posterior = successRate * (numberOfChallenges / totalNumberOfChallenges);
                posteriors.Add(posterior);
            }

            double normalizedConstant = CalculateNormalizedConstant(posteriors);

            Dictionary<string, double> results = new Dictionary<string, double>();
            // foreach (var category in data)
            // {
            //     double numberOfChallenges = category.Value["numberOfChallenges"];
            //     double successRate = CalculateSuccessRate(category);

            //     double result = CalculateBayesianTheorom(successRate, numberOfChallenges, normalizedConstant);
            //     results.Add(category.Name, result);
            // }

            foreach(var category in distinctValues){

                double[] categoryValues = getChallengesPerCategory(category, difficultyLevel);
                double numberOfChallenges = categoryValues[2];
                double successRate = categoryValues[1]/categoryValues[0];

                double result = CalculateBayesianTheorom(successRate, numberOfChallenges, normalizedConstant);
                results.Add(category, result);
            }

            string categoryWithLowestSuccessRate = CompareProbability(results);
            //string challenge = PickChallenge(data, categoryWithLowestSuccessRate);

            string challenge = PickChallenge(categoryWithLowestSuccessRate, difficultyLevel);

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

        static string PickChallenge(string category, string difficultyLevel){
            Random random = new Random();
            var data = getChallengesPerCategory(category, difficultyLevel);
            int randomIndex = random.Next(0, (int) data[2]);

            var database = dbClient.GetDatabase("CodeCraft");
            var collection = database.GetCollection<BsonDocument>("challenges_2");

            var filter = Builders<BsonDocument>.Filter.Eq("category", category);
            var filterDifficulty = Builders<BsonDocument>.Filter.Eq("difficulty", difficultyLevel);

            var combinedFilter = filter & filterDifficulty;

            var result = collection.Find(combinedFilter).ToList();

            return result[randomIndex].ToString();
        }
    }
}
