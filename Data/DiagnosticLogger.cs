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
            // Znajdź folder projektu, szukając pliku .csproj
            string projectDirectory = currentDirectory;
            while (!Directory.GetFiles(projectDirectory, "*.csproj").Any() && Directory.GetParent(projectDirectory) != null)
            {
                projectDirectory = Directory.GetParent(projectDirectory).FullName;
            }

            // Folder solucji to folder nadrzędny wobec folderu projektu
            string solutionDirectory = Directory.GetParent(projectDirectory)?.FullName ?? projectDirectory;
            string logsDirectory = Path.Combine(solutionDirectory, "Logs");
            Directory.CreateDirectory(logsDirectory);

            // Unikalna nazwa pliku z datą i godziną
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            logFilePath = Path.Combine(logsDirectory, $"diagnosticsLogs_{timestamp}.log");

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