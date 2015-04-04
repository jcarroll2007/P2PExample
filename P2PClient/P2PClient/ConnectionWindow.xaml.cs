using System;
using System.Collections.Generic;
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
        public Dictionary<string, IPAddress> UserTable { get; set; } 

        public ConnectionWindow(UdpClient client)
        {
            InitializeComponent();
            _serverRouterClient = client;
            UserTable = new Dictionary<string, IPAddress>();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ConnectToRouter(UsernameTextBox.Text);
        }

        public void ConnectToRouter(string userName)
        {
            byte[] connectionPacket = new byte[4 + 4 + userName.Length * 2];

            BitConverter.GetBytes(PacketDefinitions.NEW_CONNECTION_HEADER).CopyTo(connectionPacket, 0);
            BitConverter.GetBytes(userName.Length).CopyTo(connectionPacket, 4);
            Encoding.ASCII.GetBytes(userName).CopyTo(connectionPacket, 8);

            var remoteEndPoint = new IPEndPoint(IPAddress.Parse(Client.SERVER_ROUTER_IP), Client.SERVER_ROUTER_PORT);

            _serverRouterClient.Send(connectionPacket, connectionPacket.Length,
                                    remoteEndPoint);

            byte[] responseBytes = _serverRouterClient.Receive(ref remoteEndPoint);

            if (responseBytes.Length == 1)
            {
                ErrorLabel.Visibility = Visibility.Visible;
            }
            else
            {
                UserTable.Merge(Utility.GetRoutingTableFromBytes(responseBytes));
                this.Close();
            }

        }
    }
}
