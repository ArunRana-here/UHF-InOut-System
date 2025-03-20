using System;

namespace InOut.Models
{
    public class RfidTagData
    {
        public string EPC { get; set; }  // Tag ID
        public int ReadCount { get; set; }  // Read Count
        public DateTime Timestamp { get; set; }  // Local Timestamp
        public string Status { get; set; }  // "IN" or "OUT"
    }
}
