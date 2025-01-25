using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace FnPostDatabase
{
    public class PostDatabase
    {
        private readonly ILogger<PostDatabase> _logger;

        public PostDatabase(ILogger<PostDatabase> logger)
        {
            _logger = logger;
        }

        [Function("PostDatabase")]
        [CosmosDBOutput("%DatabaseName%", "movies", Connection = "CosmosDBConnection", CreateIfNotExists = true, PartitionKey = "title")]
        public async Task<object?> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
        {
            _logger.LogInformation("Iniciando processo de insert no banco.");

            MovieRequest movie = null;

            var content = await new StreamReader(req.Body).ReadToEndAsync();

            try
            {
                movie = JsonConvert.DeserializeObject<MovieRequest>(content);
            }
            catch (Exception ex) {
                new BadRequestObjectResult("Erro ao deserializar o objeto: " + ex.Message);
            }

            return JsonConvert.SerializeObject(movie);
        }
    }
}
