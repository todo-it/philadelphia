using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Philadelphia.Common;
using Philadelphia.Server.Common;
using Philadelphia.ServerSideUtils;
using Philadelphia.Testing.DotNetCore;

namespace ControlledByTests.Server {
    public class ConsoleBasedControllerAsLifeTimeFilter : ILifetimeFilter,IRegisterServiceInvocation {
        private Thread _consoleBasedListener;
        private ICodec _codec = new VerboseNewtonsoftJsonBasedCodec();

        private readonly object _readLck = new object();
        private readonly Func<string> _receiveCommand;

        private readonly object _sendLck = new object();
        private readonly Action<string> _sendReply;
        
        private volatile bool _shouldQuit;

        public ConsoleBasedControllerAsLifeTimeFilter() {
            _receiveCommand = () => {
                lock (_readLck) {
                    return Console.ReadLine();
                }
            };

            _sendReply = x => {
                lock(_sendLck) {
                    Console.WriteLine(x);
                }
            };
        }
        
        private void SendFilterInvocation(FilterInvocation inv) {
            Send(inv.AsCommandReply(_codec));
        }

        public void RegisterServiceInvocation(Type iface, string methodName, object[] parameters) {
            Send(new ServiceCall {
                FullInterfaceName = iface.FullName,
                MethodName = methodName,
                Params = parameters}
                    .AsCommandReply(_codec));
        }

        public void Send(CommandReply cmd) {
            _sendReply(_codec.Encode(cmd));
        }

        public void LogAsReply(string txt) {
            _sendReply(
                _codec.Encode(
                    CommandReplyUtil.CreateLog(txt)));
        }

        public Task OnServerStarted(IDiResolveReleaseOnlyContainer di) {
            _consoleBasedListener = new Thread(() => {
                Thread.CurrentThread.IsBackground = true;

                LogAsReply("listening started");
                
                while(!_shouldQuit) {
                    var rawCmd = _receiveCommand();
                    LogAsReply("received raw command");

                    try {
                        var decoded = _codec.Decode<CommandRequest>(rawCmd);

                        LogAsReply($"successfully decoded command requestId={decoded.Id} type={decoded.Type}");

                        switch (decoded.Type) {
                            case RequestType.StopServer:
                                _shouldQuit = true;
                                break;

                            default: throw new Exception("unsupported RequestType");
                        }
                        
                    } catch(Exception ex) {
                        LogAsReply($"could not decode command because of {ex}");
                    }
                }
            });
            _consoleBasedListener.Start();
            
            Send(CommandReplyUtil.CreateServerStarted());
            SendFilterInvocation(new FilterInvocation{InvType = FilterInvocationType.ServerStarted});

            return Task.CompletedTask;
        }
 
        public Task<ConnectionAction> OnConnectionBeforeHandler(
                IDiResolveReleaseOnlyContainer di, string url, object serviceInstanceOrNull, 
                MethodInfo methodOrNull, object[] parms, ResourceType res) {
 
            var ci = di.Resolve<ClientConnectionInfo>();
            var guid = Guid.NewGuid().ToString();
            LogAsReply($"OnConnectionBeforeHandler() ip={ci.ClientIpAddress} guid={guid} url={url} resourceType={res.ToString()} serviceImpl={serviceInstanceOrNull} method={methodOrNull} parms={parms}");
            
            var ctx = new FilterInvocation {
                Url = url,
                ResType = res,
                InvType = FilterInvocationType.BeforeConnection,
                FullInterfaceNameOrNull = serviceInstanceOrNull == null ? 
                    null 
                    :
                    FindServiceInterfaceType(serviceInstanceOrNull).FullName,
                MethodNameOrNull = methodOrNull?.Name,
                Guid = guid };
            SendFilterInvocation(ctx);

            return Task.FromResult(ConnectionAction.CreateNonFiltered(ctx));
        }

        private static Type FindServiceInterfaceType(object instance) {
            return instance
                .GetType()
                .GetInterfaces()
                .First(x => x.CustomAttributes.Any(y => y.AttributeType == typeof(HttpService)));
        }

        public Task OnConnectionAfterHandler(
                object rawConnCtx, IDiResolveReleaseOnlyContainer di, Exception exOrNull) {

            var connCtx = (FilterInvocation)rawConnCtx;
            LogAsReply($"OnConnectionAfterServiceHandler() guid={connCtx.Guid} success?={exOrNull == null} errorDetails={exOrNull}");
            
            connCtx.InvType = FilterInvocationType.AfterConnection;

            SendFilterInvocation(connCtx);

            return Task.CompletedTask;
        }
    }
}
