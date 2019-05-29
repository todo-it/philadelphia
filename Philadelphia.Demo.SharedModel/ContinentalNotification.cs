using System;

namespace Philadelphia.Demo.SharedModel {
    public class ContinentalNotification {
        public Country Country { get; set; }
        public string Sender { get; set; }
        public string SenderSseStreamId { get; set; }
        public DateTime SentAt { get; set; }
    }
}
