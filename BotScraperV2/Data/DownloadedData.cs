using System;
using System.Collections.Generic;

namespace BotScraperV2
{
    [Serializable]
    public class DownloadedData
    {
        public List<List<string>> Bots { get; set; }
        public int Total { get; set; }
    }
}