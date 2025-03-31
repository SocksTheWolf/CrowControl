using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.CrowControl
{
    class SnowballName : Monocle.Component
    {
        public string Name { get; set; }

        public SnowballName(bool active, bool visible, string name) : base(active, visible)
        {
            Name = name;
        }
    }
}
