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
        private Object [,] RTable; // routing table
	    private String inputLine, outputLine, destination, addr; // communication strings
	    private Socket outSocket; // socket for communicating with a destination
        private Socket inSocket;
	    private int ind; // indext in the routing table
        private System.Text.Decoder decoder;

	    // Constructor
	    public SThread(Object [,] Table, Socket toClient, int index)
	    {
                RTable = Table;
                RTable[index, 0] = addr; // IP addresses 
                RTable[index, 1] = toClient; // sockets for communication
	            inSocket = toClient;
                ind = index;
                decoder = System.Text.Encoding.UTF8.GetDecoder();
	    }
	
	    // Run method (will run for each machine that connects to the ServerRouter)
	    public void Run(Object threadContext)
	    {
           try
           {
		        // Initial sends/receives
                byte[] receiveBuffer = new byte[1024];
                int receiveBtyeCount = inSocket.Receive(receiveBuffer);// initial read (the destination for writing)
                char[] chars = new char[receiveBtyeCount];
                int charLen = decoder.GetChars(receiveBuffer, 0, receiveBtyeCount, chars, 0);
                destination = new String(chars);

		        Console.WriteLine("Forwarding to " + destination);

		        inSocket.Send(System.Text.Encoding.ASCII.GetBytes("Connected to the router.")); // confirmation of connection
		
		        // waits 10 seconds to let the routing table fill with all machines' information
		        try
                {
                     Thread.Sleep(10000); 
                }
		        catch(ThreadInterruptedException ie){
                            Console.WriteLine("Thread interrupted");
		        }
		
		        // loops through the routing table to find the destination
		        for ( int i=0; i < RTable.Length; i++) 
		        {
		            if (destination != ((String) RTable[i, 0])) continue;

		            outSocket = (Socket) RTable[i,1]; // gets the socket for communication from the table
		            Console.WriteLine("Found destination: " + destination);
		        }          
		            
                receiveBuffer = new byte[1024];
                receiveBtyeCount = inSocket.Receive(receiveBuffer);// initial receive from router (verification of connection)
                chars = new char[receiveBtyeCount];
                charLen = decoder.GetChars(receiveBuffer, 0, receiveBtyeCount, chars, 0);
                inputLine = new String(chars);

                Console.WriteLine("Client/Server said: " + inputLine);
                outputLine = inputLine;

                if (outSocket != null)			
                   outSocket.Send(System.Text.Encoding.ASCII.GetBytes(outputLine)); // writes to the destination
                        
                receiveBuffer = new byte[1024];
                receiveBtyeCount = inSocket.Receive(receiveBuffer);// initial receive from router (verification of connection)
                chars = new char[receiveBtyeCount];
                charLen = decoder.GetChars(receiveBuffer, 0, receiveBtyeCount, chars, 0);
                inputLine = new String(chars);
                        
                    if(inputLine.ToLower() == "image")
                    {
                        outputLine = inputLine; // passes the input from the machine to the output string for the destination
                        Console.WriteLine("Client/Server said: " + inputLine);
                        if (outSocket != null)
                        {				
                            outSocket.Send(System.Text.Encoding.ASCII.GetBytes(outputLine)); // writes to the destination
                        }

                        receiveBuffer = new byte[4];
                        receiveBtyeCount = inSocket.Receive(receiveBuffer); //Receiving the size of the image
                        int size = BitConverter.ToInt32(receiveBuffer, 0);
                        Console.WriteLine("Client/Server said: Size of File: " + size);
                        outSocket.Send(receiveBuffer);

                        receiveBuffer = new byte[size];
                        receiveBtyeCount = inSocket.Receive(receiveBuffer);// Receive the image
                        Console.WriteLine("Received Image from Client/Server");
                        outSocket.Send(receiveBuffer);
                        Console.WriteLine("Sent Image to Client/Server");
                        
                    }
                    else
                    {
                        outputLine = inputLine; // passes the input from the machine to the output string for the destination
                        Console.WriteLine("Client/Server said: " + inputLine);
                        if (outSocket != null)
                        {				
                            outSocket.Send(System.Text.Encoding.ASCII.GetBytes(outputLine)); // writes to the destination
                        }
                        // Communication loop	
                        while (true) 
                        {
                            receiveBuffer = new byte[1024];
                            receiveBtyeCount = inSocket.Receive(receiveBuffer);// initial receive from router (verification of connection)
                            chars = new char[receiveBtyeCount];
                            charLen = decoder.GetChars(receiveBuffer, 0, receiveBtyeCount, chars, 0);
                            inputLine = new String(chars);

                            Console.WriteLine("Client/Server said: " + inputLine);
                            if (inputLine == "Bye.") // exit statement
                                                break;
                            outputLine = inputLine; // passes the input from the machine to the output string for the destination

                            if (outSocket != null)
                            {
                                outSocket.Send(System.Text.Encoding.ASCII.GetBytes(outputLine)); // writes to the destination
                            }			
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
