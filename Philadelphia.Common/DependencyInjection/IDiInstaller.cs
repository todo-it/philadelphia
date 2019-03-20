namespace Philadelphia.Common {
    /// <summary>same idea as Windsor installer </summary>
    public interface IDiInstaller {
        void Install(IDiRegisterOnlyContainer cnt);
    }
}
