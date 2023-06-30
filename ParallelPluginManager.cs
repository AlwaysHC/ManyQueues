using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

#nullable enable

namespace NW.ManyQueues {
    public interface IParallelPluginManager: IManager {
        bool DeclarePlugin<TPlugin>(string name) where TPlugin : IPlugin;
        void SubscribePlugin<TPlugin>(string name, TPlugin pluginClass) where TPlugin : IPlugin;
        void LoadPlugins<TPlugin>(string name) where TPlugin : IPlugin;
        IReadOnlyList<ParallelFirePluginResult<TReturn>> FirePlugin<TCaller, TReturn>(TCaller caller, string name);
        IReadOnlyList<ParallelFirePluginResult<TReturn>> FirePlugin<TCaller, TReturn, TParam1>(TCaller caller, string name, TParam1 param1);
        IReadOnlyList<ParallelFirePluginResult<TReturn>> FirePlugin<TCaller, TReturn, TParam1, TParam2>(TCaller caller, string name, TParam1 param1, TParam2 param2);
        IReadOnlyList<ParallelFirePluginResult<TReturn>> FirePlugin<TCaller, TReturn, TParam1, TParam2, TParam3>(TCaller caller, string name, TParam1 param1, TParam2 param2, TParam3 param3);
        IReadOnlyList<FirePluginResult> FirePlugin<TCaller>(TCaller caller, string name);
        IReadOnlyList<FirePluginResult> FirePlugin<TCaller, TParam1>(TCaller caller, string name, TParam1 param1);
        IReadOnlyList<FirePluginResult> FirePlugin<TCaller, TParam1, TParam2>(TCaller caller, string name, TParam1 param1, TParam2 param2);
        IReadOnlyList<FirePluginResult> FirePlugin<TCaller, TParam1, TParam2, TParam3>(TCaller caller, string name, TParam1 param1, TParam2 param2, TParam3 param3);
    }

    public class ParallelFirePluginResult<TReturn>: FirePluginResult {
        public PluginReturn<TReturn>? Return {
            get;
        }

        public ParallelFirePluginResult(FirePluginResult? firePluginResult, PluginReturn<TReturn>? @return) {
            if (firePluginResult != null) {
                Blocking = firePluginResult.Blocking;
                Data = firePluginResult.Data;
                Error = firePluginResult.Error;
                Message = firePluginResult.Message;
            }
            Return = @return;
        }
    }

    public class ParallelPluginManager: BaseManager, IParallelPluginManager {
        public ParallelPluginManager() : base(null, null) {
        }

        public ParallelPluginManager(IManager? manager, ILog? log = null) : base(manager, log) {
        }

        private readonly IList<NamePlugin> _SubscribedPluginList = new List<NamePlugin>();
        private readonly IDictionary<string, Type> _DeclaredPluginList = new Dictionary<string, Type>();

        public bool DeclarePlugin<TPlugin>(string name) where TPlugin : IPlugin {
            Log.Log(MethodBase.GetCurrentMethod()!.Name, name);

            return _DeclaredPluginList.TryAdd(name, typeof(TPlugin));
        }

        public IReadOnlyList<ParallelFirePluginResult<TReturn>> FirePlugin<TCaller, TReturn>(TCaller caller, string name) {
            Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{typeof(TCaller)} {name} Start");

            IReadOnlyList<MethodPluginPriortity<TCaller>> MethodListToCall = GetMethodListToCall(caller, name).OrderBy(mp => mp.Priority).ToList();

            IList<ParallelFirePluginResult<TReturn>> R = new List<ParallelFirePluginResult<TReturn>>();
            object Lock = new();

            Parallel.ForEach(MethodListToCall, (mpp) => {
                Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{typeof(TCaller)} {name} {mpp.Plugin.GetType().Name} Start");

                PluginReturn<TReturn> Return = new(mpp.Plugin.GetType().Name, (TReturn)mpp.Method.Invoke(mpp.Plugin, null)!);

                Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{typeof(TCaller)} {name} {mpp.Plugin.GetType().Name} End");

                FirePluginResult? Result = (FirePluginResult?)mpp.MethodGetResult.Invoke(mpp.Plugin, new object[] { null! });
                lock (Lock) {
                    R.Add(new ParallelFirePluginResult<TReturn>(Result, Return));
                }
            });

            Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{typeof(TCaller)} {name} End");

            return (IReadOnlyList<ParallelFirePluginResult<TReturn>>)R;
        }

        public IReadOnlyList<ParallelFirePluginResult<TReturn>> FirePlugin<TCaller, TReturn, TParam1>(TCaller caller, string name, TParam1 param1) {
            Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{typeof(TCaller)} {name} Start");

            IReadOnlyList<MethodPluginPriortity<TCaller>> MethodListToCall = GetMethodListToCall(caller, name).OrderBy(mp => mp.Priority).ToList();

            IList<ParallelFirePluginResult<TReturn>> R = new List<ParallelFirePluginResult<TReturn>>();
            object Lock = new();

            Parallel.ForEach(MethodListToCall, (mpp) => {
                Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{typeof(TCaller)} {name} {mpp.Plugin.GetType().Name} Start");

                PluginReturn<TReturn> Return = new(mpp.Plugin.GetType().Name, (TReturn)mpp.Method.Invoke(mpp.Plugin, new object[] { param1! })!);

                Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{typeof(TCaller)} {name} {mpp.Plugin.GetType().Name} End");


                FirePluginResult? Result = (FirePluginResult?)mpp.MethodGetResult.Invoke(mpp.Plugin, new object[] { null! });
                lock (Lock) {
                    R.Add(new ParallelFirePluginResult<TReturn>(Result, Return));
                }
            });

            Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{typeof(TCaller)} {name} End");

            return (IReadOnlyList<ParallelFirePluginResult<TReturn>>)R;
        }

        public IReadOnlyList<ParallelFirePluginResult<TReturn>> FirePlugin<TCaller, TReturn, TParam1, TParam2>(TCaller caller, string name, TParam1 param1, TParam2 param2) {
            Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{typeof(TCaller)} {name} Start");

            IReadOnlyList<MethodPluginPriortity<TCaller>> MethodListToCall = GetMethodListToCall(caller, name).OrderBy(mp => mp.Priority).ToList();

            IList<ParallelFirePluginResult<TReturn>> R = new List<ParallelFirePluginResult<TReturn>>();
            object Lock = new();

            Parallel.ForEach(MethodListToCall, (mpp) => {
                Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{typeof(TCaller)} {name} {mpp.Plugin.GetType().Name} Start");

                PluginReturn<TReturn> Return = new(mpp.Plugin.GetType().Name, (TReturn)mpp.Method.Invoke(mpp.Plugin, new object[] { param1!, param2! })!);

                Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{typeof(TCaller)} {name} {mpp.Plugin.GetType().Name} End");

                FirePluginResult? Result = (FirePluginResult?)mpp.MethodGetResult.Invoke(mpp.Plugin, new object[] { null! });
                lock (Lock) {
                    R.Add(new ParallelFirePluginResult<TReturn>(Result, Return));
                }
            });

            Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{typeof(TCaller)} {name} End");

            return (IReadOnlyList<ParallelFirePluginResult<TReturn>>)R;
        }

        public IReadOnlyList<ParallelFirePluginResult<TReturn>> FirePlugin<TCaller, TReturn, TParam1, TParam2, TParam3>(TCaller caller, string name, TParam1 param1, TParam2 param2, TParam3 param3) {
            Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{typeof(TCaller)} {name} Start");

            IReadOnlyList<MethodPluginPriortity<TCaller>> MethodListToCall = GetMethodListToCall(caller, name).OrderBy(mp => mp.Priority).ToList();

            IList<ParallelFirePluginResult<TReturn>> R = new List<ParallelFirePluginResult<TReturn>>();
            object Lock = new();

            Parallel.ForEach(MethodListToCall, (mpp) => {
                Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{typeof(TCaller)} {name} {mpp.Plugin.GetType().Name} Start");

                PluginReturn<TReturn> Return = new(mpp.Plugin.GetType().Name, (TReturn)mpp.Method.Invoke(mpp.Plugin, new object[] { param1!, param2!, param3! })!);

                Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{typeof(TCaller)} {name} {mpp.Plugin.GetType().Name} End");

                FirePluginResult? Result = (FirePluginResult?)mpp.MethodGetResult.Invoke(mpp.Plugin, new object[] { null! });
                lock (Lock) {
                    R.Add(new ParallelFirePluginResult<TReturn>(Result, Return));
                }
            });

            Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{typeof(TCaller)} {name} End");

            return (IReadOnlyList<ParallelFirePluginResult<TReturn>>)R;
        }

        public IReadOnlyList<FirePluginResult> FirePlugin<TCaller>(TCaller caller, string name) {
            Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{typeof(TCaller)} {name} Start");

            IReadOnlyList<MethodPluginPriortity<TCaller>> MethodListToCall = GetMethodListToCall(caller, name).OrderBy(mp => mp.Priority).ToList();

            IList<FirePluginResult> R = new List<FirePluginResult>();
            object Lock = new();

            foreach (MethodPluginPriortity<TCaller> MPP in MethodListToCall) {
                Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{typeof(TCaller)} {name} {MPP.Plugin.GetType().Name} Start");

                MPP.Method.Invoke(MPP.Plugin, null);

                Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{typeof(TCaller)} {name} {MPP.Plugin.GetType().Name} End");

                lock (Lock) {
                    R.Add((FirePluginResult?)MPP.MethodGetResult.Invoke(MPP.Plugin, new object[] { null! })!);
                }
            }

            Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{typeof(TCaller)} {name} End");

            return (IReadOnlyList<FirePluginResult>)R;
        }

        public IReadOnlyList<FirePluginResult> FirePlugin<TCaller, TParam1>(TCaller caller, string name, TParam1 param1) {
            Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{typeof(TCaller)} {name} Start");

            IReadOnlyList<MethodPluginPriortity<TCaller>> MethodListToCall = GetMethodListToCall(caller, name).OrderBy(mp => mp.Priority).ToList();

            IList<FirePluginResult> R = new List<FirePluginResult>();
            object Lock = new();

            foreach (MethodPluginPriortity<TCaller> MPP in MethodListToCall) {
                Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{typeof(TCaller)} {name} {MPP.Plugin.GetType().Name} Start");

                MPP.Method.Invoke(MPP.Plugin, new object[] { param1! });

                Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{typeof(TCaller)} {name} {MPP.Plugin.GetType().Name} End");

                lock (Lock) {
                    R.Add((FirePluginResult?)MPP.MethodGetResult.Invoke(MPP.Plugin, new object[] { null! })!);
                }
            }

            Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{typeof(TCaller)} {name} End");

            return (IReadOnlyList<FirePluginResult>)R;
        }

        public IReadOnlyList<FirePluginResult> FirePlugin<TCaller, TParam1, TParam2>(TCaller caller, string name, TParam1 param1, TParam2 param2) {
            Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{typeof(TCaller)} {name} Start");

            IReadOnlyList<MethodPluginPriortity<TCaller>> MethodListToCall = GetMethodListToCall(caller, name).OrderBy(mp => mp.Priority).ToList();

            IList<FirePluginResult> R = new List<FirePluginResult>();
            object Lock = new();

            foreach (MethodPluginPriortity<TCaller> MPP in MethodListToCall) {
                Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{typeof(TCaller)} {name} {MPP.Plugin.GetType().Name} Start");

                MPP.Method.Invoke(MPP.Plugin, new object[] { param1!, param2! });

                Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{typeof(TCaller)} {name} {MPP.Plugin.GetType().Name} End");

                lock (Lock) {
                    R.Add((FirePluginResult?)MPP.MethodGetResult.Invoke(MPP.Plugin, new object[] { null! })!);
                }
            }

            Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{typeof(TCaller)} {name} End");

            return (IReadOnlyList<FirePluginResult>)R;
        }

        public IReadOnlyList<FirePluginResult> FirePlugin<TCaller, TParam1, TParam2, TParam3>(TCaller caller, string name, TParam1 param1, TParam2 param2, TParam3 param3) {
            Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{typeof(TCaller)} {name} Start");

            IReadOnlyList<MethodPluginPriortity<TCaller>> MethodListToCall = GetMethodListToCall(caller, name).OrderBy(mp => mp.Priority).ToList();

            IList<FirePluginResult> R = new List<FirePluginResult>();
            object Lock = new();

            foreach (MethodPluginPriortity<TCaller> MPP in MethodListToCall) {
                Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{typeof(TCaller)} {name} {MPP.Plugin.GetType().Name} Start");

                MPP.Method.Invoke(MPP.Plugin, new object[] { param1!, param2!, param3! });

                Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{typeof(TCaller)} {name} {MPP.Plugin.GetType().Name} End");

                lock (Lock) {
                    R.Add((FirePluginResult?)MPP.MethodGetResult.Invoke(MPP.Plugin, new object[] { null! })!);
                }
            }

            Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{typeof(TCaller)} {name} End");

            return (IReadOnlyList<FirePluginResult>)R;
        }

        private IReadOnlyList<MethodPluginPriortity<TCaller>> GetMethodListToCall<TCaller>(TCaller caller, string name) {
            IList<MethodPluginPriortity<TCaller>> R = new List<MethodPluginPriortity<TCaller>>();

            foreach (NamePlugin NamePlugin in _SubscribedPluginList.Where(sp => sp.Name == name)) {
                if (!_DeclaredPluginList.Any(de => de.Key == name)) {
                    continue;
                }

                foreach (MethodInfo Method in NamePlugin.Plugin.GetType().GetMethods()) {
                    if (MethodToSkip(Method)) {
                        continue;
                    }

                    R.Add(new MethodPluginPriortity<TCaller>(caller, NamePlugin, Method));
                }
            }

            return (IReadOnlyList<MethodPluginPriortity<TCaller>>)R;
        }

        private static bool MethodToSkip(MethodInfo method) {
            foreach (MethodInfo MethodIPlugin in typeof(IPlugin).GetMethods()) {
                if (method.Name == MethodIPlugin.Name) {
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

        public void SubscribePlugin<TPlugin>(string name, TPlugin pluginClass) where TPlugin : IPlugin {
            Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{name} {typeof(TPlugin)}");

            if (!_SubscribedPluginList.Any(sp => sp.Name == name && sp.Plugin.GetType() == pluginClass.GetType())) {
                _SubscribedPluginList.Add(new NamePlugin(name, pluginClass));

                Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{name} {typeof(TPlugin)} Subscribed");
            }
        }

        public void LoadPlugins<TPlugin>(string name) where TPlugin : IPlugin {
            Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{name} {typeof(TPlugin)}");

            DeclarePlugin<TPlugin>(name);

            Type TO = typeof(TPlugin);

            foreach (Assembly Ass in AppDomain.CurrentDomain.GetAssemblies()) {
                foreach (Type Type in Ass.GetTypes().Where(t => t.IsClass && TO.IsAssignableFrom(t))) {
                    MethodInfo? MethodAutoLoad = Type.GetMethod(nameof(IPlugin.AutoLoad));
                    if ((bool)(MethodAutoLoad?.Invoke(null, null) ?? true)) {
                        IPlugin? Subscriber = Type.GetConstructors()[0].GetParameters().Length == 1
                            ? (IPlugin?)Activator.CreateInstance(Type, this)
                            : (IPlugin?)Activator.CreateInstance(Type);
                        if (Subscriber != null) {
                            SubscribePlugin(name, Subscriber);
                        }
                    }
                }
            }

            Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{name} {typeof(TPlugin)} Loaded");
        }

        private class NamePlugin {
            public string Name;
            public IPlugin Plugin;
            public bool IsSetCallerCalled = false;

            public NamePlugin(string name, IPlugin plugin) {
                Name = name;
                Plugin = plugin;
            }
        }

        private class MethodPluginPriortity<TCaller> {
            public IPlugin Plugin {
                get;
            }
            public MethodInfo Method {
                get;
            }
            public int Priority {
                get;
            }
            public MethodInfo MethodGetPriority {
                get;
            }
            public MethodInfo MethodGetResult {
                get;
            }
            public MethodInfo MethodSetCaller {
                get;
            }

            public MethodPluginPriortity(TCaller caller, NamePlugin namePlugin, MethodInfo method) {
                Plugin = namePlugin.Plugin;
                Method = method;

                MethodGetPriority = Plugin.GetType().GetMethod(nameof(IPlugin.GetPriority))!;
                MethodGetResult = Plugin.GetType().GetMethod(nameof(IPlugin.GetResult))!;
                MethodSetCaller = Plugin.GetType().GetMethod(nameof(IPlugin.SetCaller))!;

                if (!namePlugin.IsSetCallerCalled) {
                    if (caller != null) {
                        MethodInfo GenericSetCaller = MethodSetCaller.MakeGenericMethod(typeof(TCaller));
                        GenericSetCaller.Invoke(namePlugin.Plugin, new object[] { caller });
                    }
                    namePlugin.IsSetCallerCalled = true;
                }

                Priority = (int)MethodGetPriority.Invoke(Plugin, null)!;
            }
        }
    }
}
