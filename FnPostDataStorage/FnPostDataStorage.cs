using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace FnPostDataStorage
{
    public class DataStorage
    {
        private readonly ILogger<DataStorage> _logger;

        public DataStorage(ILogger<DataStorage> logger)
        {
            _logger = logger;
        }

        [Function("DataStorage")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
        {
            _logger.LogInformation("Iniciando processamento de midia no Storage.");

            if (!req.Headers.TryGetValue("file-type", out var fileTypeHeader))
                return new BadRequestObjectResult("É necessário informar o Header file-type.");

            string fileType = fileTypeHeader.ToString();
            var form = await req.ReadFormAsync();
            var file = form.Files["file"];

            if (file == null || file.Length == 0)
                return new BadRequestObjectResult("Envie o arquivo a partir do objeto 'file'.");

            string connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
            string blobContainerName = fileType;
            string blobName = file.FileName;

            BlobClient blobClient = new BlobClient(connectionString, blobContainerName, blobName);
            BlobContainerClient containerClient = new BlobContainerClient(connectionString, blobContainerName);

            await containerClient.CreateIfNotExistsAsync();
            await containerClient.SetAccessPolicyAsync(PublicAccessType.BlobContainer);

            var blob = containerClient.GetBlobClient(blobName);

            using(var stream = file.OpenReadStream())
            {
                await blob.UploadAsync(stream);
            }

            _logger.LogInformation($"O arquivo {blobName} foi salvo com sucesso!")

            return new OkObjectResult(new
            {
                Message = "Arquivo armazenado com sucesso.",
                BlobUri = blob.Uri,
            });
        }
    }
}
