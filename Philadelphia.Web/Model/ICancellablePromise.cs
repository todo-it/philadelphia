using System.Threading.Tasks;

namespace Philadelphia.Web {
    /// <summary>easy way to cancel Tasks without modifying Bridge Task.FromPromise() </summary>
    interface ICancellablePromise : IPromise {
        void Cancel();
    }
}
