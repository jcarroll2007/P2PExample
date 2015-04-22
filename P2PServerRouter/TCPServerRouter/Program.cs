using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TCPServerRouter
{
    public class Program
    {
        public static Dictionary<String, Socket> RoutingTable = new Dictionary<String, Socket>();
        public static void Main(string[] args)
        {
            int SOCKET_NUMBER = 5555; // port number

            Socket clientSocket = null; // socket for the thread
            TcpListener serverSocket = new TcpListener(SOCKET_NUMBER);
            serverSocket.Start();
            while (true)
            {
                try
                {
                    clientSocket = serverSocket.AcceptSocket();

                    IPEndPoint remoteIpEndPoint = clientSocket.RemoteEndPoint as IPEndPoint;

                    Console.WriteLine("ServerRouter connected with Client/Server: " + remoteIpEndPoint.Address);

                    RoutingTable.Add(remoteIpEndPoint.Address.ToString(), clientSocket);

                    SThread t = new SThread(clientSocket);

                    ThreadPool.QueueUserWorkItem(t.Run);
                    
                   
                    
                }
                catch (IOException e) 
                {
                   Console.WriteLine("Client/Server failed to connect.");
                    break;
                }
            }//end while
			
	        //closing connections
            clientSocket.Close();
            serverSocket.EndAcceptTcpClient(null);
        }

       
    }
}
