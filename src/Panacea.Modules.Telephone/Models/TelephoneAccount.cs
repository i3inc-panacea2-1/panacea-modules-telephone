using Panacea.Modularity.Telephone;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Panacea.Modules.Telephone.Models
{
    [DataContract]
    public class TelephoneAccount:ITelephoneAccount
    {
        [DataMember(Name = "username")]
        public string Username { get; set; }

        [DataMember(Name = "password")]
        public string Password { get; set; }

        [DataMember(Name = "ip")]
        public string Server { get; set; }

        [DataMember(Name = "displayNumber")]
        public string DisplayNumber { get; set; }

        [DataMember(Name = "voipType")]
        public string VoipType { get; set; }

        public bool Compare(TelephoneAccount account)
        {
            if (account == null) return false;
            return account.Username == Username
                && account.Password == Password
                && account.Server == Server
                && account.DisplayNumber == DisplayNumber && account.VoipType == VoipType;
        }
    }
}
