using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PClient.Models
{
    class Client
    {
        #region Constatnts
        public const string SERVER_ROUTER_IP = "172.16.20.121";
        public const int SERVER_ROUTER_PORT = 5555;
        #endregion

        private string ipAddress;

        public Client() {}

        public override string ToString()
        {
            return IPAddress;
        }

        public Client(String ipAddress)
        {
            this.ipAddress = ipAddress;
        }

        public string IPAddress
        {
            get { return ipAddress;}        
        }
    }
}
