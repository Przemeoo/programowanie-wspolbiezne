using System;
using System.Collections.Concurrent;
using System.Text;

namespace TP.ConcurrentProgramming.Data
{
    internal class DiagnosticLogger : IDiagnosticLogger, IDisposable
    {
        private static readonly DiagnosticLogger _instance = new DiagnosticLogger();
        public static DiagnosticLogger Instance => _instance;

        private readonly ConcurrentQueue<string> logBuffer = new ConcurrentQueue<string>();
        private readonly Thread logThread;
        private volatile bool isRunning = true;
        private readonly StreamWriter logWriter;
        private readonly string logFilePath;
        private readonly object fileLock = new object();
        private bool disposed = false;
        private const int MaxBufferSize = 1500;

        private DiagnosticLogger()
        {
            string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string presentationDirectory = Directory.GetParent(currentDirectory)
                .Parent
                .Parent
                .Parent.FullName;

            string projectDirectory = Directory.GetParent(presentationDirectory).FullName;
            string logsDirectory = Path.Combine(projectDirectory, "Logs");
            Directory.CreateDirectory(logsDirectory);

            foreach (var file in Directory.GetFiles(logsDirectory, "diagnosticsLogs_*.log"))
            {
                if (File.GetCreationTime(file) < DateTime.Now.AddDays(-7))
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch (IOException) { }
                }
            }

            string dateName = DateTime.Now.ToString("yyyyMMdd_HHmm");
            logFilePath = Path.Combine(logsDirectory, $"diagnosticsLogs_{dateName}.log");

            logWriter = new StreamWriter(logFilePath, append: false, Encoding.ASCII) { AutoFlush = true };
            logThread = new Thread(LogToFile) { IsBackground = true };
            logThread.Start();
        }


        public void Log(string message)
        {
            if (isRunning && !disposed && logBuffer.Count < MaxBufferSize)
            {
                logBuffer.Enqueue($"{DateTime.Now:O}: {message}");
            }

        }

        private void LogToFile()
        {
            while (isRunning)
            {
                if (logBuffer.TryDequeue(out var message))
                {
                    lock (fileLock)
                    {
                        try
                        {
                            logWriter.WriteLine(message);
                        }
                        catch (IOException ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error writing to log file: {ex.Message}");
                        }
                    }
                }
                else
                {
                    Thread.Sleep(10); 
                }
            }
        }

        public void Stop()
        {
            isRunning = false;
            logThread.Join(TimeSpan.FromSeconds(5));
        }

        public void Dispose()
        {
            if (disposed)
                return;

            disposed = true;
            Stop();

            while (logBuffer.TryDequeue(out var message))
            {
                try
                {
                    lock (fileLock)
                    {
                        logWriter.WriteLine(message);
                    }
                }
                catch (IOException ex)
                {
                }
            }

            logWriter?.Dispose();
        }
    }
}