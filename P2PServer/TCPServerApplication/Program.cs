using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TCPServerApplication
{
    class Program
    {
        public static void Main(string[] args)
        {
            const String ROUTER_NAME = "172.16.20.21"; // ServerRouter host name
            const int PORT_NUM = 5555; // port number
            const String ADDRESS ="172.16.20.121"; // destination IP (Client)
            const String IMAGE_LOCATION = "C:\\Users\\Cprice\\Desktop\\image.jpg";
            
            // Varibles for setting up connection and communication
            Socket socket = null; // socket to connect with ServerRouter
			System.Text.Decoder decoder = System.Text.Encoding.UTF8.GetDecoder();

            // Tries to connect to the ServerRouter
            try 
            {
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPAddress ipAddress = IPAddress.Parse(ROUTER_NAME);
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, PORT_NUM);
                socket.Connect(remoteEP);
            } 
            catch (SocketException e) 
            {
               Console.WriteLine("Could not connect to router: " + ROUTER_NAME);
               Environment.Exit(1);
            } 
            catch (IOException e) {
               Console.WriteLine("Couldn't get I/O for the connection to: " + ROUTER_NAME);
               Environment.Exit(1);
            }
				
            // Variables for message passing			
            String fromServer; // messages sent to ServerRouter
            String fromClient; // messages received from ServerRouter      
            
			
            // Communication process (initial sends/receives)
            socket.Send(System.Text.Encoding.ASCII.GetBytes(ADDRESS));// initial send (IP of the destination Client)

            byte[] receiveBuffer = new byte[1024];
            int receiveBtyeCount = socket.Receive(receiveBuffer);// initial receive from router (verification of connection)
            char[] chars = new char[receiveBtyeCount];
            int charLen = decoder.GetChars(receiveBuffer, 0, receiveBtyeCount, chars, 0);
            fromClient = new String(chars);

            Console.WriteLine("ServerRouter: " + fromClient);
            

            receiveBuffer = new byte[1024];
            receiveBtyeCount = socket.Receive(receiveBuffer);// initial receive from router (verification of connection)
            chars = new char[receiveBtyeCount];
            charLen = decoder.GetChars(receiveBuffer, 0, receiveBtyeCount, chars, 0);
            fromClient = new String(chars);

            Console.WriteLine("Client said: " + fromClient);

            fromServer = fromClient.ToUpper(); // converting received message to upper case

            Console.WriteLine("Server said: " + fromServer);
            socket.Send(System.Text.Encoding.ASCII.GetBytes(fromServer)); // sending the converted message back to the Client via ServerRouter
            
            receiveBuffer = new byte[1024];
            receiveBtyeCount = socket.Receive(receiveBuffer);// Rec from client for file type
            chars = new char[receiveBtyeCount];
            charLen = decoder.GetChars(receiveBuffer, 0, receiveBtyeCount, chars, 0);
            fromClient = new String(chars);

            if(fromClient == "image")
            {
                
                fromServer = fromClient.ToUpper(); // converting received message to upper case
                Console.WriteLine("Server said: " + fromServer);
                socket.Send(System.Text.Encoding.ASCII.GetBytes(fromServer)); // sending the converted message back to the Client via ServerRouter

                receiveBuffer = new byte[4];
                receiveBtyeCount = socket.Receive(receiveBuffer); //Receiving the size of the image
                int size = BitConverter.ToInt32(receiveBuffer, 0);
            
                receiveBuffer = new byte[size];
                receiveBtyeCount = socket.Receive(receiveBuffer);// Receive the image

                MemoryStream ms = new MemoryStream(receiveBuffer);
                Bitmap img = new Bitmap(ms);
                img.Save(Directory.GetCurrentDirectory() + "\\ReceivedImage.png");
            }
            else
            {
                fromServer = fromClient.ToUpper(); // converting received message to upper case
                Console.WriteLine("Server said: " + fromServer);
               socket.Send(System.Text.Encoding.ASCII.GetBytes(fromServer)); // sending the converted message back to the Client via ServerRouter

                // Communication while loop
                while (true) 
                {
                    receiveBuffer = new byte[1024];
                    receiveBtyeCount = socket.Receive(receiveBuffer);// Receive from client a line of text
                    chars = new char[receiveBtyeCount];
                    charLen = decoder.GetChars(receiveBuffer, 0, receiveBtyeCount, chars, 0);
                    fromClient = new String(chars);

                    Console.WriteLine("Client said: " + fromClient);
                    if (fromClient == "Bye.") // exit statement
                        break;

                    fromServer = fromClient.ToUpper(); // converting received message to upper case
                    Console.WriteLine("Server said: " + fromServer);
                    socket.Send(System.Text.Encoding.ASCII.GetBytes(fromServer)); // sending the converted message back to the Client via ServerRouter
                }
            }
            
            
            
			
	    // closing connections
        socket.Close();
        }
    }
}
