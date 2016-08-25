using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.WebSockets;
using System.Net.Sockets;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Threading;

namespace SocketIOServer
{
    class Program
    {
        static Socket serverSocket = new Socket(AddressFamily.InterNetwork,
        SocketType.Stream, ProtocolType.IP);
        static SHA1 sha1 = SHA1CryptoServiceProvider.Create();
        static List<Socket> clientsList = new List<Socket>();
        static Socket browserClient = null;
        static Socket androidClient = null;
        static void Main()
        {
            serverSocket.Bind(new IPEndPoint(IPAddress.Parse("192.168.0.8"), 3000));
            serverSocket.Listen(128);
            serverSocket.BeginAccept(null, 0, OnAccept, null);
            Console.Read();

        }


        private static void OnAccept(IAsyncResult result)
        {

            byte[] buffer = new byte[1024];
            try
            {
                Socket client = null;
                string headerResponse = "";
                if (serverSocket != null && serverSocket.IsBound)
                {
                    Console.WriteLine("Client connected..........");
                    client = serverSocket.EndAccept(result);
                    clientsList.Add(client);
                    var i = client.Receive(buffer);
                    headerResponse = (System.Text.Encoding.UTF8.GetString(buffer)).Substring(0, i);
                    // write received data to the console
                    Console.WriteLine(headerResponse);
                }

                if (client != null)
                {
                    
                    // Browser request.
                    if (new Regex("^GET").IsMatch(headerResponse))
                    {
                        browserClient = client;
                        byte[] response = Encoding.UTF8.GetBytes("HTTP/1.1 101 Switching Protocols" + Environment.NewLine
                                                        + "Connection: Upgrade" + Environment.NewLine
                                                        + "Upgrade: websocket" + Environment.NewLine
                                                        + "Sec-WebSocket-Accept: " + Convert.ToBase64String(
                                                            SHA1.Create().ComputeHash(
                                                                Encoding.UTF8.GetBytes(
                                                                    new Regex("Sec-WebSocket-Key: (.*)").Match(headerResponse)
                                                                    .Groups[1].Value.Trim() + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11"
                                                                )
                                                            )
                                                        ) + Environment.NewLine
                                                        + Environment.NewLine);

                        Console.WriteLine("Data to be sent is :  ");
                        Console.WriteLine(System.Text.Encoding.UTF8.GetString(response));
                  
                        // send connect response.
                        client.BeginSend(response, 0, response.Length, SocketFlags.None, OnBrowserConnect, client);
                    }
                    // Android request
                    else
                    {
                        androidClient = client;
                        var bytes = Encoding.UTF8.GetBytes(headerResponse);

                        byte[] subA = encodedData(headerResponse); // Use RFC 6455 Websocket Protocol sending message
                        
                        androidClient.BeginSend(bytes, 0, bytes.Length, SocketFlags.None, OnEchoBroadcast, 
                            androidClient);

                        browserClient.BeginSend(subA, 0, subA.Length, SocketFlags.None, 
                            OnEchoBroadcast, browserClient);

                    }

                }

            }
            catch(SocketException ex)
            {
                throw ex;
            }
            finally
            {
                if (serverSocket != null && serverSocket.IsBound)
                {
                    serverSocket.BeginAccept(null, 0, OnAccept, null);
                }
            }
        }

        private static byte[] encodedData(String mess)
        {
            byte[] rawData = Encoding.UTF8.GetBytes(mess);

            int frameCount = 0;
            byte[] frame = new byte[10];

            frame[0] = (byte)129;

            if (rawData.Length <= 125)
            {
                frame[1] = (byte)rawData.Length;
                frameCount = 2;
            }
            else if (rawData.Length >= 126 && rawData.Length <= 65535)
            {
                frame[1] = (byte)126;
                int len = rawData.Length;
                frame[2] = (byte)((len >> 8) & (byte)255);
                frame[3] = (byte)(len & (byte)255);
                frameCount = 4;
            }
            else
            {
                frame[1] = (byte)127;
                int len = rawData.Length;
                frame[2] = (byte)((len >> 56) & (byte)255);
                frame[3] = (byte)((len >> 48) & (byte)255);
                frame[4] = (byte)((len >> 40) & (byte)255);
                frame[5] = (byte)((len >> 32) & (byte)255);
                frame[6] = (byte)((len >> 24) & (byte)255);
                frame[7] = (byte)((len >> 16) & (byte)255);
                frame[8] = (byte)((len >> 8) & (byte)255);
                frame[9] = (byte)(len & (byte)255);
                frameCount = 10;
            }

            int bLength = frameCount + rawData.Length;

            byte[] reply = new byte[bLength];

            int bLim = 0;
            for (int i = 0; i < frameCount; i++)
            {
                reply[bLim] = frame[i];
                bLim++;
            }

            for (int i = 0; i < rawData.Length; i++)
            {
                reply[bLim] = rawData[i];
                bLim++;
            }

            return reply;
        }

        private static void OnEchoBroadcast(IAsyncResult res)
        {
            Socket client = (Socket)res.AsyncState;
            client.EndSend(res);
            Console.WriteLine("Echo Send");
        }


        private static void OnBrowserConnect(IAsyncResult res)
        {
            Socket client = (Socket)res.AsyncState;
            client.EndSend(res);
            byte[] buffer = new byte[1024];
            var ct = client.Receive(buffer); // wait for client to send a message

            //TODO : not working need to be checked.
            string resp = decodedString(buffer); // Use RFC 6455 Websocket Protocol receiving message

            Console.WriteLine("Browser Client Connected");
        }

        static string decodedString(byte[] bytes)
        {
            string decodedString = "";

            String incomingData = String.Empty;
            Byte secondByte = bytes[1];
            Int32 dataLength = secondByte & 127;
            Int32 indexFirstMask = 2;
            if (dataLength == 126)
                indexFirstMask = 4;
            else if (dataLength == 127)
                indexFirstMask = 10;

            IEnumerable<Byte> keys = bytes.Skip(indexFirstMask).Take(4);
            Int32 indexFirstDataByte = indexFirstMask + 4;

            Byte[] decoded = new Byte[bytes.Length - indexFirstDataByte];
            for (Int32 i = indexFirstDataByte, j = 0; i < bytes.Length; i++, j++)
            {
                decoded[j] = (Byte)(bytes[i] ^ keys.ElementAt(j % 4));
            }

            return decodedString = Encoding.UTF8.GetString(decoded, 0, decoded.Length);

        }


    }
}