using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PClient.Models
{
    class Client
    {
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
