﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#nullable enable

namespace NW.ManyQueues {
    public interface IDataReader {
        public static bool AutoLoad() => true;
    }

    public interface IDataReader<TData>: IDataReader {
        public void Read(string name, IEnumerable<TData> dataList);
    }

    public interface IDataManager: IManager {
        void SubscribeDataReader<TData>(string name, IDataReader<TData> dataReader);
        void LoadDataReaders<TData>(string name);
        public void WriteSingleData<TData>(string name, TData data);
        public void StartBatchWrite(string name);
        public void WriteBatchData<TData>(string name, TData data);
        public void EndBatchWrite(string name, bool allData = true);
        public bool DeleteData(string name);
    }

    public class DataManager: BaseManager, IDataManager {
        public DataManager() : base(null, null) {
        }

        public DataManager(IManager? manager, ILog? log = null) : base(manager, log) {
        }

        private readonly IList<NameDataReaderMethod> _SubscribedDataReaderList = new List<NameDataReaderMethod>();
        private readonly IDictionary<string, IList<object?>> _DataList = new Dictionary<string, IList<object?>>();

        public void SubscribeDataReader<TData>(string name, IDataReader<TData> dataReader) {
            Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{name} {typeof(TData)}");

            if (!_SubscribedDataReaderList.Any(se => se.Name == name && se.DataReader.GetType() == dataReader.GetType())) {
                _SubscribedDataReaderList.Add(new NameDataReaderMethod(name, dataReader, typeof(TData)));

                Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{name} {typeof(TData)} Subscribed");
            }
        }

        public void LoadDataReaders<TData>(string name) {
            Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{name} {typeof(TData)}");

            Type TO = typeof(IDataReader<TData>);

            foreach (Assembly Ass in AppDomain.CurrentDomain.GetAssemblies()) {
                foreach (Type Type in Ass.GetTypes().Where(t => t.IsClass && TO.IsAssignableFrom(t))) {
                    MethodInfo? MethodAutoLoad = Type.GetMethod(nameof(IDataReader.AutoLoad));
                    if ((bool)(MethodAutoLoad?.Invoke(null, null) ?? true)) {
                        IDataReader<TData>? Subscriber = Type.GetConstructors()[0].GetParameters().Length == 1
                            ? (IDataReader<TData>?)Activator.CreateInstance(Type, this)
                            : (IDataReader<TData>?)Activator.CreateInstance(Type);
                        if (Subscriber != null) {
                            SubscribeDataReader<TData>(name, Subscriber);
                        }
                    }
                }
            }

            Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{name} {typeof(TData)} Loaded");
        }

        public bool DeleteData(string name) {
            Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{name}");

            return _DataList.Remove(name);
        }

        public void WriteSingleData<TData>(string name, TData data) {
            Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{name} {typeof(TData)} Start");

            StartBatchWrite(name);
            WriteBatchData(name, data);
            EndBatchWrite(name, true);

            Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{name} {typeof(TData)} End");
        }

        public void StartBatchWrite(string name) {
            Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{name}");

            if (!_DataList.ContainsKey(name)) {
                _DataList.Add(name, new List<object?>());

                Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{name} Started");
            }
        }

        public void WriteBatchData<TData>(string name, TData data) {
            Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{name} {typeof(TData)}");

            _DataList[name].Add(data);
        }

        public void EndBatchWrite(string name, bool allData = true) {
            foreach (NameDataReaderMethod NDRM in _SubscribedDataReaderList.Where(sdr => sdr.Name == name)) {
                Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{name} {allData} Start");

                object DataList = (allData ? NDRM.MethodCast : NDRM.MethodOfType).Invoke(null, new object[] { _DataList[name] })!;
                NDRM.MethodRead.Invoke(NDRM.DataReader, new object[] { name, DataList });

                Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{name} {allData} End");
            }
            DeleteData(name);
        }

        private static bool MethodToSkip(MethodInfo method) {
            foreach (MethodInfo MethodIDataReader in typeof(IDataReader).GetMethods()) {
                if (method.Name == MethodIDataReader.Name) {
                    return true;
                }
            }
            foreach (MethodInfo MethodObject in typeof(object).GetMethods()) {
                if (method.Name == MethodObject.Name) {
                    return true;
                }
            }
            return false;
        }

        private class NameDataReaderMethod {
            public string Name;
            public IDataReader DataReader;
            public MethodInfo MethodRead;
            public MethodInfo MethodCast;
            public MethodInfo MethodOfType;
            public Type DataType;

            public NameDataReaderMethod(string name, IDataReader dataReader, Type dataType) {
                Name = name;
                DataReader = dataReader;
                MethodRead = dataReader.GetType().GetMethod(nameof(IDataReader<object>.Read))!;
                MethodCast = typeof(Enumerable).GetMethod(nameof(Enumerable.Cast))!.MakeGenericMethod(dataType);
                MethodOfType = typeof(Enumerable).GetMethod(nameof(Enumerable.OfType))!.MakeGenericMethod(dataType);
                DataType = dataType;
            }
        }
    }
}
