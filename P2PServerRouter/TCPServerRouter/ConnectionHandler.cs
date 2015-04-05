using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace TCPServerRouter
{


    public class ConnectionHandler
    {

        #region Message Headers

        private const int NEW_CONNECTION = 0;
        private const int GET_USERS = 1;

        #endregion

        #region Public Data

        public static List<IPAddress> ServerRouters = new List<IPAddress>() {IPAddress.Parse("172.16.20.21")};
        public static DataContractSerializer Serializer = new DataContractSerializer(typeof(List<Dictionary<string, IPAddress>>));
        public static Dictionary<string, IPAddress> RoutingTable = new Dictionary<string, IPAddress>();

        #endregion

        #region Client Listener
        public static void ListenForClient(object threadContext)
        {
            UdpClient udpServer = new UdpClient(ServerRouter.CLIENT_LISTEN_PORT);
            
            while (true)
            {
                try
                {
                    #region Listen
                    var remoteEndPoint = new IPEndPoint(IPAddress.Any, ServerRouter.CLIENT_LISTEN_PORT);
                    var data = udpServer.Receive(ref remoteEndPoint); //Listen for packets
                    Console.WriteLine("Received Data From Client: " + remoteEndPoint.Address);
                    #endregion


                    //TODO: Thread the response? Do we need to do that? Will there be that many new connections?
                    #region Response

                    int header = BitConverter.ToInt32(data, 0);

                    switch (header)
                    {
                        #region New Connections
                        case NEW_CONNECTION:
                            int userNameLength = BitConverter.ToInt32(data, 4);
                            string userName = Encoding.ASCII.GetString(data, 8, userNameLength);

                            Dictionary<string, IPAddress> totalRoutingTale = GetTotalRoutingTable();

                            if (totalRoutingTale.ContainsKey(userName))
                            {
                                byte[] userNameExistsPacket = new byte[1];

                                userNameExistsPacket[0] = 255;

                                udpServer.Send(userNameExistsPacket, 1, remoteEndPoint);
                            }
                            else
                            {
                                lock (RoutingTable)
                                {
                                    RoutingTable.Add(userName, remoteEndPoint.Address);
                                }
                                byte[] currentRoutingTableResponsePacket = GetRoutingTableBytes(totalRoutingTale);

                                udpServer.Send(currentRoutingTableResponsePacket, currentRoutingTableResponsePacket.Length,
                                    remoteEndPoint);
                            }
                            break;
                        #endregion

                        case GET_USERS:
                            byte[] routingTableResponsePacket = GetRoutingTableBytes(GetTotalRoutingTable());

                            udpServer.Send(routingTableResponsePacket, routingTableResponsePacket.Length,
                                    remoteEndPoint);
                            break;
                    }

                    

                    #endregion

                }
                catch (IOException e)
                {
                    Console.WriteLine("Client/Server failed to connect.");
                    Environment.Exit(1);
                }
            }//end while
        }

        private static Dictionary<string, IPAddress> GetTotalRoutingTable()
        {
            //Our routing table with all other server routers tables that we know about
            Dictionary<string, IPAddress> totalRoutingTable = new Dictionary<string, IPAddress>();
            lock (RoutingTable)
            {
                totalRoutingTable.Merge(RoutingTable);
            }
            foreach (IPAddress ip in ServerRouters)
            {
                UdpClient client = new UdpClient(ServerRouter.SERVER_LISTEN_PORT);
                var serverRouterEndPoint = new IPEndPoint(ip, ServerRouter.SERVER_LISTEN_PORT);
                client.Send(new byte[] { 128 }, 1, serverRouterEndPoint);

                byte[] tableBytes = client.Receive(ref serverRouterEndPoint);

                totalRoutingTable.Merge(GetRoutingTableFromBytes(tableBytes));
            }

            return totalRoutingTable;
        }

        #endregion

        #region ServerListen
        internal static void ListenForServer(object threadContext)
        {
            UdpClient udpServer = new UdpClient(ServerRouter.SERVER_LISTEN_PORT);

            while (true)
            {
                #region Listen
                var remoteEndPoint = new IPEndPoint(IPAddress.Any, ServerRouter.CLIENT_LISTEN_PORT);
                var data = udpServer.Receive(ref remoteEndPoint); //Listen for packets
                Console.WriteLine("Received Data From ServerRouter: " + remoteEndPoint.Address);
                #endregion

                #region Response
                //Reply to packets on this port with our routing table
                byte[] routingTablePacket = GetRoutingTableBytes(RoutingTable);
                udpServer.Send(routingTablePacket, routingTablePacket.Length, remoteEndPoint);
                #endregion
            }
        }
        #endregion

        #region Serialization Helpers
        public static byte[] GetRoutingTableBytes(Dictionary<string, IPAddress> table)
        {
            byte[] byteArr;

            using (var ms = new System.IO.MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, table);
                byteArr = ms.ToArray();
            }
            return byteArr;
        }

        public static Dictionary<string, IPAddress> GetRoutingTableFromBytes(byte[] bytes)
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
    }
}
