using On.Monocle;
using System;
using System.Collections.Generic;
using System.Timers;
using Microsoft.Xna.Framework;
using MonoMod.Utils;
using MonoMod.Cil;
using Mono.Cecil.Cil;

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
        private Random rand = new Random();
        private Bumper currentBumper;
        private bool spawnKevin = false;
        private bool disableCommands = false;
        private int effectTime;

        private InfoPanel infoPanel;

        public static TimerPlus blurTimer;
        public static TimerPlus mirrorTimer;
        public static TimerPlus disableGrabTimer;
        public static TimerPlus invisibleTimer;
        public static TimerPlus invertTimer;
        public static TimerPlus lowFrictionTimer;
        public static TimerPlus godModeTimer;

        private static TimerPlus cawTimer;
        private static TimerPlus seekerSpawnTimer;

        private BirdNPC birdy = null;
        private Color birdColor = new Color(255, 255, 255);

        private bool extendedPathfinder = false;
        private List<Seeker> spawnedSeekers = new List<Seeker>();
        private List<Snowball> spawnedSnowballs = new List<Snowball>();
        private List<AngryOshiro> spawnedOshiros = new List<AngryOshiro>();

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

            effectTime = Settings.EffectTime;

            infoPanel = new InfoPanel(Settings);

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

            cawTimer = new TimerPlus(500);
            cawTimer.Elapsed += CawTimer_Elapsed;

            seekerSpawnTimer = new TimerPlus(250);
            seekerSpawnTimer.Elapsed += SeekerSpawnTimer_Elapsed;

            Engine.OnExiting += Engine_OnExiting;

            On.Celeste.HudRenderer.RenderContent += HudRenderer_RenderContent;
            On.Celeste.GameplayRenderer.Render += GameplayRenderer_Render;
            On.Celeste.Player.Added += Player_Added;
            On.Celeste.Level.Update += Level_Update;
            On.Celeste.Level.NextLevel += Level_NextLevel;

            On.Celeste.Bumper.OnPlayer += Bumper_OnPlayer;
            On.Celeste.Player.IntroRespawnEnd += Player_IntroRespawnEnd;
            Everest.Events.Level.OnExit += Level_OnExit;
            IL.Celeste.Pathfinder.ctor += ModPathfinderConstructor;
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

        private void Level_NextLevel(On.Celeste.Level.orig_NextLevel orig, Level self, Vector2 at, Vector2 dir)
        {
            spawnedSeekers.Clear();

            orig(self, at, dir);
        }

        private void SeekerSpawnTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            SpawnSeeker(false);
            seekerSpawnTimer.Stop();
        }

        private void Player_IntroRespawnEnd(On.Celeste.Player.orig_IntroRespawnEnd orig, Player self)
        {
            if (spawnedSeekers.Count >= 1)
            {
                seekerSpawnTimer.Stop();
                seekerSpawnTimer.Start();
            }

            orig(self);
        }

        private void Level_OnExit(Level level, LevelExit exit, LevelExit.Mode mode, Session session, HiresSnow snow)
        {
            extendedPathfinder = false;
        }

        private void CawTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Audio.Play(SFX.game_gen_bird_squawk);
            cawTimer.Stop();
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

        private void Bumper_OnPlayer(On.Celeste.Bumper.orig_OnPlayer orig, Bumper self, Player player)
        {
            currentLevel.Remove(currentBumper);

            orig(self, player);
        }

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
                ChangeTimerIntervals();
                effectTime = Settings.EffectTime;
            }

            Monocle.Draw.SpriteBatch.Begin();
            if (Settings.EnableInfoPanel)
            {
                infoPanel.Update();
                infoPanel.SetFont(Monocle.Draw.DefaultFont, Monocle.Draw.SpriteBatch.GraphicsDevice);
                infoPanel.Draw(Monocle.Draw.SpriteBatch);
            }
            Monocle.Draw.SpriteBatch.End();

            orig(self, scene);
        }

        private void Level_Update(On.Celeste.Level.orig_Update orig, Level self)
        {
            Level newLevel = self;
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

            if (newLevel.Entities.AmountOf<PlayerSeeker>() > 0)
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

            orig(self, scene);
        }

        //Disable enabled to stop the webhook thread
        private void Engine_OnExiting(Engine.orig_OnExiting orig, Monocle.Engine self, object sender, EventArgs args)
        {
            Settings.ReconnectOnDisconnect = false;
            Settings.StopThread();
        }

        private void DieAction()
        {
            BirdCaw();
            if (!ply.Dead && !currentLevel.Transitioning)
            {
                ply.Die(Vector2.Zero);
            }
        }

        private void BlurAction()
        {
            Settings.BlurEnabled = true;
            Settings.BlurLevel = 3;
            blurTimer.Stop();
            blurTimer.Start();
        }

        private void BumpAction()
        {
            if (currentLevel != null)
            {
                BirdCaw();
                Vector2 offset = new Vector2(rand.Next(-8, 8), rand.Next(-8, 8));
                currentBumper = new Bumper(ply.Position + offset, Vector2.Zero);
                currentBumper.Active = false;
                currentLevel.Add(currentBumper);
            }
        }

        private void SeekerAction()
        {
            BirdCaw();

            SpawnSeeker(true);
        }

        private void SpawnSeeker(bool addToList) 
        {
            if (currentLevel == null) 
            {
                return;
            }

            if (currentLevel.Bounds.Width / 8 > 659 || currentLevel.Bounds.Height / 8 > 407)
            {
                return;
            }

            if (!extendedPathfinder)
            {
                extendedPathfinder = true;
                currentLevel.Pathfinder = new Pathfinder(currentLevel);
            }

            for (int i = 0; i < 100; i++)
            {
                int x = rand.Next(currentLevel.Bounds.Width) + currentLevel.Bounds.X;
                int y = rand.Next(currentLevel.Bounds.Height) + currentLevel.Bounds.Y;

                // should be at least 100 pixels from the player
                double playerDistance = Math.Sqrt(Math.Pow(MathHelper.Distance(x, ply.X), 2) + Math.Pow(MathHelper.Distance(y, ply.Y), 2));

                // also check if we are not spawning in a wall, that would be a shame
                Rectangle collideRectangle = new Rectangle(x - 8, y - 8, 16, 16);
                if (playerDistance > 50 && !currentLevel.CollideCheck<Solid>(collideRectangle) && !currentLevel.CollideCheck<Seeker>(collideRectangle))
                {
                    // build a Seeker with a proper EntityID to make Speedrun Tool happy (this is useless in vanilla Celeste but the constructor call is intercepted by Speedrun Tool)
                    EntityData seekerData = generateBasicEntityData(currentLevel, 10 + 1);
                    seekerData.Position = new Vector2(x, y);
                    Seeker seeker = new Seeker(seekerData, Vector2.Zero);
                    if (addToList)
                    {
                        spawnedSeekers.Add(seeker);
                    }
                    currentLevel.Add(seeker);
                    break;
                }
            }
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
            if (extendedPathfinder)
            {
                return 659;
            }
            return 200;
        }

        private int DeterminePathfinderHeight(Pathfinder self)
        {
            if (extendedPathfinder)
            {
                return 407;
            }
            return 200;
        }

        private void MirrorAction()
        {
            BirdCaw();
            Settings.MirrorEnabled = true;
            mirrorTimer.Stop();
            mirrorTimer.Start();
        }

        private void KevinAction()
        {
            BirdCaw();
            if (currentLevel != null && !currentLevel.Transitioning)
            {
                if (ply != null)
                {
                    spawnKevin = true;
                }
            }
        }

        private void DisableGrabAction()
        {
            BirdCaw();
            Settings.DisableGrabEnabled = true;
            disableGrabTimer.Stop();
            disableGrabTimer.Start();
        }

        private void InvisibleAction()
        {
            BirdCaw();
            Settings.InvisibleEnabled = true;
            invisibleTimer.Stop();
            invisibleTimer.Start();
        }

        private void InvertAction()
        {
            BirdCaw();
            Settings.InvertEnabled = true;
            invertTimer.Stop();
            invertTimer.Start();
        }

        private void LowFrictionAction()
        {
            BirdCaw();
            Settings.LowFrictionEnabled = true;
            lowFrictionTimer.Stop();
            lowFrictionTimer.Start();
        }

        private void ArchieAction()
        {
            BirdCaw();
            birdColor = new Color(100, 230, 50);
        }

        private void OshiroAction()
        {
            BirdCaw();

            if (currentLevel == null) 
            {
                return;
            }

            Vector2 position = new Vector2(currentLevel.Bounds.Left - 32, currentLevel.Bounds.Top + currentLevel.Bounds.Height / 2);
            currentLevel.Add(new AngryOshiro(position, false));
        }

        private void SnowballAction() 
        {
            BirdCaw();

            if (currentLevel == null) 
            {
                return;
            }

            Snowball snowball = new Snowball();
            currentLevel.Add(snowball);

            snowball.X = currentLevel.Camera.Left - 60f;
        }

        private void DoubleDashAction() 
        {
            BirdCaw();

            if (ply != null) 
            {
                ply.Dashes = 2;
                Audio.Play("event:/new_content/game/10_farewell/pinkdiamond_touch");
            }
        }

        private void GodModeAction() 
        {
            BirdCaw();
            Settings.GodModeEnabled = true;
            godModeTimer.Stop();
            godModeTimer.Start();
        }

        private void BirdCaw()
        {
            if (birdy != null)
            {
                birdy.Caw().MoveNext();
                cawTimer.Start();
            }
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
                                DieAction();
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
                            BlurAction();

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
                            BumpAction();
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
                            SeekerAction();
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
                            MirrorAction();
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
                            KevinAction();
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
                            DisableGrabAction();
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
                            InvisibleAction();
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
                            InvertAction();
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
                            LowFrictionAction();
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
                            OshiroAction();
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
                            SnowballAction();
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
                            DoubleDashAction();
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
                            GodModeAction();
                            Settings.CurrentGodModeVote = 0;
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
                        DieAction();
                        break;
                    case MessageType.BLUR:
                        BlurAction();
                        break;
                    case MessageType.BUMP:
                        BumpAction();
                        break;
                    case MessageType.SEEKER:
                        SeekerAction();
                        break;
                    case MessageType.MIRROR:
                        MirrorAction();
                        break;
                    case MessageType.KEVIN:
                        KevinAction();
                        break;
                    case MessageType.DISABLEGRAB:
                        DisableGrabAction();
                        break;
                    case MessageType.INVISIBLE:
                        InvisibleAction();
                        break;
                    case MessageType.INVERT:
                        InvertAction();
                        break;
                    case MessageType.LOWFRICTION:
                        LowFrictionAction();
                        break;
                    case MessageType.OSHIRO:
                        OshiroAction();
                        break;
                    case MessageType.SNOWBALL:
                        SnowballAction();
                        break;
                    case MessageType.DOUBLEDASH:
                        DoubleDashAction();
                        break;
                    case MessageType.GODMODE:
                        GodModeAction();
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
                currentLevel.Add(new MiniTextbox(DialogIds.TextBoxConnected));
            }
        }

        public void OnDisconnect() 
        {
            Audio.Play(SFX.ui_main_message_confirm);
            if (currentLevel != null)
            {
                currentLevel.Add(new MiniTextbox(DialogIds.TextBoxDisconnected));
            }
        }

        private EntityData generateBasicEntityData(Level level, int entityNumber)
        {
            EntityData entityData = new EntityData();

            // we hash the current level name, so we will get a hopefully-unique "room hash" for each room in the level
            // the resulting hash should be between 0 and 49_999_999 inclusive
            int roomHash = Math.Abs(level.Session.Level.GetHashCode()) % 50_000_000;

            // generate an ID, minimum 1_000_000_000 (to minimize chances of conflicting with existing entities)
            // and maximum 1_999_999_999 inclusive (1_000_000_000 + 49_999_999 * 20 + 19) => max value for int32 is 2_147_483_647
            // => if the same entity (same entityNumber) is generated in the same room, it will have the same ID, like any other entity would
            entityData.ID = 1_000_000_000 + roomHash * 20 + entityNumber;

            entityData.Level = level.Session.LevelData;
            entityData.Values = new Dictionary<string, object>();

            return entityData;
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
