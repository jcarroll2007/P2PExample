using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using P2PClient.Models;

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

            //Dictionary<string, IPAddress> tableWithConvertedStrings = new Dictionary<string, IPAddress>();

            //foreach (KeyValuePair<string, IPAddress> client in table)
            //{
            //    tableWithConvertedStrings.Add(ConvertToString(client.Key), client.Value);
            //}
            return table;
        }

        //private static string ConvertToString(string p)
        //{
        //    string[] stringBytes = p.Split('-');
        //    byte[] stringAsBytes = new byte[stringBytes.Length * 2];
        //    string convertedString = "";

        //    int index = 0;

        //    foreach (string stringcharCode in stringBytes)
        //    {
        //        short.Parse(stringcharCode);

        //    }
        //}

        public static List<Client> GetClientsFromTable(Dictionary<string, IPAddress> routingTable)
        {
            List<Client> clients = new List<Client>();
            foreach (KeyValuePair<string, IPAddress> client in routingTable)
            {
                Client c = new Client(client.Value.ToString());
                c.UserName = client.Key;
                clients.Add(c);
            }

            return clients;
        }
        #endregion
    }
}
