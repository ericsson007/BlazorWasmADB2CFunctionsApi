using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Security.Claims;

namespace FunctionApp1
{
    public class Function1
    {

        private readonly ILogger _log;
        private readonly AzureADJwtBearerValidation _azureADJwtBearerValidation;

        public Function1(ILoggerFactory loggerFactory, AzureADJwtBearerValidation azureADJwtBearerValidation)
        {
            _log = loggerFactory.CreateLogger<Function1>(); ;
            _azureADJwtBearerValidation = azureADJwtBearerValidation;
        }


        [FunctionName("GetName")]
        public async Task<IActionResult> GetName(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            try
            {
                log.LogInformation("C# HTTP trigger function processed a request.");


                ClaimsPrincipal principal; // This can be used for any claims
                if ((principal = await _azureADJwtBearerValidation.ValidateTokenAsync(req.Headers["Authorization"])) == null)
                {
                    return new UnauthorizedResult();
                }

                var claimsName = $"Bearer token claim 'name': {_azureADJwtBearerValidation.GetPreferredUserName(principal)}";


                string value = req.Query["value"];

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                dynamic data = JsonConvert.DeserializeObject(requestBody);
                value ??= data?.value;

                string responseMessage = $"This HTTP triggered function executed successfully. Found claim 'name' in JWT with value:{claimsName} " +
                    (string.IsNullOrEmpty(value) ? "Pass a 'value' in the query string or in the request body for a personalized response." :
                    $"Value:{value}.");

                return new OkObjectResult(responseMessage);
            }
            catch (Exception ex)
            {
                return new OkObjectResult($"{ex.Message}");
            }
        }
    }
}
