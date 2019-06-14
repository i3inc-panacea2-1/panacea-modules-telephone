using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Panacea.Modules.Telephone.Models
{
    [DataContract]
    public enum ButtonAppearance
    {
        [DataMember(Name = "displayOnLesftSideBarWithSpeedDial")]
        DisplayOnLesftSideBarWithSpeedDial,

        [DataMember(Name = "displayOnLesftSideBarWithCallSystem")]
        DisplayOnLesftSideBarWithCallSystem,

        [DataMember(Name = "hideFromLeftSideBar")]
        HideFromLeftSideBar,

        None

    }
}
