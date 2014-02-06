using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;
using System.IO;
using PushSharp;
using PushSharp.Apple;
using PushSharp.Core;

namespace ProScanAlert
{
    public partial class Form1 : Form
    {
        const int INDEX_MESSAGE_TYPE = 0;
        const int INDEX_MESSAGE_LENGTH = 9;
        const int INDEX_MESSAGE_SEQ = 15;

        const int BYTES_MESSAGE_TYPE = 8;
        const int BYTES_MESSAGE_LENGTH = 5;
        const int BYTES_MESSAGE_SEQ = 5;

        const int MIN_MESSAGE_LENGTH = 14;

        const string MESSAGE_TYPE_STARTDAT = "STARTDAT";
        const string MESSAGE_TYPE_STARTAUD = "STARTAUD";

        private System.Timers.Timer _timer, _timerSys;
        private DateTime _timerCounter;
        private DateTime _startTime;

        private int _lastBytesReceived;

        ScannerScreen _scannerScreen;
        ScannerLog _scannerLog;

        NetworkConnection networkConnection;

        PushBroker push;

        public static ListBox lbStats;

        private static TextBox _tbMins;
        public static TextBox _Mins { get { return _tbMins; } }

        private static TextBox _tbSecs;
        public static TextBox _Secs { get { return _tbSecs; } }

        [Serializable]
        public class SystemsDetails
        {
            public string system_name { get; set; }
            public bool system_checked { get; set; }
        }

        [Serializable]
        [XmlRoot("Systems")]
        public class Systems
        {
            [XmlArray("Systems"), XmlArrayItem(typeof(SystemsDetails), ElementName = "SystemsDetails")]
            public List<SystemsDetails> SystemsList { get; set; }
        }

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            lbStats = listBox1;
            _tbMins = tbMins;
            _tbSecs = tbSecs;

#if DEBUG
            tbServerHost.Text = "eluderwrx.no-ip.info";
            tbPassword.Text = "ddm_0466";
#endif

            loadListSystems();
            loadSettings();

            _timer = new System.Timers.Timer();
            _timer.Interval = 100;
            _timer.Elapsed += new System.Timers.ElapsedEventHandler(timerElapsed);

            _timerSys = new System.Timers.Timer();
            _timerSys.Interval = 1000;
            _timerSys.Elapsed += new System.Timers.ElapsedEventHandler(timerSysElapsed);

            //Create our push services broker
            push = new PushBroker();

            //Wire up the events for all the services that the broker registers
            push.OnNotificationSent += NotificationSent;
            push.OnChannelException += ChannelException;
            push.OnServiceException += ServiceException;
            push.OnNotificationFailed += NotificationFailed;
            push.OnDeviceSubscriptionExpired += DeviceSubscriptionExpired;
            push.OnDeviceSubscriptionChanged += DeviceSubscriptionChanged;
            push.OnChannelCreated += ChannelCreated;
            push.OnChannelDestroyed += ChannelDestroyed;

            //-------------------------
            // APPLE NOTIFICATIONS
            //-------------------------
            //Configure and start Apple APNS
            // IMPORTANT: Make sure you use the right Push certificate.  Apple allows you to generate one for connecting to Sandbox,
            //   and one for connecting to Production.  You must use the right one, to match the provisioning profile you build your
            //   app with!
#if DEBUG
            var appleCert = File.ReadAllBytes(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ProScanMobile+_Dev_Certificates.p12"));
            push.RegisterAppleService(new ApplePushChannelSettings(appleCert, "$N0feaR!", false)); //Extension method
#else
            var appleCert = File.ReadAllBytes(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ProScanMobile+_Prod_Certificates.p12"));
            push.RegisterAppleService(new ApplePushChannelSettings(true, appleCert, "$N0feaR!", true)); //Extension method
#endif
        }

        private void timerSysElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (ScannerLog.All_Log.Count > 0)
            {
                listBox2.Invoke((MethodInvoker)(() => listBox2.Items.Clear()));
                var selectedSysgrp = ScannerLog.All_Log.Select(d => new { d.sys, d.grp }).Distinct();

                foreach (var sysgrp in selectedSysgrp)
                {
                    var result = ScannerLog.All_Log.Where(d => d.sys == sysgrp.sys &&
                        d.grp == sysgrp.grp);

                    long secsTotal = result.Sum(span => span.secs);

                    listBox2.Invoke((MethodInvoker)(() => listBox2.Items.Add(string.Format("{0}: {1}s", sysgrp.sys, secsTotal))));
                }
            }
            else { listBox2.Invoke((MethodInvoker)(() => listBox2.Items.Clear())); }
        }

        private void timerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                if (networkConnection.connectionStatus == NetworkConnection.ConnectionStatus.Connected)
                {
                    if (_lastBytesReceived != networkConnection.bytesReceived)
                    {
                        _lastBytesReceived = networkConnection.bytesReceived;
                        _timerCounter = DateTime.Now;
                    }

                    TimeSpan ts = DateTime.Now - _timerCounter;

                    if (ts.TotalSeconds > 5 ||
                        networkConnection.receiveDataStatus == NetworkConnection.SendStatus.Error)
                    {
                        networkConnection.Close();
                        networkConnection.closeDone.WaitOne();

                        _timer.Stop();
                        _timerSys.Stop();

                        connectToHostAndBeginPlayback(true);

                        return;
                    }

                    if (networkConnection.connectionBuffer.Count > 0)
                    {
                        int i_messageLength;
                        byte[] b_messageLength = new byte[BYTES_MESSAGE_LENGTH];
                        Array.ConstrainedCopy(networkConnection.connectionBuffer.Read(14, true),
                            INDEX_MESSAGE_LENGTH, b_messageLength, 0, BYTES_MESSAGE_LENGTH);

                        //Console.WriteLine("---------- b_messageLength........: {0} (first bytes of data buffer)", bytesToString(b_messageLength));

                        int.TryParse(bytesToString(b_messageLength), out i_messageLength);

                        //Console.WriteLine("---------- Message length.........: {0} (first bytes of data buffer)", i_messageLength.ToString());

                        bool continueParse = true;
                        while (continueParse)
                        {
                            if (networkConnection.connectionBuffer.Count < i_messageLength)
                            {
                                //Console.WriteLine("---------- **** MESSAGE LENGTH GREATER THAN DATABUFFER **** ----------");
                                continueParse = false;
                            }
                            else
                            {

                                byte[] messageReceived = new byte[i_messageLength];
                                messageReceived = networkConnection.connectionBuffer.Read(i_messageLength);
                                //Console.WriteLine("---------- m_listDataBuffer while.: {0}", networkConnection.connectionBuffer.Count.ToString());

                                byte[] b_messageType = new byte[BYTES_MESSAGE_TYPE];
                                Array.ConstrainedCopy(messageReceived, INDEX_MESSAGE_TYPE,
                                    b_messageType, 0, BYTES_MESSAGE_TYPE);

                                //Console.WriteLine(i_messageLength.ToString());
                                // Based on message type...
                                switch (bytesToString(b_messageType))
                                {
                                    case MESSAGE_TYPE_STARTDAT:
                                        _scannerScreen.processData(messageReceived, i_messageLength);
                                        _scannerLog.processData(messageReceived, i_messageLength);
                                        break;
                                }

                                if (networkConnection.connectionBuffer.Count == 0 ||
                                    networkConnection.connectionBuffer.Count < MIN_MESSAGE_LENGTH)
                                {
                                    continueParse = false;
                                }
                                else
                                {
                                    b_messageLength = new byte[BYTES_MESSAGE_LENGTH];
                                    Array.ConstrainedCopy(networkConnection.connectionBuffer.Read(MIN_MESSAGE_LENGTH, true),
                                        INDEX_MESSAGE_LENGTH, b_messageLength, 0, BYTES_MESSAGE_LENGTH);
                                    //Console.WriteLine("---------- b_messageLength while..: {0} (first bytes of data buffer)", bytesToString(b_messageLength));
                                    int.TryParse(bytesToString(b_messageLength), out i_messageLength);
                                }
                            }
                        }
                    }

                    lblLine1.Invoke((MethodInvoker)(() => lblLine1.Text = _scannerScreen.scannerScreen_Line1));
                    lblLine2.Invoke((MethodInvoker)(() => lblLine2.Text = _scannerScreen.scannerScreen_Line2));
                    lblLine3.Invoke((MethodInvoker)(() => lblLine3.Text = _scannerScreen.scannerScreen_Line3));
                    lblLine4.Invoke((MethodInvoker)(() => lblLine4.Text = _scannerScreen.scannerScreen_Line4));
                    lblLine5.Invoke((MethodInvoker)(() => lblLine5.Text = _scannerScreen.scannerScreen_Line5));

                    if (ScannerLog.CurrentAlert_Alert == true)
                    {
                        foreach (string s in checkedListBox1.CheckedItems)
                        {
                            if (s == ScannerLog._currentAlert_Sys)
                            {
                                //Console.WriteLine("SENDING ALERT!");
                                listBox1.Invoke((MethodInvoker)(() => 
                                    listBox1.Items.Add(string.Format("{0} (ALERT) Sending ALERTS for {1}", 
                                    DateTime.Now, s))));
                                listBox1.Invoke((MethodInvoker)(() => listBox1.TopIndex = listBox1.Items.Count - 1));
                                SendNotification(string.Format("High activity detected\n{1}\n{2}\n{3}\n{0}\nTap here to connect.", 
                                    DateTime.Now,
                                    ScannerLog._currentAlert_Sys, 
                                    ScannerLog._currentAlert_Grp,
                                    tbServerHost.Text), tbServerHost.Text, tbServerPort.Text);
                            }
                        }
                        ScannerLog.CurrentAlert_Alert = false;
                    }
                }
            }
            catch (Exception ex)
            {
                //messageBoxShow (NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleDisplayName").ToString(),
                //	ex.Message);
                Console.WriteLine(ex.Message);
            }
        }

        private void connectToHostAndBeginPlayback(bool reconnect = false)
        {
            for (int retries = 0; retries < 5; retries++)
            {
                networkConnection = new NetworkConnection(tbServerHost.Text.Trim(), Convert.ToInt32(tbServerPort.Text.Trim()));
                networkConnection.connectDone.WaitOne();

                if (networkConnection.connectionStatus != NetworkConnection.ConnectionStatus.Connected)
                {
                    Thread.Sleep(1000);
                }
                else
                {
                    break;
                }
            }

            if (networkConnection.connectionStatus == NetworkConnection.ConnectionStatus.Connected)
            {
                Encryption enc = new Encryption();

                string password = enc.Encrypt(tbPassword.Text);
                networkConnection.Login(string.Format("STARTDAT 00048 PS17,VERSION=6.6,PASSWORD={0} ENDDAT",
                    string.IsNullOrEmpty(password) ? string.Empty : password));
                networkConnection.loginDone.WaitOne();

                if (networkConnection.loginStatus == NetworkConnection.LoginStatus.LoggedIn)
                {
                   _scannerScreen = new ScannerScreen();
                   _scannerLog = new ScannerLog();

                    _startTime = DateTime.Now;
                    _timerCounter = DateTime.Now;
                    _timer.Start();
                    _timerSys.Start();

                }
                else
                {
                    Console.WriteLine(networkConnection._loginStatusMessage);

                    networkConnection.LogOut("STARTDAT 00026 PS05 ENDDAT");
                    networkConnection.logoutDone.WaitOne();

                    networkConnection.Close();
                    networkConnection.closeDone.WaitOne();
                }
            }
            else
            {
                listBox1.Items.Add(string.Format("{0} (SCANNER) {1}", DateTime.Now, networkConnection._connectionStatusMessage));
                listBox1.TopIndex = listBox1.Items.Count - 1;
            }
        }

        private string bytesCountToString(long byteCount)
        {
            string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" };
            if (byteCount == 0)
                return "0" + suf[0];
            long bytes = Math.Abs(byteCount);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return string.Format("{0:#0.0}", (Math.Sign(byteCount) * num)) + suf[place];
        }

        private string bytesToString(byte[] b)
        {
            // Return a string encoded byte array
            return System.Text.Encoding.ASCII.GetString(b);
        }

        private void btConnect_Click(object sender, EventArgs e)
        {
            if (networkConnection == null)
            {
                listBox1.Items.Add(string.Format("{0} (SCANNER) Connecting to ProScan server...", DateTime.Now));
                listBox1.TopIndex = listBox1.Items.Count - 1;
                Application.DoEvents();
                connectToHostAndBeginPlayback();
                if (networkConnection.connectionStatus == NetworkConnection.ConnectionStatus.Error)
                {
                    listBox1.Items.Add(string.Format("{0} (SCANNER) Cannot connect to ProScan server", DateTime.Now));
                    listBox1.TopIndex = listBox1.Items.Count - 1;
                    networkConnection = null;
                }
                else 
                {
                    listBox1.Items.Add(string.Format("{0} (SCANNER) Starting scanner playback", DateTime.Now));
                    listBox1.TopIndex = listBox1.Items.Count - 1;
                    btConnect.Text = "Close"; 
                }
            }
            else
            {
                listBox1.Items.Add(string.Format("{0} (SCANNER) Stopping scanner playback", DateTime.Now));
                listBox1.TopIndex = listBox1.Items.Count - 1;
                StopScanner();
                saveListSystems();
                saveSettings();
            }
        }

        private void StopAll()
        {
            StopServer();
            StopScanner();
            saveListSystems();
            saveSettings();
        }

        private void StopServer()
        {
            ServerConnections.StopListening();
        }

        private void StopScanner()
        {
            _timer.Stop();
            _timerSys.Stop();

            if (_scannerScreen != null)
                _scannerScreen.Dispose();

            if (_scannerLog != null)
                _scannerLog.Dispose();

            if (networkConnection != null && networkConnection.connectionStatus == NetworkConnection.ConnectionStatus.Connected)
            {
                networkConnection.LogOut("STARTDAT 00026 PS05 ENDDAT");
                networkConnection.logoutDone.WaitOne();

                networkConnection.Close();
                networkConnection.closeDone.WaitOne();

                networkConnection = null;
            }

            btConnect.Text = "Connect";
        }

        private void SendNotification(string message, string host, string port)
        {
            ServerConnections.Keys k = ServerConnections.GetKeys();
            foreach (ServerConnections.KeysDetails kd in k.KeysList )
            {
                push.QueueNotification(new AppleNotification()
                                      .ForDeviceToken(kd.key)
                                      .WithAlert(message)
                                      .WithSound("true")
                                      .WithCustomItem("host", host)
                                      .WithCustomItem("port", port));
            }
        }

        static void DeviceSubscriptionChanged(object sender, string oldSubscriptionId, string newSubscriptionId, INotification notification)
        {
            //Currently this event will only ever happen for Android GCM
            Console.WriteLine("Device Registration Changed:  Old-> " + oldSubscriptionId + "  New-> " + newSubscriptionId + " -> " + notification);
        }

        static void NotificationSent(object sender, INotification notification)
        {
            Console.WriteLine("Sent: " + sender + " -> " + notification);
        }

        static void NotificationFailed(object sender, INotification notification, Exception notificationFailureException)
        {
            Console.WriteLine("Failure: " + sender + " -> " + notificationFailureException.Message + " -> " + notification);
        }

        static void ChannelException(object sender, IPushChannel channel, Exception exception)
        {
            Console.WriteLine("Channel Exception: " + sender + " -> " + exception);
        }

        static void ServiceException(object sender, Exception exception)
        {
            Console.WriteLine("Channel Exception: " + sender + " -> " + exception);
        }

        static void DeviceSubscriptionExpired(object sender, string expiredDeviceSubscriptionId, DateTime timestamp, INotification notification)
        {
            Console.WriteLine("Device Subscription Expired: " + sender + " -> " + expiredDeviceSubscriptionId);
        }

        static void ChannelDestroyed(object sender)
        {
            Console.WriteLine("Channel Destroyed for: " + sender);
        }

        static void ChannelCreated(object sender, IPushChannel pushChannel)
        {
            Console.WriteLine("Channel Created for: " + sender);
        }

        public class Settings
        {
            public string host { get; set; }
            public string port { get; set; }
            public string pass { get; set; }
            public string lport { get; set; }
            public string mins  { get; set; }
            public string secs { get; set; }
        }

        private void saveSettings()
        {
            var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var filename = Path.Combine(documents, "proscanalert_settings.xml");

            Encryption enc = new Encryption();

            Settings s = new Settings();
            s.host = tbServerHost.Text;
            s.port = tbServerPort.Text;
            s.lport = tbLocalPort.Text;
            s.pass = enc.Encrypt(tbPassword.Text);
            s.mins = tbMins.Text;
            s.secs = tbSecs.Text;

            XmlSerializer serializer = new XmlSerializer(typeof(Settings));
            TextWriter textWriter = new StreamWriter(filename);
            serializer.Serialize(textWriter, s);
            textWriter.Close();

            s = null;
            enc = null;
        }

        private void loadSettings()
        {
            var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var filename = Path.Combine(documents, "proscanalert_settings.xml");

            if (File.Exists(filename))
            {
                Settings s = new Settings();

                XmlSerializer deserializer = new XmlSerializer(typeof(Settings));
                TextReader textReader = new StreamReader(filename, Encoding.UTF8);
                s = (Settings)deserializer.Deserialize(textReader);
                textReader.Close();

                Encryption enc = new Encryption();

                tbServerHost.Text = s.host;
                tbServerPort.Text = s.port;
                //tbLocalPort.Text = s.lport;
                tbPassword.Text = enc.Decrypt(s.pass);
                tbMins.Text = string.IsNullOrEmpty(s.mins) ? "60" : s.mins;
                tbSecs.Text = string.IsNullOrEmpty(s.secs) ? "30" : s.secs;

                enc = null;
                s = null;
            }
        }

        private void saveListSystems()
        {
            var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var filename = Path.Combine(documents, "proscanalert_systems.xml");

            Systems s = new Systems();
            s.SystemsList = new List<SystemsDetails>();

            for (int i = 0; i < checkedListBox1.Items.Count; i++)
            {
                SystemsDetails sd = new SystemsDetails();
                sd.system_checked = checkedListBox1.GetItemChecked(i);
                sd.system_name = checkedListBox1.GetItemText(checkedListBox1.Items[i]);

                s.SystemsList.Add(sd);
            }

            XmlSerializer serializer = new XmlSerializer(typeof(Systems));
            TextWriter textWriter = new StreamWriter(filename);
            serializer.Serialize(textWriter, s);
            textWriter.Close();

            s = null;
        }

        private void loadListSystems()
        {
            var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var filename = Path.Combine(documents, "proscanalert_systems.xml");

            if (File.Exists(filename))
            {
                Systems s = new Systems();

                XmlSerializer deserializer = new XmlSerializer(typeof(Systems));
                TextReader textReader = new StreamReader(filename, Encoding.UTF8);
                s = (Systems)deserializer.Deserialize(textReader);
                textReader.Close();

                foreach (SystemsDetails sd_tmp in s.SystemsList)
                {
                    checkedListBox1.Items.Add(sd_tmp.system_name, sd_tmp.system_checked);
                }

                s = null;
            }
        }

        private void btServerStart_Click(object sender, EventArgs e)
        {
            if (ServerConnections.ServerStarted)
            {
                listBox1.Items.Add(string.Format("{0} (SERVER) Stopping server", DateTime.Now));
                listBox1.TopIndex = listBox1.Items.Count - 1;
                StopServer();
                btServerStart.Text = "Start";
            }
            else
            {
                listBox1.Items.Add(string.Format("{0} (SERVER) Starting server", DateTime.Now));
                listBox1.TopIndex = listBox1.Items.Count - 1;
                new Thread(() => ServerConnections.StartListening(tbLocalPort.Text.Trim())).Start();
                btServerStart.Text = "Stop";
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            StopAll();
        }

        private void btRetrieveSystems_Click(object sender, EventArgs e)
        {
            if (networkConnection == null)
                return;

            if (networkConnection.connectionStatus != NetworkConnection.ConnectionStatus.Connected)
                return;

            listBox1.Items.Add(string.Format("{0} (SYSTEMS) Downloading system database...", DateTime.Now));
            listBox1.TopIndex = listBox1.Items.Count - 1;

            networkConnection.Send("STARTDAT 00044 PS40 4002 D/L DATABASE ENDDAT");
            networkConnection.sendDone.WaitOne();
            _scannerScreen.systemStep1.Reset();
            _scannerScreen.systemStep1.WaitOne();

            networkConnection.Send("STARTDAT 00031 PS40 4003 ENDDAT");
            networkConnection.sendDone.WaitOne();
            _scannerScreen.systemStep1.Reset();
            _scannerScreen.systemStep1.WaitOne();

            networkConnection.Send("STARTDAT 00035 PS40 SCT\rSIH\r ENDDAT");
            networkConnection.sendDone.WaitOne();
            _scannerScreen.systemStep3.Reset();
            _scannerScreen.systemStep3.WaitOne();

            for (int i = 1; i <= _scannerScreen.nbSystems; i++)
            {
                networkConnection.Send(string.Format("STARTDAT 00036 PS40 SIN,{0}\r ENDDAT", _scannerScreen.nextSystem.PadLeft(5, ' ')));
                networkConnection.sendDone.WaitOne();
                _scannerScreen.systemStep4.Reset();
                _scannerScreen.systemStep4.WaitOne();                         
            }

            networkConnection.Send("STARTDAT 00031 PS40 4004 ENDDAT");
            networkConnection.sendDone.WaitOne();

            checkedListBox1.Items.Clear();
            foreach (string s in _scannerScreen.systemsList)
            {
                checkedListBox1.Items.Add(s);
            }

            listBox1.Items.Add(string.Format("{0} (SYSTEMS) Done.", DateTime.Now));
            listBox1.TopIndex = listBox1.Items.Count - 1;
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            SendNotification(string.Format("High activity detected\n{1}\n{2}\n{3}\n{0}\nTap here to connect.",
                                    DateTime.Now,
                                    "TEST",
                                    "TEST",
                                    "TEST"), "eluderwrx.no-ip.info", "5000");
        }
    }
}
