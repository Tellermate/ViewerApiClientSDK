using System;
using System.Collections.Generic;
using System.Text;

namespace Tellermate.ViewerApiClientSDK
{
    public class Machine
    {
        public string SerialNumber { get; set; }
    }

    public class MachineStore
    {
        public string StoreID { get; set; }
        public string Name { get; set; }
        public List<Machine> Machines { get; set; }
    }

    public class MachineStoresx
    {
        public List<MachineStore> Stores { get; set; }
    }



    public class CountType
    {
        public int ID { get; set; }
        public string DefaultName { get; set; }
        public string Description { get; set; }
        public double Direction { get; set; }
        public string ContainerType { get; set; }
        public bool UsedInUnit { get; set; }
    }




}
