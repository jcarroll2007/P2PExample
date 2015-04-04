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
        public Dictionary<string, IPAddress> UserTable { get; set; }


        public ClientViewModel()
        {
            UserTable = new Dictionary<string, IPAddress>();

            _serverRouterClient = new UdpClient();

            Clients = new ObservableCollection<Client>()
            {

            };



            // Dummy Data
            Messages = "Hello\nHello2";



            // Inits
            ConnectionWindow connectWindow = new ConnectionWindow(_serverRouterClient);
            connectWindow.ShowDialog();

            UserTable = connectWindow.UserTable;

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

        

        public void GetUsers()
        {
            //TODO: ping the server router for new users
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
