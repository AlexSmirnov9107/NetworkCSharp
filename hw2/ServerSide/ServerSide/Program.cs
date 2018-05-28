using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ServerSide
{
    class Program
    {
        public static List<Socket> clients; //список подключенных 
        public class Message
        {
            public string sender { get; set; }
            public string message { get; set; }
        }
        
        static void Main(string[] args)
        {
            clients = new List<Socket>();
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 3535);
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(endPoint);
            try
            {
                socket.Listen(3);
                SocketAsyncEventArgs e = new SocketAsyncEventArgs();
                e.Completed += AcceptCallback;
                if (!socket.AcceptAsync(e))
                {
                    AcceptCallback(socket, e);
                }

            }
            catch (SocketException ex)
            {
                Console.WriteLine(ex.Message);
            }

            Console.ReadKey(true);

        }

        private static void ERecieve_Completed(object sender, SocketAsyncEventArgs e)
        {
            Socket client = (Socket)sender;
      
            try
            {
                foreach (var socket in clients)
                {
                    if (socket.Connected)
                        socket.Send(e.Buffer, 0, e.BytesTransferred, SocketFlags.None);
                    else
                    {
                        socket.Shutdown(SocketShutdown.Both); //если кто то отключился
                        clients.Remove(socket);
                    }
                }
            }
            catch (Exception)
            {

               
            }
            client.ReceiveAsync(e);
        }

        private static void AcceptCallback(object sender, SocketAsyncEventArgs e)
        {
            Socket listenSocket = (Socket)sender;
            do
            {
                try
                {
                    Socket client = e.AcceptSocket;
                    clients.Add(client);
                    SocketAsyncEventArgs eRecieve = new SocketAsyncEventArgs();

                    byte[] buffer = new byte[1024];
                    eRecieve.SetBuffer(buffer, 0, buffer.Length);
                    eRecieve.Completed += ERecieve_Completed;
                    client.ReceiveAsync(eRecieve);
                }
                catch (SocketException ex)
                {
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    e.AcceptSocket = null; 
                }
            } while (!listenSocket.AcceptAsync(e));
        }
    }
}
