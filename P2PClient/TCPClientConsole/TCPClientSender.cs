using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace P2PClient
{
    public class TCPClientSender
    {
        public string RouterName { get; set; }
        public string ServerName { get; set; }
        public int Port { get; set; }
        public string LogPath { get; set; }

        private Decoder _decoder;
        private Socket _socket;

        public TCPClientSender()
        {
            RouterName = "";
            ServerName = "";
            Port = 5555;
            LogPath = Directory.GetCurrentDirectory() + "\\EventLog.txt";
            _decoder = Encoding.UTF8.GetDecoder();
        }

        public TCPClientSender(String routerName, String serverName, int port)
        {
            RouterName = routerName;
            ServerName = serverName;
            Port = port;
            LogPath = Directory.GetCurrentDirectory() + "\\EventLog.txt";
            _decoder = Encoding.UTF8.GetDecoder();
        }

        public void SendImage(String file)
        {
            String host = Dns.GetHostName();
            StreamWriter logFileWriter = new StreamWriter(LogPath);
            logFileWriter.Write("New Run for file: " + file + "\n");
			
            // Tries to connect to the ServerRouter
            try 
            {
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPAddress ipAddress = IPAddress.Parse(RouterName);
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, Port);
                _socket.Connect(remoteEP);
            } 
             catch (SocketException e) 
            {
               Console.WriteLine("Could not connect to router: " + RouterName);
               Environment.Exit(1);
            } 
            catch (IOException e) {
               Console.WriteLine("Couldn't get I/O for the connection to: " + RouterName);
               Environment.Exit(1);
            }

            String fromServer; // messages received from ServerRouter
            String fromUser; // messages sent to ServerRouter


            // Communication process (initial sends/receives
            long initialSend, initialTotalTime, t0, t1, t=0;

            initialSend =  DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;;
            // Communication process (initial sends/receives
            SendString(ServerName);// initial send (IP of the destination Server)
            fromServer = ReceiveString();//initial receive from router (verification of connection)
            initialTotalTime =  DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond - initialSend;
            Console.WriteLine("ServerRouter: " + fromServer);
            SendString(host); // Client sends the IP of its machine as initial send
            t0 =  DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

            fromServer = ReceiveString();
            Console.WriteLine("Server: " + fromServer);

            SendString("image"); // Client sends the IP of its machine as initial send
            fromServer = ReceiveString();

            Console.WriteLine("Server: " + fromServer);
            long cycle = 0;
            long startFileSend,endFileSend,totalFileSend=0;

            startFileSend = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

            Image img = new Bitmap(file);
            byte[] imgBytes = ImageToByte(img);

            //Sending File Size
            _socket.Send(BitConverter.GetBytes(imgBytes.Length));
            //Wait for response for file size
            fromServer = ReceiveString();
            Console.WriteLine("Server: " + fromServer);
            //Send the actual image
            _socket.Send(imgBytes);
            //Wait for Response from server
            //fromServer = ReceiveString();
            //Console.WriteLine("Server: " + fromServer);

            endFileSend =  DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            totalFileSend = endFileSend - startFileSend;

            Console.WriteLine("File send time: " + totalFileSend + " ns\n");
            logFileWriter.Write("File send time: " + totalFileSend + " ns\n");

            logFileWriter.Write("\n");
            logFileWriter.Write("\n");

            logFileWriter.Close(); 

            // closing connections
            _socket.Close();
        }
        
        public void SendFile(String file)
        {
      	
            // Variables for setting up connection and communication

            String host = "";
            IPHostEntry hostip = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in hostip.AddressList)
            {
                if (ip.AddressFamily.ToString() == "InterNetwork")
                {
                    host = ip.ToString();
                }
            }

            StreamWriter logFileWriter = new StreamWriter(LogPath);
            logFileWriter.Write("New Run for file: " + file + "\n");
			
            // Tries to connect to the ServerRouter
            try 
            {
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPAddress ipAddress = IPAddress.Parse(RouterName);
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, Port);
                _socket.Connect(remoteEP);
            } 
             catch (SocketException e) 
            {
               Console.WriteLine("Could not connect to router: " + RouterName);
               Environment.Exit(1);
            } 
            catch (IOException e) {
               Console.WriteLine("Couldn't get I/O for the connection to: " + RouterName);
               Environment.Exit(1);
            }
				
            // Variables for message passing	
            StreamReader reader = new StreamReader(file); 
            String fromServer; // messages received from ServerRouter
            String fromUser; // messages sent to ServerRouter

            long t=0;

            // Communication process (initial sends/receives
            long initialSend,initialTotalTime;
            initialSend = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

            _socket.Send(Encoding.ASCII.GetBytes(ServerName)); // initial send (IP of the destination Server)

            fromServer = ReceiveString();


            initialTotalTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond - initialSend;
            Console.WriteLine("ServerRouter: " + fromServer);
            Console.WriteLine("Initial Communication Time: " + initialTotalTime + " ms");
            logFileWriter.Write("Initial Communication Time: " + initialTotalTime + " ms \n");
            
            Console.WriteLine("Sending: " + host);
            _socket.Send(Encoding.ASCII.GetBytes(host)); // Client sends the IP of its machine as initial send


            fromServer = ReceiveString();

            Console.WriteLine("Server: " + fromServer);
            long t0 = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            
            _socket.Send(System.Text.Encoding.ASCII.GetBytes("text")); // Client sends file type
            
            long cycle = 0;
            long startFileSend,endFileSend,totalFileSend=0;
            
            Console.WriteLine("Waiting for response: ");
            while (true) 
            {
                fromServer = ReceiveString();

                Console.WriteLine("Server: " + fromServer);
                
                long t1 = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
                
                t0  = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
                t+=t0-t1; //calcultates overall recieve time
                cycle++;
                if (fromServer == "Bye.") // exit statement
                   break;

                Console.WriteLine("Cycle time: " + t);
                logFileWriter.Write("Cycle time: " + t + "\n");
                
                startFileSend =  DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
                fromUser = reader.ReadLine(); // reading strings from a file

                if (fromUser != null) 
                {
                   Console.WriteLine("Client: " + fromUser);
                   _socket.Send(Encoding.ASCII.GetBytes(fromUser)); // sending the strings to the Server via ServerRouter
                   endFileSend = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
                   totalFileSend += endFileSend - startFileSend;

                    if(fromUser == "Bye.")
                    break;
                }
                else
                    break;
            }
            
            
            Console.WriteLine("Server cycle time: " + t + "ms  Number of Cycles: " + cycle + "\n");
            logFileWriter.Write("Server cycle time: " + t + "ms  Number of Cycles: " + cycle + "\n");
            Console.WriteLine("File send time: " + totalFileSend + " ms\n");
            logFileWriter.Write("File send time: " + totalFileSend + " ms\n");
            
            logFileWriter.Write("\n");
            logFileWriter.Write("\n");

            logFileWriter.Close();

            _socket.Send(Encoding.ASCII.GetBytes("Bye."));
            // closing connections
            _socket.Close();
        }

        private string ReceiveString()
        {
            byte[] receiveBuffer = new byte[1024];
            int receiveBtyeCount = _socket.Receive(receiveBuffer);// initial receive from router (verification of connection)
            char[] chars = new char[receiveBtyeCount];
            int charLen = _decoder.GetChars(receiveBuffer, 0, receiveBtyeCount, chars, 0);
            return new String(chars); //initial receive from router (verification of connection)
        }

        public static byte[] ImageToByte(Image img)
        {
            ImageConverter converter = new ImageConverter();
            return (byte[])converter.ConvertTo(img, typeof(byte[]));
        }

        public void SendString(string toSend)
        {
            _socket.Send(Encoding.ASCII.GetBytes(toSend));
        }
    }
}
