using System;
using System.Collections.Generic;
using System.Linq;
using static MessengerAnalysis.Program;

namespace MessengerAnalysis
{
    public class AnalysisResults
    {
        
        public class UserStats
        {
            public void Add(UserStats stats)
            {
                foreach(var entry in stats.RespondedTo)
                {
                    if (RespondedTo.ContainsKey(entry.Key))
                        RespondedTo[entry.Key] += stats.RespondedTo[entry.Key];
                    else
                        RespondedTo.Add(entry.Key,entry.Value);
                }


                MessagesSent += stats.MessagesSent;
                TotalCharacterCount += stats.TotalCharacterCount;
                GifsSent += stats.GifsSent;
                ImagesSent += stats.ImagesSent;
                LinksSent += stats.LinksSent;
                AllCapsSent += stats.AllCapsSent;
                CustomWordsSent += stats.CustomWordsSent;
                SwearingSent += stats.SwearingSent;
                SwearingSentMono += stats.SwearingSentMono;

                GaveCrossbones += stats.GaveCrossbones;
                GaveLaugh += stats.GaveLaugh;
                GaveSkull += stats.GaveSkull;
                GaveAlien += stats.GaveAlien;
                GaveGoblin += stats.GaveGoblin;
                GaveHowl += stats.GaveHowl;

                ReceivedCrossbones += stats.ReceivedCrossbones;
                ReceivedLaugh += stats.ReceivedLaugh;
                ReceivedSkull += stats.ReceivedSkull;
                ReceivedAlien += stats.ReceivedAlien;
                ReceivedGoblin += stats.ReceivedGoblin;
                ReceivedHowl += stats.ReceivedHowl;

                AppreciationMeter += stats.AppreciationMeter;
            }
            public string MostGivenReaction()
            {
                int mostgivencount = 0;
                string mostgivenreaction = "None given";
                Helper.MostSomethingHelper(ref mostgivencount, ref mostgivenreaction, GaveAlien, "Alien");
                Helper.MostSomethingHelper(ref mostgivencount, ref mostgivenreaction, GaveCrossbones, "Crossbones");
                Helper.MostSomethingHelper(ref mostgivencount, ref mostgivenreaction, GaveLaugh, "Laugh");
                Helper.MostSomethingHelper(ref mostgivencount, ref mostgivenreaction, GaveGoblin, "Goblin");
                Helper.MostSomethingHelper(ref mostgivencount, ref mostgivenreaction, GaveHowl, "Howl");
                Helper.MostSomethingHelper(ref mostgivencount, ref mostgivenreaction, GaveSkull, "Skull");
                return mostgivenreaction;
            }
            public string MostReceivedReaction()
            {
                int mostreceivedcount = 0;
                string mostreceivedreaction = "None received";
                Helper.MostSomethingHelper(ref mostreceivedcount, ref mostreceivedreaction, ReceivedAlien, "Alien");
                Helper.MostSomethingHelper(ref mostreceivedcount, ref mostreceivedreaction, ReceivedCrossbones, "Crossbones");
                Helper.MostSomethingHelper(ref mostreceivedcount, ref mostreceivedreaction, ReceivedLaugh, "Laugh");
                Helper.MostSomethingHelper(ref mostreceivedcount, ref mostreceivedreaction, ReceivedGoblin, "Goblin");
                Helper.MostSomethingHelper(ref mostreceivedcount, ref mostreceivedreaction, ReceivedHowl, "Howl");
                Helper.MostSomethingHelper(ref mostreceivedcount, ref mostreceivedreaction, ReceivedSkull, "Skull");
                return mostreceivedreaction;
            }

            /// <summary>
            /// Write down a list of who the user has talked with the most. For each user, this is calculated with the following formula:
            /// 
            /// Ab/At+Ba/Bt where
            /// Ab = times A has responded to B
            /// At = total message count sent by A
            /// Ba = times B has responded to A
            /// Bt = total message count sent by B
            /// </summary>
            /// <param name="globalStats"></param>
            public void WriteTalksWith(ref Dictionary<string,UserStats> globalStats)
            {
                Dictionary<string, double> talksWith = new Dictionary<string, double>();
                foreach(var user in RespondedTo)
                {
                    double otherside = 0;
                    if (globalStats[user.Key].RespondedTo.ContainsKey(Username))
                        otherside = globalStats[user.Key].RespondedTo[Username]/(double)MessagesSent;
                    talksWith.Add(user.Key, Math.Round((((double)user.Value/globalStats[user.Key].MessagesSent)+(otherside))*100, 2));
                }
                foreach (var person in talksWith.OrderBy(x => x.Value).Select(x => x.Key + " (" + x.Value + ")"))
                    Log.WriteSubtleLine(person);
            }

            public Dictionary<string, int> RespondedTo = new Dictionary<string, int>();
            public string Username { get; set; }
            public int MessagesSent { get; set; }
            public int TotalCharacterCount { get; set; }
            public int GifsSent { get; set; }
            public int ImagesSent { get; set; }
            public int LinksSent { get; set; }
            public int AllCapsSent { get; set; }
            public int CustomWordsSent { get; set; }
            public int SwearingSent { get; set; }
            public int SwearingSentMono { get; set; }

            public int GaveCrossbones { get; set; }
            public int GaveLaugh { get; set; }
            public int GaveSkull { get; set; }
            public int GaveAlien { get; set; }
            public int GaveGoblin { get; set; }
            public int GaveHowl { get; set; }

            public int ReceivedCrossbones { get; set; }
            public int ReceivedLaugh { get; set; }
            public int ReceivedSkull { get; set; }
            public int ReceivedAlien { get; set; }
            public int ReceivedGoblin { get; set; }
            public int ReceivedHowl { get; set; }
            public double AppreciationMeter { get; set; }
        }
    }
}
