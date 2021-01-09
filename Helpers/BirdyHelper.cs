using Celeste;
using Celeste.Mod.CrowControl;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace CrowControl.Helpers
{
    public class BirdyHelper
    {
        public static CrowControlModule Instance;

        private CrowControlSettings settings;
        private BirdNPC birdy = null;
        private Color birdColor = new Color(255, 255, 255);
        private static TimerPlus cawTimer;

        public BirdyHelper(CrowControlSettings settings) 
        {
            cawTimer = new TimerPlus(500);
            cawTimer.Elapsed += CawTimer_Elapsed;
            this.settings = settings;
        }

        /// <summary>
        /// Creates a new bird object for use in the renderer
        /// </summary>
        private void NewBirdy(Player ply, Level currentLevel, InfoPanel infoPanel)
        {
            if (birdy == null && currentLevel != null)
            {
                birdy = new BirdNPC(ply.Position, BirdNPC.Modes.None);
                birdy.Sprite.Color = birdColor;
                birdy.Sprite.Effects = Microsoft.Xna.Framework.Graphics.SpriteEffects.FlipHorizontally;
                if (!settings.MirrorEnabled)
                {
                    birdy.Sprite.Effects = Microsoft.Xna.Framework.Graphics.SpriteEffects.FlipHorizontally;
                    birdy.Position = currentLevel.Camera.Position + new Vector2(infoPanel.uiPos.X, infoPanel.uiPos.Y - 182f);
                }
                else
                {
                    birdy.Position = new Vector2(30, 30);
                }
            }
        }

        public void HandleGameplayRenderer_Render(Player ply, Level currentLevel, InfoPanel infoPanel) 
        {
            if (birdy == null && currentLevel != null && settings.EnableInfoPanel)
            {
                NewBirdy(ply, currentLevel, infoPanel);
            }
            else if (birdy != null && settings.EnableInfoPanel)
            {
                if (!settings.MirrorEnabled)
                {
                    birdy.Sprite.Effects = Microsoft.Xna.Framework.Graphics.SpriteEffects.FlipHorizontally;
                    birdy.Position = currentLevel.Camera.Position + new Vector2(infoPanel.uiPos.X, infoPanel.uiPos.Y - 182f);
                }
                else
                {
                    birdy.Sprite.Effects = Microsoft.Xna.Framework.Graphics.SpriteEffects.None;
                    birdy.Position = currentLevel.Camera.Position + new Vector2(-infoPanel.uiPos.X + 320f, infoPanel.uiPos.Y - 182f);
                }
                birdy.Sprite.Color = birdColor;
                birdy.Depth = -10000;
                if (currentLevel.Entities.AmountOf<BirdNPC>() < 2)
                {
                    currentLevel.Add(birdy);
                }
            }
        }

        public void BirdCaw()
        {
            if (birdy != null)
            {
                birdy.Caw().MoveNext();
                cawTimer.Start();
            }
        }

        private void CawTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (!settings.MuteCrowSounds)
            {
                Audio.Play(SFX.game_gen_bird_squawk);
            }
            cawTimer.Stop();
        }

        public void ArchieAction()
        {
            BirdCaw();
            birdColor = new Color(100, 230, 50);
        }
    }
}
