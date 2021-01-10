using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.CrowControl
{
    public class InfoPanel
    {
        private SpriteFont font;
        private CrowControlSettings settings;
        private TimerHelper timerHelper;
        public Vector2 uiPos;
        private Texture2D pixel = null;

        private string connectedText;
        private string totalRequestsText;
        private string dieText;
        private string blurText;
        private string bumpText;
        private string seekerText;
        private string mirrorText;
        private string kevinText;
        private string disableGrabText;
        private string invisibleText;
        private string invertText;
        private string lowFrictionText;
        private string oshiroText;
        private string snowballText;
        private string doubleDashText;
        private string godModeText;
        private string fishText;
        private string windText;
        private string featherText;
        private List<string> texts = new List<string>();

        public InfoPanel(CrowControlSettings settings, TimerHelper timerHelper)
        {
            this.settings = settings;
            this.timerHelper = timerHelper;

            uiPos = new Vector2(15, 219);
        }

        public void SetFont(SpriteFont font, GraphicsDevice graphicsDevice)
        {
            this.font = font;

            if (pixel == null)
            {
                pixel = new Texture2D(graphicsDevice, 1, 1);
                pixel.SetData<Color>(new Color[1] { Color.White });
            }
        }

        public void Update()
        {
            //change text based on options
            //connected
            if (settings.Connected)
            {
                connectedText = "Connected: yes";
            }
            else
            {
                connectedText = "Connected: no";
            }
            //total requests
            if (settings.ShowTotalRequests)
            {
                totalRequestsText = "Total requests: " + settings.TotalRequests;
            }
            else 
            {
                totalRequestsText = null;
            }
            //die
            if (settings.Die)
            {
                dieText = "Die (" + settings.CurrentDieVote + "/" + settings.DieVoteLimit + ")";
            }
            else
            {
                dieText = null;
            }
            //blur
            if (settings.Blur)
            {
                if (settings.BlurLevel > 1)
                {
                    blurText = "Blur (ENABLED)" + " (" + Math.Round(timerHelper.blurTimer.TimeLeft / 1000) + "s)";
                }
                else
                {
                    blurText = "Blur (" + settings.CurrentBlurVote + "/" + settings.BlurVoteLimit + ")";
                }
            }
            else
            {
                blurText = null;
            }
            //bump
            if (settings.Bump)
            {
                bumpText = "Bump (" + settings.CurrentBumpVote + "/" + settings.BumpVoteLimit + ")";
            }
            else
            {
                bumpText = null;
            }
            //seeker
            if (settings.Seeker)
            {
                seekerText = "Seeker (" + settings.CurrentSeekerVote + "/" + settings.SeekerVoteLimit + ")";
            }
            else
            {
                seekerText = null;
            }
            //mirror
            if (settings.Mirror)
            {
                if (settings.MirrorEnabled)
                {
                    mirrorText = "Mirror (ENABLED)" + " (" + Math.Round(timerHelper.mirrorTimer.TimeLeft / 1000) + "s)";
                }
                else
                {
                    mirrorText = "Mirror (" + settings.CurrentMirrorVote + "/" + settings.MirrorVoteLimit + ")";
                }
            }
            else
            {
                mirrorText = null;
            }
            //kevin
            if (settings.Kevin)
            {
                kevinText = "Kevin (" + settings.CurrentKevinVote + "/" + settings.KevinVoteLimit + ")";
            }
            else
            {
                kevinText = null;
            }
            //disable grab
            if (settings.DisableGrab)
            {
                if (settings.DisableGrabEnabled)
                {
                    disableGrabText = "Disable Grab (ENABLED)" + " (" + Math.Round(timerHelper.disableGrabTimer.TimeLeft / 1000) + "s)";
                }
                else
                {
                    disableGrabText = "Disable Grab (" + settings.CurrentDisableGrabVote + "/" + settings.DisableGrabVoteLimit + ")";
                }
            }
            else
            {
                disableGrabText = null;
            }
            //invisible
            if (settings.Invisible)
            {
                if (settings.InvisibleEnabled)
                {
                    invisibleText = "Invisible (ENABLED)" + " (" + Math.Round(timerHelper.invisibleTimer.TimeLeft / 1000) + "s)";
                }
                else
                {
                    invisibleText = "Invisible (" + settings.CurrentInvisibleVote + "/" + settings.InvisibleVoteLimit + ")";
                }
            }
            else
            {
                invisibleText = null;
            }
            //invert
            if (settings.Invert)
            {
                if (settings.InvertEnabled)
                {
                    invertText = "Invert (ENABLED)" + " (" + Math.Round(timerHelper.invertTimer.TimeLeft / 1000) + "s)";
                }
                else
                {
                    invertText = "Invert (" + settings.CurrentInvertVote + "/" + settings.InvertVoteLimit + ")";
                }
            }
            else
            {
                invertText = null;
            }
            //low friction
            if (settings.LowFriction)
            {
                if (settings.LowFrictionEnabled)
                {
                    lowFrictionText = "Low Friction (ENABLED)" + " (" + Math.Round(timerHelper.lowFrictionTimer.TimeLeft / 1000) + "s)";
                }
                else
                {
                    lowFrictionText = "Low Friction (" + settings.CurrentLowFrictionVote + "/" + settings.LowFrictionVoteLimit + ")";
                }
            }
            else
            {
                lowFrictionText = null;
            }
            //oshiro
            if (settings.Oshiro)
            {
                oshiroText = "Oshiro (" + settings.CurrentOshiroVote + "/" + settings.OshiroVoteLimit + ")";
            }
            else
            {
                oshiroText = null;
            }
            //snowball
            if (settings.Snowball)
            {
                snowballText = "Snowball (" + settings.CurrentSnowballVote + "/" + settings.SnowballVoteLimit + ")";
            }
            else 
            {
                snowballText = null;
            }
            //doubledash
            if (settings.DoubleDash)
            {
                doubleDashText = "Double Dash (" + settings.CurrentDoubleDashVote + "/" + settings.DoubleDashVoteLimit + ")";
            }
            else 
            {
                doubleDashText = null;
            }
            //godmode
            if (settings.GodMode)
            {
                if (settings.GodModeEnabled)
                {
                    godModeText = "God Mode (ENABLED)" + " (" + Math.Round(timerHelper.godModeTimer.TimeLeft / 1000) + "s)";
                }
                else
                {
                    godModeText = "God Mode (" + settings.CurrentGodModeVote + "/" + settings.GodModeVoteLimit + ")";
                }
            }
            else
            {
                godModeText = null;
            }
            //fish
            if (settings.Fish)
            {
                fishText = "Fish (" + settings.CurrentFishVote + "/" + settings.FishVoteLimit + ")";
            }
            else 
            {
                fishText = null;
            }
            //wind
            if (settings.Wind)
            {
                windText = "Wind (" + settings.CurrentWindVote + "/" + settings.WindVoteLimit + ")";
            }
            else
            {
                windText = null;
            }
            //feather
            if (settings.Feather)
            {
                featherText = "Feather (" + settings.CurrentFeatherVote + "/" + settings.FeatherVoteLimit + ")";
            }
            else 
            {
                featherText = null;
            }

            //add em to list
            if (connectedText != null)
            {
                texts.Add(connectedText);
            }
            if (totalRequestsText != null) 
            {
                texts.Add(totalRequestsText);
            }
            if (dieText != null)
            {
                texts.Add(dieText);
            }
            if (blurText != null)
            {
                texts.Add(blurText);
            }
            if (bumpText != null)
            {
                texts.Add(bumpText);
            }
            if (seekerText != null)
            {
                texts.Add(seekerText);
            }
            if (mirrorText != null)
            {
                texts.Add(mirrorText);
            }
            if (kevinText != null)
            {
                texts.Add(kevinText);
            }
            if (disableGrabText != null)
            {
                texts.Add(disableGrabText);
            }
            if (invisibleText != null)
            {
                texts.Add(invisibleText);
            }
            if (invertText != null)
            {
                texts.Add(invertText);
            }
            if (lowFrictionText != null)
            {
                texts.Add(lowFrictionText);
            }
            if (oshiroText != null) 
            {
                texts.Add(oshiroText);
            }
            if (snowballText != null) 
            {
                texts.Add(snowballText);
            }
            if (doubleDashText != null) 
            {
                texts.Add(doubleDashText);
            }
            if (godModeText != null) 
            {
                texts.Add(godModeText);
            }
            if (fishText != null) 
            {
                texts.Add(fishText);
            }
            if (windText != null) 
            {
                texts.Add(windText);
            }
            if (featherText != null) 
            {
                texts.Add(featherText);
            }
        }

        public int FindMaxWidth(List<string> texts)
        {
            if (texts.Count == 0)
            {
                return 0;
            }

            int maxWidth = int.MinValue;
            foreach (string str in texts)
            {
                int width = (int)font.MeasureString(str).X;

                if (width > maxWidth)
                {
                    maxWidth = width;
                }
            }
            return maxWidth;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(pixel, new Rectangle((int)uiPos.X, (int)uiPos.Y + 4, FindMaxWidth(texts) + ((settings.InfoPanelSize - 1) * 12) + 12 * ((settings.InfoPanelSize - 1) * 2) + 8, (texts.Count * 18) + 5 * ((settings.InfoPanelSize - 1) * 7) + 4), new Color(10, 10, 10, 200));

            for (int i = 0; i < texts.Count; i++)
            {
                spriteBatch.DrawString(font, texts[i], new Vector2(uiPos.X + 5, uiPos.Y + 4 + (i * 18) + ((settings.InfoPanelSize - 1) * (i * 2f))), Color.White, 0f, Vector2.Zero, 1 + ((settings.InfoPanelSize - 1) * 0.2f), SpriteEffects.None, 1f);
            }
            texts.Clear();
        }
    }
}
