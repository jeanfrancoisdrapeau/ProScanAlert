using System;
using System.IO;
using System.Linq;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.Text;

namespace ProScanAlert
{
	public class ScannerLog
	{
		//private const int MIN_SECONDS_BEFORE_ALERT = 30;
		//private const int MIN_MINUTES_BEFORE_NEXT_ALERT = 30;

		private class messageStruct
		{
			public byte[] all_data { get; set; }
			public int length { get; set; }
		}

		private class receptionList
		{
			public string sys { get; set; }
			public DateTime time { get; set; }
		}

		public class logsDetails
		{
			public string sys { get; set; }
			public string grp { get; set; }
			public long secs { get; set; }
			public DateTime added { get; set; }
		}

		[Serializable]
		[XmlRoot("Logs")]
		public class Logs
		{
			[XmlArray("Logs"), XmlArrayItem(typeof(LogsDisplay), ElementName = "LogsDisplay")]
			public List<LogsDisplay> LogsList = new List<LogsDisplay> ();
		}

		[Serializable]
		public class LogsDisplay
		{
			public string sys { get; set; }
			public string grp { get; set; }
			public bool alert { get; set; }
			public DateTime lastalert { get; set; }
		}

		static Logs _log = new Logs();
		public static Logs log { get { return _log; } }

		public static string _currentAlert_Sys = string.Empty;
		public static string _currentAlert_Grp = string.Empty;
		public static bool _currentAlert_Alert = false;

		public static bool CurrentAlert_Alert { 
			get { return _currentAlert_Alert; }
			set { _currentAlert_Alert = value;
				_currentAlert_Sys = string.Empty;
				_currentAlert_Grp = string.Empty;
			}
		}
		public static string CurrentAlert_Sysgrp { get { return string.Format("{0}-{1}", _currentAlert_Sys, _currentAlert_Grp); } }

		private BlockingCollection<messageStruct> _listDataBuffer_Log;

		private static List<logsDetails> _listLogDetails_Log;
        public static List<logsDetails> All_Log { get { return _listLogDetails_Log; } }
		private string _lastSys = string.Empty;
		private string _lastGrp = string.Empty;
		private DateTime _lastStart;

		public ScannerLog ()
		{
			_listDataBuffer_Log = new BlockingCollection<messageStruct> ();
			_listLogDetails_Log = new List<logsDetails> ();
		}

		public void Dispose()
		{
			_listDataBuffer_Log = null;
			_listLogDetails_Log = null;
		}

		public void processData(byte[] message, int messageLength)
		{
			messageStruct ms = new messageStruct();
			ms.all_data = new byte[messageLength];
			Array.ConstrainedCopy (message, 0, ms.all_data, 0, ms.all_data.Length);;
			ms.length = messageLength;

			// Add messagestruct to the real list that is thread safe
			_listDataBuffer_Log.Add (ms);
			ms = null;

			if (_listDataBuffer_Log.Count > 0) {
				if (_listDataBuffer_Log.ElementAt<messageStruct> (0) != null) {
					// DATA
					try
					{
						// Extract the first message
						byte[] data = new byte[_listDataBuffer_Log.ElementAt<messageStruct> (0).length];
						Array.ConstrainedCopy (_listDataBuffer_Log.ElementAt<messageStruct> (0).all_data, 0, data, 0, data.Length);

						string sdata = bytesToString (data).Trim ();
						string scase = sdata.Substring (15, 4);

						//Console.WriteLine(sdata);

						string[] ps30;
						string[] ps30_system_details;

						// Based on the metadata type
						switch (scase) {
						case "PS30":

							ps30 = sdata.Split ('\r');

							//Console.WriteLine(ps30[3]);

							ps30_system_details = ps30 [3].Split (',');

							if (ps30_system_details[5].Trim() != _lastSys ||
								ps30_system_details[6].Trim() != _lastGrp) {

								if (_lastSys.Length > 0 &&
									_lastGrp.Length > 0) {

									logsDetails ld = new logsDetails();
									ld.sys = _lastSys;
									ld.grp = _lastGrp;

									TimeSpan ts = DateTime.Now - _lastStart;
									ld.secs = (long)ts.TotalSeconds;

									ld.added = DateTime.Now;

									_listLogDetails_Log.Add(ld);

									// Add to list for display
									int i = -1;

									i = _log.LogsList.FindIndex(c => c.sys == _lastSys &&
										c.grp == _lastGrp);

									if (i == -1)
									{
										LogsDisplay Item = new LogsDisplay();
										Item.sys = _lastSys;
										Item.grp = _lastGrp;
										Item.alert = false;
										Item.lastalert = DateTime.Now;

										_log.LogsList.Add(Item);
									}
								}

								_lastSys = ps30_system_details[5].Trim();
								_lastGrp = ps30_system_details[6].Trim();
								_lastStart = DateTime.Now;
							}

							//Console.WriteLine("-- {0} ----------------------------------------", 
							//	_listLogDetails_Log.Count);

							//foreach (logsDetails ld in _listLogDetails_Log)
							//{
								//Console.WriteLine("{0}-{1}: {2}", ld.sys, ld.grp, ld.added);
							//}

							//Console.WriteLine("-- SUMS:"); 

							// Get unique sys/grp values
							if (true) {
								var selectedSysgrp =
									_listLogDetails_Log.Select(d => new {d.sys, d.grp} ).Distinct();

								foreach (var sysgrp in selectedSysgrp)
								{
									var result = _listLogDetails_Log.Where(d => d.sys == sysgrp.sys &&
										d.grp == sysgrp.grp);

									long secsTotal = result.Sum(span => span.secs);

									//Console.WriteLine("{0}-{1}: {2} secs", sysgrp.sys, sysgrp.grp, secsTotal);

									int i = _log.LogsList.FindIndex(l => l.sys == sysgrp.sys &&
										l.grp == sysgrp.grp);

									if (i != -1) {
										if (true) {
                                            int MIN_SECONDS_BEFORE_ALERT;
                                            int.TryParse(Form1._Secs.Text, out MIN_SECONDS_BEFORE_ALERT);
											if (secsTotal >= MIN_SECONDS_BEFORE_ALERT) {
												TimeSpan ts = DateTime.Now - log.LogsList[i].lastalert;

                                                int MIN_MINUTES_BEFORE_NEXT_ALERT;
                                                int.TryParse(Form1._Mins.Text, out MIN_MINUTES_BEFORE_NEXT_ALERT);
												if (ts.TotalMinutes >= MIN_MINUTES_BEFORE_NEXT_ALERT) {
													//Console.WriteLine("ALERT!: {0}-{1}", sysgrp.sys, sysgrp.grp);

													_currentAlert_Sys = sysgrp.sys;
													_currentAlert_Grp = sysgrp.grp;
													_currentAlert_Alert = true;

													log.LogsList[i].lastalert = DateTime.Now;
												}
											}
										}
									}
								}
							}

							// Remove old log from list
							DateTime dateTime = DateTime.Now;
							TimeSpan timeSpan = TimeSpan.FromSeconds(60);

							dateTime = new DateTime(
								dateTime.Ticks - timeSpan.Ticks, 
								dateTime.Kind
							);

							_listLogDetails_Log.RemoveAll(x => x.added <= dateTime);

							break;
						}
					}  catch (Exception ex) {
						Console.WriteLine (ex.Message);
					}
				}

				// Remove the up-most message from list
				// Thread safe
				_listDataBuffer_Log.Take ();
			}  
		}

		public static void SaveLogs()
		{
			var documents = Environment.GetFolderPath (Environment.SpecialFolder.MyDocuments);
			var filename = Path.Combine (documents, "proscanmobile_logs.xml");

			Logs s_tmp = new Logs ();
			s_tmp.LogsList = new List<LogsDisplay> ();

			foreach (LogsDisplay sd_tmp in _log.LogsList) {
				s_tmp.LogsList.Add (new LogsDisplay () {
					sys = XmlConvert.EncodeName(sd_tmp.sys),
					grp = XmlConvert.EncodeName(sd_tmp.grp),
					alert =  sd_tmp.alert,
					lastalert = sd_tmp.lastalert
				} );
			}

			XmlSerializer serializer = new XmlSerializer (typeof(Logs));
			TextWriter textWriter = new StreamWriter (filename);
			serializer.Serialize (textWriter, s_tmp);
			textWriter.Close ();

			s_tmp = null;
		}

		public static void GetLogs()
		{
			var documents = Environment.GetFolderPath (Environment.SpecialFolder.MyDocuments);
			var filename = Path.Combine (documents, "proscanmobile_logs.xml");

			if (File.Exists (filename)) {

				Logs s_tmp = new Logs ();

				XmlSerializer deserializer = new XmlSerializer (typeof(Logs));
				TextReader textReader = new StreamReader (filename, Encoding.UTF8);
				s_tmp = (Logs)deserializer.Deserialize (textReader);
				textReader.Close ();

				_log = new Logs ();
				_log.LogsList = new List<LogsDisplay> ();

				foreach (LogsDisplay sd_tmp in s_tmp.LogsList) {
					_log.LogsList.Add (new LogsDisplay () {
						sys = XmlConvert.DecodeName(sd_tmp.sys),
						grp = XmlConvert.DecodeName(sd_tmp.grp),
						alert = sd_tmp.alert,
						lastalert = sd_tmp.lastalert
					} );
				}

				s_tmp = null;
			}
		}

		private string bytesToString(byte[] b)
		{
			// Return a string encoded byte array
			return System.Text.Encoding.ASCII.GetString (b);
		}
	}
}