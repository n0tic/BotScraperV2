#pragma warning disable IDE0063, IDE0018

using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Security;

namespace BotScraperV2
{
    internal static class Core
    {
        #region Static App Information

        //Send debug information to the console.
        public static bool debug = false;

        //Static software information
        public static string softwareName = "BotScraper V2";
        public static string authorName = "N0tiC";
        public static string authorRealName = "Victor R";

        //Time vars
        public static int updateTimerSeconds = 25;
        public static int maxDaysDifference = 7;
        //Static epoch time
        public static DateTime epochTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        //URLs
        public static string scrapeURL = "";
        public static string downloadURL = "";
        public static string uploadURL = "";

        #endregion Static App Information

        #region BotScraperV2

        /// <summary>
        ///Turns seconds into milliseconds
        /// </summary>
        /// <param name="seconds"></param>
        /// <returns></returns>
        internal static int ResolveSecondsToMilliseconds(int seconds) => seconds * 1000;

        /// <summary>
        /// Type of console outputs
        /// </summary>
        internal enum LogType
        {
            Console, // Console outbut - Debug, Error, Warnings
            Worker, // Worker output
            Requested // User Requested Data
        }

        /// <summary>
        /// This will write a message in the console with type, datetime and message with colors.
        /// </summary>
        /// <param name="message">String message</param>
        /// <param name="logType">enum LogType</param>
        internal static void WriteLine(string message, LogType logType = LogType.Console, bool writeLog = true)
        {
            string logMessage = ""; // This is what will be written to the log
            if (logType == LogType.Console)
            {
                Console.ForegroundColor = ConsoleColor.Red; // Set Color
                Console.Write("[Console]");
                logMessage += "[Console]";
            }
            else if (logType == LogType.Worker)
            {
                Console.ForegroundColor = ConsoleColor.DarkMagenta; // Set Color
                Console.Write("[Worker]");
                logMessage += "[Worker]";
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.DarkGreen; // Set Color
                Console.Write("[Requested]");
                logMessage += "[Requested]";
            }
            Console.ForegroundColor = ConsoleColor.Cyan; // Set Color
            Console.Write("[" + DateTime.Now.ToString() + "] "); // Set date
            logMessage += "[" + DateTime.Now.ToString() + "] ";
            Console.ResetColor(); // Reset colors to default
            Console.Write(message); // Message and a new row.
            logMessage += message;
            Console.WriteLine();

            if(writeLog) WriteLog(logMessage); // Write to log if
        }

        /// <summary>
        /// Prints out the logotype and software name + version
        /// </summary>
        internal static void GetLogoType()
        {
            Console.WriteLine(@"  ____        _    _____                                 __      _____
 |  _ \      | |  / ____|                                \ \    / /__ \
 | |_) | ___ | |_| (___   ___ _ __ __ _ _ __   ___ _ __   \ \  / /   ) |
 |  _ < / _ \| __|\___ \ / __| '__/ _` | '_ \ / _ \ '__|   \ \/ /   / /
 | |_) | (_) | |_ ____) | (__| | | (_| | |_) |  __/ |       \  /   / /_
 |____/ \___/ \__|_____/ \___|_|  \__,_| .__/ \___|_|        \/   |____|
                                       | |
                                       |_|                              ");
            Console.WriteLine("\t\t\t\t\t" + softwareName + " " + GetVersion() + " by " + authorName + "\n\r\n\r\n\r");
        }

        /// <summary>
        /// Translates long to DateTime
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        internal static DateTime TranslateLongToDateTime(long time)
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(time); // Gets timespan from long time
            DateTime localDateTime = Core.epochTime.Add(timeSpan).ToLocalTime(); // Adds seconds to epochTime
            return localDateTime; // Returns DateTime
        }

        /// <summary>
        /// Takes boolean and returns string
        /// </summary>
        /// <param name="state"></param>
        /// <returns>ONLINE - OFFLINE</returns>
        internal static string ResolveBool(bool state)
        {
            if (state) return "ONLINE";
            else return "OFFLINE";
        }

        #endregion BotScraperV2

        #region Version

        //Version Data
        public static BuildTypes buildType = BuildTypes.Alpha;
        public static int majorVersion = 0;
        public static int minorVersion = 0;
        public static int buildVersion = 1;

        //Build Types
        public enum BuildTypes
        {
            Alpha,
            Beta,
            Normal
        }

        /// <summary>
        /// Returns a string combined with 3 subversions and type.
        /// </summary>
        /// <returns> x.x.x Type</returns>
        public static string GetVersion() => majorVersion.ToString() + "." + minorVersion.ToString() + "." + buildVersion.ToString() + " " + buildType.ToString();

        #endregion Version

        #region Networking

        /// <summary>
        /// Check if Internet is available
        /// </summary>
        /// <returns>boolean</returns>
        internal static bool InternetAvailable()
        {
            try
            {
                using (var client = new WebClient().OpenRead("http://www.google.com/"))
                {
                    return true; // Return true if website was successfully reached.
                }
            }
            catch (ArgumentNullException x) { WriteLine(x.Message, LogType.Console); return false; }
            catch (WebException x) { WriteLine(x.Message, LogType.Console); return false; }
        }

        /// <summary>
        /// Downloads a string and converts it to an object and finaly returns the object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <returns></returns>
        internal static T Download<T>(string url)
        {
            using (WebClient wc = new WebClient())
            {
                try
                {
                    string tmpString = wc.DownloadString(new Uri(url + "?" + GetUTCTime())); // Get a string with Json-Data with UTC Identifier to bypass cache.
                    return (T)JsonConvert.DeserializeObject<T>(tmpString); // Return Serilized object.
                }
                catch (ArgumentNullException x) { Core.WriteLine(x.Message, LogType.Console); }
                catch (WebException x) { Core.WriteLine(x.Message, LogType.Console); }
                catch (NotSupportedException x) { Core.WriteLine(x.Message, LogType.Console); }
                catch (Exception x) { Core.WriteLine(x.Message, LogType.Console); }
            }
            return default; //  Return "new T();"
        }

        /// <summary>
        /// Return UTCTime as long to be used to get new data instead of cached data from remote hosts.
        /// </summary>
        /// <returns></returns>
        internal static string GetUTCTime()
        {
            System.Int32 unixTimestamp = (System.Int32)(System.DateTime.UtcNow.Subtract(new System.DateTime(1970, 1, 1))).TotalSeconds;
            return unixTimestamp.ToString();
        }

        /// <summary>
        /// Load previous session data from URL then local, returns BotsData. Favours Online Version
        /// </summary>
        /// <returns>BotsData</returns>
        internal static BotsData LoadPreviousSession()
        {
            BotsData local = new BotsData();
            BotsData remote = new BotsData();
            string filename = "bots.json";

            if (File.Exists(GetRootDirectory() + filename))
            {
                try
                {
                    local = JsonConvert.DeserializeObject<BotsData>(File.ReadAllText(GetRootDirectory() + filename));
                }
                catch (ArgumentNullException x) { WriteLine(x.Message, LogType.Console); }
                catch (ArgumentException x) { WriteLine(x.Message, LogType.Console); }
                catch (FileNotFoundException x) { WriteLine(x.Message, LogType.Console); }
                catch(PathTooLongException x) { WriteLine(x.Message, LogType.Console); }
                catch(DirectoryNotFoundException x) { WriteLine(x.Message, LogType.Console); }
                catch(IOException x) { WriteLine(x.Message, LogType.Console); }
                catch(UnauthorizedAccessException x) { WriteLine(x.Message, LogType.Console); }
                catch(NotSupportedException x) { WriteLine(x.Message, LogType.Console); }
                catch(SecurityException x) { WriteLine(x.Message, LogType.Console); }
                catch(JsonException x) { WriteLine(x.Message, LogType.Console); }
            }

            using (WebClient wc = new WebClient())
            {
                string tmpJson = "";
                try
                {
                    tmpJson = wc.DownloadString(downloadURL);

                }
                catch (ArgumentNullException x) { WriteLine(x.Message, LogType.Console); }
                catch (ArgumentException x) { WriteLine(x.Message, LogType.Console); }
                catch (WebException x) { WriteLine(x.Message, LogType.Console); }
                catch (NotSupportedException x) { WriteLine(x.Message, LogType.Console); }

                if (tmpJson != "" && tmpJson != "NoFile" && tmpJson != "NoContent")
                {
                    try
                    {
                        remote = JsonConvert.DeserializeObject<BotsData>(tmpJson);
                    }
                    catch (JsonException x) { WriteLine(x.Message, LogType.Console); }
                }
                else
                {
                    remote = null;
                    WriteLine(tmpJson, LogType.Console);
                }
            }

            if (remote == null) return local; // Favor remote if it isnt null
            else if (remote != null) return remote;
            else return new BotsData();
        }

        /// <summary>
        /// Upload BotsData to remote server
        /// </summary>
        /// <param name="json"></param>
        internal static void UploadJsonData(string json)
        {
            try
            {
                using (WebClient wc = new WebClient()) wc.UploadString(uploadURL, json);
            }
            catch (ArgumentNullException x) { WriteLine(x.Message, LogType.Console); }
            catch (WebException x) { WriteLine(x.Message, LogType.Console); }
        }

        #endregion Networking

        #region Folders & Files

        /// <summary>
        /// Returns root folder
        /// </summary>
        /// <returns></returns>
        internal static string GetRootDirectory() => @AppDomain.CurrentDomain.BaseDirectory;

        /// <summary>
        /// Save BotsData to local file.
        /// </summary>
        /// <param name="json"></param>
        internal static void SaveJsonFile(string json)
        {
            string filename = "bots.json";
            try
            {
                using (StreamWriter sw = File.CreateText(GetRootDirectory() + filename)) sw.Write(json);
            }
            catch (UnauthorizedAccessException x) { WriteLine(x.Message, LogType.Console); }
            catch (ArgumentNullException x) { WriteLine(x.Message, LogType.Console); }
            catch (ArgumentException x) { WriteLine(x.Message, LogType.Console); }
            catch (PathTooLongException x) { WriteLine(x.Message, LogType.Console); }
            catch (DirectoryNotFoundException x) { WriteLine(x.Message, LogType.Console); }
            catch (NotSupportedException x) { WriteLine(x.Message, LogType.Console); }
            catch (IOException x) { WriteLine(x.Message, LogType.Console); }
        }

        /// <summary>
        /// Write message to log
        /// </summary>
        /// <param name="logMessage"></param>
        static void WriteLog(string logMessage)
        {
            try
            {
                using (StreamWriter sw = File.AppendText(GetRootDirectory() + @"\" + DateTime.Now.ToString("dd-MM-yyyy").ToString() + " log.txt"))
                {
                    sw.WriteLine(logMessage);
                    sw.Close();
                }
            }
            catch (UnauthorizedAccessException x) { WriteLine(x.Message, LogType.Console, false); }
            catch (ArgumentNullException x) { WriteLine(x.Message, LogType.Console, false); }
            catch (ArgumentException x) { WriteLine(x.Message, LogType.Console, false); }
            catch (DirectoryNotFoundException x) { WriteLine(x.Message, LogType.Console, false); }
            catch (PathTooLongException x) { WriteLine(x.Message, LogType.Console, false); }
            catch (NotSupportedException x) { WriteLine(x.Message, LogType.Console, false); }
            catch (IOException x) { if(debug) WriteLine(x.Message, LogType.Console, false); }
        }

        #endregion Folders & Files

        #region QuickEdit Options

        /*
         So this part allowes me to enable and disable Console Quick Edit Mode.
         */

        private const uint ENABLE_QUICK_EDIT = 0x0040;

        // STD_INPUT_HANDLE (DWORD): -10 is the standard input device.
        private const int STD_INPUT_HANDLE = -10;

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll")]
        private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

        [DllImport("kernel32.dll")]
        private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

        public static bool SetQuickEdit(bool SetEnabled)
        {
            IntPtr consoleHandle = GetStdHandle(STD_INPUT_HANDLE);

            // Get current console mode
            uint consoleMode;
            if (!GetConsoleMode(consoleHandle, out consoleMode))
            {
                // ERROR: Unable to get console mode.
                return false;
            }

            // Clear the quick edit bit in the mode flags
            if (SetEnabled) consoleMode |= ENABLE_QUICK_EDIT;
            else consoleMode &= ~ENABLE_QUICK_EDIT;

            // Set the new mode
            if (!SetConsoleMode(consoleHandle, consoleMode))
            {
                // ERROR: Unable to set console mode
                return false;
            }

            return true;
        }

        #endregion QuickEdit Options
    }
}