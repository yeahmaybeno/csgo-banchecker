using CommandLine;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace banchecker
{
    class Program
    {
        public class Options
        {
            [Option('f', "file", Required = false, HelpText = "Accounts file", Default = "accounts.txt")]
            public string File { get; set; }

            [Option('o', "output", Required = false, HelpText = "Output txt file", Default = "unbanned.txt")]
            public string OutputFile { get; set; }

            [Option('t', "threads", Required = false, HelpText = "Max thread count", Default = 2)]
            public int Threads { get; set; }

            [Option('v', "verbose", Required = false, HelpText = "verbose", Default = false)]
            public bool Verbose { get; set; }
        }

        static private void CheckAccount(string login, string log)
        {
            Log.WriteLine(log);

            var details = login.Split(':');
            new BanCheck(details[0], details[1]).Check();
        }

        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args).WithParsed(o =>
            {
                Stopwatch sw = new Stopwatch();
                Queue<Task> tasks = new Queue<Task>();

                Log.Verbose = o.Verbose;

                var count = 1;
                var thread_count = o.Threads;

                sw.Start();

                if (o.File.Contains(".json"))
                {
                    string json = File.ReadAllText(o.File);
                    var accounts = JsonConvert.DeserializeObject<List<BanCheck.AccountDetails>>(json);
                    foreach (var account in accounts)
                    {
                        var log = string.Format("{0} / {1}", count++, accounts.Count);
                        var login = account.login;
                        tasks.Enqueue(new Task(() => CheckAccount(login, log)));
                    }
                }
                else if (o.File.Contains(".txt"))
                {
                    var file_contents = File.ReadAllLines(o.File);
                    foreach (var account in file_contents)
                    {
                        var log = string.Format("{0} / {1}", count++, file_contents.Length);
                        var login = account;
                        tasks.Enqueue(new Task(() => CheckAccount(login, log)));
                    }
                }
                else
                {
                    return;
                }

                int task_count = 0;
                while (tasks.Count >= thread_count)
                {
                    tasks.ElementAt(task_count++).Start();
                    if (task_count >= thread_count)
                    {
                        for (int i = 0; i < thread_count; ++i)
                        {
                            tasks.ElementAt(i).Wait();
                        }

                        for (int i = 0; i < thread_count; ++i)
                        {
                            tasks.Dequeue();
                        }

                        task_count = 0;
                    }
                    Thread.Sleep(100);
                }

                tasks.ToList().ForEach(task => task.Start());
                tasks.ToList().ForEach(task => task.Wait());
                tasks.Clear();

                BanCheck.Dump(o.OutputFile, "accounts.json");

                sw.Stop();

                string seconds_str = TimeSpan.FromSeconds(sw.Elapsed.TotalSeconds).ToString(@"hh\:mm\:ss");
                Log.WriteLine(seconds_str);
            });
        }
    }
}
