using Microsoft.AspNetCore.Mvc;
using System.Text;
using TradingExecutorAPI.Services;
namespace TradingExecutorAPI.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class AlertController : ControllerBase
    {
        private const string SuccessCode = "200";

        [HttpPost("/forwardalert", Name = "ForwardAlert")]
        public async Task<string> ForwardAlert()
        {
            var timeUtc = DateTime.UtcNow;
            TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            DateTime easternTime = TimeZoneInfo.ConvertTimeFromUtc(timeUtc, easternZone);

            StreamWriter? sw = null;
            StreamWriter? sw2 = null;
            var req = Request;

            var callerUrl = Request.Headers.Referer;
            sw2 = new StreamWriter("CallerUrl.txt", true);
            await sw2.WriteLineAsync(callerUrl);
            await sw2!.DisposeAsync();
            // Read Request Body
            StreamReader streamReader = new StreamReader(req.Body, Encoding.UTF8);
            string reqBody = await streamReader.ReadToEndAsync();

            // Validate Token
            var valid = new AlertService().ValidateService(req.Headers["CallerToken"], reqBody, callerUrl);
            if (!valid.Item1)
            {
                sw = new StreamWriter("AlertErrors.txt", true);
                await sw.WriteLineAsync(easternTime + " , " + $"Token Validation Error, token found {req.Headers["CallerToken"]}, {valid.Item2} ");
                await sw!.DisposeAsync();
                return "Token Validation Error";
            }
            // Write to File
            try
            {
                sw = new StreamWriter("AlertDetails.txt", true);
                await sw.WriteLineAsync(easternTime + " , " + reqBody);
            }
            catch (IOException ex)
            {
                sw = new StreamWriter("AlertErrors.txt", true);
                await sw.WriteLineAsync(easternTime + " , " + ex.Message);
            }
            catch (Exception ex)
            {
                sw = new StreamWriter("AlertErrors.txt", true);
                await sw.WriteLineAsync(easternZone + " , " + ex.Message);
            }
            finally
            {
                await sw!.DisposeAsync();
            }

            return SuccessCode;
        }


        [HttpGet("/getversion", Name = "GetVersion")]
        public string GetVersion()
        {
            return "1.2";
        }
    }
}
