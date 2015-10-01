using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fitbit_to_PI
{
    public class FitbitDataService
    {
        public FitbitDataService()
        {
            //GetData();
        }
        static public void main()
        {
            string from = "2015-04-25";
            string through = "2015-04-26";
            string url = "https://toggl.com/reports/api/v2/details?workspace_id=669485&since=" + from + "&until=" + through + "&user_agent=api_test&rounding=on";

            string ApiToken = "a2e7f93070afe8475626141477fec42b";
            string userpass = ApiToken + ":api_token";
            string userpassB64 = Convert.ToBase64String(Encoding.Default.GetBytes(userpass.Trim()));
            string authHeader = "Basic " + userpassB64;
        }
    }


}
