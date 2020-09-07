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
        public ViewerApiClient(Credentials credentials)
        {
            ServerBaseAddress = "https://touchviewerdataapi.azurewebsites.net/api/"; // default address
           // ServerBaseAddress = "https://localhost:44349/api/"; // default address (dev)

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

            try
            {
                HttpResponseMessage m = Client.PostAsync("GetToken", sc).Result;

                string JsonPayload = m.Content.ReadAsStringAsync().Result;

                if (m.IsSuccessStatusCode)
                {
                    TokenRequestResponse result = JsonConvert.DeserializeObject<TokenRequestResponse>(JsonPayload);
                    //AddInfoLine("New Token recieved: " + result.Value);
                    Client.DefaultRequestHeaders.Remove("Authorization");
                    Client.DefaultRequestHeaders.Add("Authorization", result.Value);

                    Guid tkn;
                    if (Guid.TryParse(result.Value, out tkn))
                    {
                        return new ClientResult<Guid?>(RequestStatus.OK, tkn, "ALL OK :-)");
                    }

                    else

                    {
                        return new ClientResult<Guid?>(RequestStatus.BadToken, null, "The token the server returned was not in the correct format. Please contact Tellermate");
                    }
                }

                else
                {
                    if (m.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        return new ClientResult<Guid?>(RequestStatus.Unathorized, null, m.ReasonPhrase);
                    }

                    else
                    {
                        return new ClientResult<Guid?>(RequestStatus.Other, null, m.StatusCode.ToString() + ": " + m.ReasonPhrase);
                    }

                }

            }

            catch (Exception ex)
            {
                // AddInfoLine(ex.Message);
                return new ClientResult<Guid?>(RequestStatus.ClientCodeError, null, ex.Message);
            }

        }
        public ClientResult<RootOrg> getCounts(DateTime DateFrom, DateTime DateTo, int Attempt = 0)
        {


            if (Token == null)
            {

                Token = GetToken(_credentials);

                if (Token.RequestStatus != RequestStatus.OK)
                {
                    return new ClientResult<RootOrg>(Token.RequestStatus, null, Token.Message);
                }

            }

            string url = string.Format("GetCashCounts?DateFrom={0}&DateTo={1}",
                         DateFrom.ToString("yyyy-MM-dd"),
                         DateTo.ToString("yyyy-MM-dd"));


            try
            {
                HttpResponseMessage m = Client.GetAsync(url).Result;

                string JsonPayload = m.Content.ReadAsStringAsync().Result;

                if (m.IsSuccessStatusCode)
                {
                    RootOrg CountData = JsonConvert.DeserializeObject<RootOrg>(JsonPayload);
                    return new ClientResult<RootOrg>(RequestStatus.OK, CountData, JsonPayload);
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
                                    ClientResult<RootOrg> result = getCounts(DateFrom, DateTo, ++Attempt);
                                    return result;
                                }

                                else
                                {
                                    return new ClientResult<RootOrg>(RequestStatus.TooManyTries, null, "");
                                }
                            }
                            else
                            {
                                return new ClientResult<RootOrg>(Token.RequestStatus, null, Token.Message);
                            }
                        }

                        return new ClientResult<RootOrg>(RequestStatus.Unathorized, null, m.ReasonPhrase);
                    }

                    else
                    {
                        return new ClientResult<RootOrg>(RequestStatus.Other, null, "API error - Server said: " + m.StatusCode.ToString() + "...and added: " + m.ReasonPhrase);
                    }
                }
            }

            catch (Exception ex)
            {

                return new ClientResult<RootOrg>(RequestStatus.ClientCodeError, null, ex.Message);
            }
        }
        public ClientResult<List<MachineStore>> getMachineStores(int Attempt = 0)
        {
            if (Token == null)
            {

                Token = GetToken(_credentials);

                if (Token.RequestStatus != RequestStatus.OK)
                {
                    return new ClientResult<List<MachineStore>>(Token.RequestStatus, null, Token.Message);
                }

            }

            string url = "GetStoreMachines";


            try
            {
                HttpResponseMessage m = Client.GetAsync(url).Result;

                string JsonPayload = m.Content.ReadAsStringAsync().Result;

                if (m.IsSuccessStatusCode)
                {
                    List<MachineStore> data = JsonConvert.DeserializeObject<List<MachineStore>>(JsonPayload);
                    return new ClientResult<List<MachineStore>>(RequestStatus.OK, data, JsonPayload);
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
                                    ClientResult<List<MachineStore>> result = getMachineStores( ++Attempt);
                                    return result;
                                }

                                else
                                {
                                    return new ClientResult<List<MachineStore>>(RequestStatus.TooManyTries, null, "");
                                }
                            }
                            else
                            {
                                return new ClientResult<List<MachineStore>>(Token.RequestStatus, null, Token.Message);
                            }
                        }

                        return new ClientResult<List<MachineStore>>(RequestStatus.Unathorized, null, m.ReasonPhrase);
                    }

                    else
                    {
                        return new ClientResult<List<MachineStore>>(RequestStatus.Other, null, "API error - Server said: " + m.StatusCode.ToString() + "...and added: " + m.ReasonPhrase);
                    }
                }
            }

            catch (Exception ex)
            {

                return new ClientResult<List<MachineStore>>(RequestStatus.ClientCodeError, null, ex.Message);
            }



        }

        public ClientResult<List<CountType>> getCountTypes(int Attempt = 0)
        {
            if (Token == null)
            {

                Token = GetToken(_credentials);

                if (Token.RequestStatus != RequestStatus.OK)
                {
                    return new ClientResult<List<CountType>>(Token.RequestStatus, null, Token.Message);
                }

            }

            string url = "GetCountTypeDefs";


            try
            {
                HttpResponseMessage m = Client.GetAsync(url).Result;

                string JsonPayload = m.Content.ReadAsStringAsync().Result;

                if (m.IsSuccessStatusCode)
                {
                    List<CountType> data = JsonConvert.DeserializeObject<List<CountType>>(JsonPayload);
                    return new ClientResult<List<CountType>>(RequestStatus.OK, data, JsonPayload);
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
                                    ClientResult<List<CountType>> result = getCountTypes(++Attempt);
                                    return result;
                                }

                                else
                                {
                                    return new ClientResult<List<CountType>>(RequestStatus.TooManyTries, null, "");
                                }
                            }
                            else
                            {
                                return new ClientResult<List<CountType>>(Token.RequestStatus, null, Token.Message);
                            }
                        }

                        return new ClientResult<List<CountType>>(RequestStatus.Unathorized, null, m.ReasonPhrase);
                    }

                    else
                    {
                        return new ClientResult<List<CountType>>(RequestStatus.Other, null, "API error - Server said: " + m.StatusCode.ToString() + "...and added: " + m.ReasonPhrase);
                    }
                }
            }

            catch (Exception ex)
            {

                return new ClientResult<List<CountType>>(RequestStatus.ClientCodeError, null, ex.Message);
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

    /*
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

    public class GetCountResult
    {
        public RequestStatus RequestStatus { get; private set; }
        public string Message { get; private set; }
        public RootOrg Organisation { get; private set; }




        public GetCountResult(RequestStatus requestStatus, RootOrg organisation, string message)
        {
            RequestStatus = requestStatus;
            Organisation = organisation;
            Message = message;
        }

    }

    */


    public class ClientResult<T>
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

    }
}
