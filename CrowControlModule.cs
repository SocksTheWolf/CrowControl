using On.Monocle;
using System;
using System.Collections.Generic;
using System.Timers;
using Microsoft.Xna.Framework;
using MonoMod.Utils;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using System.Linq;

namespace Celeste.Mod.CrowControl
{
    public class CrowControlModule : EverestModule
    {
        public static CrowControlModule Instance;

        public override Type SettingsType => typeof(CrowControlSettings);
        public static CrowControlSettings Settings => (CrowControlSettings)Instance._Settings;
        public static bool Enabled => Settings.Enabled;

        private Player ply;
        private Level currentLevel;
        public bool spawnKevin = false;
        private bool disableCommands = false;
        private int effectTime;
        private bool inCredits = false;
        private bool ascending = false;
        private bool twoDashesOnSpawn = false;

        private InfoPanel infoPanel;

        private TimerHelper timerHelper;
        private SpawnHelper spawnHelper;

        private static TimerPlus cawTimer;
        private static TimerPlus seekerSpawnTimer;

        private BirdNPC birdy = null;
        private Color birdColor = new Color(255, 255, 255);

        private static MiniTextbox currentMiniTextBox;

        private ActionHelper actionHelper;
        

        public CrowControlModule()
        {
            Instance = this;
        }

        public override void Load()
        {
            if (!Enabled)
            {
                return;
            }

            timerHelper = new TimerHelper();
            spawnHelper = new SpawnHelper();
            actionHelper = new ActionHelper(timerHelper, spawnHelper);

            effectTime = Settings.EffectTime;

            infoPanel = new InfoPanel(Settings, timerHelper);

            cawTimer = new TimerPlus(500);
            cawTimer.Elapsed += CawTimer_Elapsed;

            seekerSpawnTimer = new TimerPlus(250);
            seekerSpawnTimer.Elapsed += SeekerSpawnTimer_Elapsed;

            Engine.OnExiting += Engine_OnExiting;

            On.Celeste.HudRenderer.RenderContent += HudRenderer_RenderContent;
            On.Celeste.GameplayRenderer.Render += GameplayRenderer_Render;
            On.Celeste.Player.Added += Player_Added;
            On.Celeste.Player.Die += Player_Die;
            On.Celeste.Level.Update += Level_Update;
            On.Celeste.Level.NextLevel += Level_NextLevel;

            On.Celeste.Bumper.OnPlayer += Bumper_OnPlayer;
            On.Celeste.Player.IntroRespawnEnd += Player_IntroRespawnEnd;
            Everest.Events.Level.OnExit += Level_OnExit;
            IL.Celeste.Pathfinder.ctor += ModPathfinderConstructor;
            IL.Celeste.MiniTextbox.Render += centerHook;
            On.Celeste.CS07_Credits.Added += CS07_Credits_Added;
            On.Celeste.CS08_Ending.OnEnd += CS08_Ending_OnEnd;
            On.Celeste.CS07_Ascend.OnBegin += CS07_Ascend_OnBegin;
            On.Celeste.Player.SummitLaunch += Player_SummitLaunch;
        }

        private PlayerDeadBody Player_Die(On.Celeste.Player.orig_Die orig, Player self, Vector2 direction, bool evenIfInvincible, bool registerDeathInStats)
        {
            if (self.Dashes == 2)
            {
                twoDashesOnSpawn = true;
            }
            else 
            {
                twoDashesOnSpawn = false;
            }

            foreach (Seeker seeker in spawnHelper.spawnedSeekers) 
            {
                SeekerName nameObj = seeker.Get<SeekerName>();
                nameObj.Name = null;
            }

            foreach (Snowball seeker in spawnHelper.spawnedSnowballs)
            {
                SnowballName nameObj = seeker.Get<SnowballName>();
                nameObj.Name = null;
            }

            return orig(self, direction, evenIfInvincible, registerDeathInStats);
        }

        public TimerHelper GetTimerHelper() 
        {
            return timerHelper;
        }

        private void ModPathfinderConstructor(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            // go everywhere where the 0.8 second delay is defined
            if (cursor.TryGotoNext(MoveType.After,
                instr => instr.OpCode == OpCodes.Ldc_I4 && (int)instr.Operand == 200,
                instr => instr.OpCode == OpCodes.Ldc_I4 && (int)instr.Operand == 200))
            {
                // we will resize the pathfinder (provided that the seekers everywhere variant is enabled) to fit all rooms in vanilla Celeste
                cursor.Emit(OpCodes.Pop);
                cursor.Emit(OpCodes.Pop);
                cursor.Emit(OpCodes.Ldarg_1);
                cursor.EmitDelegate<Func<Pathfinder, int>>(DeterminePathfinderWidth);
                cursor.Emit(OpCodes.Ldarg_1);
                cursor.EmitDelegate<Func<Pathfinder, int>>(DeterminePathfinderHeight);
            }
        }

        private int DeterminePathfinderWidth(Pathfinder self)
        {
            if (spawnHelper.extendedPathfinder)
            {
                return 659;
            }
            return 200;
        }

        private int DeterminePathfinderHeight(Pathfinder self)
        {
            if (spawnHelper.extendedPathfinder)
            {
                return 407;
            }
            return 200;
        }

        private void Player_SummitLaunch(On.Celeste.Player.orig_SummitLaunch orig, Player self, float targetX)
        {
            ascending = true;
            orig(self, targetX);
        }

        private void CS07_Ascend_OnBegin(On.Celeste.CS07_Ascend.orig_OnBegin orig, CS07_Ascend self, Level level)
        {
            ascending = true;
            orig(self, level);
        }

        private void CS08_Ending_OnEnd(On.Celeste.CS08_Ending.orig_OnEnd orig, CS08_Ending self, Level level)
        {
            inCredits = false;
            orig(self, level);
        }

        private void CS07_Credits_Added(On.Celeste.CS07_Credits.orig_Added orig, CS07_Credits self, Monocle.Scene scene)
        {
            inCredits = true;
            orig(self, scene);
        }

        private static bool Centered = true;

        private void centerHook(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            if (cursor.TryGotoNext(MoveType.After,
                instr => instr.MatchLdfld<Vector2>("X"),
                instr => instr.MatchLdarg(0),
                instr => instr.MatchLdfld<MiniTextbox>("portraitSize")))
            {
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Func<float, MiniTextbox, float>>(modPosition);
            }

            if (cursor.TryGotoNext(
                instr => instr.MatchLdcR4(0),
                instr => instr.MatchLdcR4(0.5f)))
            {
                cursor.Index++;
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Func<float, MiniTextbox, float>>(modJustify);
            }
        }

        private static float modJustify(float vanilla, MiniTextbox self)
        {
            if (self == currentMiniTextBox)
            {
                return Centered ? 0.5f : vanilla;
            }
            else 
            {
                return vanilla;
            }
        }

        private static float modPosition(float vanilla, MiniTextbox self)
        {
            if (self == currentMiniTextBox)
            {
                return Centered ? 796f : vanilla;
            }
            else 
            {
                return vanilla;
            }
        }

        private void Level_NextLevel(On.Celeste.Level.orig_NextLevel orig, Level self, Vector2 at, Vector2 dir)
        {
            spawnHelper.spawnedSeekers.Clear();
            spawnHelper.spawnedSnowballs.Clear();
            spawnHelper.spawnedOshiros.Clear();

            DisableWind();

            ascending = false;

            orig(self, at, dir);
        }

        private void DisableWind() 
        {
            currentLevel.Foreground.Backdrops.RemoveAll(backdrop => backdrop.GetType() == typeof(CrowControlWindSnowFG));
            Settings.WindEnabled = false;
        }

        private void SeekerSpawnTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            spawnHelper.SpawnSeeker(false, null);
            seekerSpawnTimer.Stop();
        }

        /// <summary>
        /// Make 1 seeker, snowball and oshiro respawn when you die if there was one in the last room
        /// </summary>
        private void Player_IntroRespawnEnd(On.Celeste.Player.orig_IntroRespawnEnd orig, Player self)
        {
            if (spawnHelper.spawnedSeekers.Count >= 1)
            {
                seekerSpawnTimer.Stop();
                seekerSpawnTimer.Start();
            }

            if (spawnHelper.spawnedSnowballs.Count >= 1) 
            {
                spawnHelper.SpawnSnowball(false, null);
            }

            if (spawnHelper.spawnedOshiros.Count >= 1) 
            {
                spawnHelper.SpawnOshiro(false);
            }

            if (twoDashesOnSpawn) 
            {
                self.Dashes = 2;
            }

            DisableWind();

            orig(self);
        }

        private void Level_OnExit(Level level, LevelExit exit, LevelExit.Mode mode, Session session, HiresSnow snow)
        {
            spawnHelper.extendedPathfinder = false;
        }

        private void CawTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (!Settings.MuteCrowSounds)
            {
                Audio.Play(SFX.game_gen_bird_squawk);
            }
            cawTimer.Stop();
        }

        private void Bumper_OnPlayer(On.Celeste.Bumper.orig_OnPlayer orig, Bumper self, Player player)
        {
            spawnHelper.RemoveCurrentBumper();

            orig(self, player);
        }

        /// <summary>
        /// Creates a new bird object for use in the renderer
        /// </summary>
        private void NewBirdy()
        {
            if (birdy == null && currentLevel != null)
            {
                birdy = new BirdNPC(ply.Position, BirdNPC.Modes.None);
                birdy.Sprite.Color = birdColor;
                birdy.Sprite.Effects = Microsoft.Xna.Framework.Graphics.SpriteEffects.FlipHorizontally;
                if (!Settings.MirrorEnabled)
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

        private void GameplayRenderer_Render(On.Celeste.GameplayRenderer.orig_Render orig, GameplayRenderer self, Monocle.Scene scene)
        {
            if (!Settings.Enabled) 
            {
                orig(self, scene);
                return;
            }

            if (birdy == null && currentLevel != null && Settings.EnableInfoPanel)
            {
                NewBirdy();
            }
            else if (birdy != null && Settings.EnableInfoPanel)
            {
                if (!Settings.MirrorEnabled)
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

            orig(self, scene);
        }

        private void HudRenderer_RenderContent(On.Celeste.HudRenderer.orig_RenderContent orig, HudRenderer self, Monocle.Scene scene)
        {
            if (!Settings.Enabled) 
            {
                orig(self, scene);
                return;
            }

            if (effectTime != Settings.EffectTime) 
            {
                timerHelper.ChangeTimerIntervals();
                effectTime = Settings.EffectTime;
            }

            Monocle.Draw.SpriteBatch.Begin();
            if (Settings.EnableInfoPanel)
            {
                infoPanel.Update();
                infoPanel.SetFont(Monocle.Draw.DefaultFont, Monocle.Draw.SpriteBatch.GraphicsDevice);
                infoPanel.Draw(Monocle.Draw.SpriteBatch);
            }

            if (Settings.ShowSeekerNames)
            {
                foreach (Seeker seeker in spawnHelper.spawnedSeekers)
                {
                    SeekerName nameObj = seeker.Get<SeekerName>();
                    if (nameObj.Name != null)
                    {
                        string name = nameObj.Name;

                        DrawTextOverObject(name, seeker.Position);
                    }
                }
            }

            if (Settings.ShowSnowballNames) 
            {
                foreach (Snowball snowball in spawnHelper.spawnedSnowballs) 
                {
                    SnowballName nameObj = snowball.Get<SnowballName>();
                    if (nameObj.Name != null) 
                    {
                        string name = nameObj.Name;

                        DrawTextOverObject(name, snowball.Position);
                    }
                }
            }

            Monocle.Draw.SpriteBatch.End();

            orig(self, scene);
        }

        private void DrawTextOverObject(string name, Vector2 objectPos) 
        {
            Vector2 levelPos = objectPos - currentLevel.LevelOffset;
            Vector2 cameraPosInLevel = currentLevel.Camera.Position - currentLevel.LevelOffset;
            Vector2 seekerDrawPos = (levelPos - cameraPosInLevel) * 6;

            Vector2 drawPos = new Vector2(-(Monocle.Draw.DefaultFont.MeasureString(name).X / 2) - 20, -95) + seekerDrawPos;
            Vector2 mirrorDrawPos = new Vector2(-drawPos.X + (320f * 5.5f), drawPos.Y);
            if (!Settings.MirrorEnabled)
            {
                Monocle.Draw.SpriteBatch.DrawString(Monocle.Draw.DefaultFont, name, drawPos, Color.White, 0f, Vector2.Zero, 2f, Microsoft.Xna.Framework.Graphics.SpriteEffects.None, 0f);
            }
            else
            {
                Monocle.Draw.SpriteBatch.DrawString(Monocle.Draw.DefaultFont, name, mirrorDrawPos, Color.White, 0f, Vector2.Zero, 2f, Microsoft.Xna.Framework.Graphics.SpriteEffects.None, 0f);
            }
        }

        private void Level_Update(On.Celeste.Level.orig_Update orig, Level self)
        {
            Level newLevel = self;
            actionHelper.SetLevel(newLevel);
            spawnHelper.SetLevel(newLevel);
            currentLevel = newLevel;

            if (Settings.Blur && Settings.BlurEnabled)
            {
                BlurCurrentLevel(newLevel, Settings.BlurLevel);
            }
            else
            {
                BlurCurrentLevel(newLevel, 1);
            }

            if (Settings.DisableGrab && Settings.DisableGrabEnabled)
            {
                SaveData.Instance.Assists.NoGrabbing = true;
            }
            else
            {
                SaveData.Instance.Assists.NoGrabbing = false;
            }

            if (Settings.Mirror && Settings.MirrorEnabled)
            {
                SaveData.Instance.Assists.MirrorMode = true;
                if (Settings.Invert && Settings.InvertEnabled)
                {
                    Input.MoveX.Inverted = false;
                    Input.Aim.InvertedX = false;
                }
                else
                {
                    Input.MoveX.Inverted = true;
                    Input.Aim.InvertedX = true;
                }
            }
            else
            {
                SaveData.Instance.Assists.MirrorMode = false;
                if (Settings.Invert && Settings.InvertEnabled)
                {
                    Input.MoveX.Inverted = true;
                    Input.Aim.InvertedX = true;
                }
                else
                {
                    Input.MoveX.Inverted = false;
                    Input.Aim.InvertedX = false;
                }
            }

            if (Settings.Invisible && Settings.InvisibleEnabled)
            {
                SaveData.Instance.Assists.InvisibleMotion = true;
            }
            else
            {
                SaveData.Instance.Assists.InvisibleMotion = false;
            }

            if (Settings.LowFriction && Settings.LowFrictionEnabled)
            {
                SaveData.Instance.Assists.LowFriction = true;
            }
            else
            {
                SaveData.Instance.Assists.LowFriction = false;
            }

            if (Settings.GodMode && Settings.GodModeEnabled)
            {
                SaveData.Instance.Assists.Invincible = true;
            }
            else 
            {
                SaveData.Instance.Assists.Invincible = false;
            }

            if (spawnKevin)
            {
                CrushBlock crushBlock = new CrushBlock(ply.Position - new Vector2(12, 50), 25, 25, CrushBlock.Axes.Vertical, false);
                currentLevel.Add(crushBlock);
                currentLevel.Entities.UpdateLists();
                crushBlock.OnDashCollide(ply, new Vector2(0, -1));
                spawnKevin = false;
            }

            if (newLevel.Entities.AmountOf<PlayerSeeker>() > 0 || inCredits || ascending)
            {
                disableCommands = true;
            }
            else 
            {
                disableCommands = false;
            }

            orig(newLevel);
        }

        private void BlurCurrentLevel(Level currentLevel, int blurLevel)
        {
            currentLevel.Camera.Zoom = 1f / blurLevel;
            Vector2 newCameraPos = new Vector2(currentLevel.Camera.Position.X, currentLevel.Camera.Position.Y);
            currentLevel.Camera.Position = newCameraPos;
            currentLevel.Zoom = 1f * blurLevel;
        }

        private void Player_Added(On.Celeste.Player.orig_Added orig, Player self, Monocle.Scene scene)
        {
            ply = self;
            actionHelper.SetPlayer(self);
            spawnHelper.SetPlayer(self);

            orig(self, scene);
        }

        //Disable enabled to stop the webhook thread
        private void Engine_OnExiting(Engine.orig_OnExiting orig, Monocle.Engine self, object sender, EventArgs args)
        {
            Settings.ReconnectOnDisconnect = false;
            Settings.StopThread();
        }

        private void ArchieAction()
        {
            BirdCaw();
            birdColor = new Color(100, 230, 50);
        }

        public void BirdCaw()
        {
            if (birdy != null)
            {
                birdy.Caw().MoveNext();
                cawTimer.Start();
            }   
        }

        private List<string> windVotes = new List<string>();
        private void CheckForWindMessages(string parameter) 
        {
            if (parameter.IndexOf("left", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                windVotes.Add("left");
            }
            else if (parameter.IndexOf("right", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                windVotes.Add("right");
            }
            else if (parameter.IndexOf("up", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                windVotes.Add("up");
            }
            else if (parameter.IndexOf("down", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                windVotes.Add("down");
            }
        }

        private string GetWindMessageWinner() 
        {
            string winner = "right";

            var item = windVotes.GroupBy(i => i).OrderByDescending(grp => grp.Count()).Select(grp => grp.Key).Where(x => x != null).First();

            if (item == "left" || item == "right" || item == "down" || item == "up") 
            {
                winner = item;
            }

            windVotes.Clear();
            return winner;
        }

        public void OnCustomRewardMessage(ChatMessage msg)
        {
            MessageType messageType = msg.CustomRewardMessageType;

            if (disableCommands) 
            {
                return;
            }

            if (!Settings.Enabled) 
            {
                return;
            }

            Settings.TotalRequests++;

            if (CheckIfMessageTypeEnabled(messageType))
            {
                switch (messageType)
                {
                    case MessageType.DIE:
                        if (Settings.CurrentDieVote < Settings.DieVoteLimit - 1)
                        {
                            Settings.CurrentDieVote++;
                        }
                        else
                        {
                            if (!ply.Dead)
                            {
                                actionHelper.DieAction();
                            }
                            else
                            {
                                // if player is already dead set so it only needs 1 more vote
                                Settings.CurrentDieVote = Settings.DieVoteLimit - 1;
                            }
                            Settings.CurrentDieVote = 0;
                        }
                        break;
                    case MessageType.BLUR:
                        if (Settings.CurrentBlurVote < Settings.BlurVoteLimit - 1)
                        {
                            Settings.CurrentBlurVote++;
                        }
                        else
                        {
                            actionHelper.BlurAction();

                            Settings.CurrentBlurVote = 0;
                        }
                        break;
                    case MessageType.BUMP:
                        if (Settings.CurrentBumpVote < Settings.BumpVoteLimit - 1)
                        {
                            Settings.CurrentBumpVote++;
                        }
                        else
                        {
                            actionHelper.BumpAction();
                            Settings.CurrentBumpVote = 0;
                        }
                        break;
                    case MessageType.SEEKER:
                        if (Settings.CurrentSeekerVote < Settings.SeekerVoteLimit - 1)
                        {
                            Settings.CurrentSeekerVote++;
                        }
                        else
                        {
                            actionHelper.SeekerAction(msg.Username);
                            Settings.CurrentSeekerVote = 0;
                        }
                        break;
                    case MessageType.MIRROR:
                        if (Settings.CurrentMirrorVote < Settings.MirrorVoteLimit - 1)
                        {
                            Settings.CurrentMirrorVote++;
                        }
                        else
                        {
                            actionHelper.MirrorAction();
                            Settings.CurrentMirrorVote = 0;
                        }
                        break;
                    case MessageType.KEVIN:
                        if (Settings.CurrentKevinVote < Settings.KevinVoteLimit - 1)
                        {
                            Settings.CurrentKevinVote++;
                        }
                        else
                        {
                            actionHelper.KevinAction();
                            Settings.CurrentKevinVote = 0;
                        }
                        break;
                    case MessageType.DISABLEGRAB:
                        if (Settings.CurrentDisableGrabVote < Settings.DisableGrabVoteLimit - 1)
                        {
                            Settings.CurrentDisableGrabVote++;
                        }
                        else
                        {
                            actionHelper.DisableGrabAction();
                            Settings.CurrentDisableGrabVote = 0;
                        }
                        break;
                    case MessageType.INVISIBLE:
                        if (Settings.CurrentInvisibleVote < Settings.InvisibleVoteLimit - 1)
                        {
                            Settings.CurrentInvisibleVote++;
                        }
                        else
                        {
                            actionHelper.InvisibleAction();
                            Settings.CurrentInvisibleVote = 0;
                        }
                        break;
                    case MessageType.INVERT:
                        if (Settings.CurrentInvertVote < Settings.InvertVoteLimit - 1)
                        {
                            Settings.CurrentInvertVote++;
                        }
                        else
                        {
                            actionHelper.InvertAction();
                            Settings.CurrentInvertVote = 0;
                        }
                        break;
                    case MessageType.LOWFRICTION:
                        if (Settings.CurrentLowFrictionVote < Settings.LowFrictionVoteLimit - 1)
                        {
                            Settings.CurrentLowFrictionVote++;
                        }
                        else
                        {
                            actionHelper.LowFrictionAction();
                            Settings.CurrentLowFrictionVote = 0;
                        }
                        break;
                    case MessageType.OSHIRO:
                        if (Settings.CurrentOshiroVote < Settings.OshiroVoteLimit - 1)
                        {
                            Settings.CurrentOshiroVote++;
                        }
                        else
                        {
                            actionHelper.OshiroAction();
                            Settings.CurrentOshiroVote = 0;
                        }
                        break;
                    case MessageType.SNOWBALL:
                        if (Settings.CurrentSnowballVote < Settings.SnowballVoteLimit - 1)
                        {
                            Settings.CurrentSnowballVote++;
                        }
                        else
                        {
                            actionHelper.SnowballAction(msg.Username);
                            Settings.CurrentSnowballVote = 0;
                        }
                        break;
                    case MessageType.DOUBLEDASH:
                        if (Settings.CurrentDoubleDashVote < Settings.DoubleDashVoteLimit - 1)
                        {
                            Settings.CurrentDoubleDashVote++;
                        }
                        else 
                        {
                            actionHelper.DoubleDashAction();
                            Settings.CurrentDoubleDashVote = 0;
                        }
                        break;
                    case MessageType.GODMODE:
                        if (Settings.CurrentGodModeVote < Settings.GodModeVoteLimit - 1)
                        {
                            Settings.CurrentGodModeVote++;
                        }
                        else 
                        {
                            actionHelper.GodModeAction();
                            Settings.CurrentGodModeVote = 0;
                        }
                        break;
                    case MessageType.FISH:
                        if (Settings.CurrentFishVote < Settings.FishVoteLimit - 1)
                        {
                            Settings.CurrentFishVote++;
                        }
                        else
                        {
                            actionHelper.FishAction();
                            Settings.CurrentFishVote = 0;
                        }
                        break;
                    case MessageType.WIND:
                        CheckForWindMessages(msg.CustomRewardParameter);
                        if (Settings.CurrentWindVote < Settings.WindVoteLimit - 1)
                        {
                            Settings.CurrentWindVote++;
                        }
                        else
                        {
                            actionHelper.WindAction(GetWindMessageWinner());
                            Settings.CurrentWindVote = 0;
                        }
                        break;
                    case MessageType.ARCHIE:
                        ArchieAction();
                        break;
                }
            }
        }

        // Bit functionality
        public void OnMessageWithBits(ChatMessage msg)
        {
            Settings.TotalRequests++;

            if (CheckIfMessageTypeEnabled(msg.CustomRewardMessageType))
            {
                switch (msg.CustomRewardMessageType)
                {
                    case MessageType.DIE:
                        actionHelper.DieAction();
                        break;
                    case MessageType.BLUR:
                        actionHelper.BlurAction();
                        break;
                    case MessageType.BUMP:
                        actionHelper.BumpAction();
                        break;
                    case MessageType.SEEKER:
                        actionHelper.SeekerAction(msg.Username);
                        break;
                    case MessageType.MIRROR:
                        actionHelper.MirrorAction();
                        break;
                    case MessageType.KEVIN:
                        actionHelper.KevinAction();
                        break;
                    case MessageType.DISABLEGRAB:
                        actionHelper.DisableGrabAction();
                        break;
                    case MessageType.INVISIBLE:
                        actionHelper.InvisibleAction();
                        break;
                    case MessageType.INVERT:
                        actionHelper.InvertAction();
                        break;
                    case MessageType.LOWFRICTION:
                        actionHelper.LowFrictionAction();
                        break;
                    case MessageType.OSHIRO:
                        actionHelper.OshiroAction();
                        break;
                    case MessageType.SNOWBALL:
                        actionHelper.SnowballAction(msg.Username);
                        break;
                    case MessageType.DOUBLEDASH:
                        actionHelper.DoubleDashAction();
                        break;
                    case MessageType.GODMODE:
                        actionHelper.GodModeAction();
                        break;
                    case MessageType.FISH:
                        actionHelper.FishAction();
                        break;
                    case MessageType.WIND:
                        actionHelper.WindAction(msg.CustomRewardParameter);
                        break;
                    case MessageType.ARCHIE:
                        ArchieAction();
                        break;
                }
            }
        }

        public void OnConnect() 
        {
            Audio.Play(SFX.ui_main_message_confirm);
            if (currentLevel != null) 
            {
                currentMiniTextBox = new MiniTextbox(DialogIds.TextBoxConnected);
                currentLevel.Add(currentMiniTextBox);
            }
        }

        public void OnDisconnect() 
        {
            Audio.Play(SFX.ui_main_message_confirm);
            if (currentLevel != null)
            {
                currentMiniTextBox = new MiniTextbox(DialogIds.TextBoxDisconnected);
                currentLevel.Add(currentMiniTextBox);
            }
        }

        private bool CheckIfMessageTypeEnabled(MessageType type)
        {
            switch (type)
            {
                case MessageType.DIE:
                    if (Settings.Die)
                    {
                        return true;
                    }
                    break;
                case MessageType.BLUR:
                    if (Settings.Blur)
                    {
                        return true;
                    }
                    break;
                case MessageType.BUMP:
                    if (Settings.Bump)
                    {
                        return true;
                    }
                    break;
                case MessageType.SEEKER:
                    if (Settings.Seeker)
                    {
                        return true;
                    }
                    break;
                case MessageType.MIRROR:
                    if (Settings.Mirror)
                    {
                        return true;
                    }
                    break;
                case MessageType.KEVIN:
                    if (Settings.Kevin)
                    {
                        return true;
                    }
                    break;
                case MessageType.DISABLEGRAB:
                    if (Settings.DisableGrab)
                    {
                        return true;
                    }
                    break;
                case MessageType.INVISIBLE:
                    if (Settings.Invisible)
                    {
                        return true;
                    }
                    break;
                case MessageType.INVERT:
                    if (Settings.Invert)
                    {
                        return true;
                    }
                    break;
                case MessageType.LOWFRICTION:
                    if (Settings.LowFriction)
                    {
                        return true;
                    }
                    break;
                case MessageType.OSHIRO:
                    if (Settings.Oshiro) 
                    {
                        return true;
                    }
                    break;
                case MessageType.SNOWBALL:
                    if (Settings.Snowball) 
                    {
                        return true;
                    }
                    break;
                case MessageType.DOUBLEDASH:
                    if (Settings.DoubleDash) 
                    {
                        return true;
                    }
                    break;
                case MessageType.GODMODE:
                    if (Settings.GodMode) 
                    {
                        return true;
                    }
                    break;
                case MessageType.FISH:
                    if (Settings.Fish) 
                    {
                        return true;
                    }
                    break;
                case MessageType.WIND:
                    if (Settings.Wind) 
                    {
                        return true;
                    }
                    break;
                case MessageType.ARCHIE:
                    return true;
            }

            return false;
        }

        // Optional, initialize anything after Celeste has initialized itself properly.
        public override void Initialize()
        {

        }

        // Unload the entirety of your mod's content, remove any event listeners and undo all hooks.
        public override void Unload()
        {
        }
    }
}
