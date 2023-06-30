using System;
using System.IO;

#nullable enable

namespace NW.ManyQueues {
    public interface ILog {
        public void Log(string method, string log);
    }

    public class LogFile: ILog {
        static readonly object LOCK = new();

        void ILog.Log(string method, string log) {
            lock (LOCK) {
                File.AppendAllText("ManyQueues.log", $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} {method}: {log}{Environment.NewLine}" );
            }
        }
    }

    public class LogNo: ILog {
        void ILog.Log(string method, string log) {
        }
    }
}