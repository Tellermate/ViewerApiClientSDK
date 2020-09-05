using System;
using System.Collections.Generic;
using System.Text;

namespace Tellermate.ViewerApiClientSDK
{
    public class TokenRequest
    {
        public string ID { get; set; }
        public string Key { get; set; }
    }


    public class TokenRequestResponse
    {
        public string Value { get; set; }
        public int TokenLifeTimeInSeconds { get; set; }
        public DateTime ExpiresServerTime { get; set; }
    }

}
