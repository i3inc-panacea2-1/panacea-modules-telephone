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
    public class TelephoneSettings : Translatable
    {
        [DataMember(Name = "codecsForSip")]
        public string Codecs { get; set; }

        [DataMember(Name = "transportType")]
        public string TransportType { get; set; }

        [IsTranslatable]
        [DataMember(Name = "helpText")]
        public string HelpText
        {
            get => GetTranslation();
            set => SetTranslation(value);
        }

        [DataMember(Name = "nurseBtnAppearance")]
        public ButtonAppearance NurseBtnAppearance { get; set; }

        [DataMember(Name = "userSignedIn")]
        public bool RequiresUserSignedIn { get; set; }

        [DataMember(Name = "allowVideoCalls")]
        public bool AllowVideoCalls { get; set; }

        [DataMember(Name = "handsetKeyboards")]
        public string HandsetKeyboards { get; set; }

    }
}
