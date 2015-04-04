using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Timers;
using P2PClient.Models;
using P2PClient.Helpers;


namespace P2PClient
{
    class ClientViewModel : ViewModelBase
    {
        #region constants
        //the port to listen for other clients on
        private const int CLIENT_PORT = 9999;
        //How often do we update our client table from the server router?
        private const int GET_UPDATED_USERS_TICK_RATE = 1000;
        #endregion

        #region Properties

        //Dictionary of the users to their IP addresses
        public List<Client> Clients { get; set; }

        // Properties are accessed in the XAML of the MainWindow (MainWindow.xaml)
        public string Messages
        {
            get
            {
                return _messages;
            }
            set
            {
                if (_messages != value)
                {
                    _messages = value;
                    RaisePropertyChanged("Messages");
                }
            }
        }

        public string Message
        {
            get
            {
                return _message;
            }
            set
            {
                if (_message != value)
                {
                    _message = value;
                    RaisePropertyChanged("Message");
                }
            }
        }

        public Client SelectedClient
        {
            get
            {
                return _selectedClient;
            }
            set
            {
                if (_selectedClient != value)
                {
                    _selectedClient = value;
                    RaisePropertyChanged("SelectedClient");
                }
            }
        }

        #endregion Properties

        #region private variables
        // List of clients displayed in the listbox
        // The selected client from the list box
        private Client _selectedClient;
        // messages that are displayed on the message terminal (this should probably be a list of Messages, a model that needs to be created)
        private string _messages;
        // The message that the user types in to send to a specified client.
        private string _message;
        //our UDP client to talk to the server router
        private UdpClient _serverRouterClient;
        //Our table of users
        private string _myUserName;
        //Timer to ping the serverrouter for new peers
        private System.Timers.Timer _updatePeersTimer;
        //My IP
        private string _myIP;

        #endregion


        public ClientViewModel()
        {
            IPHostEntry host;
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    _myIP = ip.ToString();
                }
            }

            _serverRouterClient = new UdpClient();

            

            // Dummy Data
            Messages = "Hello\nHello2";

            // Inits
            ConnectionWindow connectWindow = new ConnectionWindow(_serverRouterClient);
            connectWindow.ShowDialog();

            Clients = connectWindow.Clients;
            _myUserName = connectWindow.UserName;

            _updatePeersTimer = new System.Timers.Timer();
            _updatePeersTimer.Interval = GET_UPDATED_USERS_TICK_RATE;
            _updatePeersTimer.Elapsed += new ElapsedEventHandler(GetUpdatedUsers);
            _updatePeersTimer.Start();

            ThreadPool.QueueUserWorkItem(ListenForConnections);
        }

        private void GetUpdatedUsers(object sender, ElapsedEventArgs e)
        {
            byte[] connectionPacket = new byte[4];

            BitConverter.GetBytes(PacketDefinitions.GET_USERS_HEADER).CopyTo(connectionPacket, 0);

            var remoteEndPoint = new IPEndPoint(IPAddress.Parse(Client.SERVER_ROUTER_IP), Client.SERVER_ROUTER_PORT);

            _serverRouterClient.Send(connectionPacket, connectionPacket.Length,
                                    remoteEndPoint);

            byte[] responseBytes = _serverRouterClient.Receive(ref remoteEndPoint);

            List<Client> incomingClients = Utility.GetClientsFromTable(Utility.GetRoutingTableFromBytes(responseBytes));

            //TODO: This sucks! really slow code in a lock. Does this even need to be locked? (I think yes because of iterators)
            lock (Clients)
            {
                foreach (Client c in Clients)
                {
                    c.Alive = false;
                }

                foreach (Client incomingCient in incomingClients)
                {
                    Client existingClient = ContainsUser(incomingCient);
                    if (existingClient != null)
                        existingClient.Alive = true;
                    else
                    {
                        incomingCient.Alive = true;
                        Clients.Add(incomingCient);
                    }
                }

                for (int i = Clients.Count - 1; i >= 0; i--)
                {
                    if(!Clients[i].Alive)
                        Clients.RemoveAt(i);
                }
                
            }

            RaisePropertyChanged("Clients");
        }

        private Client ContainsUser(Client incomingClient)
        {
            foreach (Client existingClient in Clients)
            {
                if (existingClient.UserName == incomingClient.UserName)
                    return existingClient;    
            }

            return null;
        }



        #region Methods

        public void ListenForConnections(object threadContext)
        {
            IPAddress localAddr = IPAddress.Parse(_myIP);

            TcpListener server = new TcpListener(localAddr, CLIENT_PORT);
            server.Start();

            while (true)
            {
                

                TcpClient client = server.AcceptTcpClient();

                Console.WriteLine("Accepted Connection!");

                IPEndPoint ipep = (IPEndPoint) client.Client.RemoteEndPoint;
                IPAddress ipa = ipep.Address;

                NetworkStream stream = client.GetStream();

                Byte[] message = new Byte[4096];
                int bytesRead = stream.Read(message, 0, 4096);

                MessagePacket packet;
                using (var ms = new System.IO.MemoryStream(message))
                {
                    var formatter = new BinaryFormatter();
                    packet = (MessagePacket)formatter.Deserialize(ms);
                }

                lock (Clients)
                {
                    foreach (Client c in Clients)
                    {
                        if (c.UserName == packet.UserNameFrom)
                        {
                            c.ClientSocket = client;
                            ThreadPool.QueueUserWorkItem(ListenToPeer, c);
                        }
                    }
                }

            }
        }


        public void SendMessage()
        {
            if (SelectedClient == null)
                return;

            if (SelectedClient.ClientSocket == null)
            {
                SelectedClient.ClientSocket = new TcpClient(SelectedClient.IPAddress, CLIENT_PORT);
                ThreadPool.QueueUserWorkItem(ListenToPeer, SelectedClient);
            }

            MessagePacket packet = new MessagePacket();
            packet.UserNameFrom = _myUserName;
            packet.Message = Message;

            byte[] packetBytes;

            using (var ms = new System.IO.MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, packet);
                packetBytes = ms.ToArray();
            }

            NetworkStream stream = SelectedClient.ClientSocket.GetStream();
            stream.Write(packetBytes, 0, packetBytes.Length);
        }

        public void GetUsers()
        {
            byte[] connectionPacket = new byte[4];

            BitConverter.GetBytes(PacketDefinitions.GET_USERS_HEADER).CopyTo(connectionPacket, 0);

            var remoteEndPoint = new IPEndPoint(IPAddress.Parse(Client.SERVER_ROUTER_IP), Client.SERVER_ROUTER_PORT);

            _serverRouterClient.Send(connectionPacket, connectionPacket.Length,
                                    remoteEndPoint);

            byte[] responseBytes = _serverRouterClient.Receive(ref remoteEndPoint);

            lock (Clients)
            {
                Clients = Utility.GetClientsFromTable(Utility.GetRoutingTableFromBytes(responseBytes));
            }

        }

        private void ListenToPeer(object threadContext)
        {
            Client c = threadContext as Client;

            if (c == null)
                return;

            while (true)
            {
                TcpClient client = c.ClientSocket;

                NetworkStream stream = client.GetStream();

                var message = new byte[4096];
                int bytesRead = stream.Read(message, 0, 4096);

                MessagePacket packet;
                using (var ms = new System.IO.MemoryStream(message))
                {
                    var formatter = new BinaryFormatter();
                    packet = (MessagePacket)formatter.Deserialize(ms);
                }

                lock (Clients)
                {
                    foreach (Client existingClient in Clients)
                    {
                        if (existingClient.UserName == packet.UserNameFrom)
                        {
                            existingClient.Conversation += packet.UserNameFrom + ": " +packet.Message + "\n";
                            break;
                        }
                    }
                }
            }
        }
        

        #endregion Methods


        #region Commands
        // A command is tied to a button to create the link between the button and a method
        private ICommand sendCommand;

        public ICommand SendCommand
        {
            get
            {
                if (sendCommand == null)
                {
                    sendCommand = new RelayCommand(
                        param => this.SendMessage()
                    );
                }
                return sendCommand;
            }
        }
        #endregion Commands

    }

}
