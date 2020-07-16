using System.Collections.Generic;
using Xunit;

#nullable enable

namespace NW.ManyQueues.Test {

    public class DataTest {
        readonly static IManager M = new Manager();
        readonly static ILog LOG = new LogFile();
        readonly IDataManager _DM = new DataManager(M, LOG);

        [Fact]
        public void TestBatch() {
            _DM.LoadDataReaders<int>("AAA");

            M.WriteConf("Start", 1000);

            _DM.StartBatchWrite("AAA");
            _DM.WriteBatchData("AAA", 1);
            _DM.WriteBatchData("AAA", "a");
            _DM.EndBatchWrite("AAA", false);
        }

        [Fact]
        public void TestSingle() {
            _DM.LoadDataReaders<int>("AAA");

            M.WriteConf("Start", 1000);

            _DM.WriteSingleData("AAA", 2);
        }
    }
}
