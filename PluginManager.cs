using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#nullable enable

namespace NW.ManyQueues {
#warning Possibilità di iniettare valori dall'esterno
#warning Integrare Plugin e ParallelPlugin
#warning I vari manager devono avere dei metodi per salvare dei dati configurazione e per leggere tali valori all'interno degli oggetti creati (che ricevono I*Manager nel costruttore)

    //TODO Funzionamento:
    //TODO - Plugin: un oggetto definisce dei punti di ingresso dove altre classi possono intervenire. Coda sugli eventi
    //TODO - Messaggistica: si registra un evento che viene eseguito se ci sono i dati richiesti. Coda sui dati
    //TODO - Pipeline statica: passaggio di un token tra i vari step, la lista degli step è definita dal chiamante
    //TODO - Pipeline dinamica: passaggio di un token tra i vari step, la lista degli step varia dimanicamente

    //TODO Static
    //public void SearchForSubscriber() {
    //    Type TO = typeof(IManyQueuesSubscriberStatic);
    //
    //    foreach (Assembly Ass in AppDomain.CurrentDomain.GetAssemblies()) {
    //        foreach (Type T in Ass.GetTypes()) {
    //            if (TO.IsAssignableFrom(T)) {
    //                MethodInfo MI = T.GetMethod("SubscriptionStatic");
    //                MI.Invoke(null, new object[] { this });
    //            }
    //        }
    //    }
    //}

    public interface IPlugin {
        public void SetCaller<T>(T caller) where T : class;
        public static bool AutoLoad() => true;
        //TODO public int GetPriority(Func<int> minPriority, Func<int> maxPriority);
        public int GetPriority();
        public FirePluginResult GetResult(FirePluginResult? previous);
    }

    public interface IPluginManager: IManager {
        bool DeclarePlugin<TPlugin>(string name) where TPlugin : IPlugin;
        void SubscribePlugin<TPlugin>(string name, TPlugin pluginClass) where TPlugin : IPlugin;
        void LoadPlugins<TPlugin>(string name) where TPlugin : IPlugin;
        FirePluginResult? FirePlugin<TCaller, TReturn>(TCaller caller, string name, out IList<PluginReturn<TReturn>> returns);
        FirePluginResult? FirePlugin<TCaller, TReturn, TParam1>(TCaller caller, string name, TParam1 param1, out IList<PluginReturn<TReturn>> returns);
        FirePluginResult? FirePlugin<TCaller, TReturn, TParam1, TParam2>(TCaller caller, string name, TParam1 param1, TParam2 param2, out IList<PluginReturn<TReturn>> returns);
        FirePluginResult? FirePlugin<TCaller, TReturn, TParam1, TParam2, TParam3>(TCaller caller, string name, TParam1 param1, TParam2 param2, TParam3 param3, out IList<PluginReturn<TReturn>> returns);
        FirePluginResult? FirePlugin<TCaller>(TCaller caller, string name);
        FirePluginResult? FirePlugin<TCaller, TParam1>(TCaller caller, string name, TParam1 param1);
        FirePluginResult? FirePlugin<TCaller, TParam1, TParam2>(TCaller caller, string name, TParam1 param1, TParam2 param2);
        FirePluginResult? FirePlugin<TCaller, TParam1, TParam2, TParam3>(TCaller caller, string name, TParam1 param1, TParam2 param2, TParam3 param3);
    }

    public class PluginReturn<TReturn> {
        public string ClassName {
            get;
        }
        public TReturn Return {
            get;
        }
        public PluginReturn(string className, TReturn @return) {
            ClassName = className;
            Return = @return;
        }
    }

    public class FirePluginResult {
        private bool _Blocking;
        public bool Blocking {
            get {
                return _Blocking;
            }
            set {
                if (value) {
                    Error = true;
                    _Blocking = true;
                }
                else {
                    _Blocking = false;
                }
            }
        }

        public bool Error = false;

        public string Message = "";

        public object? Data = null;
    }

    public class PluginManager: BaseManager, IPluginManager {
        public PluginManager() : base(null, null) {
        }

        public PluginManager(IManager? manager, ILog? log) : base(manager, log) {
        }

        private readonly IList<NamePlugin> _SubscribedPluginList = new List<NamePlugin>();
        private readonly IDictionary<string, Type> _DeclaredPluginList = new Dictionary<string, Type>();

        public bool DeclarePlugin<TPlugin>(string name) where TPlugin : IPlugin {
            Log.Log(MethodBase.GetCurrentMethod()!.Name, name);

            return _DeclaredPluginList.TryAdd(name, typeof(TPlugin));
        }

        public FirePluginResult? FirePlugin<TCaller, TReturn>(TCaller caller, string name, out IList<PluginReturn<TReturn>> returns) {
            Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{typeof(TCaller)} {name} Start");

            IList<MethodPluginPriortity<TCaller>> MethodListToCall = GetMethodListToCall(caller, name).OrderBy(mp => mp.Priority).ToList();

            FirePluginResult? R = null;
            returns = new List<PluginReturn<TReturn>>();

            foreach (MethodPluginPriortity<TCaller> MPP in MethodListToCall) {
                Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{typeof(TCaller)} {name} {MPP.Plugin.GetType().Name} Start");

                returns.Add(new PluginReturn<TReturn>(MPP.Plugin.GetType().Name, (TReturn)MPP.Method.Invoke(MPP.Plugin, null)!));

                Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{typeof(TCaller)} {name} {MPP.Plugin.GetType().Name} End");

                R = (FirePluginResult?)MPP.MethodGetResult.Invoke(MPP.Plugin, new object[] { R! });
                if (R != null && R.Blocking) {
                    break;
                }
            }

            Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{typeof(TCaller)} {name} End - {R?.Blocking} {R?.Error} {R?.Message}");

            return R;
        }

        public FirePluginResult? FirePlugin<TCaller, TReturn, TParam1>(TCaller caller, string name, TParam1 param1, out IList<PluginReturn<TReturn>> returns) {
            Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{typeof(TCaller)} {name} Start");

            IList<MethodPluginPriortity<TCaller>> MethodListToCall = GetMethodListToCall(caller, name).OrderBy(mp => mp.Priority).ToList();

            FirePluginResult? R = null;
            returns = new List<PluginReturn<TReturn>>();

            foreach (MethodPluginPriortity<TCaller> MPP in MethodListToCall) {
                Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{typeof(TCaller)} {name} {MPP.Plugin.GetType().Name} Start");

                returns.Add(new PluginReturn<TReturn>(MPP.Plugin.GetType().Name, (TReturn)MPP.Method.Invoke(MPP.Plugin, new object[] { param1! })!));
            
                Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{typeof(TCaller)} {name} {MPP.Plugin.GetType().Name} End");

                R = (FirePluginResult?)MPP.MethodGetResult.Invoke(MPP.Plugin, new object[] { R! });
                if (R != null && R.Blocking) {
                    break;
                }
            }

            Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{typeof(TCaller)} {name} End - {R?.Blocking} {R?.Error} {R?.Message}");

            return R;
        }

        public FirePluginResult? FirePlugin<TCaller, TReturn, TParam1, TParam2>(TCaller caller, string name, TParam1 param1, TParam2 param2, out IList<PluginReturn<TReturn>> returns) {
            Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{typeof(TCaller)} {name} Start");

            IList<MethodPluginPriortity<TCaller>> MethodListToCall = GetMethodListToCall(caller, name).OrderBy(mp => mp.Priority).ToList();

            FirePluginResult? R = null;
            returns = new List<PluginReturn<TReturn>>();

            foreach (MethodPluginPriortity<TCaller> MPP in MethodListToCall) {
                Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{typeof(TCaller)} {name} {MPP.Plugin.GetType().Name} Start");

                returns.Add(new PluginReturn<TReturn>(MPP.Plugin.GetType().Name, (TReturn)MPP.Method.Invoke(MPP.Plugin, new object[] { param1!, param2! })!));

                Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{typeof(TCaller)} {name} {MPP.Plugin.GetType().Name} End");

                R = (FirePluginResult?)MPP.MethodGetResult.Invoke(MPP.Plugin, new object[] { R! });
                if (R != null && R.Blocking) {
                    break;
                }
            }

            Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{typeof(TCaller)} {name} End - {R?.Blocking} {R?.Error} {R?.Message}");

            return R;
        }

        public FirePluginResult? FirePlugin<TCaller, TReturn, TParam1, TParam2, TParam3>(TCaller caller, string name, TParam1 param1, TParam2 param2, TParam3 param3, out IList<PluginReturn<TReturn>> returns) {
            Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{typeof(TCaller)} {name} Start");

            IList<MethodPluginPriortity<TCaller>> MethodListToCall = GetMethodListToCall(caller, name).OrderBy(mp => mp.Priority).ToList();

            FirePluginResult? R = null;
            returns = new List<PluginReturn<TReturn>>();

            foreach (MethodPluginPriortity<TCaller> MPP in MethodListToCall) {
                Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{typeof(TCaller)} {name} {MPP.Plugin.GetType().Name} Start");

                returns.Add(new PluginReturn<TReturn>(MPP.Plugin.GetType().Name, (TReturn)MPP.Method.Invoke(MPP.Plugin, new object[] { param1!, param2!, param3! })!));

                Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{typeof(TCaller)} {name} {MPP.Plugin.GetType().Name} End");

                R = (FirePluginResult?)MPP.MethodGetResult.Invoke(MPP.Plugin, new object[] { R! });
                if (R != null && R.Blocking) {
                    break;
                }
            }

            Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{typeof(TCaller)} {name} End - {R?.Blocking} {R?.Error} {R?.Message}");

            return R;
        }

        public FirePluginResult? FirePlugin<TCaller>(TCaller caller, string name) {
            Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{typeof(TCaller)} {name} Start");

            IList<MethodPluginPriortity<TCaller>> MethodListToCall = GetMethodListToCall(caller, name).OrderBy(mp => mp.Priority).ToList();

            FirePluginResult? R = null;

            foreach (MethodPluginPriortity<TCaller> MPP in MethodListToCall) {
                Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{typeof(TCaller)} {name} {MPP.Plugin.GetType().Name} Start");

                MPP.Method.Invoke(MPP.Plugin, null);

                Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{typeof(TCaller)} {name} {MPP.Plugin.GetType().Name} End");

                R = (FirePluginResult?)MPP.MethodGetResult.Invoke(MPP.Plugin, new object[] { R! });
                if (R != null && R.Blocking) {
                    break;
                }
            }

            Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{typeof(TCaller)} {name} End - {R?.Blocking} {R?.Error} {R?.Message}");

            return R;
        }

        public FirePluginResult? FirePlugin<TCaller, TParam1>(TCaller caller, string name, TParam1 param1) {
            Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{typeof(TCaller)} {name} Start");

            IList<MethodPluginPriortity<TCaller>> MethodListToCall = GetMethodListToCall(caller, name).OrderBy(mp => mp.Priority).ToList();

            FirePluginResult? R = null;

            foreach (MethodPluginPriortity<TCaller> MPP in MethodListToCall) {
                Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{typeof(TCaller)} {name} {MPP.Plugin.GetType().Name} Start");

                MPP.Method.Invoke(MPP.Plugin, new object[] { param1! });

                Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{typeof(TCaller)} {name} {MPP.Plugin.GetType().Name} End");

                R = (FirePluginResult?)MPP.MethodGetResult.Invoke(MPP.Plugin, new object[] { R! });
                if (R != null && R.Blocking) {
                    break;
                }
            }

            Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{typeof(TCaller)} {name} End - {R?.Blocking} {R?.Error} {R?.Message}");

            return R;
        }

        public FirePluginResult? FirePlugin<TCaller, TParam1, TParam2>(TCaller caller, string name, TParam1 param1, TParam2 param2) {
            Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{typeof(TCaller)} {name} Start");

            IList<MethodPluginPriortity<TCaller>> MethodListToCall = GetMethodListToCall(caller, name).OrderBy(mp => mp.Priority).ToList();

            FirePluginResult? R = null;

            foreach (MethodPluginPriortity<TCaller> MPP in MethodListToCall) {
                Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{typeof(TCaller)} {name} {MPP.Plugin.GetType().Name} Start");

                MPP.Method.Invoke(MPP.Plugin, new object[] { param1!, param2! });

                Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{typeof(TCaller)} {name} {MPP.Plugin.GetType().Name} End");

                R = (FirePluginResult?)MPP.MethodGetResult.Invoke(MPP.Plugin, new object[] { R! });
                if (R != null && R.Blocking) {
                    break;
                }
            }

            Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{typeof(TCaller)} {name} End - {R?.Blocking} {R?.Error} {R?.Message}");

            return R;
        }

        public FirePluginResult? FirePlugin<TCaller, TParam1, TParam2, TParam3>(TCaller caller, string name, TParam1 param1, TParam2 param2, TParam3 param3) {
            Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{typeof(TCaller)} {name} Start");

            IList<MethodPluginPriortity<TCaller>> MethodListToCall = GetMethodListToCall(caller, name).OrderBy(mp => mp.Priority).ToList();

            FirePluginResult? R = null;

            foreach (MethodPluginPriortity<TCaller> MPP in MethodListToCall) {
                Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{typeof(TCaller)} {name} {MPP.Plugin.GetType().Name} Start");

                MPP.Method.Invoke(MPP.Plugin, new object[] { param1!, param2!, param3! });

                Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{typeof(TCaller)} {name} {MPP.Plugin.GetType().Name} End");

                R = (FirePluginResult?)MPP.MethodGetResult.Invoke(MPP.Plugin, new object[] { R! });
                if (R != null && R.Blocking) {
                    break;
                }
            }

            Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{typeof(TCaller)} {name} End - {R?.Blocking} {R?.Error} {R?.Message}");

            return R;
        }

        private IList<MethodPluginPriortity<TCaller>> GetMethodListToCall<TCaller>(TCaller caller, string name) {
            IList<MethodPluginPriortity<TCaller>> R = new List<MethodPluginPriortity<TCaller>>();

            foreach (NamePlugin NamePlugin in _SubscribedPluginList.Where(SP => SP.Name == name)) {
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

            return R;
        }

        private bool MethodToSkip(MethodInfo Method) {
            foreach (MethodInfo MethodIPlugin in typeof(IPlugin).GetMethods()) {
                if (Method.Name == MethodIPlugin.Name) {
                    return true;
                }
            }
            foreach (MethodInfo MethodObject in typeof(object).GetMethods()) {
                if (Method.Name == MethodObject.Name) {
                    return true;
                }
            }
            return false;
        }

        public void SubscribePlugin<TPlugin>(string name, TPlugin pluginClass) where TPlugin : IPlugin {
            Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{name} {typeof(TPlugin)}");

            if (!_SubscribedPluginList.Any(SP => SP.Name == name && SP.Plugin.GetType() == pluginClass.GetType())) {
                _SubscribedPluginList.Add(new NamePlugin(name, pluginClass));

                Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{name} {typeof(TPlugin)} Subscribed");
            }
        }

        public void LoadPlugins<TPlugin>(string name) where TPlugin : IPlugin {
            Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{name} {typeof(TPlugin)}");

            DeclarePlugin<TPlugin>(name);

            Type TO = typeof(TPlugin);

            foreach (Assembly Ass in AppDomain.CurrentDomain.GetAssemblies()) {
                foreach (Type Type in Ass.GetTypes().Where(T => T.IsClass && TO.IsAssignableFrom(T))) {
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
