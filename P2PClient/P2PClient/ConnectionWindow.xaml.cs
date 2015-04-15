using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using P2PClient.Models;

namespace P2PClient
{
    /// <summary>
    /// Interaction logic for ConnectionWindow.xaml
    /// </summary>
    public partial class ConnectionWindow : Window
    {
        private UdpClient _serverRouterClient;

        public ObservableCollection<Client> Clients { get; set; }


        public string ServerRouterIP { get; set; }
        public string ClientPort { get; set; }
        public string UserName { get; set; }


        public ConnectionWindow(UdpClient client)
        {
            InitializeComponent();
            _serverRouterClient = client;
            RouterIPBox.Text = "172.16.20.21";
            ClientPortBox.Text = "55555";
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ConnectToRouter(UsernameTextBox.Text);
        }

        public void ConnectToRouter(string userName)
        {
            byte[] connectionPacket = new byte[4 + 4 + userName.Length * 2];

            ServerRouterIP = RouterIPBox.Text;
            ClientPort = ClientPortBox.Text;

            BitConverter.GetBytes(PacketDefinitions.NEW_CONNECTION_HEADER).CopyTo(connectionPacket, 0);
            BitConverter.GetBytes(userName.Length).CopyTo(connectionPacket, 4);
            Encoding.ASCII.GetBytes(userName).CopyTo(connectionPacket, 8);

            var remoteEndPoint = new IPEndPoint(IPAddress.Parse(ServerRouterIP), Client.SERVER_ROUTER_PORT);

            _serverRouterClient.Send(connectionPacket, connectionPacket.Length,
                                    remoteEndPoint);

            byte[] responseBytes = _serverRouterClient.Receive(ref remoteEndPoint);

            if (responseBytes.Length == 1)
            {
                ErrorLabel.Visibility = Visibility.Visible;
            }
            else
            {
                Clients = Utility.GetClientsFromTable(Utility.GetRoutingTableFromBytes(responseBytes));
                UserName = UsernameTextBox.Text;
                this.Close();
            }

        }

        

        
    }
}
