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
    public class GetVoipSettingsResponse
    {
        [DataMember(Name = "uservoip")]
        public TelephoneAccount UserAccount { get; set; }

        [DataMember(Name = "terminal_voip")]
        public TelephoneAccount TerminalAccount { get; set; }

        [DataMember(Name = "categories")]
        public Categories Categories { get; set; }

        [DataMember(Name = "settings")]
        public TelephoneSettings Settings { get; set; }

      
    }


    
}
