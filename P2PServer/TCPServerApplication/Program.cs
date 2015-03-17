using System;
using System.Collections.Generic;
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
            String routerName = "172.16.20.21"; // ServerRouter host name
            int portNum = 5555; // port number
            String address ="172.16.20.121"; // destination IP (Client)
            String imageLocation = "C:\\Users\\Cprice\\Desktop\\image.jpg";
            
            // Variables for setting up connection and communication
            Socket Socket = null; // socket to connect with ServerRouter
            IPAddress addr = InetAddress.getLocalHost();
            String host = addr.getHostAddress(); // Server machine's IP		

            PrintWriter out = null; // for writing to ServerRouter
            BufferedReader in = null; // for reading form ServerRouter
            DataInputStream dis = null;
            DataOutputStream os = null;
            	
            FileOutputStream fout = new FileOutputStream(imageLocation);
			
            // Tries to connect to the ServerRouter
            try 
            {
                Socket = new Socket(routerName, portNum);
                Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                Socket.Connect(routerName, portNum);


                out = new PrintWriter(Socket.getOutputStream(), true);
                in = new BufferedReader(new InputStreamReader(Socket.getInputStream()));
                dis = new DataInputStream(Socket.getInputStream());
                os = new DataOutputStream(Socket.getOutputStream());
            } 
            catch (UnknownHostException e) 
            {
               System.err.println("Don't know about router: " + routerName);
               System.exit(1);
            } 
            catch (IOException e) {
               System.err.println("Couldn't get I/O for the connection to: " + routerName);
               System.exit(1);
            }
				
            // Variables for message passing			
            String fromServer; // messages sent to ServerRouter
            String fromClient; // messages received from ServerRouter      
            
			
            // Communication process (initial sends/receives)
            out.println(address);// initial send (IP of the destination Client)
            fromClient = in.readLine();// initial receive from router (verification of connection)
            System.out.println("ServerRouter: " + fromClient);
            
            fromClient = in.readLine();// initial receive from client 
            System.out.println("Client said: " + fromClient);

            fromServer = fromClient.toUpperCase(); // converting received message to upper case
            System.out.println("Server said: " + fromServer);
            out.println(fromServer); // sending the converted message back to the Client via ServerRouter
            
            if((fromClient = in.readLine()).equals("image"))
            {
                fromServer = fromClient.toUpperCase(); // converting received message to upper case
                System.out.println("Server said: " + fromServer);
                out.println(fromServer); // sending the converted message back to the Client via ServerRouter
            
                int i;
                // Communication while loop
                while ((i = dis.read()) > -1) 
                {
                    System.out.println("Client said: " + i);
                     fout.write(i);
                     System.out.println("Server said: " + i);
                     os.write(i);
                }
            }
            else
            {
                fromServer = fromClient.toUpperCase(); // converting received message to upper case
                System.out.println("Server said: " + fromServer);
                out.println(fromServer); // sending the converted message back to the Client via ServerRouter
            
                // Communication while loop
                while ((fromClient = in.readLine()) != null) 
                {
                    System.out.println("Client said: " + fromClient);
                    if (fromClient.equals("Bye.")) // exit statement
                        break;

                    fromServer = fromClient.toUpperCase(); // converting received message to upper case
                    System.out.println("Server said: " + fromServer);
                    out.println(fromServer); // sending the converted message back to the Client via ServerRouter
                }
            }
            
            
            
			
	// closing connections
        out.close();
        in.close();
        dis.close();
        Socket.close();
        }
    }
}
