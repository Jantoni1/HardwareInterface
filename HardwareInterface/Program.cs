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
            try
            {
                if (args.Length != 1)
                {
                    throw new Exception("Usage: hardware_daemon.exe <ip_address>:<port>");
                }
                String[] Array = args[0].Split(':');
                Server server = new HardwareInterface.Server(Array[0], Int32.Parse(Array[1]));
                server.serverLoop();

            }
            catch (Exception e) {
                Console.WriteLine(e.Message);
            }

        }
    }

}
