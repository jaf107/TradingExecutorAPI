﻿using Microsoft.AspNetCore.Http;
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

        [HttpPost("/fowardalert", Name = "ForwardAlert")]
        public async Task<string> ForwardAlert()
        {
            StreamWriter? sw = null;
            var req = Request;

            // Read Request Body
            StreamReader streamReader = new StreamReader(req.Body, Encoding.UTF8);
            string reqBody = await streamReader.ReadToEndAsync();

            // Validate Token
            bool valid = new AlertService().ValidateService(req.Headers["CallerToken"], reqBody);
            if (!valid)
                return "Token Validation Error";

            // Write to File
            try
            {
                sw = new StreamWriter("ClientAlertDetails.txt", true);
                await sw.WriteLineAsync(DateTime.Now + " , " + reqBody);
            }
            catch (IOException ex)
            {
                sw = new StreamWriter("AlertErrors.txt", true);
                await sw.WriteLineAsync(DateTime.Now + " , " + ex.Message);
            }
            catch (Exception ex)
            {
                sw = new StreamWriter("AlertErrors.txt", true);
                await sw.WriteLineAsync(DateTime.Now + " , " + ex.Message);
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
            return "1.0";
        }
    }
}