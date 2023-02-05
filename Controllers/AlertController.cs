using Microsoft.AspNetCore.Mvc;
using TradingExecutorAPI.Models;
using TradingExecutorAPI.Services;

namespace TradingExecutorAPI.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class AlertController : ControllerBase
    {
        private const string SuccessCode = "200";

        [HttpPost("/forwardalert", Name = "ForwardAlert")]
        public async Task<string> ForwardAlert(AlertModel alert)
        {
            /*var timeUtc = DateTime.UtcNow;
            TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            DateTime easternTime = TimeZoneInfo.ConvertTimeFromUtc(timeUtc, easternZone);*/

            var req = Request;
            StreamWriter sw1 = new StreamWriter("AlertDetails.txt", true);
            StreamWriter sw2 = new StreamWriter("AlertErrors.txt", true);


            // Read Request Body
            string reqBody = alert.TimeAndMessage!;
            string alertTime = alert.TimeAndMessage?.Split("||")[0]!;
            var message = alert.TimeAndMessage?.Split("||")[1]!;
            
            // Validate Token
            var valid = new AlertService().ValidateReqHeader(req.Headers["CallerToken"], reqBody);
            if (!valid.Item1)
            {
                await sw2.WriteLineAsync( alertTime + " , " + $"Token Validation Error, token found {req.Headers["CallerToken"]}, {valid.Item2} ");
                await sw2!.DisposeAsync();
                return "Token Validation Error";
            }
            // Write to File
            try
            {
                await sw1.WriteLineAsync(alertTime + " , " + message);
            }
            catch (IOException ex)
            {
                await sw2.WriteLineAsync(alertTime + " , " + ex.Message);
            }
            catch (Exception ex)
            {
                await sw2.WriteLineAsync(alertTime + " , " + ex.Message);
            }
            finally
            {
                await sw1!.DisposeAsync();
                await sw2!.DisposeAsync();
            }

            return SuccessCode;
        }
        
        [HttpGet("/getversion", Name = "GetVersion")]
        public string GetVersion()
        {
            return "1.0";
        }
    }
}
