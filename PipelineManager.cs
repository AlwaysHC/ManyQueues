﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#nullable enable

namespace NW.ManyQueues {
    public interface IPipeline {
        public void SetCaller<T>(T caller) where T : class;
        public static bool AutoLoad() => true;
    }

    public interface IPipeline<TToken>: IPipeline where TToken : class, new() {
        public void SetToken(TToken token);
    }

    public interface IFluentPipelineManager<TToken> where TToken : class, new() {
        IFluentPipelineManager<TToken> AddStep<TPipeline>() where TPipeline : IPipeline<TToken>;
        IFluentPipelineManager<TToken> AddStep(IPipeline<TToken> alreadyCreatedStep);
        IFluentPipelineManager<TToken> AddSteps(params Type[] steps);
        IFluentPipelineManager<TToken> AddSteps(params IPipeline<TToken>[] alreadyCreatedSteps);
    }

    public interface IPipelineManager: IManager {
        bool CreatePipeline<TToken>(Type[] stepsSequence, params IPipeline<TToken>[] alreadyCreatedSteps) where TToken : class, new();
        IFluentPipelineManager<TToken> AddStep<TToken, TPipeline>() where TPipeline : IPipeline<TToken> where TToken : class, new();
        IFluentPipelineManager<TToken> AddStep<TToken>(IPipeline<TToken> alreadyCreatedStep) where TToken : class, new();
        IFluentPipelineManager<TToken> AddSteps<TToken>(params Type[] steps) where TToken : class, new();
        IFluentPipelineManager<TToken> AddSteps<TToken>(params IPipeline<TToken>[] alreadyCreatedSteps) where TToken : class, new();
        int RunPipeline<TCaller, TToken>(TCaller caller, out TToken token, params object?[]? @params) where TToken : class, new();
        int RunPipeline<TCaller, TToken>(TCaller caller, TToken token, params object?[]? @params) where TToken : class;
    }

    public class PipelineManager: BaseManager, IPipelineManager {
        public PipelineManager() : base(null, null) {
        }

        public PipelineManager(IManager? manager, ILog? log = null) : base(manager, log) {
        }

        private readonly IDictionary<string, IList<NamePipeline>> _PipelinesList = new Dictionary<string, IList<NamePipeline>>();

        private static string TokenName<TToken>()
            => typeof(TToken).AssemblyQualifiedName ?? typeof(TToken).FullName ?? typeof(TToken).Name;

        public bool CreatePipeline<TToken>(Type[] stepsSequence, params IPipeline<TToken>[] alreadyCreatedSteps) where TToken : class, new() {
            Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{TokenName<TToken>()} {typeof(TToken)} {stepsSequence.Length} {alreadyCreatedSteps?.Length}");

            if (!_PipelinesList.TryAdd(TokenName<TToken>(), new List<NamePipeline>())) {
                return false;
            }

            foreach (Type Type in stepsSequence) {
                IPipeline<TToken>? Pipeline = alreadyCreatedSteps?.FirstOrDefault(s => s.GetType().IsAssignableFrom(Type));
                if (Pipeline == null) {
                    try {
                        Pipeline = Type.GetConstructors()[0].GetParameters().Length == 1
                            ? (IPipeline<TToken>?)Activator.CreateInstance(Type, this)
                            : (IPipeline<TToken>?)Activator.CreateInstance(Type);
                        Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{TokenName<TToken>()} {Type} created");
                    }
                    catch {
                    }
                }
                if (Pipeline != null) {
                    _PipelinesList[TokenName<TToken>()].Add(new NamePipeline(TokenName<TToken>(), Pipeline));
                }
            }

            return _PipelinesList[TokenName<TToken>()].Count == stepsSequence.Length;
        }

        public IFluentPipelineManager<TToken> AddStep<TToken, TPipeline>() where TPipeline : IPipeline<TToken> where TToken : class, new()
            => AddSteps<TToken>(typeof(TPipeline));

        public IFluentPipelineManager<TToken> AddStep<TToken>(IPipeline<TToken> alreadyCreatedStep) where TToken : class, new()
            => AddSteps(alreadyCreatedStep);

        public IFluentPipelineManager<TToken> AddSteps<TToken>(params Type[] steps) where TToken : class, new() {
            Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{TokenName<TToken>()} {steps?.Length}");

            _PipelinesList.TryAdd(TokenName<TToken>(), new List<NamePipeline>());

            if (steps != null) {
                foreach (Type Type in steps) {
                    IPipeline<TToken>? Pipeline = null;
                    try {
                        Pipeline = Type.GetConstructors()[0].GetParameters().Length == 1
                            ? (IPipeline<TToken>?)Activator.CreateInstance(Type, this)
                            : (IPipeline<TToken>?)Activator.CreateInstance(Type);
                        Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{TokenName<TToken>()} {Type} created");
                    }
                    catch {
                    }
                    if (Pipeline != null) {
                        _PipelinesList[TokenName<TToken>()].Add(new NamePipeline(TokenName<TToken>(), Pipeline));
                    }
                }
            }

            return new FluentPipelineManager<TToken>(this);
        }

        public IFluentPipelineManager<TToken> AddSteps<TToken>(params IPipeline<TToken>[] alreadyCreatedSteps) where TToken : class, new() {
            Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{TokenName<TToken>()} {typeof(TToken)} {alreadyCreatedSteps?.Length}");

            _PipelinesList.TryAdd(TokenName<TToken>(), new List<NamePipeline>());

            if (alreadyCreatedSteps != null) {
                foreach (IPipeline<TToken> Pipeline in alreadyCreatedSteps) {
                    _PipelinesList[TokenName<TToken>()].Add(new NamePipeline(TokenName<TToken>(), Pipeline));
                }
            }

            return new FluentPipelineManager<TToken>(this);
        }

        public int RunPipeline<TCaller, TToken>(TCaller caller, out TToken token) where TToken : class, new() {
            Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{typeof(TCaller)} {TokenName<TToken>()} {typeof(TToken)} Start");

            IReadOnlyList<MethodPipeline<TCaller>> MethodListToCall = GetMethodListToCall(caller, TokenName<TToken>()).ToList();

            token = new TToken();

            foreach (MethodPipeline<TCaller> MPP in MethodListToCall) {
                MPP.MethodSetToken.Invoke(MPP.Pipeline, new object[] { token });

                Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{typeof(TCaller)} {TokenName<TToken>()} {MPP.Pipeline.GetType()}.{MPP.Method.Name} Start");

                MPP.Method.Invoke(MPP.Pipeline, null);

                Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{typeof(TCaller)} {TokenName<TToken>()} {MPP.Pipeline.GetType()}.{MPP.Method.Name} End");
            }

            Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{typeof(TCaller)} {TokenName<TToken>()} {typeof(TToken)} End");

            return MethodListToCall.Count;
        }

        public int RunPipeline<TCaller, TToken>(TCaller caller, out TToken token, params object?[]? @params) where TToken : class, new() {
            Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{typeof(TCaller)} {TokenName<TToken>()} {typeof(TToken)} Start");

            IReadOnlyList<MethodPipeline<TCaller>> MethodListToCall = GetMethodListToCall(caller, TokenName<TToken>()).ToList();

            token = new TToken();

            foreach (MethodPipeline<TCaller> MPP in MethodListToCall) {
                MPP.MethodSetToken.Invoke(MPP.Pipeline, new object[] { token });

                Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{typeof(TCaller)} {TokenName<TToken>()} {MPP.Pipeline.GetType()}.{MPP.Method.Name} Start");

                MPP.Method.Invoke(MPP.Pipeline, @params);

                Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{typeof(TCaller)} {TokenName<TToken>()} {MPP.Pipeline.GetType()}.{MPP.Method.Name} End");
            }

            Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{typeof(TCaller)} {TokenName<TToken>()} {typeof(TToken)} End");

            return MethodListToCall.Count;
        }

        public int RunPipeline<TCaller, TToken>(TCaller caller, TToken token, params object?[]? @params) where TToken : class {
            Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{typeof(TCaller)} {TokenName<TToken>()} {typeof(TToken)} Start");

            IReadOnlyList<MethodPipeline<TCaller>> MethodListToCall = GetMethodListToCall(caller, TokenName<TToken>()).ToList();

            foreach (MethodPipeline<TCaller> MPP in MethodListToCall) {
                MPP.MethodSetToken.Invoke(MPP.Pipeline, new object[] { token });

                Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{typeof(TCaller)} {TokenName<TToken>()} {MPP.Pipeline.GetType()}.{MPP.Method.Name} Start");

                MPP.Method.Invoke(MPP.Pipeline, @params);

                Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{typeof(TCaller)} {TokenName<TToken>()} {MPP.Pipeline.GetType()}.{MPP.Method.Name} End");
            }

            Log.Log(MethodBase.GetCurrentMethod()!.Name, $"{typeof(TCaller)} {TokenName<TToken>()} {typeof(TToken)} End");

            return MethodListToCall.Count;
        }

        private IReadOnlyList<MethodPipeline<TCaller>> GetMethodListToCall<TCaller>(TCaller caller, string name) {
            IList<MethodPipeline<TCaller>> R = new List<MethodPipeline<TCaller>>();

            foreach (NamePipeline NamePipeline in _PipelinesList[name]) {

                foreach (MethodInfo Method in NamePipeline.Pipeline.GetType().GetMethods()) {
                    if (MethodToSkip(Method)) {
                        continue;
                    }

                    R.Add(new MethodPipeline<TCaller>(caller, NamePipeline, Method));
                }
            }

            return (IReadOnlyList<MethodPipeline<TCaller>>)R;
        }

        private static bool MethodToSkip(MethodInfo method) {
            foreach (MethodInfo MethodIPipeline in typeof(IPipeline<object>).GetMethods()) {
                if (method.Name == MethodIPipeline.Name) {
                    return true;
                }
            }
            foreach (MethodInfo MethodIPipeline in typeof(IPipeline).GetMethods()) {
                if (method.Name == MethodIPipeline.Name) {
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

    public class FluentPipelineManager<TToken>: IFluentPipelineManager<TToken> where TToken : class, new() {
        private readonly IPipelineManager _PipelineManager;

        public FluentPipelineManager(IPipelineManager pipelineManager) {
            _PipelineManager = pipelineManager;
        }

        public IFluentPipelineManager<TToken> AddStep<TPipeline>() where TPipeline : IPipeline<TToken>
            => AddSteps(typeof(TPipeline));

        public IFluentPipelineManager<TToken> AddStep(IPipeline<TToken> alreadyCreatedStep)
            => AddSteps(alreadyCreatedStep);

        public IFluentPipelineManager<TToken> AddSteps(params Type[] steps) {
            _PipelineManager.AddSteps<TToken>(steps);
            return this;
        }

        public IFluentPipelineManager<TToken> AddSteps(params IPipeline<TToken>[] alreadyCreatedSteps) {
            _PipelineManager.AddSteps(alreadyCreatedSteps);
            return this;
        }
    }
}
