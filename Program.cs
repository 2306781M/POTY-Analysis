using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using static MessengerAnalysis.GroupConvo;


namespace MessengerAnalysis
{
    
    class Program
    {
        public static int poty = 0;
        public static int poty2 = 0;
        public static int poty3 = 0;
        public static string potywinner = "";
        public static string potywinner2 = "";
        public static string potywinner3 = "";
        public static string potydate = "";
        public static string potydate2 = "";
        public static string potydate3 = "";
        public static string potytime = "";
        public static string potycontent = "";
        public static string potycontent2 = "";
        public static string potycontent3 = "";
        public static List<Photo> potyphoto;
        public static List<Gif> potygif;
        //heartlaughwowsadangrylikedislike
        const string EmojiCrossbones = "\u00e2\u0098\u00a0\u00ef\u00b8\u008f"; //0xE2 0x98 0xA0
        const string EmojiLaugh = "\u00f0\u009f\u0098\u0086";
        const string EmojiSkull = "\u00f0\u009f\u0092\u0080"; //0xF0 0x9F 0x92 0x80
        const string EmojiAlien = "\u00f0\u009f\u0091\u00bd"; //0xF0 0x9F 0x91 0xBD
        const string EmojiGoblin = "\u00f0\u009f\u0091\u00ba"; //0xF0 0x9F 0x91 0xBA
        const string EmojiHowl = "\u00f0\u009f\u008d\u0089"; //0xF0 0x9F 0x90 0xBA \u00f0\u009f\u0090\u00ba

        public static class Log
        {
            public static StringBuilder LogString = new StringBuilder();
            public static void WriteLine(string text = "")
            {
                Console.WriteLine(text);
                LogString.Append("<span>" + text + "</span><br/>");
            }
            public static void WriteBoldLine(string text)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(text);
                Console.ResetColor();
                LogString.Append("<span class='bold'>" + text + "</span><br/>");
            }
            public static void WriteSubtleLine(string text)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine(text);
                Console.ResetColor();
                LogString.Append("<span class='subtle'>" + text + "</span><br/>");
            }
            public static void WriteSubtleishLine(string text)
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine(text);
                Console.ResetColor();
                LogString.Append("<span class='subtleish'>" + text + "</span><br/>");
            }
            public static string ReadLine()
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                var input = Console.ReadLine();
                Console.ResetColor();
                LogString.Append("<span class='subtle'>" + input + "</span><br/>");
                return input;
            }
            public static string GetOutput()
            {
                return LogString.ToString();
            }
        }

        static void Main(string[] args)
        {
            Log.WriteSubtleLine("MessengerAnalysis 1.0.0");
            Log.WriteSubtleLine("Copyright (c) 2019 Piers Deseilligny");
            if (args == null || args.Length == 0)
            {
                Log.WriteLine("Please specify an input file");
                Process(Log.ReadLine());
            }
            else
            {
                Process(args[0]);
            }
        }
        static HashSet<string> Participants = new HashSet<string>();
        public static Dictionary<string, Dictionary<string, AnalysisResults.UserStats>> AllDates = new Dictionary<string, Dictionary<string, AnalysisResults.UserStats>>();
        public static Dictionary<string, int> AppreciationMeter = new Dictionary<string, int>();
        //Stats per user, regardless of date
        public static Dictionary<string, AnalysisResults.UserStats> GlobalStats = new Dictionary<string, AnalysisResults.UserStats>();
        //Stats for everyone combined
        public static AnalysisResults.UserStats UniversalStats = new AnalysisResults.UserStats();


        public static Dictionary<string, string> HtmlReplacements = new Dictionary<string, string>();
        public static Dictionary<string, int> AverageMessageLengths = new Dictionary<string, int>();

        public static Dictionary<DayOfWeek, Dictionary<int, int>> ActiveTimes = new Dictionary<DayOfWeek, Dictionary<int, int>>()
        {
            {DayOfWeek.Monday, new Dictionary<int, int>() },
            {DayOfWeek.Tuesday, new Dictionary<int, int>() },
            {DayOfWeek.Wednesday, new Dictionary<int, int>() },
            {DayOfWeek.Thursday, new Dictionary<int, int>() },
            {DayOfWeek.Friday, new Dictionary<int, int>() },
            {DayOfWeek.Saturday, new Dictionary<int, int>() },
            {DayOfWeek.Sunday, new Dictionary<int, int>() }
        };

        public static Root root = null;
        public static void Process(string path)
        {
            Log.WriteLine();
            Log.WriteLine("Opening file...");
            string rawjson;
            try
            {
                rawjson = File.ReadAllText(path.Trim('"'));
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Couldn't open file (" + ex.Message + ")!");
                Console.ResetColor();
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
                return;
            }

            root = Newtonsoft.Json.JsonConvert.DeserializeObject<Root>(rawjson);

            Stopwatch sw = new Stopwatch();
            sw.Start();

            //Get date span
            var start = DateTimeOffset.FromUnixTimeMilliseconds(root.messages[root.messages.Count - 1].timestamp_ms);
            if (root.thread_type == "Regular" && root.messages[root.messages.Count - 1].content.StartsWith("Say hi to your new Facebook friend,") && root.messages.Count>1)
            {
                //Ignore the "Say hi to your new friend" message
                start = DateTimeOffset.FromUnixTimeMilliseconds(root.messages[root.messages.Count - 2].timestamp_ms);
            }
            var end = DateTimeOffset.FromUnixTimeMilliseconds(root.messages[0].timestamp_ms).DateTime;

            if (start > end)
            {
                //This should never happen, but if it does, swap the values
                DateTime tempswap = end;
                start = end;
                end = tempswap;
            }

            //Add all dates between the start and end in the AllDates dictionary and in the AllDates string
            for (var i = start; i.Date < end.Date; i = i.AddDays(1))
            {
                var str = i.Date.ToString("s");
                if (!AllDates.ContainsKey(str))
                    AllDates.Add(i.Date.ToString("s"), new Dictionary<string, AnalysisResults.UserStats>());
            }

            //Add all 5-minute zones to each day to the ActiveTimes structure
            foreach (var day in ActiveTimes)
            {
                for (var i = 0; i < 288; i += 1)
                {
                    day.Value.Add(i * 5, 0);
                }
            }

            Log.WriteLine("Processing " + root.messages.Count + " messages sent in '" + root.title + "' over a period of " + AllDates.Count + " days");
            var start1=start.DateTime;
            Log.WriteLine("First message sent on " + start1.ToShortDateString() + " at " + start1.ToShortTimeString());
            Log.WriteLine("Last message sent on " + end.ToShortDateString() + " at " + end.ToShortTimeString());

            Dictionary<string, int> KickedOutCounter = new Dictionary<string, int>();
            int ticker = 0;
            int progressCounter = 0;
            Message previousMessage = null;
            root.messages.Reverse();//The messages should be processed from oldest to newest
            var sanitizedMessages = new List<Message>();
            //Sanitize the message list, by combining anything sent by the same person less than 10 seconds apart.
            bool firstmessage = true;
            foreach (var message in root.messages)
            {
                if (firstmessage && !string.IsNullOrEmpty(message.content) && message.content.StartsWith("Say hi to your new Facebook friend,"))
                {
                    firstmessage = false;
                    continue;
                }
                if (!Participants.Contains(message.sender_name))
                {
                    AppreciationMeter.Add(message.sender_name, 0);
                    Participants.Add(message.sender_name);
                }

                if (previousMessage != null && previousMessage.sender_name == message.sender_name && (message.timestamp_ms - previousMessage.timestamp_ms) < 5000 && message.type == "Generic" && previousMessage.type == "Generic")
                {
                    //If the previous message was sent less than 5 seconds ago and by the same person who sent the current one, combine them
                    if (previousMessage.content == null)
                        previousMessage.content = message.content;
                    else
                        previousMessage.content += "\n" + message.content;
                    if (message.gifs != null)
                    {
                        if (previousMessage.gifs == null)
                            previousMessage.gifs = message.gifs;
                        else
                            previousMessage.gifs.AddRange(message.gifs);
                    }
                    if (message.photos != null)
                    {
                        if (previousMessage.photos == null)
                            previousMessage.photos = message.photos;
                        else
                            previousMessage.photos.AddRange(message.photos);
                    }
                    if (message.reactions != null)
                    {
                        if (previousMessage.reactions == null)
                            previousMessage.reactions = message.reactions;
                        else
                            previousMessage.reactions.AddRange(message.reactions);
                    }
                    sanitizedMessages.RemoveAt(sanitizedMessages.Count - 1);
                    sanitizedMessages.Add(previousMessage);
                }
                else
                {
                    sanitizedMessages.Add(message);
                }
                previousMessage = message;
            }
            previousMessage = null;
            Log.WriteLine("Sanitized message count:" + sanitizedMessages.Count);
            Log.WriteLine();
            foreach (var key in AllDates.Keys)
            {
                foreach (var participant in Participants)
                {
                    AllDates[key].Add(participant, new AnalysisResults.UserStats());
                }
            }
            foreach (var message in sanitizedMessages)
            {
                ticker++;
                if (ticker == 10)
                {
                    Console.Write("\r" + (100 * progressCounter) / sanitizedMessages.Count + "% Analysed");
                    ticker = 0;
                }
                var messageDateRaw = DateTimeOffset.FromUnixTimeMilliseconds(message.timestamp_ms).DateTime;
                var messageDate = DateTimeOffset.FromUnixTimeMilliseconds(message.timestamp_ms).DateTime.Date.ToString("s");
                if (!AllDates.ContainsKey(messageDate))
                    AllDates.Add(messageDate, new Dictionary<string, AnalysisResults.UserStats>());

                foreach (var participant in Participants)
                    if (!AllDates[messageDate].ContainsKey(participant))
                        AllDates[messageDate].Add(participant, new AnalysisResults.UserStats());

                //Messages sent
                var senderstats = AllDates[messageDate][message.sender_name];
                senderstats.MessagesSent++;
                senderstats.AppreciationMeter -= 0.015; //This is to make the appreciation meter somewhat proportional to the amount someone talks

                //Active times stuff
                var closestLower = Convert.ToInt32(Math.Floor(messageDateRaw.TimeOfDay.TotalMinutes / 5) * 5);
                ActiveTimes[messageDateRaw.DayOfWeek][closestLower]++;

                //Kicked out counter
                if (message.type == "Unsubscribe" && message.users != null)
                {
                    foreach (var user in message.users)
                    {
                        if (user.name != message.sender_name)
                        {
                            if (!KickedOutCounter.ContainsKey(user.name))
                                KickedOutCounter.Add(user.name, 1);
                            else
                                KickedOutCounter[user.name]++;
                            AllDates[messageDate][user.name].AppreciationMeter -= 5;
                        }
                    }
                }

                if (message.content != null)
                {
                    var sanitized = Helper.Sanitize(message.content);
                    //Total character count (used to get average later on)
                    senderstats.TotalCharacterCount += message.content.Length;

                    //Messages sent in all caps
                    if (sanitized.IsAllCaps())
                        senderstats.AllCapsSent++;

                    //Swearing
                    int swearcount = Profanity.Counter(message.content);
                    senderstats.SwearingSent += swearcount;
                    if (swearcount > 0) senderstats.SwearingSentMono += 1;
                }

                //Gifs sent
                if (message.gifs != null)
                    senderstats.GifsSent += message.gifs.Count;

                //Images sent
                if (message.photos != null)
                    senderstats.ImagesSent += message.photos.Count;

                //Links sent
                if (message.share != null)
                    senderstats.LinksSent++;

                //Responses
                if (previousMessage != null && previousMessage.sender_name != message.sender_name)
                {
                    if((message.timestamp_ms - previousMessage.timestamp_ms) < 120000){
                        //If the previous message was sent less than 2 minutes ago and not by the same person who sent the current one
                        if (senderstats.RespondedTo.ContainsKey(previousMessage.sender_name))
                            senderstats.RespondedTo[previousMessage.sender_name]++;
                        else
                            senderstats.RespondedTo.Add(previousMessage.sender_name, 1);
                    }
                    else
                    {
                        AllDates[messageDate][previousMessage.sender_name].AppreciationMeter -= 0.1;
                    }

                }

                //Reactions
                if (message.reactions != null)
                {
                    int patterscore = 0; //senderstats.AppreciationMeter
                    foreach (var reaction in message.reactions)
                    {
                        if (!AllDates[messageDate].ContainsKey(reaction.actor))
                            AllDates[messageDate].Add(reaction.actor, new AnalysisResults.UserStats());
                        var actorstats = AllDates[messageDate][reaction.actor];
                        switch (reaction.reaction)
                        {
                            case EmojiCrossbones:
                                senderstats.ReceivedCrossbones++;
                                actorstats.GaveCrossbones++;
                                patterscore+=3;
                                break;
                            case EmojiLaugh:
                                senderstats.ReceivedLaugh++;
                                actorstats.GaveLaugh++;
                                patterscore++;
                                break;
                            case EmojiGoblin:
                                senderstats.ReceivedGoblin++;
                                actorstats.GaveGoblin++;
                                patterscore+=3;
                                break;
                            case EmojiSkull:
                                senderstats.ReceivedSkull++;
                                actorstats.GaveSkull++;
                                patterscore+=2;
                                break;
                            case EmojiHowl:
                                senderstats.ReceivedHowl++;
                                actorstats.GaveHowl++;
                                patterscore+=3000;
                                break;
                            case EmojiAlien:
                                senderstats.ReceivedAlien++;
                                actorstats.GaveAlien++;
                                patterscore+=3;;
                                break;
                        }
                        actorstats.GaveReact+=patterscore;

                        AllDates[messageDate][reaction.actor] = actorstats;
                    }
                    // to-do this is where POTY calculation should be
                    //
                    // IF current patterscore > previous highest THEN highest = current patterscore AND potywinner = current sendername AND add message somehow?
                    if (patterscore > poty3){
                        poty3=patterscore;
                        
                        if (patterscore > poty2){
                            poty3=poty2;
                            poty2=patterscore;
                            if (patterscore > poty){
                                poty2=poty;
                                poty=patterscore;
                                potywinner=message.sender_name;
                                potydate=messageDate;
                                if (message.content != null){potycontent=message.content;}
                            }
                            else {potywinner2=message.sender_name; potydate2=messageDate; if (message.content != null){potycontent2=message.content;}}
                        }
                        else {potywinner3=message.sender_name; potydate3=messageDate; if (message.content != null){potycontent3=message.content;}}
                        
                    }

                    //
                    //
                    senderstats.AppreciationMeter+=patterscore;
                }

                AllDates[messageDate][message.sender_name] = senderstats;
                progressCounter++;
                previousMessage = message;
            }
            Console.WriteLine("\r100% Analysed");
            Log.WriteLine("Analysed in " +sw.ElapsedMilliseconds+"ms");
            Log.WriteLine();

            foreach (var day in AllDates)
            {
                foreach (var user in day.Value)
                {
                    if (!GlobalStats.ContainsKey(user.Key))
                        GlobalStats.Add(user.Key, new AnalysisResults.UserStats());
                    GlobalStats[user.Key].Add(user.Value);
                    GlobalStats[user.Key].Username = user.Key;
                }
            }

            
            foreach (var user in GlobalStats)
            {
                if (user.Value.MessagesSent == 0)
                    AverageMessageLengths.Add(user.Key, 0);
                else
                    AverageMessageLengths.Add(user.Key, user.Value.TotalCharacterCount / user.Value.MessagesSent);
                UniversalStats.Add(user.Value);
            }

            

            Log.WriteBoldLine("MESSAGES SENT:");
            Helper.WriteStats(GlobalStats, UniversalStats, "MessagesSent");



            Log.WriteBoldLine("LAUGHING REACTS GIVEN:");
            Helper.WriteStats(GlobalStats, UniversalStats, "GaveLaugh");

            Log.WriteBoldLine("SKULL REACTS GIVEN:");
            Helper.WriteStats(GlobalStats, UniversalStats, "GaveSkull");

            Log.WriteBoldLine("CROSSBONE REACTS GIVEN:");
            Helper.WriteStats(GlobalStats, UniversalStats, "GaveCrossbones");

            Log.WriteBoldLine("ALIEN REACTS GIVEN:");
            Helper.WriteStats(GlobalStats, UniversalStats, "GaveAlien");

            Log.WriteBoldLine("GOBLIN REACTS GIVEN:");
            Helper.WriteStats(GlobalStats, UniversalStats, "GaveGoblin");

            Log.WriteBoldLine("HOWL REACTS GIVEN:");
            Helper.WriteStats(GlobalStats, UniversalStats, "GaveHowl");

            Log.WriteBoldLine("TOTAL REACTS GIVEN:");
            Helper.WriteStats(GlobalStats, UniversalStats, "GaveReact");


            Log.WriteBoldLine("LAUGH REACTS RECEIVED:");
            Helper.WriteStats(GlobalStats, UniversalStats, "ReceivedLaugh");
            Helper.WriteStatsProportional(GlobalStats, UniversalStats, "ReceivedLaugh");

            Log.WriteBoldLine("SKULL REACTS RECEIVED:");
            Helper.WriteStats(GlobalStats, UniversalStats, "ReceivedSkull");
            Helper.WriteStatsProportional(GlobalStats, UniversalStats, "ReceivedSkull");

            Log.WriteBoldLine("CROSSBONES REACTS RECEIVED:");
            Helper.WriteStats(GlobalStats, UniversalStats, "ReceivedCrossbones");
            Helper.WriteStatsProportional(GlobalStats, UniversalStats, "ReceivedCrossbones");

            Log.WriteBoldLine("ALIEN REACTS RECEIVED:");
            Helper.WriteStats(GlobalStats, UniversalStats, "ReceivedAlien");
            Helper.WriteStatsProportional(GlobalStats, UniversalStats, "ReceivedAlien");

            Log.WriteBoldLine("HOWL REACTS RECEIVED:");
            Helper.WriteStats(GlobalStats, UniversalStats, "ReceivedHowl");
            Helper.WriteStatsProportional(GlobalStats, UniversalStats, "ReceivedHowl");

            Log.WriteBoldLine("GOBLIN REACTS RECEIVED:");
            Helper.WriteStats(GlobalStats, UniversalStats, "ReceivedGoblin");
            Helper.WriteStatsProportional(GlobalStats, UniversalStats, "ReceivedGoblin");

            //USER-SPECIFIC STATS
            Log.WriteLine();
            foreach (var user in GlobalStats)
            {
                Log.WriteLine();
                Log.WriteBoldLine(user.Key.ToUpper());
                Log.WriteLine("Most given reaction:");
                Log.WriteSubtleishLine(user.Value.MostGivenReaction());
                Log.WriteLine("Most received reaction:");
                Log.WriteSubtleishLine(user.Value.MostReceivedReaction());
                Log.WriteLine("Talks with:");
                user.Value.WriteTalksWith(ref GlobalStats);
            }

            //SUSTAINED PATTER
            Log.WriteLine();
            Log.WriteBoldLine("SUSTAINED PATTER LEVEL:");
            Helper.WriteStats(GlobalStats, UniversalStats, "AppreciationMeter");

            //PATTER OF THE YEAR
            Log.WriteLine();
            Log.WriteBoldLine("PATTER OF THE YEAR WINNER:");
            string potytext = potywinner+", with a total patter rating of "+poty.ToString();
            Log.WriteLine(potytext);
            Log.WriteSubtleishLine("At: "+potydate);
            
            if (potycontent != null){
                Log.WriteLine("\""+potycontent+"\"");
            }

            //SECOND
            Log.WriteLine();
            Log.WriteBoldLine("SECOND PLACE:");
            string potytext2 = potywinner2+", with a total patter rating of "+poty2.ToString();
            Log.WriteLine(potytext);
            Log.WriteSubtleishLine("At: "+potydate2);

            if (potycontent2 != null){
                Log.WriteLine("\""+potycontent2+"\"");
            }

             //THIRD
            Log.WriteLine();
            Log.WriteBoldLine("THIRD PLACE:");
            string potytext3 = potywinner3+", with a total patter rating of "+poty3.ToString();
            Log.WriteLine(potytext3);
            Log.WriteSubtleishLine("At: "+potydate3);

            if (potycontent3 != null){
                Log.WriteLine("\""+potycontent3+"\"");
            }


            //CSV FILES + PLOTS
            Log.WriteLine();
            Log.WriteLine();
            Log.WriteLine("Creating csv and html files...");
            string plots = "";
            plots += CreateCsvAndJavascript("messages.csv", "MessagesSent", "Messages sent");
            plots += "," + CreateCsvAndJavascript("appreciation.csv", "AppreciationMeter", "Appreciation");
            plots += "," + CreateCsvAndJavascript("laugh.csv", "ReceivedLaugh", "Received laugh reacts");
            plots += "," + CreateCsvAndJavascript("skull.csv", "ReceivedSkull", "Received skull reacts");
            plots += "," + CreateCsvAndJavascript("crossbone.csv", "ReceivedCrossbones", "Received crossbone reacts");
            plots += "," + CreateCsvAndJavascript("alien.csv", "ReceivedAlien", "Received alien reacts");
            plots += "," + CreateCsvAndJavascript("howl.csv", "ReceivedHowl", "Received howl reacts");
            plots += "," + CreateCsvAndJavascript("goblin.csv", "ReceivedGoblin", "Received goblin reacts");
            
            



            //HTML
            HtmlReplacements.Add("/*PLOTS*/", plots);
            HtmlReplacements.Add("/*PAGETITLE*/", root.title);
            HtmlReplacements.Add("/*DATESPAN*/", string.Join(',', AllDates.Select(x => "\"" + x.Key + "\"")));
            string html = Helper.ReadFile("Template.html");
            foreach (var replacement in HtmlReplacements)
                html = html.Replace(replacement.Key, replacement.Value);

            Log.WriteLine("\nDone in " + sw.ElapsedMilliseconds + "ms");

            html = html.Replace("/*OUTPUT*/", Log.GetOutput());
            Helper.SaveFile(root.title, "output.html", html);

            sw.Stop();
            
            Log.WriteLine("Press Y to analyse another file, any other key to exit...");

            if (Console.ReadKey().Key == ConsoleKey.Y)
            {
                ResetVariables();
                Main(null);
            }
        }
        public static void ResetVariables()
        {
            Participants = new HashSet<string>();
            AllDates = new Dictionary<string, Dictionary<string, AnalysisResults.UserStats>>();
            AppreciationMeter = new Dictionary<string, int>();
            GlobalStats = new Dictionary<string, AnalysisResults.UserStats>();
            UniversalStats = new AnalysisResults.UserStats();
            HtmlReplacements = new Dictionary<string, string>();
            AverageMessageLengths = new Dictionary<string, int>();
            ActiveTimes = new Dictionary<DayOfWeek, Dictionary<int, int>>()
            {
                {DayOfWeek.Monday, new Dictionary<int, int>() },
                {DayOfWeek.Tuesday, new Dictionary<int, int>() },
                {DayOfWeek.Wednesday, new Dictionary<int, int>() },
                {DayOfWeek.Thursday, new Dictionary<int, int>() },
                {DayOfWeek.Friday, new Dictionary<int, int>() },
                {DayOfWeek.Saturday, new Dictionary<int, int>() },
                {DayOfWeek.Sunday, new Dictionary<int, int>() }
            };
        }

        public static string CreateActiveTimesPlot(Dictionary<DayOfWeek, Dictionary<int, int>> activeTimes)
        {
            string z = "[";
            Dictionary<int, float> AverageTimes = new Dictionary<int, float>();
            foreach (var dayofweek in activeTimes)
            {
                z += "[";
                foreach (var time in dayofweek.Value)
                {
                    z += time.Value;
                    if (!AverageTimes.ContainsKey(time.Key))
                        AverageTimes.Add(time.Key, 0);
                    AverageTimes[time.Key] += time.Value;
                    if (time.Key != 1435)
                        z += ",";
                }
                z += "],";
            }
            z += "[" + string.Join(',', AverageTimes.Select(x => (x.Value / 7).ToString().Replace(',','.'))) + "]";
            z += "]";
            return @"{
                traces:[{
                    z: " + z + @",
                    y: ['Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday', 'Sunday', 'Average'],
                    x: ['00:00','00:05','00:10','00:15','00:20','00:25','00:30','00:35','00:40','00:45','00:50','00:55','01:00','01:05','01:10','01:15','01:20','01:25','01:30','01:35','01:40','01:45','01:50','01:55','02:00','02:05','02:10','02:15','02:20','02:25','02:30','02:35','02:40','02:45','02:50','02:55','03:00','03:05','03:10','03:15','03:20','03:25','03:30','03:35','03:40','03:45','03:50','03:55','04:00','04:05','04:10','04:15','04:20','04:25','04:30','04:35','04:40','04:45','04:50','04:55','05:00','05:05','05:10','05:15','05:20','05:25','05:30','05:35','05:40','05:45','05:50','05:55','06:00','06:05','06:10','06:15','06:20','06:25','06:30','06:35','06:40','06:45','06:50','06:55','07:00','07:05','07:10','07:15','07:20','07:25','07:30','07:35','07:40','07:45','07:50','07:55','08:00','08:05','08:10','08:15','08:20','08:25','08:30','08:35','08:40','08:45','08:50','08:55','09:00','09:05','09:10','09:15','09:20','09:25','09:30','09:35','09:40','09:45','09:50','09:55','10:00','10:05','10:10','10:15','10:20','10:25','10:30','10:35','10:40','10:45','10:50','10:55','11:00','11:05','11:10','11:15','11:20','11:25','11:30','11:35','11:40','11:45','11:50','11:55','12:00','12:05','12:10','12:15','12:20','12:25','12:30','12:35','12:40','12:45','12:50','12:55','13:00','13:05','13:10','13:15','13:20','13:25','13:30','13:35','13:40','13:45','13:50','13:55','14:00','14:05','14:10','14:15','14:20','14:25','14:30','14:35','14:40','14:45','14:50','14:55','15:00','15:05','15:10','15:15','15:20','15:25','15:30','15:35','15:40','15:45','15:50','15:55','16:00','16:05','16:10','16:15','16:20','16:25','16:30','16:35','16:40','16:45','16:50','16:55','17:00','17:05','17:10','17:15','17:20','17:25','17:30','17:35','17:40','17:45','17:50','17:55','18:00','18:05','18:10','18:15','18:20','18:25','18:30','18:35','18:40','18:45','18:50','18:55','19:00','19:05','19:10','19:15','19:20','19:25','19:30','19:35','19:40','19:45','19:50','19:55','20:00','20:05','20:10','20:15','20:20','20:25','20:30','20:35','20:40','20:45','20:50','20:55','21:00','21:05','21:10','21:15','21:20','21:25','21:30','21:35','21:40','21:45','21:50','21:55','22:00','22:05','22:10','22:15','22:20','22:25','22:30','22:35','22:40','22:45','22:50','22:55','23:00','23:05','23:10','23:15','23:20','23:25','23:30','23:35','23:40','23:45','23:50','23:55'],
                    type: 'heatmap'
                }],
                id:'ActiveTimes',
                layout: {
                    title:'Active Times'
                }
            }";
        }

        /// <summary>
        /// Creates a csv file and returns an array of filled trace templates
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="property"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        public static string CreateCsvAndJavascript(string filename, string property, string title)
        {
            string csvFile = "date," + string.Join(',', Participants.OrderBy(x => x));
            string traceTemplate = @"{
  x: datespan,
  y: [%YDATA%],
  mode: 'line',
  line: {shape: 'spline'},
  name: '%NAME%',
  connectgaps:true
}";
            var datelist = AllDates.ToList();
            Dictionary<string, double> messageCounter = new Dictionary<string, double>();
            Dictionary<string, List<string>> traceValues = new Dictionary<string, List<string>>();
            foreach (var participant in Participants)
            {
                messageCounter.Add(participant, 0);
                traceValues.Add(participant, new List<string>());
            }
            for (int i = 0; i < AllDates.Count; i++)
            {
                csvFile += "\n" + datelist[i].Key + ",";
                var ordered = datelist[i].Value.OrderBy(x => x.Key).ToList();
                for (int j = 0; j < ordered.Count; j++)
                {
                    var value = Convert.ToDouble(typeof(AnalysisResults.UserStats).GetProperty(property).GetValue(ordered[j].Value));
                    if (messageCounter.ContainsKey(ordered[j].Key))
                    {
                        messageCounter[ordered[j].Key] += value;

                        csvFile += messageCounter[ordered[j].Key];
                        if (value == 0)
                            traceValues[ordered[j].Key].Add("undefined");
                        else
                            traceValues[ordered[j].Key].Add(messageCounter[ordered[j].Key].ToString().Replace(",","."));
                        if (j != ordered.Count - 1)
                            csvFile += ",";
                    }

                }
            }
            List<string> traces = new List<string>();
            foreach (var person in traceValues)
            {
                traces.Add(traceTemplate.Replace("%NAME%", person.Key.Replace("'", "\\'")).Replace("%YDATA%", string.Join(',', person.Value)));
            }
            Helper.SaveFile(root.title, filename, csvFile);

            string plot = @"{
                traces:[/*TRACES*/],
                id:/*ID*/,
                layout: {
                    title: '/*TITLE*/',
                    xaxis: {
                        autorange: true,
                        type: 'date'
                    },
                    yaxis: {
                        autorange: true,
                        type: 'linear'
                    }
                }
            }".Replace("/*TITLE*/", title)
            .Replace("/*TRACES*/", string.Join(',', traces))
            .Replace("/*ID*/", "\"" + property + "\"");
            return plot;
        }
    }
}
