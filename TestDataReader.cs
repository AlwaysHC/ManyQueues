using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NW.ManyQueues {
    class TestDataReader: IDataReader<int> {
        readonly IDataManager _DataManager;

        public TestDataReader(IDataManager dataManager) {
            _DataManager = dataManager;
        }

        public int Sum = 0;

        public void Read(string name, IEnumerable<int> dataList) {
            Sum = _DataManager.ReadConf<int>("Start");

            foreach (int Data in dataList) {
                Sum += Data;
            }
        }
    }
}