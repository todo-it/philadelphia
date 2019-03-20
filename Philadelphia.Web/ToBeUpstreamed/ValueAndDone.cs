using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bridge;

namespace Philadelphia.Web {
    [External]
    public class ValueAndDone<T> {
        public T value;
        public bool done;
    }
}
