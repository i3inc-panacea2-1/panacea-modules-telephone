using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Panacea.Modules.Telephone.Models
{
    public class UserSpeedDial
    {
        public string Label { get; set; }

        public string Number { get; set; }

        public UserSpeedDial Clone()
        {
            return new UserSpeedDial()
            {
                Label = Label,
                Number = Number
            };
        }
    }
}
