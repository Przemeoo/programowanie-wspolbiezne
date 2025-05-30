using System;
using System.Collections.Concurrent;
using System.Text;

namespace TP.ConcurrentProgramming.Data
{
    internal class DiagnosticLogger : IDiagnosticLogger, IDisposable
    {
        private static readonly DiagnosticLogger _instance = new DiagnosticLogger();
        public static DiagnosticLogger Instance => _instance;

        private readonly ConcurrentQueue<string> _logBuffer = new ConcurrentQueue<string>();
        private readonly Thread _logThread;
        private volatile bool _isRunning = true;
        private readonly StreamWriter _logWriter;
        private readonly string _logFilePath;
        private readonly object _fileLock = new object();
        private bool _disposed = false;
        private const int MaxBufferSize = 15000;

        private DiagnosticLogger()
        {

            string projectDirectory = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..")); 
            string logsDirectory = Path.Combine(projectDirectory, "Logs");
            Directory.CreateDirectory(logsDirectory); 
            _logFilePath = Path.Combine(logsDirectory, "diagnostics.log");

            _logWriter = new StreamWriter(_logFilePath, append: true, Encoding.ASCII) { AutoFlush = true };
            _logThread = new Thread(LogToFile) { IsBackground = true };
            _logThread.Start();
        }


        public void Log(string message)
        {
            if (_isRunning && !_disposed && _logBuffer.Count < MaxBufferSize)
            {
                _logBuffer.Enqueue($"{DateTime.Now:O}: {message}");
            }

        }

        private void LogToFile()
        {
            while (_isRunning)
            {
                if (_logBuffer.TryDequeue(out var message))
                {
                    lock (_fileLock)
                    {
                        try
                        {
                            _logWriter.WriteLine(message);
                        }
                        catch (IOException ex)
                        {
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
            _isRunning = false;
            _logThread.Join(TimeSpan.FromSeconds(5));
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            Stop();

            while (_logBuffer.TryDequeue(out var message))
            {
                try
                {
                    lock (_fileLock)
                    {
                        _logWriter.WriteLine(message);
                    }
                }
                catch (IOException ex)
                {
                }
            }

            _logWriter?.Dispose();
        }
    }
}