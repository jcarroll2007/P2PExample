using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace P2PClient
{
    public static class Utility
    {
        #region Helper Methods
        public static  Dictionary<string, IPAddress> GetRoutingTableFromBytes(byte[] bytes)
        {
            Dictionary<string, IPAddress> table;

            using (var ms = new System.IO.MemoryStream(bytes))
            {
                var formatter = new BinaryFormatter();
                table = (Dictionary<string, IPAddress>)formatter.Deserialize(ms);
            }
            return table;
        }
        #endregion
    }
}
