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
using P2PClient.Models;
using P2PClient.Helpers;


namespace P2PClient
{
    class ClientViewModel : ViewModelBase
    {

        #region Constatnts
        private const string SERVER_ROUTER_IP = "156.156.145.156";
        private const int SERVER_ROUTER_PORT = 55555;
        #endregion

        // List of clients displayed in the listbox
        public ObservableCollection<Client> Clients { get; set; }
        // The selected client from the list box
        private Client _selectedClient;
        // messages that are displayed on the message terminal (this should probably be a list of Messages, a model that needs to be created)
        private string _messages;
        // The message that the user types in to send to a specified client.
        private string _message;
        //our UDP client to talk to the server router
        private UdpClient _serverRouterClient;
        //Our table of users
        private Dictionary<string, IPAddress> _userTable;


        public ClientViewModel()
        {
            _userTable = new Dictionary<string, IPAddress>();

            _serverRouterClient = new UdpClient(SERVER_ROUTER_IP, SERVER_ROUTER_PORT);

            Clients = new ObservableCollection<Client>()
            {
                new Client("156.156.156.156")
            };

            //Dummy clients
            Client c = new Client("156.156.156.156");
            Clients.Add(c);
            c = new Client("157.157.157.157");
            Clients.Add(c);
            c = new Client("158.158.158.158");
            Clients.Add(c);

            // Dummy Data
            Messages = "Hello\nHello2";



            // Inits
            ConnectToRouter();
            ListenForConnections();
        }

        #region Properties
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
                    RaisePropertyChanged("Message");
                    ConnectToClient(SelectedClient);
                }
            }
        }
        #endregion Properties

        #region Methods

        public void ListenForConnections()
        {
            // TODO: Start a new thread and socket to listen for connections from other clients
        }

        public void ConnectToClient(Client client)
        {
            MessageBox.Show(client.IPAddress);
            // TODO: connect to specific client and send data
        }

        public void SendMessage()
        {
            MessageBox.Show(Message);
            // TODO: Send message
        }

        public void ConnectToRouter(string userName)
        {
            byte[] connectionPacket = new byte[4 + userName.Length * 2];

            BitConverter.GetBytes(userName.Length).CopyTo(connectionPacket, 0);
            Encoding.ASCII.GetBytes(userName).CopyTo(connectionPacket, 4);

            var remoteEndPoint = new IPEndPoint(IPAddress.Parse(SERVER_ROUTER_IP), SERVER_ROUTER_PORT);

            _serverRouterClient.Send(connectionPacket, connectionPacket.Length,
                                    remoteEndPoint);

            byte[] responseBytes = _serverRouterClient.Receive(ref remoteEndPoint);

            if (responseBytes.Length == 1)
            {
                //TODO: That user name already exists!
            }
            else
            {
                _userTable.Merge(GetRoutingTableFromBytes(responseBytes));
            }

        }

        public void GetUsers()
        {
            
        }

        

        #endregion Methods

        #region Helper Methods
        private Dictionary<string, IPAddress> GetRoutingTableFromBytes(byte[] bytes)
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
