using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TCPServerRouter
{
    public class ServerRouter
    {
        public const int CLIENT_LISTEN_PORT = 5555;
        public const int SERVER_LISTEN_PORT = 6666;
        

        public static void Main(string[] args)
        {
            
            Console.WriteLine("Starting Client Listener...");
            ThreadPool.QueueUserWorkItem(ConnectionHandler.ListenForClient);

            Console.WriteLine("Starting Server Listener...");
            ThreadPool.QueueUserWorkItem(ConnectionHandler.ListenForServer);

            Console.ReadKey();
        }

        
    }

    /// <summary>
    /// Taken from http://stackoverflow.com/questions/4015204/c-sharp-merging-2-dictionaries
    /// </summary>
    public static class DictionaryExtensionMethods
    {
        public static void Merge<TKey, TValue>(this Dictionary<TKey, TValue> me, Dictionary<TKey, TValue> merge)
        {
            foreach (var item in merge)
            {
                me[item.Key] = item.Value;
            }
        }
    }
}
