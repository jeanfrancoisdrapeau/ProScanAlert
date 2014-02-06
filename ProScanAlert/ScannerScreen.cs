using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;

namespace ProScanAlert
{
	public class ScannerScreen
	{
		private class messageStruct
		{
			public byte[] all_data { get; set; }
			public int length { get; set; }
		}

        public ManualResetEvent systemStep1 = new ManualResetEvent(false);
        public ManualResetEvent systemStep3 = new ManualResetEvent(false);
        public ManualResetEvent systemStep4 = new ManualResetEvent(false);
        public ManualResetEvent systemStep5 = new ManualResetEvent(false);
        public ManualResetEvent systemStep6 = new ManualResetEvent(false);

        public List<string> systemsList;
        public int nbSystems;
        public string nextSystem;
        public string nextGroup;

        private string systemName;

		private string _scannerScreen_Model;
		public string scannerScreen_Model { get { return _scannerScreen_Model; } }

		private int _scannerScreen_Signal;
		public int scannerScreen_Signal { get { return _scannerScreen_Signal; } }

		private string _scannerScreen_Line1;
		public string scannerScreen_Line1 { get { return _scannerScreen_Line1; } }
		private string _scannerScreen_Line2;
		public string scannerScreen_Line2 { get { return _scannerScreen_Line2; } }
		private string _scannerScreen_Line3;
		public string scannerScreen_Line3 { get { return _scannerScreen_Line3; } }
		private string _scannerScreen_Line4;
		public string scannerScreen_Line4 { get { return _scannerScreen_Line4; } }
		private string _scannerScreen_Line5;
		public string scannerScreen_Line5 { get { return _scannerScreen_Line5; } }

	 	private BlockingCollection<messageStruct> _listDataBuffer_Screen;

		public ScannerScreen ()
		{
			_listDataBuffer_Screen = new BlockingCollection<messageStruct> ();
		}

		public void Dispose()
		{
			_listDataBuffer_Screen = null;
		}

		public void processData(byte[] message, int messageLength)
		{
			messageStruct ms = new messageStruct();

            //Console.WriteLine(bytesToString(message));

			ms.all_data = new byte[messageLength];
			Array.ConstrainedCopy (message, 0, ms.all_data, 0, ms.all_data.Length);;
			ms.length = messageLength;

			// Add messagestruct to the real list that is thread safe
			_listDataBuffer_Screen.Add (ms);
			ms = null;

			if (_listDataBuffer_Screen.Count > 0) {
				if (_listDataBuffer_Screen.ElementAt<messageStruct> (0) != null) {
					// DATA
					try
					{
						// Extract the first message
						byte[] data = new byte[_listDataBuffer_Screen.ElementAt<messageStruct> (0).length];
						Array.ConstrainedCopy (_listDataBuffer_Screen.ElementAt<messageStruct> (0).all_data, 0, data, 0, data.Length);

						string sdata = bytesToString (data).Trim ();
						string scase = sdata.Substring (15, 4);

						//Console.WriteLine(scase);

						string[] ps01;
						string[] ps02;
						string[] ps30;
                        string[] ps40;
						string[] ps01_system_details;
						string[] ps02_system_details;
						string[] ps30_system_details;
                        string[] ps40_system_details;

						// Based on the metadata type
						switch (scase) {
						case "PS01":
							ps01 = sdata.Split ('\r');
							ps01_system_details = ps01 [0].Split (',');

							_scannerScreen_Model = ps01_system_details [1].Trim ();

							break;
						case "PS02":
							ps02 = sdata.Split ('\r');
							ps02_system_details = ps02 [0].Split (',');

							_scannerScreen_Model = string.Format("{0} *ADMIN*", ps02_system_details [1].Trim ());

							break;
						case "PS30":
							// The scanner screen
							// Scanner model, Signal strength
							// Line 1: System
							// Line 2: Group
							// Line 3: Frequency
							// Line 4: Activated systems
							// Line 5: Activated groups per system

							ps30 = sdata.Split ('\r');

							ps30_system_details = ps30 [6].Split (',');

							int.TryParse(ps30_system_details[1].Trim(), out _scannerScreen_Signal);

							ps30_system_details = ps30 [2].Split (',');
							_scannerScreen_Line1 = ps30_system_details [4].Trim (); 

							string line2 = ps30_system_details [6];
							line2 = line2.Substring(0, line2.Length - 3);
							_scannerScreen_Line2 = line2; 

							_scannerScreen_Line3 = ps30_system_details [8].Trim (); 
							_scannerScreen_Line4 = ps30_system_details [10].Trim ();
							_scannerScreen_Line5 = ps30_system_details [12].Trim ();
							break;
                        case "PS40":
                            Console.WriteLine("<--" + sdata);
                            ps40 = sdata.Split ('\r');
							ps40_system_details = ps40 [0].Split (' ');

                            if (sdata == "STARTDAT 00028 PS40   ENDDAT")
                                systemStep1.Set();
                            
                            if (ps40_system_details[3].Substring(0, 3) == "SCT")
                            {
                                systemsList = new List<string>();

                                string[] temp = ps40_system_details[3].Split(',');
                                nbSystems = Convert.ToInt32(temp[1]);

                                ps40_system_details = ps40[1].Split(' ');

                                temp = ps40_system_details[0].Split(',');

                                nextSystem = temp[1];
                                systemStep3.Set();
                            }

                            if (ps40_system_details[3].Substring(0, 3) == "SIN")
                            {
                                ps40_system_details = ps40[0].Split(',');
                                systemName = ps40_system_details[2];

                                systemsList.Add(string.Format("{0}", systemName));

                                if (ps40_system_details[13] != "-1")
                                    nextSystem = ps40_system_details[13];
                                systemStep4.Set();
                            }

                            break;
						}
					} catch {
					}
				}

				// Remove the up-most message from list
				// Thread safe
				_listDataBuffer_Screen.Take ();
			} 
		}

		private string bytesToString(byte[] b)
		{
			// Return a string encoded byte array
			return System.Text.Encoding.ASCII.GetString (b);
		}
	}
}

