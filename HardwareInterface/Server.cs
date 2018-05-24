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

        public Server(String ip_address, Int32 port) {
            ip_address_ = IPAddress.Parse(ip_address);
            port_ = port;
            thisComputer = new Computer() { CPUEnabled = true };
            thisComputer.Open();
        }

        

        public void serverLoop() {
            TcpListener server = null;
            try
            {

                // TcpListener server = new TcpListener(port);
                server = new TcpListener(ip_address_, port_);

                // Start listening for client requests.
                server.Start();

                // Buffer for reading data
                Byte[] bytes = new Byte[256];
                String data = null;

                // Enter the listening loop.
                while (true)
                {
                    Console.Write("Waiting for a connection... ");

                    // Perform a blocking call to accept requests.
                    // You could also user server.AcceptSocket() here.
                    TcpClient client = server.AcceptTcpClient();
                    Console.WriteLine("Connected!");

                    data = null;

                    // Get a stream object for reading and writing
                    NetworkStream stream = client.GetStream();

                    int i;

                    // Loop to receive all the data sent by the client.
                    while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        // Translate data bytes to a ASCII string.
                        data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
                        Console.WriteLine("Received: {0}", data);

                        byte[] msg = getCpuTemperature();

                        // Send back a response.
                        stream.Write(msg, 0, msg.Length);
                        Console.WriteLine("Sent: {0}", data);
                    }

                    // Shutdown and end connection
                    client.Close();
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
            finally
            {
                // Stop listening for new clients.
                server.Stop();
            }


            Console.WriteLine("\nHit enter to continue...");
            Console.Read();
        }

        private byte[] getCpuTemperature()
        {
            String temp = "";
            Int32 temperature = -1;

            Int64 unixTimestamp = IPAddress.HostToNetworkOrder((Int64)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds);
            foreach (var hardwareItem in thisComputer.Hardware)
            {
                if (hardwareItem.HardwareType == HardwareType.CPU)
                {
                    hardwareItem.Update();
                    foreach (IHardware subHardware in hardwareItem.SubHardware)
                        subHardware.Update();

                    foreach (var sensor in hardwareItem.Sensors)
                    {
                        if (sensor.SensorType == SensorType.Temperature && String.Compare(sensor.Name, "CPU Package", false) == 0)
                        {
                            temperature = IPAddress.HostToNetworkOrder(sensor.Value.HasValue ? (Int32)sensor.Value.Value : -1);
                            temp += String.Format("{0} Temperature = {1}\r\n", sensor.Name, sensor.Value.HasValue ? sensor.Value.Value.ToString() : "no value");
                        }
                    }
                }
            }
            byte[] temperature_bytes = BitConverter.GetBytes(temperature);
            byte[] timestamp_bytes = BitConverter.GetBytes(unixTimestamp);
            byte[] byte_array = new byte[12];
            temperature_bytes.CopyTo(byte_array, 0);
            timestamp_bytes.CopyTo(byte_array, 4);
            Console.WriteLine(temp);
            Console.WriteLine(Encoding.Default.GetString(byte_array));
            Console.ReadLine();
            return byte_array;
        }

        private Computer thisComputer;

    }


}
