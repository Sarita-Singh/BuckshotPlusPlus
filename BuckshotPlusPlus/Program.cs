﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace BuckshotPlusPlus
{
    class Program
    {
        static bool IsHTTP(string FilePath)
        {
            return FilePath.Contains("http");
        }

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Formater.CriticalError("To display all commands: -h");
            }

            string FilePath = args[0];
            if (IsHTTP(FilePath) || File.Exists(FilePath))
            {
                var taskController = new CancellationTokenSource();
                var token = taskController.Token;

                Task t = Task.Run(() => {
                    WebServer.WebServer MyWebServer = StartWebServer(FilePath, token);
                }, token);

                if (!IsHTTP(FilePath)) 
                {
                    FileSystemWatcher watcher = new FileSystemWatcher();
                    watcher.Path = Directory.GetParent(FilePath).FullName;

                    watcher.IncludeSubdirectories = true;
                    watcher.NotifyFilter = NotifyFilters.Attributes |
                    NotifyFilters.CreationTime |
                    NotifyFilters.DirectoryName |
                    NotifyFilters.FileName |
                    NotifyFilters.LastAccess |
                    NotifyFilters.LastWrite |
                    NotifyFilters.Security |
                    NotifyFilters.Size;
                    watcher.Filter = "*.*";
                    watcher.Changed += delegate (object source, FileSystemEventArgs e)
                    {
                        Formater.SuccessMessage("File changed!");
                        taskController.Cancel();
                        t.Wait();
                        t.Dispose();
                        taskController = new CancellationTokenSource();
                        token = taskController.Token;

                        t = Task.Run(() => {
                            WebServer.WebServer MyWebServer = StartWebServer(FilePath, token);
                        }, token);
                    };
                    watcher.Created += delegate (object source, FileSystemEventArgs e)
                    {
                        Formater.SuccessMessage("File created!");
                    };
                    watcher.Deleted += delegate (object source, FileSystemEventArgs e)
                    {
                        Formater.SuccessMessage("File deleted!");
                    };
                    watcher.Renamed += delegate (object source, RenamedEventArgs e)
                    {
                        Formater.SuccessMessage("File renamed!");
                    };
                    //Start monitoring.  
                    watcher.EnableRaisingEvents = true;
                }

                Console.ReadLine();
            }
            else
            {
                Formater.CriticalError($"File {FilePath} not found");
            }
        }

        private static WebServer.WebServer StartWebServer(string FilePath, CancellationToken token)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            Tokenizer MyTokenizer = new Tokenizer(FilePath);

            Console.WriteLine("----------||  BUCKSHOT++  ||----------");
            stopwatch.Stop();
            Formater.SuccessMessage($"Successfully compiled in {stopwatch.ElapsedMilliseconds} ms");

            WebServer.WebServer MyWebServer = new WebServer.WebServer();
            MyWebServer.token = token;
            MyWebServer.Start(MyTokenizer);
            return MyWebServer;
        }

        private static void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}