using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TCPServerRouter
{
    public class SThread
    {
        private String _inputLine, _outputLine, _destination, _address; // communication strings
	    private Socket _outSocket; // socket for communicating with a destination
        private Socket _inSocket;
	    private int ind; // indext in the routing table
        private System.Text.Decoder decoder;

	    // Constructor
	    public SThread(Socket inSock)
	    {
	        _inSocket = inSock;
            decoder = System.Text.Encoding.UTF8.GetDecoder();
	    }
	
	    // Run method (will run for each machine that connects to the ServerRouter)
	    public void Run(Object threadContext)
	    {
           try
           {
		        // Initial sends/receives
                byte[] receiveBuffer = new byte[1024];
                int receiveBtyeCount = _inSocket.Receive(receiveBuffer);// initial read (the destination for writing)
                char[] chars = new char[receiveBtyeCount];
                int charLen = decoder.GetChars(receiveBuffer, 0, receiveBtyeCount, chars, 0);
                _destination = new String(chars);

		        Console.WriteLine("Forwarding to " + _destination);

		        _inSocket.Send(System.Text.Encoding.ASCII.GetBytes("Connected to the router.")); // confirmation of connection
		
		        // waits 10 seconds to let the routing table fill with all machines' information
		        try
                {
                     Thread.Sleep(10000); 
                }
		        catch(ThreadInterruptedException ie){
                            Console.WriteLine("Thread interrupted");
		        }

                receiveBuffer = new byte[1024];
                receiveBtyeCount = _inSocket.Receive(receiveBuffer);// initial receive from router (verification of connection)
                chars = new char[receiveBtyeCount];
                charLen = decoder.GetChars(receiveBuffer, 0, receiveBtyeCount, chars, 0);
                _inputLine = new String(chars);

                Console.WriteLine("Client/Server said: " + _inputLine);
                _outputLine = _inputLine;

               lock (Program.RoutingTable)
               {
                   // loops through the routing table to find the destination
                   foreach (KeyValuePair<String, Socket> kvp in Program.RoutingTable)
                   {
                       if (_destination != (kvp.Key)) continue;

                       _outSocket = kvp.Value; // gets the socket for communication from the table
                       Console.WriteLine("Found destination: " + _destination);
                   }
               }

               if (_outSocket != null)			
                   _outSocket.Send(System.Text.Encoding.ASCII.GetBytes(_outputLine)); // writes to the destination
                        
                receiveBuffer = new byte[1024];
                receiveBtyeCount = _inSocket.Receive(receiveBuffer);// initial receive from router (verification of connection)
                chars = new char[receiveBtyeCount];
                charLen = decoder.GetChars(receiveBuffer, 0, receiveBtyeCount, chars, 0);
                _inputLine = new String(chars);
                        
                    if(_inputLine.ToLower() == "image")
                    {
                        try
                        {


                            _outputLine = _inputLine;
                            // passes the input from the machine to the output string for the destination
                            Console.WriteLine("Client/Server said: " + _inputLine);
                            if (_outSocket != null)
                            {
                                _outSocket.Send(System.Text.Encoding.ASCII.GetBytes(_outputLine));
                                // writes to the destination
                            }

                            receiveBuffer = new byte[4];
                            receiveBtyeCount = _inSocket.Receive(receiveBuffer); //Receiving the size of the image
                            int size = BitConverter.ToInt32(receiveBuffer, 0);
                            Console.WriteLine("Client/Server said: Size of File: " + size);
                            _outSocket.Send(receiveBuffer);

                            receiveBuffer = new byte[size];
                            receiveBtyeCount = _inSocket.Receive(receiveBuffer); // Receive the image
                            Console.WriteLine("Received Image from Client/Server");
                            _outSocket.Send(receiveBuffer);
                            Console.WriteLine("Sent Image to Client/Server");
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("File Transfer Complete");
                        }

                    }
                    else
                    {
                        _outputLine = _inputLine; // passes the input from the machine to the output string for the destination
                        Console.WriteLine("Client/Server said: " + _inputLine);
                        if (_outSocket != null)
                        {				
                            _outSocket.Send(System.Text.Encoding.ASCII.GetBytes(_outputLine)); // writes to the destination
                        }
                        // Communication loop	
                        while (true) 
                        {
                            receiveBuffer = new byte[1024];
                            receiveBtyeCount = _inSocket.Receive(receiveBuffer);// initial receive from router (verification of connection)
                            chars = new char[receiveBtyeCount];
                            charLen = decoder.GetChars(receiveBuffer, 0, receiveBtyeCount, chars, 0);
                            _inputLine = new String(chars);

                            if (_inputLine == "BYE.") // exit statement
                                break;
                            _outputLine = _inputLine; // passes the input from the machine to the output string for the destination

                            if (_outSocket != null)
                            {
                                _outSocket.Send(System.Text.Encoding.ASCII.GetBytes(_outputLine)); // writes to the destination
                            }

                            Console.WriteLine("Client/Server said: " + _inputLine);
                            if (_inputLine == "Bye.") // exit statement
                                break;
                        }// end while
                    }
                }// end try
                catch (IOException e) 
                {
                   Console.WriteLine("Could not listen to socket.");
                   Environment.Exit(1);
                }
	    }
    }
}
