using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace P2PClient.Models
{
    public class Client
    {
        #region Constatnts
        public const string SERVER_ROUTER_IP = "172.16.20.121";
        public const int SERVER_ROUTER_PORT = 5555;
        #endregion

        private string _ipAddress;
        private string _userName;
        private string _conversation;
        private TcpClient _client;
        private bool _alive;


        public Client() {}

        public override string ToString()
        {
            return UserName;
        }

        public Client(String ipAddress)
        {
            this._ipAddress = ipAddress;
        }

        public string IPAddress
        {
            get { return _ipAddress;}
            set { _ipAddress = value; }
        }

        public string UserName
        {
            get { return _userName; }
            set { _userName = value; }
        }

        public TcpClient ClientSocket
        {
            get { return _client; }
            set { _client = value; }
        }

        public string Conversation
        {
            get { return _conversation; }
            set { _conversation = value; }
        }

        public bool Alive
        {
            get { return _alive; }
            set { _alive = value; }
        }
    }
}
