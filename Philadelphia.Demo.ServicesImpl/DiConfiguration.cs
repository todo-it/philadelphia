using System;
using System.Globalization;
using Philadelphia.Common;
using Philadelphia.Demo.SharedModel;
using Philadelphia.Server.Common;
using Philadelphia.ServerSideUtils;
using IDiRegisterOnlyContainer = Philadelphia.Common.IDiRegisterOnlyContainer;

namespace Philadelphia.Demo.ServicesImpl {
    class FancyConnectionIdProvider : IClientConnectionInfoConnectionIdProvider {
        private readonly ClientConnectionInfo _info;

        public FancyConnectionIdProvider(ClientConnectionInfo info) {
            _info = info;
        }

        public string Provide() {
            return $"{_info.ClientIpAddress}-{DateTime.UtcNow.ToString("s", CultureInfo.InvariantCulture)}-{Guid.NewGuid().ToString()}";
        }
    }

    public class DiConfiguration : IDiInstaller {
        public void Install(IDiRegisterOnlyContainer cnt) {
            cnt.RegisterInstance(new DemoConfig {
                    ActuallyMutateDataServerSide =
                        Configuration.getBoolConfigVarOrDefault(true, "ALLOW_SERVER_SIDE_MUTATION")}, 
                LifeStyle.Singleton);

            if (Configuration.getBoolConfigVarOrDefault(true, "USE_FANCY_CONNECTION_ID")) {
                cnt.RegisterAlias<IClientConnectionInfoConnectionIdProvider,FancyConnectionIdProvider>(
                    LifeStyle.Scoped);
            }
        }
    }
}
