using Microsoft.AspNetCore.Mvc;
using TradingExecutorAPI.Models;
using TradingExecutorAPI.Services;
using TradingExecutorAPI.Utils;

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
        private string _prevData = "";
        private StreamWriter? _sw1;
        private StreamWriter? _sw2;
        private StreamWriter? _sw3;

        private StreamWriter? _custom_sw;
        private string _prevCustomData = "";

        private string _alertTime = "";

        [HttpPost("/forwardalert", Name = "ForwardAlert")]
        public async Task<string> ForwardAlert(AlertModel alert)
        {
            try
            {
                // Initiate writer for alert error, sw3 = AlertErrors.txt
                // Creating at the beginning so that exception can be caught from the very first point
                _sw3 = new StreamWriter(AlertErrorsFileName, true);
                
                // Create Directory if not exists 
                FileUtils.CreateDirectory(FileDirectory);
            
                // Read prev content from file before writing/doing anything
                using StreamReader sReader = new StreamReader(FilePathWithName);
                _prevData = await sReader.ReadToEndAsync();
                sReader.Dispose();
                
                // Read Request Body/Contents
                HttpRequest req = Request;
                string reqBody = alert.TimeAndMessage!;
                _alertTime = alert.TimeAndMessage?.Split("||")[0]!;
                string message = alert.TimeAndMessage?.Split("||")[1]!;
                string messageWithImpInfo;
                string tradeType = message.Split(",")[0];

                string CustomAlertFile = FileDirectory+ "\\"+ tradeType + "_Alerts.txt";


                try
                {
                    using StreamReader customStreamReader = new StreamReader(CustomAlertFile);
                    _prevCustomData = await customStreamReader.ReadToEndAsync();
                    customStreamReader.Dispose();
                }
                catch (Exception ex)
                {
                    await _sw3!.WriteLineAsync(_alertTime + " , " + ex.Message);
                }

                // Validate Token
                var valid = new AlertService().ValidateReqHeader(req.Headers["CallerToken"], reqBody);
                if (!valid.Item1)
                {
                    await _sw3.WriteLineAsync( _alertTime + " , " + $"Token Validation Error, token found {req.Headers["CallerToken"]}, {valid.Item2} ");
                    await _sw3!.DisposeAsync();
                    return "Token Validation Error";
                }
        
                // Read Message Content
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
                
                // Delete the existing file
                //System.IO.File.Delete(FilePathWithName);
                
                // Clear the existing file
                await System.IO.File.WriteAllTextAsync(FilePathWithName, string.Empty);
                
                // Initiate all writers for alert (except AlertErrors.txt),
                // sw1 = Alerts.txt, sw2 = AlertDetails.txt
                _sw1 = new StreamWriter(FilePathWithName, false);
                _sw2 = new StreamWriter(AlertDetailsFileName, true);
                _custom_sw = new StreamWriter(CustomAlertFile, false);
                
                //// File Output Pattern ////
                // Time,Crossing Up/Down,TradeType \n
                // Prev Rows
                await _sw1.WriteAsync(_alertTime + "," + messageWithImpInfo + "," + tradeType + Environment.NewLine + _prevData);
                await _sw2.WriteLineAsync(_alertTime + " , " + message);
                await _custom_sw.WriteAsync(_alertTime + "," + messageWithImpInfo + "," + tradeType + Environment.NewLine + _prevCustomData);
                
            }
            // Exceptions
            catch (IOException ex)
            {
                await _sw3!.WriteLineAsync(_alertTime + " , " + ex.Message);
            }
            catch (Exception ex)
            {
                await _sw3!.WriteLineAsync(_alertTime + " , " + ex.Message);
            }
            finally
            {
                if(_sw1 != null) await _sw1!.DisposeAsync();
                if(_sw2 != null) await _sw2!.DisposeAsync();
                if(_sw3 != null) await _sw3!.DisposeAsync();
                if (_custom_sw != null) await _custom_sw!.DisposeAsync();
            }

            return SuccessCode;
        }
        
        [HttpGet("/getversion", Name = "GetVersion")]
        public string GetVersion()
        {
            return "1.3";
        }
    }
}
