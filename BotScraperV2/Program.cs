#pragma warning disable IDE0044, IDE0060, IDE0059

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;

namespace BotScraperV2
{
    internal class Program
    {
        //"Loopers"
        static bool runMainMenu = true, runWorker = true, debug = false;

        //Worker statistics and references
        static int started, stopped, removedIndexesCount, loops;
        static DateTime? startTimer = null;
        static Thread botScraperThread;

        //Data Collector
        static DownloadedData downloadedData;
        static BotsData bots = new BotsData();

        private static void Main(string[] args)
        {
            //Setup console
            Setup();

            //Display logo
            Core.GetLogoType();

            Core.WriteLine("Checking Internet Connection...");

            //Check if Internet connection is available
            if (!Core.InternetAvailable())
            {
                Core.WriteLine(Core.softwareName + " " + Core.GetVersion() + " requires an active internet connection. Detection failed. Exiting...");
                Thread.Sleep(15000);
                Environment.Exit(0);
            }
            else
            {
                //Load local and remote bots and save the most recent one.
                Core.WriteLine("Loading session. Please wait...");
                bots = Core.LoadPreviousSession();

                //Clear Console.
                Console.Clear();

                //Display logo
                Core.GetLogoType();

                //Set thread information and start it.
                botScraperThread = new Thread(() => Do_Work());
                botScraperThread.Start();

                //Loop main menu.
                MainMenu();
            }
        }

        private static void Setup()
        {
            //Allow usage of HTTPS/secure protocol
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            //Disable quick edit of console window
            Core.SetQuickEdit(false);
        }

        /// <summary>
        /// Loop and show the main menu
        /// </summary>
        private static void MainMenu()
        {
            Thread.Sleep(100);
            Console.WriteLine("? or help will show commands");
            Console.WriteLine("Enter a command:");
            while (runMainMenu)
            {
                switch (Console.ReadLine().ToLower())
                {
                    #region Case help

                    case "?":
                    case "help":
                        Core.WriteLine("Commands:" +
                            "\n\rclear, cls   -- This will clear the console." +
                            "\n\rdebug, dev   -- This will activate debug console output." +
                            "\n\rstart        -- This will start the worker." +
                            "\n\rstop         -- This will stop the worker after work has finished." +
                            "\n\rupload       -- This will upload current bot data to the remote server." +
                            "\n\rdownload     -- This will download new data to the software." +
                            "\n\rreset, r     -- WARNING! Instantly resets bots data and overwrite data on next pass." +
                            "\n\rstatus, info -- Shows software information and session data." +
                            "\n\rlist, bots   -- Lists all bots and their data in the console." +
                            "\n\rexit, quit   -- Instantly closes the software.", Core.LogType.Requested);
                        break;

                    #endregion Case help

                    #region Case clear

                    case "clear":
                    case "cls":
                        Console.Clear();
                        break;

                    #endregion Case clear

                    #region Case debug

                    case "dev":
                    case "debug":
                        debug = !debug;
                        Core.debug = !Core.debug;
                        Core.WriteLine("Debug mode is now set " + debug.ToString(), Core.LogType.Requested);
                        break;

                    #endregion Case debug

                    #region Worker Controller

                    #region Case start

                    case "start":
                        if (botScraperThread.IsAlive)
                        {
                            Core.WriteLine("Worker is already ONLINE.", Core.LogType.Requested);
                            break;
                        }
                        runWorker = true;
                        try
                        {
                            botScraperThread = new Thread(() => Do_Work());
                            botScraperThread.Start();
                            Core.WriteLine("Worker is starting...", Core.LogType.Requested);
                        }
                        catch(ThreadStateException x) { Core.WriteLine(x.Message, Core.LogType.Console); }
                        catch(OutOfMemoryException x) { Core.WriteLine(x.Message, Core.LogType.Console); }

                        break;

                    #endregion Case start

                    #region Case stop

                    case "stop":
                        if (!botScraperThread.IsAlive)
                        {
                            Core.WriteLine("Worker is already OFFLINE...", Core.LogType.Requested);
                            break;
                        }
                        runWorker = false;
                        Core.WriteLine("Worker is will finish the current work and then stop...", Core.LogType.Requested);
                        break;

                    #endregion Case stop

                    #region Case upload

                    case "upload":
                        string json = SaveDataToLocalJsonFile();
                        UploadJsonToDatabase(json);
                        Core.WriteLine("Upload complete.", Core.LogType.Requested);
                        break;

                    #endregion Case upload

                    #region Case download

                    case "download":
                        //Download new bot-data
                        downloadedData = Core.Download<DownloadedData>(Core.scrapeURL);

                        //Create update bots data
                        PerformWorkOnData(downloadedData);
                        Core.WriteLine("Download Complete.", Core.LogType.Requested);
                        break;

                    #endregion Case download

                    #endregion Worker Controller

                    #region Bot Controller

                    #region Case Reset

                    case "r":
                    case "reset":
                        bots = new BotsData();
                        Core.WriteLine("Bots have been reset. Save and upload will begin in next cycle.", Core.LogType.Requested);
                        break;

                    #endregion Case Reset

                    #region Case status/info

                    case "status":
                    case "info":
                        SendStatusInformation();
                        Core.WriteLine("This software was created by " + Core.authorName + "/" + Core.authorRealName + " @ 2020", Core.LogType.Requested);
                        break;

                    #endregion Case status/info

                    #region Case list bots

                    case "list":
                    case "bots":
                        try
                        {
                            if (bots.bot.Count > 0)
                            {
                                foreach (Bot bot in bots.bot) Core.WriteLine(bot.name + " has been sighted " + bot.timesSeen.ToString() + " times and was last seen: " + Core.TranslateLongToDateTime(bot.lastSeen).ToString(), Core.LogType.Requested);
                            }
                            else
                            {
                                Core.WriteLine("There are no bots in the list.", Core.LogType.Requested);
                            }
                        }
                        catch (InvalidOperationException x) { Core.WriteLine(x.Message); }
                        break;

                    #endregion Case list bots

                    #endregion Bot Controller

                    #region Case exit

                    case "exit":
                    case "quit":
                        Environment.Exit(0);
                        break;

                    #endregion Case exit

                    default:
                        Core.WriteLine("That is not an accepted command. Please try again.\n\rFor help & commands, write \"help\" or \"?\".\n\r");
                        break;
                }
            }
        }

        private static void SendStatusInformation()
        {
            if (debug) Core.WriteLine("Gathering bot information...");
            string sendString = Core.softwareName + " " + Core.GetVersion() + "'s worker was launched at " + startTimer.ToString() + "." +
                "\n\rWorker is currently " + Core.ResolveBool(botScraperThread.IsAlive) + "." +
                "\n\rWork has passed the loop " + loops.ToString() + " time(s)." +
                "\n\rStarted " + started.ToString() + " time(s)." +
                "\n\rStopped " + stopped.ToString() + " time(s).  " +
                "\n\rWorker currently has a total of " + bots.bot.Count.ToString() + " bot(s) marked as active. " +
                "\n\rWorker has removed " + removedIndexesCount.ToString() + " total inactive bots this session.";
            Core.WriteLine(sendString, Core.LogType.Requested);
        }

        private static void Do_Work()
        {
            Core.WriteLine("Worker started...", Core.LogType.Worker);
            if (startTimer == null) startTimer = DateTime.Now;
            started++;

            while (runWorker)
            {
                if (debug) Core.WriteLine("New Loop...");
                //Keep track of how many software-loops have been started.
                loops++;

                //Clean
                if (debug) Core.WriteLine("Cleaning data...");
                CleanData();

                //Download new bot-data
                if (debug) Core.WriteLine("Download new data...");
                downloadedData = Core.Download<DownloadedData>(Core.scrapeURL);

                if (downloadedData == null)
                {
                    Core.WriteLine("Downloaded data was not valid. Aborting...", Core.LogType.Worker);
                    return;
                }

                //Create update bots data
                PerformWorkOnData(downloadedData);

                //Save local json file
                if (debug) Core.WriteLine("Saving bot data to local file...");
                string json = SaveDataToLocalJsonFile();

                //Upload Data
                if (debug) Core.WriteLine("Upload checking...");
                UploadJsonToDatabase(json);

                //We wait before running the bot again.
                if (debug) Core.WriteLine("Waiting for another pass to start...");
                Thread.Sleep(Core.ResolveSecondsToMilliseconds(Core.updateTimerSeconds));
            }

            stopped++;
            Core.WriteLine("Worker stopped...", Core.LogType.Worker);
        }

        private static void UploadJsonToDatabase(string json)
        {
            //if bots are populated, upload.
            if (bots.bot.Count > 0)
            {
                if (debug) Core.WriteLine("Uploading bot data to remote server...");
                Core.UploadJsonData(json);
            }
            else if (debug) Core.WriteLine("Bot data is not populated. Skipping upload...");
        }

        private static string SaveDataToLocalJsonFile()
        {
            string json = "";
            try
            {
                json = JsonConvert.SerializeObject(bots);
            }
            catch (JsonException x) { Core.WriteLine(x.Message, Core.LogType.Console); }

            if (json == "")
            {
                Core.WriteLine("Something went wrong. Json was \"\" - String.Empty", Core.LogType.Console);
                return "";
            }

            Core.SaveJsonFile(json);

            return json;
        }

        private static void CleanData()
        {
            GC.Collect(); // Is this needed?
            downloadedData = new DownloadedData(); // New collector. Begin anew!
        }

        private static void PerformWorkOnData(DownloadedData downloadedData)
        {
            List<int> indexesToRemove = new List<int>();

            for (int i = 0; i < downloadedData.Bots.Count; i++)
            {
                //If the bot doesn't exist in the database - Add the bot
                if (!DoesBotAlreadyExist(downloadedData.Bots[i][0]))
                {
                    if (debug) Core.WriteLine("Adding a new bot...");
                    bots.AddBot(downloadedData.Bots[i][0], Int32.Parse(downloadedData.Bots[i][1]), long.Parse(downloadedData.Bots[i][2]));
                }
                else // If the bot does exist
                {
                    if (IsBotTooOld(i))
                    {
                        if (debug) Core.WriteLine("Detected an inactive bot at " + i.ToString() + ". Removing...");
                        indexesToRemove.Add(i);
                    }
                    else
                    {
                        //Update data to new data.
                        if (debug) Core.WriteLine("Updating bot data...");
                        bots.bot[i].timesSeen = Int32.Parse(downloadedData.Bots[i][1]);
                        bots.bot[i].lastSeen = long.Parse(downloadedData.Bots[i][2]);
                    }
                }
            }

            //Remove data that are too old
            if(indexesToRemove.Count > 0)
            {
                indexesToRemove.Reverse(); // Go through all indexes from highest to lowest to not change order when removing.
                foreach (int i in indexesToRemove)
                {
                    try
                    {
                        bots.bot.RemoveAt(i); // Remove bot
                        removedIndexesCount++;
                    }
                    catch(ArgumentOutOfRangeException x) { Core.WriteLine(x.Message, Core.LogType.Console); }
                }
                indexesToRemove.Clear();
            }

            bots.lastUpdated = DateTime.Now.ToString("dd/MM/yyy HH:mm:ss");
            GC.Collect();
        }

        private static bool IsBotTooOld(int i)
        {
            DateTime lastSeenTime = Core.TranslateLongToDateTime(bots.bot[i].lastSeen);
            DateTime maxLastSeenTime = Core.TranslateLongToDateTime(bots.bot[i].lastSeen).AddDays(7);

            if (lastSeenTime > maxLastSeenTime) return true;
            return false;
        }

        private static bool DoesBotAlreadyExist(string botName)
        {
            try
            {
                foreach (Bot bot in bots.bot)
                {
                    if (bot.name == botName)
                    {
                        return true;
                    }
                }
                return false;
            }
            catch (InvalidOperationException x) { Core.WriteLine(x.Message); return false; }
        }
    }
}