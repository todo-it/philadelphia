using System;

namespace Philadelphia.Demo.SharedModel {
    public class ContinentalNotification {
        public Country Country { get; set; }
        public string Sender { get; set; }
        public DateTime SentAt { get; set; }

        /// <summary>Bridge.net bug workaround</summary>
        public void PostDeserializationFix() {
            object tmp = SentAt;
            SentAt = Convert.ToDateTime(tmp);
        }
    }
}
