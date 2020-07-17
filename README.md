# ManyQueues
Solve many problems using interfaces, generics, reflection and queues

# PluginManager

## Define the plugin interface and a class that implement that
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

## Create the manager, declare explicitly the plugin and link the class to the manager

```C#
IPluginManager PM = new PluginManager();
PM.DeclarePlugin<IPluginInterface>("FirstPlugin");

PM.SubscribePlugin("FirstPlugin", new PluginClass());
```

## Finally you can fire the plugin

```C#
FirePluginResult? R = PM.FirePlugin(this, "FirstPlugin", 1, out IList<PluginReturn<bool>> Returns);
```

## You can do the same using reflection and LoadPlugins method. It will declare and subscribe all plugins that implement IPluginInterface interface

```C#
IPluginManager PM = new PluginManager();
PM.LoadPlugins<IPluginInterface>("FirstPlugin");
```
