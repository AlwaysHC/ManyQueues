using System.Collections.Generic;

#nullable enable

namespace NW.ManyQueues {
    public interface IManager {
        public void WriteConf(string name, string value);
        public void WriteConf<TValue>(string name, TValue value);
        public string ReadConf(string name);
        public TValue ReadConf<TValue>(string name);
    }

    public class Manager: IManager {
        private readonly Dictionary<string, object> _Conf = new Dictionary<string, object>();

        public string ReadConf(string name) {
            return (string)_Conf[name];
        }

        public TValue ReadConf<TValue>(string name) {
            return (TValue)_Conf[name];
        }

        public void WriteConf(string name, string value) {
            _Conf[name] = value;
        }

        public void WriteConf<TValue>(string name, TValue value) {
            _Conf[name] = value!;
        }
    }

    public class BaseManager: IManager {
        readonly private IManager _Manager;
        readonly protected ILog Log;

        public BaseManager(IManager? manager, ILog? log) {
            _Manager = manager ?? new Manager();
            Log = log ?? new LogNo();
        }

        public void WriteConf(string name, string value) {
            _Manager.WriteConf(name, value);
        }

        public void WriteConf<TValue>(string name, TValue value) {
            _Manager.WriteConf(name, value);
        }

        public string ReadConf(string name) {
            return _Manager.ReadConf(name);
        }

        public TValue ReadConf<TValue>(string name) {
            return _Manager.ReadConf<TValue>(name);
        }
    }
}