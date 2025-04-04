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

        public string GetUniqueUserForSpawnAction(MessageType type, List<string> names)
        {
            // This is, technically terrible if clear spawns on death is off.
            // The reason being is that over time, the spawned list will
            // get very large, and this will potentially slow down.
            switch (type)
            {
                case MessageType.SEEKER:
                    return names.Where(user => SpawnHelper.spawnedSeekers.All((seeker) => {
                        return seeker.Get<CrowControlName>().Name != user;
                    })).FirstOrDefault<string>(string.Empty);
                case MessageType.SNOWBALL:
                    return names.Where(user => SpawnHelper.spawnedSnowballs.All((snow) => {
                        return snow.Get<CrowControlName>().Name != user;
                    })).FirstOrDefault<string>(string.Empty);
                case MessageType.OSHIRO:
                    return names.Where(user => SpawnHelper.spawnedOshiros.All((oshiro) => {
                        return oshiro.Get<CrowControlName>().Name != user;
                    })).FirstOrDefault<string>(string.Empty);
            }
            return string.Empty;
        }

        public bool DoesUserHaveUniqueSpawn(MessageType type, string user)
        {
            switch (type)
            {
                case MessageType.SEEKER:
                    return SpawnHelper.DoesUserHaveSeeker(user);
                case MessageType.SNOWBALL:
                    return SpawnHelper.DoesUserHaveSnowball(user);
                case MessageType.OSHIRO:
                    return SpawnHelper.DoesUserHaveOshiro(user);
            }
            return false;
        }

        public void DieAction()
        {
            if (!ply.Dead && !currentLevel.Transitioning)
            {
                Module.BirdyHelper.BirdCaw();
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
            Module.BirdyHelper.BirdCaw();

            SpawnHelper.SpawnBumper();
        }

        public void SeekerAction(string name, bool checkUniques=false)
        {
            if (SpawnHelper.SpawnSeeker(true, name, checkUniques))
                Module.BirdyHelper.BirdCaw();
        }

        public void MirrorAction()
        {
            Module.BirdyHelper.BirdCaw();
            Settings.MirrorEnabled = true;
            TimerHelper.mirrorTimer.Stop();
            TimerHelper.mirrorTimer.Start();
        }

        public void KevinAction()
        {
            if (currentLevel != null && !currentLevel.Transitioning)
            {
                if (ply != null)
                {
                    Module.BirdyHelper.BirdCaw();
                    Module.spawnKevin = true;
                }
            }
        }

        public void DisableGrabAction()
        {
            Module.BirdyHelper.BirdCaw();
            Settings.DisableGrabEnabled = true;
            TimerHelper.disableGrabTimer.Stop();
            TimerHelper.disableGrabTimer.Start();
        }

        public void InvisibleAction()
        {
            Module.BirdyHelper.BirdCaw();
            Settings.InvisibleEnabled = true;
            TimerHelper.invisibleTimer.Stop();
            TimerHelper.invisibleTimer.Start();
        }

        public void InvertAction()
        {
            Module.BirdyHelper.BirdCaw();
            Settings.InvertEnabled = true;
            TimerHelper.invertTimer.Stop();
            TimerHelper.invertTimer.Start();
        }

        public void LowFrictionAction()
        {
            Module.BirdyHelper.BirdCaw();
            Settings.LowFrictionEnabled = true;
            TimerHelper.lowFrictionTimer.Stop();
            TimerHelper.lowFrictionTimer.Start();
        }

        public void OshiroAction(string userName, bool checkUniques=false)
        {
            if (SpawnHelper.SpawnOshiro(true, userName, checkUniques))
                Module.BirdyHelper.BirdCaw();
        }

        public void SnowballAction(string name, bool checkUniques=false)
        {
            if (SpawnHelper.SpawnSnowball(true, name, checkUniques))
                Module.BirdyHelper.BirdCaw();
        }

        public void DoubleDashAction()
        {
            if (ply != null)
            {
                Module.BirdyHelper.BirdCaw();
                ply.Dashes = 2;
                Audio.Play("event:/new_content/game/10_farewell/pinkdiamond_touch");
            }
        }

        public void GodModeAction()
        {
            Module.BirdyHelper.BirdCaw();
            Settings.GodModeEnabled = true;
            TimerHelper.godModeTimer.Stop();
            TimerHelper.godModeTimer.Start();
        }

        public void FishAction() 
        {
            if (SpawnHelper.SpawnFish())
                Module.BirdyHelper.BirdCaw();
        }

        public void WindAction(string parameter) 
        {
            Module.BirdyHelper.BirdCaw();

            WindController.Patterns pattern = WindController.Patterns.Right;

            if (parameter.IndexOf("left", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                pattern = WindController.Patterns.Left;
            }
            else if (parameter.IndexOf("right", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                pattern = WindController.Patterns.Right;
            }
            else if (parameter.IndexOf("up", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                pattern = WindController.Patterns.Up;
            }
            else if (parameter.IndexOf("down", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                pattern = WindController.Patterns.Down;
            }

            if (!Settings.WindEnabled)
            {
                currentLevel.Foreground.Backdrops.Add(new CrowControlWindSnowFG() { Alpha = 0f });
                Audio.SetAmbience("event:/env/amb/04_main", true);

                WindController controller = currentLevel.Entities.FindFirst<WindController>();

                if (controller == null)
                {
                    controller = new WindController(pattern);
                    controller.SetStartPattern();
                    currentLevel.Add(controller);
                }
                else
                {
                    controller.SetPattern(pattern);
                }

                Settings.WindEnabled = true;
            }
        }

        public void FeatherAction() 
        {
            SpawnHelper.SpawnFeatherOnPlayer();
        }
    }
}
