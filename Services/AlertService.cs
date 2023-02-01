using System.Text;

namespace TradingExecutorAPI.Services
{
    public class AlertService
    {
        private const string TargetApiUrl = "https://tradingalertapi.azurewebsites.net";

        public (bool,string) ValidateService(string reqHeader, string reqBody, string callerUrl)
        {
            
            //if (callerUrl.Contains( TargetApiUrl))
            //    return (false,"");
            return (reqHeader == PerformXOR(reqBody.Trim('\"').Split("||").ToList()), PerformXOR(reqBody.Trim('\"').Split("||").ToList()));
        }


        private string PerformXOR(List<string> textsBeforeModification)
        {
            // Check if list contains any null value, if so then throw Security exception
            if (textsBeforeModification.Any(x => x == null))
            {
                throw new Exception();
            }

            const int targetTextLength = 16;
            const char paddingCharacter = '9';
            List<StringBuilder> listStrBuilders = new();

            // Get String Builder ready and make texts contain equal number of characters
            List<string> textsAfterModification = new List<string>();
            foreach (string text in textsBeforeModification)
            {
                // make every texts length of 16 characters; 1 character = 1 byte
                string modifiedText = text.Length < targetTextLength ? text.PadLeft(targetTextLength, paddingCharacter) : text[..targetTextLength];
                textsAfterModification.Add(modifiedText);

                // create string builders of similar length of modified texts
                listStrBuilders.Add(new StringBuilder(modifiedText));
            }

            StringBuilder outputStringBuilder = new StringBuilder(textsAfterModification[0].Length);

            // Do XOR char by char
            var xoredChar = 'A';
            for (int i = 0; i < textsAfterModification[0].Length; i++)
            {
                for (int j = 0; j < listStrBuilders.Count; j++)
                {
                    if (j == 0)
                    {
                        xoredChar = listStrBuilders[j][i];
                    }
                    else
                    {
                        char charToXorWith = listStrBuilders[j][i];
                        xoredChar = (char)(xoredChar ^ charToXorWith);
                    }
                }
                outputStringBuilder.Append(xoredChar);
            }

            // Return XORed Value
            var xorOutput = outputStringBuilder.ToString();
            return Convert.ToBase64String(Encoding.ASCII.GetBytes(xorOutput));
        }


    }
}