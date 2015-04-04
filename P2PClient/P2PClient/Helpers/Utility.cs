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
            return table;
        }

        public static System.Collections.ObjectModel.ObservableCollection<Client> GetClientsFromTable(Dictionary<string, IPAddress> routingTable)
        {
            ObservableCollection<Client> clients = new ObservableCollection<Client>();
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
