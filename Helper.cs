using System;
using System.Collections.Generic;
using System.Linq;
using static MessengerAnalysis.AnalysisResults;
using System.Text.RegularExpressions;
using System.IO;
using static MessengerAnalysis.Program;

namespace MessengerAnalysis
{
    public static class Helper
    {
        /// <summary>
        /// Save a file in a folder in the location of the executable
        /// </summary>
        /// <param name="filename">The filename (including the extension)</param>
        /// <param name="data">A string to write to the file</param>
        public static void SaveFile(string foldername, string filename, string data)
        {
            foldername = SanitizeFolderName(foldername);
            var dir =Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + Path.DirectorySeparatorChar + foldername);
            File.WriteAllText(dir.FullName + Path.DirectorySeparatorChar + filename, data);
        }
        public static string SanitizeFolderName(string name)
        {
            string regexSearch = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            var r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
            return r.Replace(name, "");
        }
        /// <summary>
        /// Read a file from the location of the executable
        /// </summary>
        /// <param name="filename">The file name (including the extension)</param>
        /// <returns></returns>
        public static string ReadFile(string filename)
        {
            return File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + Path.DirectorySeparatorChar + filename);
        }
        /// <summary>
        /// Check if the input is written entirely in all caps
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool IsAllCaps(this string input)
        {
            Regex r = new Regex("^[A-Z ]*$");
            return !string.IsNullOrEmpty(r.Match(input).Value);
            return true;
        }
        public static string Sanitize(string input)
        {
            return Regex.Replace(input, @"(@[A-Za-z0-9]+)|([^0-9A-Za-z \t])|(\w+:\/\/\S+)", " ");
        }

        public static string PercentOfDouble(this double input, double total)
        {
            if (total == 0) return "0%";
            return Math.Round((input * 100f / total), 1) + "%";
        }
        public static double TryDivide(this double input, double divider)
        {
            if (divider == 0) return 0;
            else return (input / divider);
        }
    
        /// <summary>
        /// Write down the stats for the required property 
        /// </summary>
        /// <param name="stats">The requested user's stats</param>
        /// <param name="universalstats">The universal stats variable</param>
        /// <param name="property">The name of the property from UserStats</param>
        /// <returns></returns>
        public static void WriteStats(Dictionary<string, UserStats> stats, UserStats universalstats, string property)
        {
            foreach(var user in stats.OrderByDescending(x => Convert.ToDouble(typeof(UserStats).GetProperty(property).GetValue(x.Value))))
            {
                double value = Convert.ToDouble(typeof(UserStats).GetProperty(property).GetValue(user.Value));
                if (value != 0)
                {
                    double total = Convert.ToDouble(typeof(UserStats).GetProperty(property).GetValue(universalstats));

                    Log.WriteSubtleishLine(user.Key + ": " + value + " (" + value.PercentOfDouble(total) + " of total)");
                }
            }
            Log.WriteLine();
        }
        /// <summary>
        /// Write down the stats for the required property, relative to the amount of messages a user has sent
        /// </summary>
        /// <param name="stats">The requested user's stats</param>
        /// <param name="universalstats">The universal stats variable</param>
        /// <param name="property">The name of the property from UserStats</param>
        /// <returns></returns>
        public static void WriteStatsProportional(Dictionary<string, UserStats> stats, UserStats universalstats, string property)
        {
            foreach (var user in stats.OrderByDescending(x => (Convert.ToDouble(typeof(UserStats).GetProperty(property).GetValue(x.Value))).TryDivide(x.Value.MessagesSent)))
            {
                double value = (Convert.ToDouble(typeof(UserStats).GetProperty(property).GetValue(user.Value)).TryDivide(user.Value.MessagesSent));
                if (value != 0)
                {
                    var percent = Math.Round(value * 100, 2);
                    Log.WriteSubtleishLine(user.Key + ": " + percent + "%");
                }
                
            }
            Log.WriteLine();
        }

        public static void MostSomethingHelper(ref int mostcount, ref string moststring, int verify, string name)
        {
            if (verify > mostcount)
            {
                moststring = name;
                mostcount = verify;
            }
        }
    }
}
