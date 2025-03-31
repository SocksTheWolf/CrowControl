using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.CrowControl
{
    class SeekerName : Monocle.Component
    {
        public string Name { get; set; }

        public SeekerName(bool active, bool visible, string name) : base(active, visible)
        {
            Name = name;
        }
    }
}
