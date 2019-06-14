using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Panacea.Modules.Telephone.Models
{
    [DataContract]
    public class SpeedDialCategory
    {
        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "speedDials")]
        public List<SpeedDialInstance> SpeedDials { get; set; }
    }
}
