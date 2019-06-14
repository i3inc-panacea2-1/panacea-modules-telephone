using Panacea.Models;
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
    public class SpeedDial : Translatable
    {
        [IsTranslatable]
        [DataMember(Name = "label")]
        public string Label
        {
            get => GetTranslation();
            set => SetTranslation(value);
        }

        [DataMember(Name = "_id")]
        public string Id { get; set; }


        [IsTranslatable]
        [DataMember(Name = "description")]
        public string Description
        {
            get => GetTranslation();
            set => SetTranslation(value);
        }

        [DataMember(Name = "number")]
        public string Number { get; set; }

        [DataMember(Name = "img_thumbnail")]
        public Thumbnail Image { get; set; }

        [DataMember(Name = "free")]
        public bool Free { get; set; }

        [DataMember(Name = "videoCall")]
        public bool VideoCall { get; set; }


        [DataMember(Name = "visible")]
        public bool Visible { get; set; }

        [DataMember(Name = "allowCallback")]
        public bool AllowCallback { get; set; }

        [DataMember(Name = "overrideBusySettings")]
        public bool OverrideBusySettings { get; set; }
    }
}
