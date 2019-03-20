using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphia.Common {
    public class DefaultUniqueIdGenerator : IUniqueIdGeneratorImplementation {
        private int _id = 1;

        public int Generate() {
            return _id++;
        }
    }
}
