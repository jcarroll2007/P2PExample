using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using P2PClient;

namespace TCPClientConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            TCPClientSender sender = new TCPClientSender();
            sender.RouterName = "172.16.20.121";
            sender.ServerName = "172.16.20.21";
            sender.Port = 5555;

            sender.SendFile(Directory.GetCurrentDirectory() + "\\Input\\TestFile.txt");

            //sender.SendImage(Directory.GetCurrentDirectory() + "\\Input\\avatar.jpg");

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
