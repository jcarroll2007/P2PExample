﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Timers;
using System.Windows.Data;
using System.Windows.Threading;
using P2PClient.Models;
using P2PClient.Helpers;


namespace P2PClient
{
    public delegate void MessageHandler(object sender, Client client);

    class ClientViewModel : ViewModelBase
    {
        #region constants
        //the port to listen for other clients on
        public int ClientPort = 9999;
        //How often do we update our client table from the server router?
        private const int GET_UPDATED_USERS_TICK_RATE = 1000;
        #endregion

        #region Properties

        //Dictionary of the users to their IP addresses
        public ObservableCollection<Client> Clients { get; set; }

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

        private string _serverRouterIP;
       

        #endregion

        #region events

        public event MessageHandler NewConnection;

        public void OnNewMessage(Client client)
        {
            if (NewConnection != null)
                NewConnection(this, client);
        }

        #endregion

        #region Constructor
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
            _serverRouterIP = connectWindow.ServerRouterIP;
            ClientPort = int.Parse(connectWindow.ClientPort);

            _updatePeersTimer = new System.Timers.Timer();
            _updatePeersTimer.Interval = GET_UPDATED_USERS_TICK_RATE;
            _updatePeersTimer.Elapsed += new ElapsedEventHandler(GetUpdatedUsers);
            _updatePeersTimer.Start();

            ThreadPool.QueueUserWorkItem(ListenForConnections);
        }

        #endregion

        #region Private Methods

        private void GetUpdatedUsers(object sender, ElapsedEventArgs e)
        {
            byte[] connectionPacket = new byte[4 + 4 + _myUserName.Length * 2];

            BitConverter.GetBytes(PacketDefinitions.GET_USERS_HEADER).CopyTo(connectionPacket, 0);
            BitConverter.GetBytes(_myUserName.Length).CopyTo(connectionPacket, 4);
            Encoding.ASCII.GetBytes(_myUserName).CopyTo(connectionPacket, 8);

            var remoteEndPoint = new IPEndPoint(IPAddress.Parse(_serverRouterIP), Client.SERVER_ROUTER_PORT);
            byte[] responseBytes;

            using (StreamWriter file = new StreamWriter(Directory.GetCurrentDirectory() + "\\UpdateUsersTime.txt", true))
            {
                Stopwatch s = new Stopwatch();

                s.Start();
                _serverRouterClient.Send(connectionPacket, connectionPacket.Length,
                    remoteEndPoint);

                responseBytes = _serverRouterClient.Receive(ref remoteEndPoint);
                s.Stop();
                file.WriteLine((double)s.ElapsedTicks / Stopwatch.Frequency * 1000000000.0 + ",");
            }

            ObservableCollection<Client> incomingClients = Utility.GetClientsFromTable(Utility.GetRoutingTableFromBytes(responseBytes));

            //TODO: This sucks! really slow code in a lock. Does this even need to be locked? (I think yes because of iterators)
            lock (Clients)
            {
                foreach (Client c in Clients)
                {
                    c.Alive = false;
                }

                foreach (Client incomingCient in incomingClients)
                {
                    if (incomingCient.UserName == _myUserName)
                        continue;
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

        #endregion

        #region Public Methods

        public void ListenForConnections(object threadContext)
        {
            IPAddress localAddr = IPAddress.Parse(_myIP);

            TcpListener server = new TcpListener(localAddr, ClientPort);
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
                            c.Conversation += packet.UserNameFrom + ": " + packet.Message + "\n";
                            ThreadPool.QueueUserWorkItem(ListenToPeer, c);
                            OnNewMessage(c);
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
                SelectedClient.ClientSocket = new TcpClient(SelectedClient.IPAddress, ClientPort);
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

            using (StreamWriter file = new StreamWriter(Directory.GetCurrentDirectory() + "\\UpdateSendTime.txt", true))
            {
                Stopwatch s = new Stopwatch();

                s.Start();
                stream.Write(packetBytes, 0, packetBytes.Length);
                s.Stop();
                file.WriteLine((double)s.ElapsedTicks / Stopwatch.Frequency * 1000000000.0 + ",");
            }

            SelectedClient.Conversation += _myUserName + ": " + Message + "\n";
            OnNewMessage(SelectedClient);
            Message = "";
        }

        public void GetUsers()
        {
            byte[] connectionPacket = new byte[4 + 4 + _myUserName.Length * 2];

            BitConverter.GetBytes(PacketDefinitions.GET_USERS_HEADER).CopyTo(connectionPacket, 0);
            BitConverter.GetBytes(_myUserName.Length).CopyTo(connectionPacket, 4);
            Encoding.ASCII.GetBytes(_myUserName).CopyTo(connectionPacket, 8);

            var remoteEndPoint = new IPEndPoint(IPAddress.Parse(_serverRouterIP), Client.SERVER_ROUTER_PORT);

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
                try
                {
                    TcpClient client = c.ClientSocket;

                    NetworkStream stream = client.GetStream();

                    var message = new byte[4096];
                    int bytesRead = stream.Read(message, 0, 4096);

                    MessagePacket packet;
                    using (var ms = new System.IO.MemoryStream(message))
                    {
                        var formatter = new BinaryFormatter();
                        packet = (MessagePacket) formatter.Deserialize(ms);
                    }

                    //TODO: Can we just do c.Conversation += data?
                    lock (Clients)
                    {
                        foreach (Client existingClient in Clients)
                        {
                            if (existingClient.UserName == packet.UserNameFrom)
                            {
                                existingClient.Conversation += packet.UserNameFrom + ": " + packet.Message + "\n";
                                OnNewMessage(existingClient);
                                break;
                            }
                        }
                    }
                }
                catch (IOException e)
                {
                    lock (Clients)
                    {
                        foreach (Client existingClient in Clients)
                        {
                            if (existingClient.UserName == c.UserName)
                            {
                                existingClient.Conversation += "-----Client Disconnected----- \n";
                                OnNewMessage(existingClient);
                                break;
                            }
                        }
                    }

                    break;
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
