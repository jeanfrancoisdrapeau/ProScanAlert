using System;
using System.Drawing;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Collections;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace ProScanAlert
{	
	/// <summary>
	/// Class for the Network connection.
	/// </summary>
	/// <description>
	/// This class is used to maintain a connection to a PSM server
	/// </description>
	public class NetworkConnection
	{
		private Socket _tcpSocket;

		public enum ConnectionStatus
		{
			Disconnected,
			Connected,
			Error
		}
		private ConnectionStatus _connectionStatus;
		public ConnectionStatus connectionStatus { get { return _connectionStatus; } }
		public string _connectionStatusMessage;

		public enum SendStatus
		{
			Ok,
			Error
		}
		private SendStatus _sendStatus;
		public SendStatus sendStatus { get { return _sendStatus; } }
		public string _sendStatusMessage;

		public enum ReceiveStatus
		{
			Ok,
			Error
		}
		private SendStatus _receiveHttpStatus;
		public SendStatus receiveHttpStatus { get { return _receiveHttpStatus; } }
		public string _receiveHttpStatusMessage;

		private SendStatus _receiveDataStatus;
		public SendStatus receiveDataStatus { get { return _receiveDataStatus; } }
		public string _receiveDataStatusMessage;

		public enum LoginStatus
		{
			LoggedIn,
			LoggedOut,
			Error
		}
		private LoginStatus _loginStatus;
		public LoginStatus loginStatus { get { return _loginStatus; } }
		public string _loginStatusMessage;

		public enum ReceiveType
		{
			Http,
			Data
		}

		public DateTime _startTime;

		private ManualResetEvent _connectDone = new ManualResetEvent(false);
		public ManualResetEvent connectDone { get { return _connectDone; } }
		private ManualResetEvent _closeDone = new ManualResetEvent(false);
		public ManualResetEvent closeDone { get { return _closeDone; } }

		private ManualResetEvent _sendDone = new ManualResetEvent(false);
		public ManualResetEvent sendDone { get { return _sendDone; } }

		private ManualResetEvent _receiveDone = new ManualResetEvent(false);
        public ManualResetEvent receiveDone { get { return _receiveDone; } }

		private ManualResetEvent _loginDone = new ManualResetEvent(false);
		public ManualResetEvent loginDone { get { return _loginDone; } }
		private ManualResetEvent _logoutDone = new ManualResetEvent(false);
		public ManualResetEvent logoutDone { get { return _logoutDone; } }

		public class StateObject {
			// Client socket.
			public Socket workSocket = null;
			// Size of receive buffer.
			public const int BufferSize = 65535;
			// Receive buffer.
			public byte[] buffer = new byte[BufferSize];
		}

		private string _httpResponse = string.Empty;

		private int _bytesReceived;
		public int bytesReceived { get { return _bytesReceived; } }

		private ReadWriteBuffer _connectionBuffer;
		public ReadWriteBuffer connectionBuffer { get { return _connectionBuffer; } }

		private const string CONST_HTTP_OK = "HTTP/1.0 200 OK";

		/// <summary>
		/// Initializes a new instance of the <see cref="ProScanMobile.NetworkConnection"/> class.
		/// </summary>
		/// <description>
		/// Creates a new _tcpSocket and tries to connect to Host/Port 
		/// </description>
		/// <param name="host">Host</param>
		/// <param name="port">Port</param>
		public NetworkConnection (string host, int port)
		{
			_connectDone.Reset ();

			try
			{
				IPHostEntry ipHostInfo = Dns.GetHostEntry (host);
				IPAddress ipAddress = ipHostInfo.AddressList [0];
				IPEndPoint remoteEP = new IPEndPoint (ipAddress, port);

				_tcpSocket = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				_tcpSocket.Blocking = true;

				var result = _tcpSocket.BeginConnect (remoteEP, null, null);

				bool success = result.AsyncWaitHandle.WaitOne (5000, true);
				if (success) {
					_tcpSocket.EndConnect (result);
					_connectionStatus = ConnectionStatus.Connected;
					_connectionStatusMessage = "Connected.";
				} else {
					_tcpSocket.Close ();
					_connectionStatus = ConnectionStatus.Error;
					_connectionStatusMessage = "Connection timed out.";
				}
			} catch {
				_connectionStatus = ConnectionStatus.Error;
				_connectionStatusMessage = string.Format("An error occured connecting to {0} on port {1}.", host, port);
                _connectDone.Set();
				return;
			} finally {
				_connectDone.Set ();
			}
		}

		/// <summary>
		/// Send login message to a PSM server
		/// </summary>
		/// <description>
		/// You can login to a PSM server in two ways:
		/// 1. Anonymous : You will only hear the scanner sound playback and see the scanner screen.
		/// 2. Admin : You will have full control over the scanner
		/// </description>
		/// <param name="m">Message</param>
		/// <seealso cref="ProScanMobile.Encryption"/>
		public void Login(string m)
		{
			_loginDone.Reset ();
			_httpResponse = string.Empty;

			Send (m);
			_sendDone.WaitOne ();

			if (_sendStatus == SendStatus.Ok) {
				Receive (ReceiveType.Http);
				_receiveDone.WaitOne ();

				if (_httpResponse == CONST_HTTP_OK) {

					_connectionBuffer = new ReadWriteBuffer (65535);
					_bytesReceived = 0;

					Receive (ReceiveType.Data);
					_receiveDone.WaitOne ();

					string[] message = bytesTostring(_connectionBuffer.Read (_connectionBuffer.Count, true)).Split(' ');

					if (message [2] == "PS20") {
						_loginStatus = LoginStatus.Error;
						_loginStatusMessage = "Connection refused.";
					} else {
						_loginStatus = LoginStatus.LoggedIn;
						_loginStatusMessage = "Logged in.";
					}
				} else {
					_loginStatus = LoginStatus.Error;
					_loginStatusMessage = "Connection refused.";
				}
			} else {
				_loginStatus = LoginStatus.Error;
				_loginStatusMessage = "Connection refused.";
			}

			_loginDone.Set ();
		}

		/// <summary>
		/// Send logout message to a PSM server
		/// </summary>
		/// <description>
		/// This method sends a message to the PSM server to logout.
		/// This is usually followed by <see cref="Close"/>
		/// </description>
		/// <param name="m">Message</param>
		/// <example>
		/// LogOut("STARTDAT 00026 PS05 ENDDAT"): Typical logout message
		/// </example>
		public void LogOut(string m)
		{
			_logoutDone.Reset ();

			Send (m);
			_sendDone.WaitOne ();

			_loginStatus = LoginStatus.LoggedOut;
			_loginStatusMessage = "Logged out.";

			_logoutDone.Set ();
		}

		/// <summary>
		/// Close this instance.
		/// </summary>
		/// <description>
		/// Close the connection to the PSM server
		/// </description>
		public void Close()
		{
			_closeDone.Reset ();

			if (_tcpSocket != null) {

				_tcpSocket.Close ();

				_connectionStatus = ConnectionStatus.Disconnected;
				_connectionStatusMessage = "Closed.";

				_loginStatus = LoginStatus.LoggedOut;
				_loginStatusMessage = "Logged out.";

				_connectionBuffer = null;

				_tcpSocket = null;
			}

			_closeDone.Set ();
		}

		/// <summary>
		/// Receive data on the TCP connection
		/// </summary>
		/// <description>
		/// This method has two sections based on what we want to receive (<see cref="ReceiveType"/>)
		/// 1. HTTP : We want to receive a HTTP response from the server
		/// 2. DATA : We are waiting data from the server
		/// </description>
		/// <param name="rt">Receive Type</param>
		public void Receive(ReceiveType rt) 
		{
			_receiveDone.Reset ();

			// Create the state object.
			StateObject state = new StateObject();
			state.workSocket = _tcpSocket;

			// Begin receiving the data from the remote device.
			if (rt == ReceiveType.Http) {
				_receiveHttpStatus = SendStatus.Ok;
				_tcpSocket.BeginReceive (state.buffer, 0, StateObject.BufferSize, 0,
					new AsyncCallback (receiveCallBackHttp), state);
			} else {
				_tcpSocket.BeginReceive (state.buffer, 0, StateObject.BufferSize, 0,
					new AsyncCallback (receiveCallBackData), state);
			}
		}

		/// <summary>
		/// Callback method to receive a HTTP response
		/// </summary>
		/// <param name="ar">Async Result</param>
		private void receiveCallBackHttp( IAsyncResult ar ) {
			try {
				StateObject state = (StateObject) ar.AsyncState;
				Socket client = state.workSocket;

				int bytesRead = client.EndReceive(ar);

				if (bytesRead > 0) {
					byte[] tmpdata = new byte[15];
					Array.Copy(state.buffer, tmpdata, 15);

					_httpResponse = bytesTostring(tmpdata);
				}
			} catch {
				_receiveHttpStatus = SendStatus.Error;
				_receiveHttpStatusMessage = "An error occured receiving http response.";
			} finally {
				_receiveDone.Set();
			}
		}

		/// <summary>
		/// Callback method to receive DATA
		/// </summary>
		/// <param name="ar">Async Result</param>
		private void receiveCallBackData( IAsyncResult ar ) {
			try {
				_receiveDataStatus = SendStatus.Ok;

				StateObject state = (StateObject) ar.AsyncState;
				Socket client = state.workSocket;

				// Read data from the remote device.
				int bytesRead = client.EndReceive(ar);

				if (bytesRead > 0) {
					// Stock number of bytes received so far (to show on screen)
					_bytesReceived += bytesRead;

					//Console.WriteLine("---------- NEW MESSAGE ----------------------------------------------------");
					//Console.WriteLine("---------- Received bytes.........: {0}", bytesRead.ToString());

					// Write bytes read to main data buffer
					byte[] tmpdata = new byte[bytesRead];
					Array.Copy(state.buffer, tmpdata, bytesRead);
					_connectionBuffer.Write(tmpdata);

					//Console.WriteLine("---------- END MESSAGE ----------------------------------------------------");

					// Continue getting data
					client.BeginReceive(state.buffer,0,StateObject.BufferSize,0,
						new AsyncCallback(receiveCallBackData), state);
				}
			} catch {
				_receiveDataStatus = SendStatus.Error;
				_receiveDataStatusMessage = "An error occured receiving scanner data.";
			} finally {
				_receiveDone.Set();
			}
		}

		/// <summary>
		/// Send the specified message to the TCP socket.
		/// </summary>
		/// <param name="m">Message</param>
		public void Send(string m)
		{
			_sendDone.Reset ();

            if (_tcpSocket == null)
                return;

            Console.WriteLine("-->" + m);

			byte[] data = Encoding.ASCII.GetBytes (m);

			_tcpSocket.BeginSend(data, 0, data.Length, 0,
				new AsyncCallback(sendCallback), _tcpSocket);
		}

		/// <summary>
		/// Send callback
		/// </summary>
		/// <param name="ar">Async Result</param>
		private void sendCallback(IAsyncResult ar) 
		{
			try
			{
				Socket client = (Socket) ar.AsyncState;
				client.EndSend(ar);

				_sendStatus = SendStatus.Ok;
				_sendStatusMessage = "Sent.";
			} catch {
				_sendStatus = SendStatus.Error;
				_sendStatusMessage = "An error occured while sending data.";
			} finally {
				_sendDone.Set ();
			}
		}

		/// <summary>
		/// Bytes to string
		/// </summary>
		/// <description>
		/// Converts a bytes array to a string
		/// </description>
		/// <returns>ASCII encoded string</returns>
		/// <param name="b">Bytes array</param>
		private string bytesTostring(byte[] b)
		{
			// Return a string encoded byte array
			return System.Text.Encoding.ASCII.GetString (b);
		}
	}
}

