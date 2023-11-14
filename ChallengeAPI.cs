using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;

namespace Baysian_Model
{
    public class ChallengeAPI
    {
        private readonly ILogger _logger;

        public ChallengeAPI(ILoggerFactory loggerFactory){
            _logger = loggerFactory.CreateLogger<ChallengeAPI>();
        }

        [Function("ChallengeAPI")]
        public HttpResponseData Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req){

            _logger.LogInformation("C# HTTP trigger function processed a request.");

            string id = req.Query["id"];

            if (id == null){
                var invalidResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                invalidResponse.WriteString("Please pass a name on the query string or in the request body");
                return invalidResponse;
            }

            int userLevel = 0;
            try {
                userLevel = AI_Model.getUserLevel(id);
            } catch {
                var invalidResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                invalidResponse.WriteString("Please pass a valid id");
                return invalidResponse;
            }

            //int userLevel = AI_Model.getUserLevel(id);

            string challengeDifficulty = AI_Model.userDifficulty(userLevel);

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json; charset=utf-8");

            //AI_Model.updateFields();

            //Console.WriteLine(AI_Model.Run());

            Console.WriteLine("Number of challenges" + AI_Model.getNumberOfChallenges("Easy"));
            Console.WriteLine(AI_Model.Run(challengeDifficulty));

            string res = AI_Model.Run(challengeDifficulty);

            var document = BsonDocument.Parse(res);
            var json = document.ToJson(new MongoDB.Bson.IO.JsonWriterSettings { OutputMode = MongoDB.Bson.IO.JsonOutputMode.Strict });

            response.WriteString(json);

            return response;
        }
    }
}