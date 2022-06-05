using System;

namespace dropcatch.com.Services
{
    public class Reporter
    {
        public static event EventHandler<string> OnLog;
        public static event EventHandler<string> OnError;
        public static event EventHandler<(int nbr, int max, string text)> OnProgress;

        public static void Log(string s)
        {
            OnLog?.Invoke(null, s);
        }

        public static void Error(string s)
        {
            OnError?.Invoke(null, s);
        }

        public static void Progress(int nbr, int max, string s)
        {
            OnProgress?.Invoke(null, (nbr, max, s));
        }
    }
}