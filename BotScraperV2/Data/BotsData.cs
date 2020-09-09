using System;
using System.Collections.Generic;

namespace BotScraperV2
{
    [Serializable]
    public class BotsData
    {
        public string lastUpdated;
        public List<Bot> bot = new List<Bot>();

        public BotsData() => lastUpdated = DateTime.Now.ToString("dd/MM/yyy HH:mm:ss");

        public void AddBot(string name, int times, long time) => bot.Add(new Bot(name, times, time));
    }

    public class Bot
    {
        public string name;
        public int timesSeen;
        public long lastSeen;

        public Bot(string _name, int _seen, long _time)
        {
            name = _name;
            timesSeen = _seen;
            lastSeen = _time;
        }
    }
}