using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Newtonsoft;
using System.Net;
using Newtonsoft.Json;

namespace ClientSide
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Socket socket;
        public class Message
        {
            public string sender { get; set; }
            public string message { get; set; }
        }
        public MainWindow()
        {
            InitializeComponent();
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        private void SendMessage(object sender, RoutedEventArgs e)
        {
            Message m = new Message { sender = username.Text, message = messageBox.Text };
            socket.Send(Encoding.Default.GetBytes(JsonConvert.SerializeObject(m)));
        }

        private void ConnectToChat(object sender, RoutedEventArgs e)
        {

            IPEndPoint serverPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 3535);
            SocketAsyncEventArgs connectEvent = new SocketAsyncEventArgs();
            connectEvent.Completed += Connect_Completed;
            connectEvent.RemoteEndPoint = serverPoint;

            Message m = new Message { sender = username.Text, message = " joined" };
            string json = JsonConvert.SerializeObject(m);
            byte[] buffer = new byte[1024];
            buffer = Encoding.Default.GetBytes(json);

            connectEvent.SetBuffer(buffer, 0, json.Length);
            try
            {
                socket.ConnectAsync(connectEvent);
            }
            catch (SocketException ex)
            {

                Console.WriteLine(ex.Message);
            }

        }

        private void RecieveEvent_Completed(object sender, SocketAsyncEventArgs e)
        {
            Socket client = (Socket)sender;
            try
            {

                string json = Encoding.Default.GetString(e.Buffer,0,e.BytesTransferred);
                Dispatcher.Invoke(() =>
                {
                    Message m = JsonConvert.DeserializeObject<Message>(json);
                    richBox.AppendText("\n"+m.sender+" : "+m.message);
                });
                client.ReceiveAsync(e);
            }
            catch (SocketException ex)
            {

                Console.WriteLine(ex.Message);
            }


        }

        private void Connect_Completed(object sender, SocketAsyncEventArgs e)
        {
            Socket client = (Socket)sender;
            if (client.Connected)
            {

                byte[] buffer = new byte[1024];
                SocketAsyncEventArgs recieveEvent = new SocketAsyncEventArgs();
                recieveEvent.Completed += RecieveEvent_Completed;
                recieveEvent.SetBuffer(buffer, 0, buffer.Length);
                client.ReceiveAsync(recieveEvent);
            }
            else
            {
                Dispatcher.Invoke(() =>
                {
                    richBox.AppendText("No connection with server!");
                });

            }
        }

        private void username_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
