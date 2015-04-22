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
        public static void Main(string[] args)
        {
            int SOCKET_NUMBER = 5555; // port number
            int ROUTING_TABLE_SIZE = 10;

            Socket clientSocket = null; // socket for the thread
            Object[,] RoutingTable = new Object[ROUTING_TABLE_SIZE, 2]; // routing table
            int ind = 0; // indext in the routing table	
            //Accepting connections
            TcpListener serverSocket = new TcpListener(SOCKET_NUMBER);
            serverSocket.Start();
			
            // Creating threads with accepted connections
            while (true)
            {
                try
                {
                    clientSocket = serverSocket.AcceptSocket();
                    
                    SThread t = new SThread(RoutingTable, clientSocket, ind); // creates a thread with a random port
                    ThreadPool.QueueUserWorkItem(t.Run);
                    
                    
                    ind++; // increments the index
                    IPEndPoint remoteIpEndPoint = clientSocket.RemoteEndPoint as IPEndPoint;

                    Console.WriteLine("ServerRouter connected with Client/Server: " + remoteIpEndPoint.Address);
                }
                catch (IOException e) 
                {
                   Console.WriteLine("Client/Server failed to connect.");
                   Environment.Exit(1);
                }
            }//end while
			
	    //closing connections
            clientSocket.Close();
            serverSocket.EndAcceptTcpClient(null);
        }

       
    }
}
