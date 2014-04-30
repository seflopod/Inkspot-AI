// Logger.cs
// by: Peter Bartosch <bartoschp@gmail.com>
using UnityEngine;
using System.IO;
using System.Collections.Generic;

namespace Inkspot.Utility
{
	/// <summary>
	/// A wrapper for <c>UnityEngine.Debug.Log</c>.  Looking through the Unity
	/// debug log is a nightmare, so this creates a separate file that can be
	/// parsed.
	/// </summary>
	public static class Logger
	{
		///<summary>The level of verbosity for logging.</summary>
		public enum Verbosity
		{
			VeryHigh = 5,
			High = 4,
			Medium = 3,
			Low = 2,
			VeryLow = 1
		};

		private static readonly byte[] crlf = new byte[2] {0x0D, 0x0A};
		private static bool _winEnd = true;
		private static Verbosity _logLevel = Verbosity.Medium;
		private static bool _loggingOn = true;
		private static string _defaultFileName = @"logger.log";

		//need a queue to avoid simultaneous access issues.  And for those of
		//you who may be grading this, just know that I didn't think of this
		//until AFTER the exceptions were thrown and I had no idea why my
		//logging class was throwing fits.  In other words, I suck with thread
		//safety.
		private static Queue<KeyValuePair<string, bool>> _msgQueue = new Queue<KeyValuePair<string,bool>>();

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="Inkspot.Utility.Logger"/> use windows line endings (CRLF).
		/// </summary>
		/// <value><c>true</c> if use windows line end; otherwise, <c>false</c>.</value>
		/// <remarks>
		/// If this is false, CR is used instead of CRLF.
		/// </remarks>
		public static bool UseWindowsLineEnd
		{
			get { return _winEnd; }
			set { _winEnd = value; }
		}

		/// <summary>
		/// Gets or sets the log level.
		/// </summary>
		/// <value>The log level.</value>
		/// <remarks>
		/// The level of logging determines the maximum level logged.
		/// </remarks>
		public static Verbosity LogLevel
		{
			get { return _logLevel; }
			set {_logLevel = value; }
		}

		/// <summary>
		/// Gets or sets a value indicating whether logging is on or not.
		/// </summary>
		/// <value><c>true</c> if logging on; otherwise, <c>false</c>.</value>
		public static bool LoggingOn
		{
			get { return _loggingOn; }
			set { _loggingOn = value; }
		}

		/// <summary>
		/// Gets or sets the default name of the log file.  This can contain a
		/// path in the name.
		/// </summary>
		/// <value>The default name of the file.</value>
		public static string DefaultFileName
		{
			get { return _defaultFileName; }
			set { _defaultFileName = value; }
		}

		/// <summary>
		/// Logs the message.
		/// </summary>
		/// <param name="msg">Message to write.</param>
		/// <param name="fileName">File name to write the message.</param>
		/// <param name="mode">The mode for writing to the file.</param>
		/// <param name="showInUnity">If set to <c>true</c> show in Unity debug console.</param>
		/// <param name="minLogLevel">Minimum log level.</param>
		public static void LogMessage(string msg, string fileName, FileMode mode, bool showInUnity, Verbosity minLogLevel)
		{
			if(LoggingOn && LogLevel >= minLogLevel)
			{
				try
				{
					using(FileStream f = File.Open(fileName, mode, FileAccess.Write))
					{
						System.Text.ASCIIEncoding ae = new System.Text.ASCIIEncoding();

						//first clear any messages in the message queue
						while(_msgQueue.Count > 0)
						{
							string qmsg = _msgQueue.Peek().Key;
							bool show = _msgQueue.Dequeue().Value;
							f.Write(ae.GetBytes(qmsg), 0, qmsg.Length);
							f.Write(crlf, 0, ((_winEnd)?2:1));
							if(show) Debug.Log(qmsg);
						}

						f.Write(ae.GetBytes(msg), 0, msg.Length);
						f.Write(crlf, 0, ((_winEnd)?2:1));
						if(showInUnity) Debug.Log(msg);
					}
				}
				catch(IOException ioe)
				{
					//This is a workaround for locking issues that can arise
					//from using "using".
					if(ioe.Message.Contains("Sharing violation"))
					{
						_msgQueue.Enqueue(new KeyValuePair<string, bool>(msg, showInUnity));
					}
					else
					{
						throw;
					}
				}
			}
		}

		public static void LogMessage(string msg, string fileName, FileMode mode, bool showInUnity)
		{
			Logger.LogMessage(msg, fileName, mode, showInUnity, Verbosity.Medium);
		}

		public static void LogMessage(string msg, string fileName, FileMode mode)
		{
			Logger.LogMessage(msg, fileName, mode, true);
		}

		public static void LogMessage(string msg, string fileName, Verbosity minLogLevel)
		{
			Logger.LogMessage(msg, fileName, FileMode.Append, true, minLogLevel);
		}

		public static void LogMessage(string msg, string fileName)
		{
			Logger.LogMessage(msg, fileName, FileMode.Append, true);
		}

		public static void LogMessage(string msg, Verbosity minLogLevel)
		{
			Logger.LogMessage(msg, _defaultFileName, FileMode.Append, true, Verbosity.Medium);
		}

		public static void LogMessage(string msg)
		{
			Logger.LogMessage(msg, _defaultFileName, FileMode.Append, true);
		}
	}
}