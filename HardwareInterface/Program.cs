using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenHardwareMonitor;
using OpenHardwareMonitor.Hardware;
using System.Net;
using System.Net.Sockets;

namespace HardwareInterface
{
    class Program
    {


        static void Main(string[] args)
        {
            //if (args.Length == 0) {
            //    Console.WriteLine("Usage: hardware_daemon.exe <ip_address>:<port>");
            //}
            //String[] Array = args[0].Split(':');
            //Server server = new HardwareInterface.Server(Array[0], Int32.Parse(Array[1]));
            Server server = new HardwareInterface.Server("127.0.0.1", 13002);
            server.serverLoop();
        }
    }

}
