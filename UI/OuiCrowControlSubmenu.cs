using Celeste.Mod.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.CrowControl
{
    class OuiCrowControlSubmenu : AbstractSubmenu
    {
        private CrowControlSettings Settings => CrowControlModule.Settings;

        private TextMenu.Option<bool> dieEnabledOption;
        private TextMenu.Option<int> dieVoteLimitOption;

        public OuiCrowControlSubmenu() : base("CROWCONTROL_COMMAND_SETTINGS", null) { }

        protected override void addOptionsToMenu(TextMenu menu, bool inGame, object[] parameters)
        {
            dieEnabledOption = new TextMenuExt.OnOff(Dialog.Clean(DialogIds.Die), Settings.Die, true).Change(i => Settings.Die = i);
            dieVoteLimitOption = new TextMenuExt.Slider(Dialog.Clean(DialogIds.DieVoteLimit), i => i.ToString(), 1, 100, Settings.DieVoteLimit, 1).Change(i => Settings.DieVoteLimit = i);

            menu.Add(dieEnabledOption);
            menu.Add(dieVoteLimitOption);
        }

        protected override void gotoMenu(Overworld overworld)
        {
            overworld.Goto<OuiModOptions>();
        }
    }
}
