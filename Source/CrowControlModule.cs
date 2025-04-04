using On.Monocle;
using System;
using System.Collections.Generic;
using System.Timers;
using Microsoft.Xna.Framework;
using MonoMod.Utils;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using System.Linq;
using CrowControl;
using CrowControl.Helpers;

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

        public BirdyHelper BirdyHelper { get; set; }
        private TimerHelper timerHelper;
        private SpawnHelper spawnHelper;

        private static TimerPlus seekerSpawnTimer;

        private static MiniTextbox currentMiniTextBox;

        private ActionHelper actionHelper;

        // Keep track of all the people who voted for snowballs, seekers and oshiros so we can make sure
        // that during votes, if the unique user spawns flag is on, we try to find someone that doesn't
        // have a spawn yet.
        private List<string> snowballVoters;
        private List<string> seekerVoters;
        private List<string> oshiroVoters;

        public CrowControlModule()
        {
            Instance = this;
        }

        public override void Load()
        {
            if (!Enabled)
                return;

            BirdyHelper = new BirdyHelper(Settings);
            timerHelper = new TimerHelper();
            spawnHelper = new SpawnHelper();
            actionHelper = new ActionHelper(timerHelper, spawnHelper);

            // Allocate lists
            snowballVoters = new List<string>();
            seekerVoters = new List<string>();
            oshiroVoters = new List<string>();

            effectTime = Settings.EffectTime;

            infoPanel = new InfoPanel(Settings, timerHelper);

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
            spawnHelper.ClearAllSpawnLists();
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
            if (Settings.ClearSpawnsOnDeath)
            {
                spawnHelper.ClearSeekers();
                spawnHelper.ClearSnowballs();
                spawnHelper.ClearOshiros();
            }
            else
            {
                // Note: This code block will create a never-ending spawn of spawned elements
                // until you beat the level or restart.
                foreach (Seeker seeker in spawnHelper.spawnedSeekers) 
                {
                    CrowControlName nameObj = seeker.Get<CrowControlName>();
                    nameObj.Name = null;
                }

                foreach (Snowball seeker in spawnHelper.spawnedSnowballs)
                {
                    CrowControlName nameObj = seeker.Get<CrowControlName>();
                    nameObj.Name = null;
                }

                foreach (AngryOshiro oshiro in spawnHelper.spawnedOshiros)
                {
                    CrowControlName nameObj = oshiro.Get<CrowControlName>();
                    nameObj.Name = null;
                }

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
                    spawnHelper.SpawnOshiro(false, null);
                }
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

        private void Bumper_OnPlayer(On.Celeste.Bumper.orig_OnPlayer orig, Bumper self, Player player)
        {
            spawnHelper.RemoveCurrentBumper();

            orig(self, player);
        }

        private void GameplayRenderer_Render(On.Celeste.GameplayRenderer.orig_Render orig, GameplayRenderer self, Monocle.Scene scene)
        {
            if (!Settings.Enabled) 
            {
                orig(self, scene);
                return;
            }

            BirdyHelper.HandleGameplayRenderer_Render(ply, currentLevel, infoPanel);

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
                    CrowControlName nameObj = seeker.Get<CrowControlName>();
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
                    CrowControlName nameObj = snowball.Get<CrowControlName>();
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

        static private void Engine_OnExiting(Engine.orig_OnExiting orig, Monocle.Engine self, object sender, EventArgs args)
        {
            Settings.ReconnectOnDisconnect = false;
            Settings.IsExiting = true;
            Settings.StopThread();
            orig(self, sender, args);
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

        // Handles the rules and instructions for spawning things such as Seeker, Snowball and Oshiro
        private void HandleEnemySpawn(string inUsername, MessageType messageType, ref List<string> currentVoteCounts, ref List<string> voters, int voteLimit, bool enforceActiveSpawn)
        {
            bool moreThanOne = voteLimit > 1;
            if (currentVoteCounts.Count >= voteLimit)
            {
                string userName = (moreThanOne && enforceActiveSpawn) ? actionHelper.GetUniqueUserForSpawnAction(messageType, voters) : inUsername;
                if (!string.IsNullOrEmpty(userName))
                {
                    switch (messageType)
                    {
                        case MessageType.SEEKER:
                            actionHelper.SeekerAction(userName, enforceActiveSpawn);
                        break;
                        case MessageType.SNOWBALL:
                            actionHelper.SnowballAction(userName, enforceActiveSpawn);
                        break;
                        case MessageType.OSHIRO:
                            actionHelper.OshiroAction(userName, enforceActiveSpawn);
                        break;
                        default:
                            return;
                    }
                }
                currentVoteCounts.Clear();

                if (moreThanOne)
                    voters.Clear();
            }
            else if (moreThanOne)
            {
                // If we need more than one user, and we're enforcing active spawns and the user already has a spawn
                // remove their vote, otherwise add them to the list.
                if (enforceActiveSpawn && actionHelper.DoesUserHaveUniqueSpawn(messageType, inUsername))
                    currentVoteCounts.RemoveAt(currentVoteCounts.Count - 1);
                else
                    voters.Add(inUsername);
            }
        }

        // channel points and chat messages
        public void OnCustomRewardMessage(ChatMessage msg)
        {
            if (disableCommands) 
                return;

            if (!Settings.Enabled) 
                return;

            MessageType messageType = msg.CustomRewardMessageType;            
            if (CheckIfMessageTypeEnabled(messageType))
            {
                Settings.TotalRequests++;
                var currentVoteCounts = Settings.currentVoteCounts[messageType];
                if (!msg.IsCustomReward && Settings.RequireUniqueUsers) {
                    if (currentVoteCounts.Contains(msg.UserId)) {
                        return;
                    }
                }

                // Unique spawns should only be enforced if this is not a custom reward.
                bool enforceActiveSpawn = !msg.IsCustomReward && Settings.OnlyOneActiveSpawnPerUser;

                currentVoteCounts.Add(msg.UserId);
                switch (messageType)
                {
                    case MessageType.DIE:
                        if (currentVoteCounts.Count >= Settings.DieVoteLimit)
                        {
                            if (!ply.Dead)
                            {
                                actionHelper.DieAction();
                                currentVoteCounts.Clear();
                            }
                            else
                            {
                                // if player is already dead set so it only needs 1 more vote
                                currentVoteCounts.RemoveAt(currentVoteCounts.Count - 1);
                            }
                        }
                        break;
                    case MessageType.BLUR:
                        if (currentVoteCounts.Count >= Settings.BlurVoteLimit)
                        {
                            actionHelper.BlurAction();
                            currentVoteCounts.Clear();
                        }
                        break;
                    case MessageType.BUMP:
                        if (currentVoteCounts.Count >= Settings.BumpVoteLimit)
                        {
                            actionHelper.BumpAction();
                            currentVoteCounts.Clear();
                        }
                        break;
                    case MessageType.SEEKER:
                        HandleEnemySpawn(msg.Username, messageType, ref currentVoteCounts, ref seekerVoters, Settings.SeekerVoteLimit, enforceActiveSpawn);
                        break;
                    case MessageType.MIRROR:
                        if (currentVoteCounts.Count >= Settings.MirrorVoteLimit)
                        {
                            actionHelper.MirrorAction();
                            currentVoteCounts.Clear();
                        }
                        break;
                    case MessageType.KEVIN:
                        if (currentVoteCounts.Count >= Settings.KevinVoteLimit)
                        {
                            actionHelper.KevinAction();
                            currentVoteCounts.Clear();
                        }
                        break;
                    case MessageType.DISABLEGRAB:
                        if (currentVoteCounts.Count >= Settings.DisableGrabVoteLimit)
                        {
                            actionHelper.DisableGrabAction();
                            currentVoteCounts.Clear();
                        }
                        break;
                    case MessageType.INVISIBLE:
                        if (currentVoteCounts.Count >= Settings.InvisibleVoteLimit)
                        {
                            actionHelper.InvisibleAction();
                            currentVoteCounts.Clear();
                        }
                        break;
                    case MessageType.INVERT:
                        if (currentVoteCounts.Count >= Settings.InvertVoteLimit)
                        {
                            actionHelper.InvertAction();
                            currentVoteCounts.Clear();
                        }
                        break;
                    case MessageType.LOWFRICTION:
                        if (currentVoteCounts.Count >= Settings.LowFrictionVoteLimit)
                        {
                            actionHelper.LowFrictionAction();
                            currentVoteCounts.Clear();
                        }
                        break;
                    case MessageType.OSHIRO:
                        HandleEnemySpawn(msg.Username, messageType, ref currentVoteCounts, ref oshiroVoters, Settings.OshiroVoteLimit, enforceActiveSpawn);
                        break;
                    case MessageType.SNOWBALL:
                        HandleEnemySpawn(msg.Username, messageType, ref currentVoteCounts, ref snowballVoters, Settings.SnowballVoteLimit, enforceActiveSpawn);  
                        break;
                    case MessageType.DOUBLEDASH:
                        if (currentVoteCounts.Count >= Settings.DoubleDashVoteLimit)
                        {
                            actionHelper.DoubleDashAction();
                            currentVoteCounts.Clear();
                        }
                        break;
                    case MessageType.GODMODE:
                        if (currentVoteCounts.Count >= Settings.GodModeVoteLimit)
                        {
                            actionHelper.GodModeAction();
                            currentVoteCounts.Clear();
                        }
                        break;
                    case MessageType.FISH:
                        if (currentVoteCounts.Count >= Settings.FishVoteLimit)
                        {
                            actionHelper.FishAction();
                            currentVoteCounts.Clear();
                        }
                        break;
                    case MessageType.WIND:
                        CheckForWindMessages(msg.CustomRewardParameter);
                        if (currentVoteCounts.Count >= Settings.WindVoteLimit)
                        {
                            actionHelper.WindAction(GetWindMessageWinner());
                            currentVoteCounts.Clear();
                        }
                        break;
                    case MessageType.FEATHER:
                        if (currentVoteCounts.Count >= Settings.FeatherVoteLimit)
                        {
                            actionHelper.FeatherAction();
                            currentVoteCounts.Clear();
                        }
                        break;
                    case MessageType.ARCHIE:
                        BirdyHelper.ArchieAction();
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
                        actionHelper.OshiroAction(msg.Username);
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
                    case MessageType.FEATHER:
                        actionHelper.FeatherAction();
                        break;
                    case MessageType.ARCHIE:
                        BirdyHelper.ArchieAction();
                        break;
                }
            }
        }

        public void OnConnect() 
        {
            if (!Settings.IsReconnecting)
            {
                Audio.Play(SFX.ui_main_message_confirm);
                if (currentLevel != null)
                {
                    currentMiniTextBox = new MiniTextbox(DialogIds.TextBoxConnected);
                    currentLevel.Add(currentMiniTextBox);
                }
            }
        }

        public void OnDisconnect() 
        {
            if (!Settings.IsReconnecting)
            {
                Audio.Play(SFX.ui_main_message_confirm);
                if (currentLevel != null)
                {
                    currentMiniTextBox = new MiniTextbox(DialogIds.TextBoxDisconnected);
                    currentLevel.Add(currentMiniTextBox);
                }
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
                case MessageType.FEATHER:
                    if (Settings.Feather) 
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