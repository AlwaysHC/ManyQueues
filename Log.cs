using System;
using System.Collections.Generic;
using System.IO;

#nullable enable

namespace NW.ManyQueues {
    public interface ILog {
        public void Log(string method, string log);
    }

    public class LogFile: ILog {
        void ILog.Log(string method, string log) {
            File.AppendAllLines("ManyQueues.log", new List<string>() { $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} {method}: {log}" });
        }
    }

    public class LogNo: ILog {
        void ILog.Log(string method, string log) {
        }
    }
}