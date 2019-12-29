using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.CrowControl
{
    public class ActionHelper
    {
        private CrowControlModule Module { get; set; }
        private CrowControlSettings Settings { get; set; }
        private TimerHelper TimerHelper { get; set; }
        private SpawnHelper SpawnHelper { get; set; }

        private Player ply;
        private Level currentLevel;

        public ActionHelper(TimerHelper timerHelper, SpawnHelper spawnHelper) 
        {
            Module = CrowControlModule.Instance;
            Settings = CrowControlModule.Settings;
            TimerHelper = timerHelper;
            SpawnHelper = spawnHelper;
        }

        public void SetPlayer(Player ply) 
        {
            this.ply = ply;
        }

        public void SetLevel(Level currentLevel) 
        {
            this.currentLevel = currentLevel;
        }

        public void DieAction()
        {
            Module.BirdCaw();
            if (!ply.Dead && !currentLevel.Transitioning)
            {
                ply.Die(Vector2.Zero);
            }
        }

        public void BlurAction()
        {
            Settings.BlurEnabled = true;
            Settings.BlurLevel = 3;
            TimerHelper.blurTimer.Stop();
            TimerHelper.blurTimer.Start();
        }

        public void BumpAction()
        {
            Module.BirdCaw();

            SpawnHelper.SpawnBumper();
        }

        public void SeekerAction(string name)
        {
            Module.BirdCaw();

            SpawnHelper.SpawnSeeker(true, name);
        }

        public void MirrorAction()
        {
            Module.BirdCaw();
            Settings.MirrorEnabled = true;
            TimerHelper.mirrorTimer.Stop();
            TimerHelper.mirrorTimer.Start();
        }

        public void KevinAction()
        {
            Module.BirdCaw();
            if (currentLevel != null && !currentLevel.Transitioning)
            {
                if (ply != null)
                {
                    Module.spawnKevin = true;
                }
            }
        }

        public void DisableGrabAction()
        {
            Module.BirdCaw();
            Settings.DisableGrabEnabled = true;
            TimerHelper.disableGrabTimer.Stop();
            TimerHelper.disableGrabTimer.Start();
        }

        public void InvisibleAction()
        {
            Module.BirdCaw();
            Settings.InvisibleEnabled = true;
            TimerHelper.invisibleTimer.Stop();
            TimerHelper.invisibleTimer.Start();
        }

        public void InvertAction()
        {
            Module.BirdCaw();
            Settings.InvertEnabled = true;
            TimerHelper.invertTimer.Stop();
            TimerHelper.invertTimer.Start();
        }

        public void LowFrictionAction()
        {
            Module.BirdCaw();
            Settings.LowFrictionEnabled = true;
            TimerHelper.lowFrictionTimer.Stop();
            TimerHelper.lowFrictionTimer.Start();
        }

        public void OshiroAction()
        {
            Module.BirdCaw();

            SpawnHelper.SpawnOshiro(true);
        }

        public void SnowballAction()
        {
            Module.BirdCaw();

            SpawnHelper.SpawnSnowball(true);
        }

        public void DoubleDashAction()
        {
            Module.BirdCaw();

            if (ply != null)
            {
                ply.Dashes = 2;
                Audio.Play("event:/new_content/game/10_farewell/pinkdiamond_touch");
            }
        }

        public void GodModeAction()
        {
            Module.BirdCaw();
            Settings.GodModeEnabled = true;
            TimerHelper.godModeTimer.Stop();
            TimerHelper.godModeTimer.Start();
        }

        public void FishAction() 
        {
            Module.BirdCaw();

            Console.WriteLine("spawn fish");
            SpawnHelper.SpawnFish();
        }
    }
}
