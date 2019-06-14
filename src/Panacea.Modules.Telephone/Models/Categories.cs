using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Panacea.Modules.Telephone.Models
{
    [DataContract]
    public class Categories
    {
        [DataMember(Name = "Telephone")]
        public ServerTelephone Telephone { get; set; }
    }
}
