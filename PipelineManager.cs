using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#nullable enable

namespace NW.ManyQueues {
    public interface IPipeline {
        public void SetCaller<T>(T caller) where T : class;
        public static bool AutoLoad() => true;
    }

    public interface IPipeline<TToken>: IPipeline where TToken : class {
        public void SetToken(TToken token);
    }

    public interface IPipelineManager: IManager {
        bool CreatePipeline<TToken>(string name, Type[] stepsSequence, IPipeline<TToken>[]? steps) where TToken : class, new();
        int RunPipeline<TCaller, TToken>(TCaller caller, string name, out TToken token) where TToken : class, new();
        int RunPipeline<TCaller, TToken, TParam1>(TCaller caller, string name, TParam1 param1, out TToken token) where TToken : class, new();
        int RunPipeline<TCaller, TToken, TParam1, TParam2>(TCaller caller, string name, TParam1 param1, TParam2 param2, out TToken token) where TToken : class, new();
        int RunPipeline<TCaller, TToken, TParam1, TParam2, TParam3>(TCaller caller, string name, TParam1 param1, TParam2 param2, TParam3 param3, out TToken token) where TToken : class, new();
        int RunPipeline<TCaller, TToken>(TCaller caller, string name, TToken token) where TToken : class;
        int RunPipeline<TCaller, TToken, TParam1>(TCaller caller, string name, TParam1 param1, TToken token) where TToken : class;
        int RunPipeline<TCaller, TToken, TParam1, TParam2>(TCaller caller, string name, TParam1 param1, TParam2 param2, TToken token) where TToken : class;
        int RunPipeline<TCaller, TToken, TParam1, TParam2, TParam3>(TCaller caller, string name, TParam1 param1, TParam2 param2, TParam3 param3, TToken token) where TToken : class;
    }

    public class PipelineManager: BaseManager, IPipelineManager {
        public PipelineManager() : base(null, null) {
        }

        public PipelineManager(IManager? manager, ILog? log) : base(manager, log) {
        }

        private readonly IDictionary<string, IList<NamePipeline>> _PipelinesList = new Dictionary<string, IList<NamePipeline>>();

        public bool CreatePipeline<TToken>(string name, Type[] stepsSequence, IPipeline<TToken>[]? steps) where TToken : class, new() {
            Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{name} {typeof(TToken)} {stepsSequence.Length} {steps?.Length}");

            if (!_PipelinesList.TryAdd(name, new List<NamePipeline>())) {
                return false;
            }

            foreach (Type Type in stepsSequence) {
                IPipeline<TToken>? Pipeline = steps?.FirstOrDefault(S => S.GetType().IsAssignableFrom(typeof(IPipeline<TToken>)));
                if (Pipeline == null) {
                    Pipeline = Type.GetConstructors()[0].GetParameters().Length == 1
                        ? (IPipeline<TToken>?)Activator.CreateInstance(Type, this)
                        : (IPipeline<TToken>?)Activator.CreateInstance(Type);

                    Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{name} {Type} created");
                }
                if (Pipeline != null) {
                    _PipelinesList[name].Add(new NamePipeline(name, Pipeline));
                }
            }

            return _PipelinesList[name].Count == stepsSequence.Count();
        }

        public int RunPipeline<TCaller, TToken>(TCaller caller, string name, out TToken token) where TToken : class, new() {
            Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{typeof(TCaller)} {name} {typeof(TToken)} Start");

            IList<MethodPipeline<TCaller>> MethodListToCall = GetMethodListToCall(caller, name).ToList();

            token = new TToken();

            foreach (MethodPipeline<TCaller> MPP in MethodListToCall) {
                MPP.MethodSetToken.Invoke(MPP.Pipeline, new object[] { token });

                Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{typeof(TCaller)} {name} {MPP.Pipeline.GetType()}.{MPP.Method.Name} Start");

                MPP.Method.Invoke(MPP.Pipeline, null);

                Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{typeof(TCaller)} {name} {MPP.Pipeline.GetType()}.{MPP.Method.Name} End");
            }

            Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{typeof(TCaller)} {name} {typeof(TToken)} End");

            return MethodListToCall.Count;
        }

        public int RunPipeline<TCaller, TToken, TParam1>(TCaller caller, string name, TParam1 param1, out TToken token) where TToken : class, new() {
            Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{typeof(TCaller)} {name} {typeof(TToken)} Start");

            IList<MethodPipeline<TCaller>> MethodListToCall = GetMethodListToCall(caller, name).ToList();

            token = new TToken();

            foreach (MethodPipeline<TCaller> MPP in MethodListToCall) {
                MPP.MethodSetToken.Invoke(MPP.Pipeline, new object[] { token });

                Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{typeof(TCaller)} {name} {MPP.Pipeline.GetType()}.{MPP.Method.Name} Start");

                MPP.Method.Invoke(MPP.Pipeline, new object[] { param1! });

                Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{typeof(TCaller)} {name} {MPP.Pipeline.GetType()}.{MPP.Method.Name} End");
            }

            Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{typeof(TCaller)} {name} {typeof(TToken)} End");

            return MethodListToCall.Count;
        }

        public int RunPipeline<TCaller, TToken, TParam1, TParam2>(TCaller caller, string name, TParam1 param1, TParam2 param2, out TToken token) where TToken : class, new() {
            Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{typeof(TCaller)} {name} {typeof(TToken)} Start");

            IList<MethodPipeline<TCaller>> MethodListToCall = GetMethodListToCall(caller, name).ToList();

            token = new TToken();

            foreach (MethodPipeline<TCaller> MPP in MethodListToCall) {
                MPP.MethodSetToken.Invoke(MPP.Pipeline, new object[] { token });

                Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{typeof(TCaller)} {name} {MPP.Pipeline.GetType()}.{MPP.Method.Name} Start");

                MPP.Method.Invoke(MPP.Pipeline, new object[] { param1!, param2! });

                Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{typeof(TCaller)} {name} {MPP.Pipeline.GetType()}.{MPP.Method.Name} End");
            }

            Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{typeof(TCaller)} {name} {typeof(TToken)} End");

            return MethodListToCall.Count;
        }

        public int RunPipeline<TCaller, TToken, TParam1, TParam2, TParam3>(TCaller caller, string name, TParam1 param1, TParam2 param2, TParam3 param3, out TToken token) where TToken : class, new() {
            Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{typeof(TCaller)} {name} {typeof(TToken)} Start");

            IList<MethodPipeline<TCaller>> MethodListToCall = GetMethodListToCall(caller, name).ToList();

            token = new TToken();

            foreach (MethodPipeline<TCaller> MPP in MethodListToCall) {
                MPP.MethodSetToken.Invoke(MPP.Pipeline, new object[] { token });

                Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{typeof(TCaller)} {name} {MPP.Pipeline.GetType()}.{MPP.Method.Name} Start");

                MPP.Method.Invoke(MPP.Pipeline, new object[] { param1!, param2!, param3! });

                Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{typeof(TCaller)} {name} {MPP.Pipeline.GetType()}.{MPP.Method.Name} End");
            }

            Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{typeof(TCaller)} {name} {typeof(TToken)} End");

            return MethodListToCall.Count;
        }

        public int RunPipeline<TCaller, TToken>(TCaller caller, string name, TToken token) where TToken : class {
            Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{typeof(TCaller)} {name} {typeof(TToken)} Start");

            IList<MethodPipeline<TCaller>> MethodListToCall = GetMethodListToCall(caller, name).ToList();

            foreach (MethodPipeline<TCaller> MPP in MethodListToCall) {
                MPP.MethodSetToken.Invoke(MPP.Pipeline, new object[] { token });

                Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{typeof(TCaller)} {name} {MPP.Pipeline.GetType()}.{MPP.Method.Name} Start");

                MPP.Method.Invoke(MPP.Pipeline, null);

                Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{typeof(TCaller)} {name} {MPP.Pipeline.GetType()}.{MPP.Method.Name} End");
            }

            Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{typeof(TCaller)} {name} {typeof(TToken)} End");

            return MethodListToCall.Count;
        }

        public int RunPipeline<TCaller, TToken, TParam1>(TCaller caller, string name, TParam1 param1, TToken token) where TToken : class {
            Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{typeof(TCaller)} {name} {typeof(TToken)} Start");

            IList<MethodPipeline<TCaller>> MethodListToCall = GetMethodListToCall(caller, name).ToList();

            foreach (MethodPipeline<TCaller> MPP in MethodListToCall) {
                MPP.MethodSetToken.Invoke(MPP.Pipeline, new object[] { token });

                Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{typeof(TCaller)} {name} {MPP.Pipeline.GetType()}.{MPP.Method.Name} Start");

                MPP.Method.Invoke(MPP.Pipeline, new object[] { param1! });

                Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{typeof(TCaller)} {name} {MPP.Pipeline.GetType()}.{MPP.Method.Name} End");
            }

            Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{typeof(TCaller)} {name} {typeof(TToken)} End");

            return MethodListToCall.Count;
        }

        public int RunPipeline<TCaller, TToken, TParam1, TParam2>(TCaller caller, string name, TParam1 param1, TParam2 param2, TToken token) where TToken : class {
            Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{typeof(TCaller)} {name} {typeof(TToken)} Start");

            IList<MethodPipeline<TCaller>> MethodListToCall = GetMethodListToCall(caller, name).ToList();

            foreach (MethodPipeline<TCaller> MPP in MethodListToCall) {
                MPP.MethodSetToken.Invoke(MPP.Pipeline, new object[] { token });

                Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{typeof(TCaller)} {name} {MPP.Pipeline.GetType()}.{MPP.Method.Name} Start");

                MPP.Method.Invoke(MPP.Pipeline, new object[] { param1!, param2! });

                Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{typeof(TCaller)} {name} {MPP.Pipeline.GetType()}.{MPP.Method.Name} End");
            }

            Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{typeof(TCaller)} {name} {typeof(TToken)} End");

            return MethodListToCall.Count;
        }

        public int RunPipeline<TCaller, TToken, TParam1, TParam2, TParam3>(TCaller caller, string name, TParam1 param1, TParam2 param2, TParam3 param3, TToken token) where TToken : class {
            Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{typeof(TCaller)} {name} {typeof(TToken)} Start");

            IList<MethodPipeline<TCaller>> MethodListToCall = GetMethodListToCall(caller, name).ToList();

            foreach (MethodPipeline<TCaller> MPP in MethodListToCall) {
                MPP.MethodSetToken.Invoke(MPP.Pipeline, new object[] { token });

                Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{typeof(TCaller)} {name} {MPP.Pipeline.GetType()}.{MPP.Method.Name} Start");

                MPP.Method.Invoke(MPP.Pipeline, new object[] { param1!, param2!, param3! });

                Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{typeof(TCaller)} {name} {MPP.Pipeline.GetType()}.{MPP.Method.Name} End");
            }

            Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{typeof(TCaller)} {name} {typeof(TToken)} End");

            return MethodListToCall.Count;
        }

        private IList<MethodPipeline<TCaller>> GetMethodListToCall<TCaller>(TCaller caller, string name) {
            IList<MethodPipeline<TCaller>> R = new List<MethodPipeline<TCaller>>();

            foreach (NamePipeline NamePipeline in _PipelinesList[name]) {

                foreach (MethodInfo Method in NamePipeline.Pipeline.GetType().GetMethods()) {
                    if (MethodToSkip(Method)) {
                        continue;
                    }

                    R.Add(new MethodPipeline<TCaller>(caller, NamePipeline, Method));
                }
            }

            return R;
        }

        private bool MethodToSkip(MethodInfo Method) {
            foreach (MethodInfo MethodIPipeline in typeof(IPipeline<object>).GetMethods()) {
                if (Method.Name == MethodIPipeline.Name) {
                    return true;
                }
            }
            foreach (MethodInfo MethodIPipeline in typeof(IPipeline).GetMethods()) {
                if (Method.Name == MethodIPipeline.Name) {
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

        private class NamePipeline {
            public string Name;
            public IPipeline Pipeline;
            public bool IsSetCallerCalled = false;

            public NamePipeline(string name, IPipeline pipeline) {
                Name = name;
                Pipeline = pipeline;
            }
        }

        private class MethodPipeline<TCaller> {
            public IPipeline Pipeline {
                get;
            }
            public MethodInfo Method {
                get;
            }
            public MethodInfo MethodSetCaller {
                get;
            }
            public MethodInfo MethodSetToken {
                get;
            }

            public MethodPipeline(TCaller caller, NamePipeline namePipeline, MethodInfo method) {
                Pipeline = namePipeline.Pipeline;
                Method = method;

                MethodSetCaller = Pipeline.GetType().GetMethod(nameof(IPipeline.SetCaller))!;
                MethodSetToken = Pipeline.GetType().GetMethod(nameof(IPipeline<object>.SetToken))!;

                if (!namePipeline.IsSetCallerCalled) {
                    if (caller != null) {
                        MethodInfo GenericSetCaller = MethodSetCaller.MakeGenericMethod(typeof(TCaller));
                        GenericSetCaller.Invoke(namePipeline.Pipeline, new object[] { caller });
                    }
                    namePipeline.IsSetCallerCalled = true;
                }
            }
        }
    }
}
