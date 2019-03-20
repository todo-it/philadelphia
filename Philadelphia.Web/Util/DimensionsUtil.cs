using System;

namespace Philadelphia.Web {
    public class DimensionsUtil {
        public static (int width,int height) CalculateDimensionsNotLargerThan(
                (int width,int height) asked, (int width,int height) maxSize) {

            var factor = Math.Max(
                (asked.width+0.0) / maxSize.width, 
                (asked.height+0.0) / maxSize.height);
            return ((int)(asked.width / factor), (int)(asked.height / factor));
        }

    }
}
