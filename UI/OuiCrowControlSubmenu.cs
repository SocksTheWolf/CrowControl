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

        //DIE
        private TextMenu.Option<bool> dieEnabledOption;
        private TextMenu.Option<int> dieVoteLimitOption;

        //BLUR
        private TextMenu.Option<bool> blurEnabledOption;
        private TextMenu.Option<int> blurVoteLimitOption;

        //BUMP
        private TextMenu.Option<bool> bumpEnabledOption;
        private TextMenu.Option<int> bumpVoteLimitOption;

        //SEEKER
        private TextMenu.Option<bool> seekerEnabledOption;
        private TextMenu.Option<int> seekerVoteLimitOption;
        private TextMenu.Option<bool> seekerShowSeekerNamesOption;

        //MIRROR
        private TextMenu.Option<bool> mirrorEnabledOption;
        private TextMenu.Option<int> mirrorVoteLimitOption;

        //KEVIN
        private TextMenu.Option<bool> kevinEnabledOption;
        private TextMenu.Option<int> kevinVoteLimitOption;

        //DISABLE GRAB
        private TextMenu.Option<bool> disableGrabEnabledOption;
        private TextMenu.Option<int> disableGrabVoteLimitOption;

        //INVISIBLE
        private TextMenu.Option<bool> invisibleEnabledOption;
        private TextMenu.Option<int> invisibleVoteLimitOption;

        //INVERT
        private TextMenu.Option<bool> invertEnabledOption;
        private TextMenu.Option<int> invertVoteLimitOption;

        //LOW FRICTION 
        private TextMenu.Option<bool> lowFrictionEnabledOption;
        private TextMenu.Option<int> lowFrictionVoteLimitOption;

        //OSHIRO
        private TextMenu.Option<bool> oshiroEnabledOption;
        private TextMenu.Option<int> oshiroVoteLimitOption;

        //SNOWBALL
        private TextMenu.Option<bool> snowballEnabledOption;
        private TextMenu.Option<int> snowballVoteLimitOption;

        //DOUBLE DASH
        private TextMenu.Option<bool> doubleDashEnabledOption;
        private TextMenu.Option<int> doubleDashVoteLimitOption;

        //GODMODE
        private TextMenu.Option<bool> godModeEnabledOption;
        private TextMenu.Option<int> godModeVoteLimitOption;

        //FISH
        private TextMenu.Option<bool> fishEnabledOption;
        private TextMenu.Option<int> fishVoteLimitOption;

        public OuiCrowControlSubmenu() : base("CROWCONTROL_COMMAND_SETTINGS", null) { }

        protected override void addOptionsToMenu(TextMenu menu, bool inGame, object[] parameters)
        {
            //DIE
            dieEnabledOption = new TextMenuExt.OnOff(Dialog.Clean(DialogIds.Die), Settings.Die, true).Change(i => Settings.Die = i);
            dieVoteLimitOption = new TextMenuExt.Slider(Dialog.Clean(DialogIds.DieVoteLimit), i => i.ToString(), 1, 100, Settings.DieVoteLimit, 1).Change(i => Settings.DieVoteLimit = i);
            menu.Add(dieEnabledOption);
            menu.Add(dieVoteLimitOption);

            //BLUR
            blurEnabledOption = new TextMenuExt.OnOff(Dialog.Clean(DialogIds.Blur), Settings.Blur, true).Change(i => Settings.Blur = i);
            blurVoteLimitOption = new TextMenuExt.Slider(Dialog.Clean(DialogIds.BlurVoteLimit), i => i.ToString(), 1, 100, Settings.BlurVoteLimit, 1).Change(i => Settings.BlurVoteLimit = i);
            menu.Add(blurEnabledOption);
            menu.Add(blurVoteLimitOption);

            //BUMP
            bumpEnabledOption = new TextMenuExt.OnOff(Dialog.Clean(DialogIds.Bump), Settings.Bump, true).Change(i => Settings.Bump = i);
            bumpVoteLimitOption = new TextMenuExt.Slider(Dialog.Clean(DialogIds.BumpVoteLimit), i => i.ToString(), 1, 100, Settings.BumpVoteLimit, 1).Change(i => Settings.BumpVoteLimit = i);
            menu.Add(bumpEnabledOption);
            menu.Add(bumpVoteLimitOption);

            //SEEKER
            seekerEnabledOption = new TextMenuExt.OnOff(Dialog.Clean(DialogIds.Seeker), Settings.Seeker, true).Change(i => Settings.Seeker = i);
            seekerVoteLimitOption = new TextMenuExt.Slider(Dialog.Clean(DialogIds.SeekerVoteLimit), i => i.ToString(), 1, 100, Settings.SeekerVoteLimit, 1).Change(i => Settings.SeekerVoteLimit = i);
            seekerShowSeekerNamesOption = new TextMenuExt.OnOff(Dialog.Clean(DialogIds.ShowSeekerNames), Settings.ShowSeekerNames, true).Change(i => Settings.ShowSeekerNames = i);
            menu.Add(seekerEnabledOption);
            menu.Add(seekerVoteLimitOption);
            menu.Add(seekerShowSeekerNamesOption);

            //MIRROR
            mirrorEnabledOption = new TextMenuExt.OnOff(Dialog.Clean(DialogIds.Mirror), Settings.Mirror, true).Change(i => Settings.Mirror = i);
            mirrorVoteLimitOption = new TextMenuExt.Slider(Dialog.Clean(DialogIds.MirrorVoteLimit), i => i.ToString(), 1, 100, Settings.MirrorVoteLimit, 1).Change(i => Settings.MirrorVoteLimit = i);
            menu.Add(mirrorEnabledOption);
            menu.Add(mirrorVoteLimitOption);

            //KEVIN
            kevinEnabledOption = new TextMenuExt.OnOff(Dialog.Clean(DialogIds.Kevin), Settings.Kevin, true).Change(i => Settings.Kevin = i);
            kevinVoteLimitOption = new TextMenuExt.Slider(Dialog.Clean(DialogIds.KevinVoteLimit), i => i.ToString(), 1, 100, Settings.KevinVoteLimit, 1).Change(i => Settings.KevinVoteLimit = i);
            menu.Add(kevinEnabledOption);
            menu.Add(kevinVoteLimitOption);

            //DISABLE GRAB
            disableGrabEnabledOption = new TextMenuExt.OnOff(Dialog.Clean(DialogIds.DisableGrab), Settings.DisableGrab, true).Change(i => Settings.DisableGrab = i);
            disableGrabVoteLimitOption = new TextMenuExt.Slider(Dialog.Clean(DialogIds.DisableGrabVoteLimit), i => i.ToString(), 1, 100, Settings.DisableGrabVoteLimit, 1).Change(i => Settings.DisableGrabVoteLimit = i);
            menu.Add(disableGrabEnabledOption);
            menu.Add(disableGrabVoteLimitOption);

            //INVISIBLE
            invisibleEnabledOption = new TextMenuExt.OnOff(Dialog.Clean(DialogIds.Invisible), Settings.Invisible, true).Change(i => Settings.Invisible = i);
            invisibleVoteLimitOption = new TextMenuExt.Slider(Dialog.Clean(DialogIds.InvisibleVoteLimit), i => i.ToString(), 1, 100, Settings.InvisibleVoteLimit, 1).Change(i => Settings.InvisibleVoteLimit = i);
            menu.Add(invisibleEnabledOption);
            menu.Add(invisibleVoteLimitOption);

            //INVERT
            invertEnabledOption = new TextMenuExt.OnOff(Dialog.Clean(DialogIds.Invert), Settings.Invert, true).Change(i => Settings.Invert = i);
            invertVoteLimitOption = new TextMenuExt.Slider(Dialog.Clean(DialogIds.InvertVoteLimit), i => i.ToString(), 1, 100, Settings.InvertVoteLimit, 1).Change(i => Settings.InvertVoteLimit = i);
            menu.Add(invertEnabledOption);
            menu.Add(invertVoteLimitOption);

            //LOW FRICTION
            lowFrictionEnabledOption = new TextMenuExt.OnOff(Dialog.Clean(DialogIds.LowFriction), Settings.LowFriction, true).Change(i => Settings.LowFriction = i);
            lowFrictionVoteLimitOption = new TextMenuExt.Slider(Dialog.Clean(DialogIds.LowFrictionVoteLimit), i => i.ToString(), 1, 100, Settings.LowFrictionVoteLimit, 1).Change(i => Settings.LowFrictionVoteLimit = i);
            menu.Add(lowFrictionEnabledOption);
            menu.Add(lowFrictionVoteLimitOption);

            //OSHIRO
            oshiroEnabledOption = new TextMenuExt.OnOff(Dialog.Clean(DialogIds.Oshiro), Settings.Oshiro, true).Change(i => Settings.Oshiro = i);
            oshiroVoteLimitOption = new TextMenuExt.Slider(Dialog.Clean(DialogIds.OshiroVoteLimit), i => i.ToString(), 1, 100, Settings.OshiroVoteLimit, 1).Change(i => Settings.OshiroVoteLimit = i);
            menu.Add(oshiroEnabledOption);
            menu.Add(oshiroVoteLimitOption);

            //SNOWBALL
            snowballEnabledOption = new TextMenuExt.OnOff(Dialog.Clean(DialogIds.Snowball), Settings.Snowball, true).Change(i => Settings.Snowball = i);
            snowballVoteLimitOption = new TextMenuExt.Slider(Dialog.Clean(DialogIds.SnowballVoteLimit), i => i.ToString(), 1, 100, Settings.SnowballVoteLimit, 1).Change(i => Settings.SnowballVoteLimit = i);
            menu.Add(snowballEnabledOption);
            menu.Add(snowballVoteLimitOption);

            //DOUBLE DASH
            doubleDashEnabledOption = new TextMenuExt.OnOff(Dialog.Clean(DialogIds.DoubleDash), Settings.DoubleDash, true).Change(i => Settings.DoubleDash = i);
            doubleDashVoteLimitOption = new TextMenuExt.Slider(Dialog.Clean(DialogIds.DoubleDashVoteLimit), i => i.ToString(), 1, 100, Settings.DoubleDashVoteLimit, 1).Change(i => Settings.DoubleDashVoteLimit = i);
            menu.Add(doubleDashEnabledOption);
            menu.Add(doubleDashVoteLimitOption);

            //GODMODE
            godModeEnabledOption = new TextMenuExt.OnOff(Dialog.Clean(DialogIds.GodMode), Settings.GodMode, true).Change(i => Settings.GodMode = i);
            godModeVoteLimitOption = new TextMenuExt.Slider(Dialog.Clean(DialogIds.GodModeVoteLimit), i => i.ToString(), 1, 100, Settings.GodModeVoteLimit, 1).Change(i => Settings.GodModeVoteLimit = i);
            menu.Add(godModeEnabledOption);
            menu.Add(godModeVoteLimitOption);

            //FISH
            fishEnabledOption = new TextMenuExt.OnOff(Dialog.Clean(DialogIds.Fish), Settings.Fish, true).Change(i => Settings.Fish = i);
            fishVoteLimitOption = new TextMenuExt.Slider(Dialog.Clean(DialogIds.FishVoteLimit), i => i.ToString(), 1, 100, Settings.FishVoteLimit, 1).Change(i => Settings.FishVoteLimit = i);
            menu.Add(fishEnabledOption);
            menu.Add(fishVoteLimitOption);
        }

        protected override void gotoMenu(Overworld overworld)
        {
            overworld.Goto<OuiModOptions>();
        }
    }
}
