using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Panacea.Modules.Telephone.Models
{
    [DataContract]
    public class CallLogItem
    {
        [DataMember(Name = "_id")]
        public string Id { get; set; }

        [DataMember(Name = "direction")]
        public CallDirection Direction { get; set; }

        [DataMember(Name = "status")]
        public CallStatus Status { get; set; }

        [DataMember(Name = "callType")]
        public CallType Type { get; set; }

        public string Display { get; set; }

        [DataMember(Name = "number")]
        public string Number { get; set; }

        [DataMember(Name = "timestamp")]
        public DateTime TimeStamp { get; set; }

        [DataMember(Name = "duration")]
        public int Seconds { get; set; }

        public TimeSpan Duration
        {
            get { return TimeSpan.FromSeconds(Seconds); }
        }

        public double Rotation
        {
            get { return Direction == CallDirection.Incoming ? -45 : 135; }
        }

        public Brush ColorBrush
        {
            get
            {
                if (Status == CallStatus.Missed) return Brushes.Red;
                if (Direction == CallDirection.Incoming) return Brushes.DodgerBlue;
                if (Status == CallStatus.Cancelled || Status == CallStatus.Failed || Status == CallStatus.Busy) return Brushes.Red;
                return Brushes.LimeGreen;
            }
        }
    }
}
