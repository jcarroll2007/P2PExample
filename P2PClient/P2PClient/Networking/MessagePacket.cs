using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PClient
{
    [Serializable]
    class MessagePacket
    {
        public string UserNameFrom { get; set; }
        public string Message { get; set; }
    }
}
