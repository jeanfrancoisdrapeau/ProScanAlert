using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace ProScanAlert
{
    public class ServerConnections
    {
        public class StateObject
        {
            // Client  socket.
            public Socket workSocket = null;
            // Size of receive buffer.
            public const int BufferSize = 65535;
            // Receive buffer.
            public byte[] buffer = new byte[BufferSize];
            // Received data string.
            public StringBuilder sb = new StringBuilder();
        }

        private static Socket listener;

        [Serializable]
        [XmlRoot("Keys")]
        public class Keys
        {
            [XmlArray("Keys"), XmlArrayItem(typeof(KeysDetails), ElementName = "KeysDetails")]
            public List<KeysDetails> KeysList { get; set; }
        }

        [Serializable]
        public class KeysDetails
        {
            public string key { get; set; }
        }

        public static bool ServerStarted = false;
        private static ManualResetEvent allDone = new ManualResetEvent(false);

        public ServerConnections()
        {
        }

        public static void StopListening()
        {
            ServerStarted = false;
            if (listener != null)
                listener.Close();
        }

        public static void StartListening(string sPort)
        {
            // Data buffer for incoming data.
            byte[] bytes = new Byte[1024];

            // Establish the local endpoint for the socket.
            // The DNS name of the computer
            // running the listener is "host.contoso.com".
            int port;
            int.TryParse(sPort, out port);

            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, port);

            // Create a TCP/IP socket.
            listener = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);

            // Bind the socket to the local endpoint and listen for incoming connections.
            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(100);

                // Start an asynchronous socket to listen for connections.
                //Console.WriteLine("Waiting for a connection...");
                Form1.lbStats.Invoke((MethodInvoker)(() =>
                    Form1.lbStats.Items.Add(string.Format("{0} (SERVER) Waiting for a connection...", DateTime.Now))));
                Form1.lbStats.Invoke((MethodInvoker)(() => Form1.lbStats.TopIndex = Form1.lbStats.Items.Count - 1));

                ServerStarted = true;

                while (ServerStarted)
                {
                    allDone.Reset();
                    listener.BeginAccept(
                        new AsyncCallback(AcceptCallback),
                        listener);
                    allDone.WaitOne();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public static void AcceptCallback(IAsyncResult ar)
        {
            allDone.Set();
            if (ServerStarted == false) return;

            // Get the socket that handles the client request.
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);

            IPEndPoint remoteIpEndPoint = handler.RemoteEndPoint as IPEndPoint;

            Form1.lbStats.Invoke((MethodInvoker)(() =>
                    Form1.lbStats.Items.Add(string.Format("{0} (SERVER) Connection from {1}",
                    DateTime.Now, remoteIpEndPoint.Address)))); 
            Form1.lbStats.Invoke((MethodInvoker)(() => Form1.lbStats.TopIndex = Form1.lbStats.Items.Count - 1));


            // Create the state object.
            StateObject state = new StateObject();
            state.workSocket = handler;
            handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(ReadCallback), state);
        }

        public static void ReadCallback(IAsyncResult ar)
        {
            String content = String.Empty;

            // Retrieve the state object and the handler socket
            // from the asynchronous state object.
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;

            // Read data from the client socket. 
            int bytesRead = handler.EndReceive(ar);

            if (bytesRead > 0)
            {
                // There  might be more data, so store the data received so far.
                state.sb.Append(Encoding.ASCII.GetString(
                    state.buffer, 0, bytesRead));

                // Check for end-of-file tag. If it is not there, read 
                // more data.
                content = state.sb.ToString();
                
                    // All the data has been read from the 
                    // client. Display it on the console.
                
                    Console.WriteLine("Read {0} bytes from socket. \n Data : {1}",
                        content.Length, content);
                    // Save/remove device key
                    string status = string.Empty;

                    string[] contentArray = content.Split(',');

                    IPEndPoint remoteIpEndPoint = handler.RemoteEndPoint as IPEndPoint;
                    Form1.lbStats.Invoke((MethodInvoker)(() =>
                        Form1.lbStats.Items.Add(string.Format("{0} (SERVER) {1} {2}", 
                        DateTime.Now, remoteIpEndPoint.Address, contentArray[0]))));
                    Form1.lbStats.Invoke((MethodInvoker)(() => Form1.lbStats.TopIndex = Form1.lbStats.Items.Count - 1));

                    switch (contentArray[0])
                    {
                        case "REG":
                            status = saveKey(contentArray[1]);
                            break;
                        case "UNREG":
                            status = removeKey(contentArray[1]);
                            break;
                    }

                    Form1.lbStats.Invoke((MethodInvoker)(() =>
                            Form1.lbStats.Items.Add(string.Format("{0} (SERVER) {1} {2}", 
                            DateTime.Now, remoteIpEndPoint.Address, status))));
                    Form1.lbStats.Invoke((MethodInvoker)(() => Form1.lbStats.TopIndex = Form1.lbStats.Items.Count - 1));

                    // Send status to client
                    Send(handler, status);
                
                    // Not all data received. Get more.
                    //handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    //    new AsyncCallback(ReadCallback), state);
                
            }
        }

        private static string saveKey(string key)
        {
            try
            { 
                Keys s = GetKeys();

                KeysDetails kd = s.KeysList.Find(k => k.key == key);
                if (kd == null)
                {
                    kd = new KeysDetails();
                    kd.key = key;

                    s.KeysList.Add(kd);

                    SaveKeys(s);
                    return "Success.";
                }
                else
                {
                    return "This device is already registered.";
                }  
            }
            catch { return "Error."; }
        }

        private static string removeKey(string key)
        {
            try
            {
                Keys s = GetKeys();

                int i = s.KeysList.FindIndex(k => k.key == key);
                if (i != -1)
                {
                    s.KeysList.RemoveAt(i);

                    SaveKeys(s);
                    return "Success.";
                }
                else
                {
                    return "This device was not registered.";
                }  
            }
            catch { return "Error."; }
        }

        private static void SaveKeys(Keys s)
        {
            var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var filename = Path.Combine(documents, "proscanalert_keys.xml");

            Keys s_tmp = new Keys();
            s_tmp.KeysList = new List<KeysDetails>();

            foreach (KeysDetails sd_tmp in s.KeysList)
            {
                s_tmp.KeysList.Add(new KeysDetails()
                {
                    key = sd_tmp.key
                });
            }

            XmlSerializer serializer = new XmlSerializer(typeof(Keys));
            TextWriter textWriter = new StreamWriter(filename);
            serializer.Serialize(textWriter, s_tmp);
            textWriter.Close();

            s_tmp = null;
        }

        public static Keys GetKeys()
        {
            var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var filename = Path.Combine(documents, "proscanalert_keys.xml");

            Keys s = new Keys();
            s.KeysList = new List<KeysDetails>();

            if (File.Exists(filename))
            {
                Keys s_tmp = new Keys();

                XmlSerializer deserializer = new XmlSerializer(typeof(Keys));
                TextReader textReader = new StreamReader(filename, Encoding.UTF8);
                s_tmp = (Keys)deserializer.Deserialize(textReader);
                textReader.Close();

                foreach (KeysDetails sd_tmp in s_tmp.KeysList)
                {
                    s.KeysList.Add(new KeysDetails()
                    {
                        key = sd_tmp.key
                    });
                }

                s_tmp = null;
            }

            return s;
        }

        private static void Send(Socket handler, String data)
        {
            // Convert the string data to byte data using ASCII encoding.
            byte[] byteData = Encoding.ASCII.GetBytes(data);

            // Begin sending the data to the remote device.
            handler.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), handler);
        }

        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket handler = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.
                int bytesSent = handler.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to client.", bytesSent);

                handler.Shutdown(SocketShutdown.Both);
                handler.Close();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
