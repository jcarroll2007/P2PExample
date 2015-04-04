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
using P2PClient.Models;
using P2PClient.Helpers;


namespace P2PClient
{
    class ClientViewModel : ViewModelBase
    {
        #region constants
        //the port to listen for other clients on
        private const int CLIENT_PORT = 9999;
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
                    ConnectToClient(SelectedClient);
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
        

        #endregion


        public ClientViewModel()
        {
            _serverRouterClient = new UdpClient();

            // Dummy Data
            Messages = "Hello\nHello2";

            // Inits
            ConnectionWindow connectWindow = new ConnectionWindow(_serverRouterClient);
            connectWindow.ShowDialog();

            Clients = connectWindow.Clients;

            ThreadPool.QueueUserWorkItem(ListenForConnections);
        }



        #region Methods

        public void ListenForConnections(object threadContext)
        {
            int port = 13000;
            IPAddress localAddr = IPAddress.Parse("127.0.0.1");

            TcpListener server = new TcpListener(localAddr, port);
            server.Start();

            while (true)
            {
                

                TcpClient client = server.AcceptTcpClient();

                Console.WriteLine("Accepted Connection!");

                IPEndPoint ipep = (IPEndPoint) client.Client.RemoteEndPoint;
                IPAddress ipa = ipep.Address;

                NetworkStream stream = client.GetStream();

                int bytesRead;

                Byte[] message = new Byte[4096];
                bytesRead = stream.Read(message, 0, 4096);

                MessagePacket packet;
                using (var ms = new System.IO.MemoryStream(message))
                {
                    var formatter = new BinaryFormatter();
                    packet = (MessagePacket)formatter.Deserialize(ms);
                }

                foreach (Client c in Clients)
                {
                    if (c.UserName == packet.UserNameFrom)
                        c.ClientSocket = client;
                }
                
            }
        }

        public void ConnectToClient(Client client)
        {
            // TODO: connect to specific client and send data
        }

        public void SendMessage()
        {
            if (SelectedClient.ClientSocket == null)
            {
                TcpClient client = new TcpClient(SelectedClient.IPAddress, CLIENT_PORT); 
 
            }
        }



        

        public void GetUsers()
        {
            //TODO: ping the server router for new users
        }

        private void HandleConnection()
        {
            
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
