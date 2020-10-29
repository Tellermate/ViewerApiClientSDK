using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Security.Principal;
using System.Text;
using System.Collections.Generic;

namespace Tellermate.ViewerApiClientSDK
{


    public enum RequestStatus { OK, BadToken, NoToken, Unathorized, Other, ApiCodeError, TooManyTries, ClientCodeError }
    public class ViewerApiClient
    {

        private ClientResult<Guid?> Token;
        private HttpClient Client;
        private Credentials _credentials;
        public string ServerBaseAddress { get; set; }
        public RequestStatus Status { get; set; }
        public delegate void delActivity(string msg);
        public delActivity Activity { get; set; }
     

        public Guid? CurrentToken {
            get{ 

                return Token!=null? Token.ObjectValue:null;
            }
        }


        public ViewerApiClient(Credentials credentials,  delActivity messageCalBack = null)
        {
            ServerBaseAddress = "https://touchviewerdataapi.azurewebsites.net/api/";
        }
    public ViewerApiClient(Credentials credentials, string BaseAddress, delActivity messageCalBack = null)
        {

            Activity = messageCalBack;


            ServerBaseAddress = BaseAddress;


            _credentials = credentials;

            ClientResult<HttpClient> result = NewNetClient();

            if (result.RequestStatus == RequestStatus.OK)
            {
                Client = result.ObjectValue;
                Status = RequestStatus.OK;
            }

            else

            {
                Status = result.RequestStatus;
            }

        }
        private ClientResult<Guid?> GetToken(Credentials credentials)
        {
            string json = JsonConvert.SerializeObject(credentials);
            StringContent sc = new StringContent(json, Encoding.UTF8, "application/json");


            Activity?.Invoke("Getting new token");

            try
            {
                HttpResponseMessage m = Client.PostAsync("GetToken", sc).Result;

                string JsonPayload = m.Content.ReadAsStringAsync().Result;

                if (m.IsSuccessStatusCode)
                {
                    TokenRequestResponse result = JsonConvert.DeserializeObject<TokenRequestResponse>(JsonPayload);
                  
                    Client.DefaultRequestHeaders.Remove("Authorization");
                    Client.DefaultRequestHeaders.Add("Authorization", result.Value);

                    Guid tkn;
                    if (Guid.TryParse(result.Value, out tkn))
                    {
                        Activity?.Invoke("GetToken - OK: Token = " + tkn.ToString());
                        return new ClientResult<Guid?>(RequestStatus.OK, tkn, "ALL OK :-)");
                    }

                    else

                    {
                        Activity?.Invoke("GetToken - BAD TOKEN: The token the server returned was not in the correct format.");
                        return new ClientResult<Guid?>(RequestStatus.BadToken, null, "The token the server returned was not in the correct format. Please contact Tellermate");
                    }
                }

                else
                {
                    if (m.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        Activity?.Invoke("GetToken - UNAUTHORIZED: because " + m.ReasonPhrase);
                        return new ClientResult<Guid?>(RequestStatus.Unathorized, null, m.ReasonPhrase);
                        
                    }

                    else
                    {
                        Activity?.Invoke("GetToken - " + m.StatusCode.ToString() + " - because " + m.ReasonPhrase);
                        return new ClientResult<Guid?>(RequestStatus.Other, null, m.StatusCode.ToString() + ": " + m.ReasonPhrase);
                    }

                }

            }

            catch (Exception ex)
            {
                Activity?.Invoke("GetToken - PROGRAM ERROR: " + ex.Message);
                return new ClientResult<Guid?>(RequestStatus.ClientCodeError, null, ex.Message);
            }

        }

        public ClientResult<RootOrg> getCounts(DateTime DateFrom, DateTime DateTo)
        {

            string url = string.Format("GetCashCounts?DateFrom={0}&DateTo={1}",
             DateFrom.ToString("yyyy-MM-dd"),
             DateTo.ToString("yyyy-MM-dd"));

            return getServerResponse<RootOrg>(url);

         
        }

        public ClientResult<List<MachineStore>> getMachineStores()
        {
            return getServerResponse<List<MachineStore>>("GetStoreMachines");
        }

        public ClientResult<List<CountType>> getCountTypes()
        {
            return getServerResponse<List<CountType>>("GetCountTypeDefs");
        }

        public ClientResult<T> getServerResponse<T>(string EndPointURL, int Attempt = 0) 
        {

            string EndpointName = EndPointURL;
            int n = EndPointURL.IndexOf('?');
            if (n!=-1)
            {
                EndpointName = EndPointURL.Substring(0, n);
            }


            if (Token == null)
            {
                Activity?.Invoke(EndpointName + " - There is no token");
                Token = GetToken(_credentials);

                if (Token.RequestStatus != RequestStatus.OK)
                {

                    return new ClientResult<T>(Token.RequestStatus,  Token.Message);
                }

            }

            try
            {
                HttpResponseMessage m = Client.GetAsync(EndPointURL).Result;

                string JsonPayload = m.Content.ReadAsStringAsync().Result;


                Activity?.Invoke(JsonPayload);

                if (m.IsSuccessStatusCode)
                {
                    T data = JsonConvert.DeserializeObject<T>(JsonPayload);

                    Activity?.Invoke(EndpointName + " - Data retrieved successfully");

                    return new ClientResult<T>(RequestStatus.OK, data, JsonPayload);
                }

                else
                {
                    if (m.StatusCode == HttpStatusCode.Unauthorized)
                    {


                        if (m.ReasonPhrase == "NotFound" ||
                            m.ReasonPhrase == "Expired" ||
                            m.ReasonPhrase == "UnexpectedIp")
                        {
                            Token = GetToken(_credentials);

                            if (Token.RequestStatus == RequestStatus.OK)
                            {
                                if (Attempt < 4)
                                {
                                    Activity?.Invoke(EndpointName + " - Attempt number " + Attempt.ToString());
                                    ClientResult<T> result = getServerResponse<T>(EndPointURL,++Attempt);
                                    return result;
                                }

                                else
                                {
                                    Activity?.Invoke(EndpointName + " - Max attempts exceeded, exiting with error");
                                    return new ClientResult<T>(RequestStatus.TooManyTries,  "");
                                }
                            }
                            else
                            {
                                return new ClientResult<T>(Token.RequestStatus,  Token.Message);
                            }
                        }
                        Activity?.Invoke(EndpointName + " - UNAUTHORIZED : " + m.ReasonPhrase);
                        return new ClientResult<T>(RequestStatus.Unathorized,  m.ReasonPhrase);
                    }

                    else
                    {
                        Activity?.Invoke(EndpointName + " - API error Server said: " + m.StatusCode.ToString() + "...and added: " + m.ReasonPhrase);
                        return new ClientResult<T>(RequestStatus.Other,  "API error - Server said: " + m.StatusCode.ToString() + "...and added: " + m.ReasonPhrase);
                    }
                }
            }

            catch (Exception ex)
            {
                Activity?.Invoke(EndpointName + " - Client error: " + ex.Message);
                return new ClientResult<T>(RequestStatus.ClientCodeError,  ex.Message);
            }

        }





        private ClientResult<HttpClient> NewNetClient()
        {
            HttpClient NewClient = new HttpClient();

            NewClient.BaseAddress = new Uri(ServerBaseAddress);
            NewClient.DefaultRequestHeaders.Accept.Clear();
            NewClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            if (Token != null && Token.RequestStatus == RequestStatus.OK)
            {
                NewClient.DefaultRequestHeaders.Add("Authorization", Token.ObjectValue.ToString());
                
            }

            return new ClientResult<HttpClient>(RequestStatus.OK, NewClient, "");

        }


    }


    public class ClientResult<T> //where T : class
    {
        public RequestStatus RequestStatus { get; private set; }
        public string Message { get; private set; }
        public T ObjectValue { get; private set; }




        public ClientResult(RequestStatus requestStatus, T ReturnedValue, string message) 
        {
            RequestStatus = requestStatus;
            ObjectValue = ReturnedValue;
            Message = message;
        }

        public ClientResult(RequestStatus requestStatus,  string message)
        {
            RequestStatus = requestStatus;
           // ObjectValue = null;
            Message = message;
        }

    }
}
