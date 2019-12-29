using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Celeste.Mod.CrowControl
{
    public class TimerHelper
    {
        private CrowControlSettings Settings { get; set; }

        public TimerPlus blurTimer;
        public TimerPlus mirrorTimer;
        public TimerPlus disableGrabTimer;
        public TimerPlus invisibleTimer;
        public TimerPlus invertTimer;
        public TimerPlus lowFrictionTimer;
        public TimerPlus godModeTimer;

        public TimerHelper() 
        {
            Settings = CrowControlModule.Settings;

            blurTimer = new TimerPlus(Settings.EffectTime * 1000);
            blurTimer.Elapsed += BlurTimer_Elapsed;

            mirrorTimer = new TimerPlus(Settings.EffectTime * 1000);
            mirrorTimer.Elapsed += MirrorTimer_Elapsed;

            disableGrabTimer = new TimerPlus(Settings.EffectTime * 1000);
            disableGrabTimer.Elapsed += DisableGrabTimer_Elapsed;

            invisibleTimer = new TimerPlus(Settings.EffectTime * 1000);
            invisibleTimer.Elapsed += InvisibleTimer_Elapsed;

            invertTimer = new TimerPlus(Settings.EffectTime * 1000);
            invertTimer.Elapsed += InvertTimer_Elapsed;

            lowFrictionTimer = new TimerPlus(Settings.EffectTime * 1000);
            lowFrictionTimer.Elapsed += LowFrictionTimer_Elapsed;

            godModeTimer = new TimerPlus(Settings.EffectTime * 1000);
            godModeTimer.Elapsed += GodModeTimer_Elapsed;
        }

        public void ChangeTimerIntervals()
        {
            BlurTimer_Elapsed(null, null);
            MirrorTimer_Elapsed(null, null);
            DisableGrabTimer_Elapsed(null, null);
            InvisibleTimer_Elapsed(null, null);
            InvertTimer_Elapsed(null, null);
            LowFrictionTimer_Elapsed(null, null);
            GodModeTimer_Elapsed(null, null);

            blurTimer.Interval = Settings.EffectTime * 1000;
            mirrorTimer.Interval = Settings.EffectTime * 1000;
            disableGrabTimer.Interval = Settings.EffectTime * 1000;
            invisibleTimer.Interval = Settings.EffectTime * 1000;
            invertTimer.Interval = Settings.EffectTime * 1000;
            lowFrictionTimer.Interval = Settings.EffectTime * 1000;
            godModeTimer.Interval = Settings.EffectTime * 1000;
        }

        private void BlurTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Settings.BlurLevel = 1;
            Settings.BlurEnabled = false;
            blurTimer.Stop();
        }

        private void MirrorTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Settings.MirrorEnabled = false;
            mirrorTimer.Stop();
        }

        private void DisableGrabTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Settings.DisableGrabEnabled = false;
            disableGrabTimer.Stop();
        }

        private void InvisibleTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Settings.InvisibleEnabled = false;
            invisibleTimer.Stop();
        }

        private void InvertTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Settings.InvertEnabled = false;
            invertTimer.Stop();
        }

        private void LowFrictionTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Settings.LowFrictionEnabled = false;
            lowFrictionTimer.Stop();
        }

        private void GodModeTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Settings.GodModeEnabled = false;
            godModeTimer.Stop();
        }
    }
}
