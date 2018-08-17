
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LetsBuildIt.Web.API
{
    public static class WeatherForecastsFunction
    {
        private static string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private const string ObjectIdentifierClaimType = "http://schemas.microsoft.com/identity/claims/objectidentifier";

        [FunctionName("GetWeatherForecasts")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "sampledata/weatherforecasts")]HttpRequest req, ILogger log)
        {
            log.LogInformation("C# user login HTTP trigger function processed a request.");

            // Authentication boilerplate code start
            ClaimsPrincipal principal = await BearerTokenValidator.ValidateAsync(req, log);

            if (principal == null)
            {
                return new BadRequestObjectResult("Not authorised");
            }

            string userId = principal.ClaimOfType(ObjectIdentifierClaimType);

            log.LogInformation("User ID for request: " + userId);

            var rng = new Random();
            return new OkObjectResult(Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                DateFormatted = DateTime.Now.AddDays(index).ToString("d"),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            }));
        }
    }

    public class WeatherForecast
    {
        public string DateFormatted { get; set; }
        public int TemperatureC { get; set; }
        public string Summary { get; set; }

        public int TemperatureF
        {
            get
            {
                return 32 + (int)(TemperatureC / 0.5556);
            }
        }
    }
}
