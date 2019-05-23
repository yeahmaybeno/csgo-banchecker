using System;
using System.IO;
using System.Threading;

namespace banchecker
{
    public class Log
    {
        private static ReaderWriterLock rwl = new ReaderWriterLock();
        static int writerTimeouts = 0;
        static int writes = 0;

        public static string LogFile = "log.txt";
        public static bool Verbose { get; set; }

        private static string GenerateIndent(int indent)
        {
            string str = string.Empty;
            for(int i = 0; i < indent; ++i)
            {
                str += "\t";
            }
            return str;
        }

        public static void WriteLine(string msg, bool logFile = false, int indent = 0)
        {
            var log = string.Format("[INFO]: {0}{1}", GenerateIndent(indent), msg);
            if (Verbose)
            {
                Console.WriteLine(log);
            }

            if (logFile)
            {
                try
                {
                    rwl.AcquireWriterLock(50);
                    try
                    {
                        // It's safe for this thread to access from the shared resource.
                        File.AppendAllText(LogFile, log + Environment.NewLine);
                        Interlocked.Increment(ref writes);
                    }
                    finally
                    {
                        // Ensure that the lock is released.
                        rwl.ReleaseWriterLock();
                    }
                }
                catch (ApplicationException)
                {
                    // The writer lock request timed out.
                    Interlocked.Increment(ref writerTimeouts);
                }
            }
        }
    }
}
