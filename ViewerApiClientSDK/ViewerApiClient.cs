using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Security.Principal;
using System.Text;

namespace Tellermate.ViewerApiClientSDK
{


    public enum RequestStatus { OK, BadToken, Unathorized, Other, CodeError }
    public class ViewerApiClient
    {

        private GetTokenResult Token;
        private HttpClient client;
        private Credentials _credentials;

        ViewerApiClient(Credentials credentials)
        {
            _credentials = credentials;

            Token = GetToken(credentials);





        }


        private GetTokenResult GetToken(Credentials credentials)
        {
            string json = JsonConvert.SerializeObject(credentials);
            StringContent sc = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                HttpResponseMessage m = client.PostAsync("GetToken", sc).Result;

                string JsonPayload = m.Content.ReadAsStringAsync().Result;

                if (m.IsSuccessStatusCode)
                {
                    TokenRequestResponse result = JsonConvert.DeserializeObject<TokenRequestResponse>(JsonPayload);
                    //AddInfoLine("New Token recieved: " + result.Value);
                    client.DefaultRequestHeaders.Remove("Authorization");
                    client.DefaultRequestHeaders.Add("Authorization", result.Value);

                    Guid tkn;
                    if (Guid.TryParse(result.Value, out tkn))
                    {
                        return new GetTokenResult(RequestStatus.OK, tkn, "ALL OK :-)");
                    }

                    else

                    {
                        return new GetTokenResult(RequestStatus.BadToken, null, "The token the server returned was not in the correct format. Please contact Tellermate");
                    }
                }

                else
                {
                    if (m.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        return new GetTokenResult(RequestStatus.Unathorized, null, m.ReasonPhrase);
                    }

                    else
                    {
                        return new GetTokenResult(RequestStatus.Other, null, m.StatusCode.ToString() + ": " + m.ReasonPhrase);
                    }

                }

            }

            catch (Exception ex)
            {
                // AddInfoLine(ex.Message);
                return new GetTokenResult(RequestStatus.CodeError, null, ex.Message);
            }

        }

    }

    public class GetTokenResult
    {
        public RequestStatus RequestStatus { get; private set; }
        public Guid? Token { get; private set; }
        public string Message { get; private set; }



        public GetTokenResult(RequestStatus requestStatus, Guid? token, string message)
        {
            RequestStatus = requestStatus;
            Token = token;
            Message = message;

        }
    }
}
