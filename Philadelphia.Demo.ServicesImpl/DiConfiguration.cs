using Philadelphia.Common;
using Philadelphia.Demo.SharedModel;
using Philadelphia.ServerSideUtils;
using IDiRegisterOnlyContainer = Philadelphia.Common.IDiRegisterOnlyContainer;

namespace Philadelphia.Demo.ServicesImpl {
    public class DiConfiguration : IDiInstaller {
        public void Install(IDiRegisterOnlyContainer cnt) {
            cnt.RegisterInstance(new DemoConfig {
                    ActuallyMutateDataServerSide =
                        Configuration.getBoolConfigVarOrDefault(true, "ALLOW_SERVER_SIDE_MUTATION")}, 
                LifeStyle.Singleton);
        }
    }
}
