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
    class Server
    {
        private IPAddress ip_address_;
        private Int32 port_;
        TcpListener server;
        TcpClient client;
        bool interrupted;
        
        public Server(String ip_address, Int32 port) {
            ip_address_ = IPAddress.Parse(ip_address);
            port_ = port;
            thisComputer = new Computer() { CPUEnabled = true };
            thisComputer.Open();
            server = null;
            client = null;
            interrupted = false;
            SetConsoleCtrlHandler(new HandlerRoutine(ConsoleCtrlCheck), true);
        }

        [System.Runtime.InteropServices.DllImport("Kernel32")]
        public static extern bool SetConsoleCtrlHandler(HandlerRoutine Handler, bool Add);

        // A delegate type to be used as the handler routine 
        // for SetConsoleCtrlHandler.
        public delegate bool HandlerRoutine(CtrlTypes CtrlType);

        public enum CtrlTypes
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT,
            CTRL_CLOSE_EVENT,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT
        }

        private bool ConsoleCtrlCheck(CtrlTypes ctrlType)
        {
            Console.WriteLine("Stopping...");
            interrupted = true;
            return true;
        }


        public void serverLoop() {
          
                // TcpListener server = new TcpListener(port);
                server = new TcpListener(ip_address_, port_);

                // Start listening for client requests.
                server.Start();

                // Buffer for reading data
                Byte[] bytes = new Byte[256];
                String data = null;

                // Enter the listening loop.
                try {
                    while (true)
                    {
                        Console.WriteLine("Waiting for a connection. .. ");

                        // Perform a blocking call to accept requests.
                        // You could also user server.AcceptSocket() here.
                        while (!server.Pending())
                        {
                            if (interrupted) {
                                throw new SocketException();
                            }
                            System.Threading.Thread.Sleep(2);
                        }
                        client = server.AcceptTcpClient();
                        client.ReceiveTimeout = 2000;

                        Console.WriteLine("Connected!");

                        data = null;

                        // Get a stream object for reading and writing
                        NetworkStream stream = client.GetStream();

                        int i;

                        // Loop to receive all the data sent by the client.
                        while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                        {
                            if (interrupted) {
                                throw new SocketException();
                            }
                            // Translate data bytes to a ASCII string.
                            data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);

                            byte[] msg = getCpuTemperature();
                            // Send back a response.
                            stream.Write(msg, 0, msg.Length);
                        }

                        // Shutdown and end connection
                        client.Close();
                    }
                }
                catch (SocketException)
                {
                }
                finally
                {
                    // Stop listening for new clients.
                    server.Stop();
                }

        }


        private Int32 findHardwareItem(IHardware hardwareItem) {
            hardwareItem.Update();
            foreach (IHardware subHardware in hardwareItem.SubHardware)
                subHardware.Update();

            foreach (var sensor in hardwareItem.Sensors)
            {
                if (sensor.SensorType == SensorType.Temperature && String.Compare(sensor.Name, "CPU Package", false) == 0)
                {
                    return IPAddress.HostToNetworkOrder(sensor.Value.HasValue ? (Int32)sensor.Value.Value : -1);
                }
            }
            return int.MinValue;
        }


        private Int32 getTemperature() {
            foreach (var hardwareItem in thisComputer.Hardware)
            {
                if (hardwareItem.HardwareType == HardwareType.CPU && findHardwareItem(hardwareItem) != int.MinValue)
                {
                    return findHardwareItem(hardwareItem);
                }
            }
            return int.MinValue;
        }

        private byte[] getCpuTemperature()
        {
            String temp = "";
            Int32 temperature = getTemperature();

            Int64 unixTimestamp = IPAddress.HostToNetworkOrder((Int64)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds);
            
            byte[] temperature_bytes = BitConverter.GetBytes(temperature);
            byte[] timestamp_bytes = BitConverter.GetBytes(unixTimestamp);
            byte[] byte_array = new byte[12];
            temperature_bytes.CopyTo(byte_array, 0);
            timestamp_bytes.CopyTo(byte_array, 4);
            return byte_array;
        }

        private Computer thisComputer;

    }


}
