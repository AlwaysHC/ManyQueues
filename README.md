# ManyQueues
Solve many problems using interfaces, generics, reflection and queues

## PluginManager

### Define the plugin interface and a class that implement that
```C#
public interface IPluginInterface: IPlugin {
    public bool Interface(int parameter);
}

class PluginClass: IPluginInterface {
    public PluginClass() {
    }

    public int GetPriority() {
        return 1;
    }

    public FirePluginResult GetResult(FirePluginResult? previous) {
        return previous ?? new FirePluginResult();
    }

    public void SetCaller<T>(T caller) where T : class {
    }

    //Something really interesting to do!
    public bool Interface(int parameter) {
        return parameter >= 0;
    }
}
```

### Create the manager, declare explicitly the plugin and link the class to the manager

```C#
IPluginManager PM = new PluginManager();
PM.DeclarePlugin<IPluginInterface>("FirstPlugin");

PM.SubscribePlugin("FirstPlugin", new PluginClass());
```

### Finally you can fire the plugin

```C#
FirePluginResult? R = PM.FirePlugin(this, "FirstPlugin", 1, out IList<PluginReturn<bool>> Returns);
```

### You can do the same using reflection and LoadPlugins method. It will declare and subscribe all plugins that implement IPluginInterface interface

```C#
IPluginManager PM = new PluginManager();
PM.LoadPlugins<IPluginInterface>("FirstPlugin");
```

## ParallelPluginManager

The same of PluginManager (you can use the same IPluginInterfaces) but it execute methods in parallel.

## Data manager

### Implemente the IDataReader interface and define the type of object that it will manage
```C#
class TestDataReader: IDataReader<int> {
    readonly IDataManager _DataManager;

    public TestDataReader(IDataManager dataManager) {
        _DataManager = dataManager;
    }

    public void Read(string name, IEnumerable<int> dataList) {
        int Sum = _DataManager.ReadConf<int>("Start");

        foreach (int Data in dataList) {
            Sum += Data;
        }

        //Something interesting here...
    }
}
```

### Create the manager and link the class to the manager

```C#
IDataManager DM = new DataManager();
DM.SubscribeDataReader("Test", new TestDataReader(DM));
```

### You can do the same using reflection and LoadDataReaders method. It will create all classes that implement IDataReader<Type> interface

_DM.LoadDataReaders<int>("Test");

### You can load single data (the Read method will be called immediatelly)

```C#
DM.WriteSingleData("Test", 2);
```

### You can load batch data (the Read method will be called at the end of load operations)

```C#
DM.StartBatchWrite("Test");
DM.WriteBatchData("Test", 1);
DM.WriteBatchData("Test", 2);
DM.EndBatchWrite("Test");
```

## PipelineManager

### Define a token class and implement some classes from IPipeline<Token>

```C#
class Token {
    public int Number = 0;
}

class TestPipelineStep1: IPipeline<Token> {
    Token _Token = new Token();

    public void SetCaller<T>(T caller) where T : class {
    }

    public void Execute1(int number) {
        _Token.Number += number;
    }

    public void SetToken(Token token) {
        _Token = token;
    }
}

...
```

### Create the manager and declare the pipeline steps

```C#
IPipelineManager _PM = new PipelineManager();
PM.CreatePipeline("1_2_3", new Type[] { typeof(TestPipelineStep1), typeof(TestPipelineStep2), typeof(TestPipelineStep3) }, null);
```

### You can run the pipeline passing a pre created Token

```C#
Token Token = new Token();
PM.RunPipeline(this, "1_2_3", 10, Token);
```

### Or wait that the pipeline will create that for you

```C#
PM.RunPipeline(this, "1_2_3", 20, out Token Token);
```
