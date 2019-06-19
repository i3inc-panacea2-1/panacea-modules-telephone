using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Panacea.Modules.Telephone.Models
{
    [DataContract]
    public class SendCookieTelephoneResponse
    {
        [DataMember(Name = "pluginName")]
        public string PluginName { get; set; }


        [DataMember(Name = "data")]
        public ObservableCollection<UserSpeedDial> UserDials { get; set; }
    }
}
