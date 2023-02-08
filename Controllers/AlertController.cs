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
        private const string FileDirectory = "C:\\Alerts";
        private const string FilePathWithName = FileDirectory + "\\Alerts.txt";
        private const string AlertDetailsFileName = "AlertDetails.txt";
        private const string AlertErrorsFileName = "AlertErrors.txt";
        

        [HttpPost("/forwardalert", Name = "ForwardAlert")]
        public async Task<string> ForwardAlert(AlertModel alert)
        {
            // Read Request Body
            string reqBody = alert.TimeAndMessage!;
            string alertTime = alert.TimeAndMessage?.Split("||")[0]!;
            string message = alert.TimeAndMessage?.Split("||")[1]!;
            string messageWithImpInfo;

            string stockName = message.Split(",")[0];

            if (!Directory.Exists(FileDirectory))
            {
                Directory.CreateDirectory(FileDirectory);
            }
            var req = Request;

            StreamWriter sw3 = new StreamWriter(AlertErrorsFileName, true);

            StreamReader? sr1 = null;
            StreamReader? sr2 = null;

            string OldAlertDetails = "";
            
            try
            {
                sr1= new StreamReader(AlertDetailsFileName);
                OldAlertDetails = sr1.ReadToEnd();
            }
            catch(Exception ex)
            {
                await sw3.WriteLineAsync(alertTime + ex.Message);
            }
            finally
            {
                sr1.Dispose();
            }


            string OldAlerts = "";
            try
            {
                sr2 = new StreamReader(FilePathWithName);
                OldAlerts = sr2.ReadToEnd();
            }
            catch (Exception ex)
            {
                await sw3.WriteLineAsync(alertTime + ex.Message);
            }
            finally
            {
                sr2.Dispose();
            }

            StreamWriter sw1 = new StreamWriter(FilePathWithName, false);
            StreamWriter sw2 = new StreamWriter(AlertDetailsFileName, false);
            
            
            if (message.Contains("Crossing Up"))
            {
                messageWithImpInfo = "Crossing Up";
            } 
            else if (message.Contains("Crossing Down"))
            {
                messageWithImpInfo = "Crossing Down";
            }
            else
            {
                messageWithImpInfo = message;
            }

            // Validate Token
            var valid = new AlertService().ValidateReqHeader(req.Headers["CallerToken"], reqBody);
            if (!valid.Item1)
            {
                await sw3.WriteLineAsync( alertTime + " , " + $"Token Validation Error, token found {req.Headers["CallerToken"]}, {valid.Item2} ");
                await sw1!.DisposeAsync();
                await sw2!.DisposeAsync();
                await sw3!.DisposeAsync();
                return "Token Validation Error";
            }
            // Write to File
            try
            {

                string NewAlertDetails = alertTime + " , " + message;
                string DetailedMessageToWrite = NewAlertDetails + "\n" + OldAlertDetails;

                string NewAlert = alertTime + "," + messageWithImpInfo + "," + stockName;
                string AlertsToWrite = NewAlert + "\n" + OldAlerts;
                
                await sw1.WriteAsync(AlertsToWrite);
                await sw2.WriteAsync(DetailedMessageToWrite);
            }
            catch (IOException ex)
            {
                await sw3.WriteLineAsync(alertTime + " , " + ex.Message);
            }
            catch (Exception ex)
            {
                await sw3.WriteLineAsync(alertTime + " , " + ex.Message);
            }
            finally
            {
                await sw1!.DisposeAsync();
                await sw2!.DisposeAsync();
                await sw3!.DisposeAsync();
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
