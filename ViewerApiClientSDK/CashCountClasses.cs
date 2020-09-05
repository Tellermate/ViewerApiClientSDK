using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tellermate.ViewerApiClientSDK
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    public partial class Denomination 
    {
        public int Quantity { get; set; }
        public decimal Value { get; set; }
        public string Format { get; set; }
        public string InfoLegend { get; set; }

   }

    public partial class Currency
    {
        public string NAME { get; set; }
        public string isocurrencycode { get; set; }
        public int decimalplaces { get; set; }
        public string htmlsymbol { get; set; }
        public List<Denomination> Denominations { get; set; }

    }

    public partial class CashCount
    {
        public string UnitSerial { get; set; }
        public DateTime CountTime { get; set; }
        public DateTime UploadTime { get; set; }
        public int sequencenumber { get; set; }
        public string CountType { get; set; }
        public int Total { get; set; }
        public List<Currency> Currencies { get; set; }

    }

    public partial class Store
    {
        public string Name { get; set; }
        public List<CashCount> CashCounts { get; set; }

    }

    public partial class RootOrg
    {
        public string Name { get; set; }
        public List<Store> Stores { get; set; }

    }



}