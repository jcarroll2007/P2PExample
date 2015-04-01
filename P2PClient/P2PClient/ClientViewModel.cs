using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using System.Collections.ObjectModel;
using P2PClient.Models;
using P2PClient.Helpers;


namespace P2PClient
{
    class ClientViewModel : ViewModelBase
    {

        // List of clients displayed in the listbox
        public ObservableCollection<Client> Clients { get; set; }
        // The selected client from the list box
        private Client selectedClient;
        // messages that are displayed on the message terminal (this should probably be a list of Messages, a model that needs to be created)
        private string messages;
        // The message that the user types in to send to a specified client.
        private string message;

        public ClientViewModel()
        {
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
                return messages;
            }
            set
            {
                if (messages != value)
                {
                    messages = value;
                    RaisePropertyChanged("Messages");
                }
            }
        }

        public string Message
        {
            get
            {
                return message;
            }
            set
            {
                if (message != value)
                {
                    message = value;
                    RaisePropertyChanged("Message");
                }
            }
        }

        public Client SelectedClient
        {
            get
            {
                return selectedClient;
            }
            set
            {
                if (selectedClient != value)
                {
                    selectedClient = value;
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

        public void ConnectToRouter()
        {
            // TODO: Connect to router and request client ips
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
