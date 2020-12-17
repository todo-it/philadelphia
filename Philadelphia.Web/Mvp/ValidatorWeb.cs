using System.Collections.Generic;
using Philadelphia.Common;

namespace Philadelphia.Web {
    public static class ValidatorWeb {
        public static Validate<List<RemoteFileDescr>> LimitSize(int maxSize) =>
            (newVal, errors) => errors.IfTrueAdd(
                newVal != null && newVal.Count > maxSize,
                I18n.Translate("Not allowed to have more than {0} files")
                    .MessageFormat(maxSize));
    }
}
