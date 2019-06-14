using Panacea.Multilinguality;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Panacea.Modules.Telephone.Models
{
    [DataContract]
    public class SpeedDialInstance : Translatable
    {
        [DataMember(Name = "speedDial")]
        public SpeedDial SpeedDial { get; set; }


    }
}
