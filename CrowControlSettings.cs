﻿using System;
using YamlDotNet.Serialization;
using WebSocketSharp;
using System.Threading;
using Celeste.Mod.UI;

namespace Celeste.Mod.CrowControl
{
    [SettingName(DialogIds.CrowControl)]
    public class CrowControlSettings : EverestModuleSettings
    {
        private CrowControlModule CrowControlModule = CrowControlModule.Instance;

        [SettingName(DialogIds.Enabled)] public bool Enabled { get; set; } = true;
        [SettingName(DialogIds.EnableInfoPanel)] public bool EnableInfoPanel { get; set; }
        [SettingRange(1, 5)] [SettingName(DialogIds.InfoPanelSize)] public int InfoPanelSize { get; set; } = 1;
        [SettingName(DialogIds.ShowTotalRequests)] public bool ShowTotalRequests { get; set; }
        [SettingRange(1, 200)] [SettingName(DialogIds.MinimumBitsToSkip)] public int MinimumBitsToSkip { get; set; } = 50;
        [SettingRange(1, 120)] [SettingName(DialogIds.EffectTime)] public int EffectTime { get; set; } = 30;
        [SettingName(DialogIds.ReconnectOnDisconnect)] public bool ReconnectOnDisconnect { get; set; } = true;
        [SettingName(DialogIds.ChannelName)] public string ChannelName { get; set; } = "";
        [SettingName(DialogIds.Connect)] public string Connect { get; set; } = "";
        [SettingName(DialogIds.Disconnect)] public string Disconnect { get; set; } = "";

        [YamlIgnore] [SettingIgnore] public bool Connected { get; set; } = false;
        [YamlIgnore] [SettingIgnore] public int TotalRequests { get; set; } = 0;

        //Command options
        //DIE
        [SettingName(DialogIds.Die)] public bool Die { get; set; } = true;
        [SettingRange(1, 100)] [SettingName(DialogIds.DieVoteLimit)] public int DieVoteLimit { get; set; } = 10;
        [YamlIgnore] [SettingIgnore] public int CurrentDieVote { get; set; } = 0;

        //BLUR
        [SettingName(DialogIds.Blur)] public bool Blur { get; set; } = true;
        [SettingIgnore][YamlIgnore] [SettingName(DialogIds.BlurLevel)] public int BlurLevel { get; set; } = 1;
        [SettingRange(1, 100)] [SettingName(DialogIds.BlurVoteLimit)] public int BlurVoteLimit { get; set; } = 8;
        [YamlIgnore] [SettingIgnore] public int CurrentBlurVote { get; set; } = 0;
        [YamlIgnore] [SettingIgnore] public bool BlurEnabled { get; set; } = false;

        //BUMP
        [SettingName(DialogIds.Bump)] public bool Bump { get; set; } = true;
        [SettingRange(1, 100)] [SettingName(DialogIds.BumpVoteLimit)] public int BumpVoteLimit { get; set; } = 1;
        [YamlIgnore] [SettingIgnore] public int CurrentBumpVote { get; set; } = 0;

        //SEEKER
        [SettingName(DialogIds.Seeker)] public bool Seeker { get; set; } = true;
        [SettingRange(1, 100)] [SettingName(DialogIds.SeekerVoteLimit)] public int SeekerVoteLimit { get; set; } = 1;
        [YamlIgnore] [SettingIgnore] public int CurrentSeekerVote { get; set; } = 0;

        //MIRROR
        [SettingName(DialogIds.Mirror)] public bool Mirror { get; set; } = true;
        [SettingRange(1, 100)] [SettingName(DialogIds.MirrorVoteLimit)] public int MirrorVoteLimit { get; set; } = 8;
        [YamlIgnore] [SettingIgnore] public int CurrentMirrorVote { get; set; } = 0;
        [YamlIgnore] [SettingIgnore] public bool MirrorEnabled { get; set; } = false;

        //KEVIN
        [SettingName(DialogIds.Kevin)] public bool Kevin { get; set; } = true;
        [SettingRange(1, 100)] [SettingName(DialogIds.KevinVoteLimit)] public int KevinVoteLimit { get; set; } = 2;
        [YamlIgnore] [SettingIgnore] public int CurrentKevinVote { get; set; } = 0;

        //DISABLE GRAB
        [SettingName(DialogIds.DisableGrab)] public bool DisableGrab { get; set; } = true;
        [SettingRange(1, 100)] [SettingName(DialogIds.DisableGrabVoteLimit)] public int DisableGrabVoteLimit { get; set; } = 8;
        [YamlIgnore] [SettingIgnore] public int CurrentDisableGrabVote { get; set; } = 0;
        [YamlIgnore] [SettingIgnore] public bool DisableGrabEnabled { get; set; } = false;

        //INVISIBLE
        [SettingName(DialogIds.Invisible)] public bool Invisible { get; set; } = true;
        [SettingRange(1, 100)] [SettingName(DialogIds.InvisibleVoteLimit)] public int InvisibleVoteLimit { get; set; } = 8;
        [YamlIgnore] [SettingIgnore] public int CurrentInvisibleVote { get; set; } = 0;
        [YamlIgnore] [SettingIgnore] public bool InvisibleEnabled { get; set; } = false;

        //INVERT
        [SettingName(DialogIds.Invert)] public bool Invert { get; set; } = true;
        [SettingRange(1, 100)] [SettingName(DialogIds.InvertVoteLimit)] public int InvertVoteLimit { get; set; } = 12;
        [YamlIgnore] [SettingIgnore] public int CurrentInvertVote { get; set; } = 0;
        [YamlIgnore] [SettingIgnore] public bool InvertEnabled { get; set; } = false;

        //LOW FRICTION
        [SettingName(DialogIds.LowFriction)] public bool LowFriction { get; set; } = true;
        [SettingRange(1, 100)] [SettingName(DialogIds.LowFrictionVoteLimit)] public int LowFrictionVoteLimit { get; set; } = 8;
        [YamlIgnore] [SettingIgnore] public int CurrentLowFrictionVote { get; set; } = 0;
        [YamlIgnore] [SettingIgnore] public bool LowFrictionEnabled { get; set; } = false;

        //OSHIRO
        [SettingName(DialogIds.Oshiro)] public bool Oshiro { get; set; } = true;
        [SettingRange(1, 100)] [SettingName(DialogIds.OshiroVoteLimit)] public int OshiroVoteLimit { get; set; } = 2;
        [YamlIgnore] [SettingIgnore] public int CurrentOshiroVote { get; set; } = 0;

        //SNOWBALL
        [SettingName(DialogIds.Snowball)] public bool Snowball { get; set; } = true;
        [SettingRange(1, 100)] [SettingName(DialogIds.SnowballVoteLimit)] public int SnowballVoteLimit { get; set; } = 1;
        [YamlIgnore] [SettingIgnore] public int CurrentSnowballVote { get; set; } = 0;

        //DOUBLEDASH
        [SettingName(DialogIds.DoubleDash)] public bool DoubleDash { get; set; } = true;
        [SettingRange(1, 100)] [SettingName(DialogIds.DoubleDashVoteLimit)] public int DoubleDashVoteLimit { get; set; } = 1;
        [YamlIgnore] [SettingIgnore] public int CurrentDoubleDashVote { get; set; } = 0;

        //GODMODE
        [SettingName(DialogIds.GodMode)] public bool GodMode { get; set; } = true;
        [SettingRange(1, 100)] [SettingName(DialogIds.GodModeVoteLimit)] public int GodModeVoteLimit { get; set; } = 20;
        [YamlIgnore] [SettingIgnore] public int CurrentGodModeVote { get; set; } = 0;
        [YamlIgnore] [SettingIgnore] public bool GodModeEnabled { get; set; } = false;

        //ARCHIE
        [YamlIgnore] [SettingIgnore] public bool ArchieEnabled { get; set; } = false;
        [YamlIgnore] [SettingIgnore] public string TwitchUsername { get; private set; } = "justinfan";

        private Thread webSocketThread;
        private Random rand = new Random();

        public CrowControlSettings() 
        {
            int randNum = rand.Next(10000, 99999);
            TwitchUsername += randNum;
        }

        //TODO: FIX THIS
        public void EffectTimeChange()
        {
            CrowControlModule.ChangeTimerIntervals();
        }

        public void CreateChannelNameEntry(TextMenu textMenu, bool inGame) 
        {
            if (!inGame)
            {
                textMenu.Add(new TextMenu.Button(DialogIds.ChannelName + ": " + ChannelName).Pressed(() =>
                {
                    textMenu.SceneAs<Overworld>().Goto<OuiModOptionString>().Init<OuiModOptions>(
                        ChannelName,
                        v => ChannelName = v,
                        maxValueLength: 30
                        );
                }));
            }
        }

        public void CreateConnectEntry(TextMenu textMenu, bool inGame)
        {
            textMenu.Add(new TextMenu.Button(DialogIds.Connect).Pressed(() =>
            {
                if (webSocketThread == null && Enabled)
                {
                    webSocketThread = new Thread(StartWebSocket);
                    webSocketThread.Start();
                }
            }));
        }

        public void CreateDisconnectEntry(TextMenu textMenu, bool inGame)
        {
            textMenu.Add(new TextMenu.Button(DialogIds.Disconnect).Pressed(() =>
            {
                if (webSocketThread != null && webSocketThread.IsAlive)
                {
                    StopThread();
                }
            }));
        }

        public void StopThread()
        {
            if (webSocketThread != null && webSocketThread.IsAlive)
            {
                webSocketThread.Abort();
                webSocketThread = null;

                Console.WriteLine("stopped thread");
            }
        }

        public void StartWebSocket()
        {
            using (var ws = new WebSocket("wss://irc-ws.chat.twitch.tv:443"))
            {
                ws.Compression = CompressionMethod.None;

                ws.OnMessage += Ws_OnMessage;
                ws.OnOpen += Ws_OnOpen;
                ws.OnClose += Ws_OnClose;
                ws.OnError += Ws_OnError;

                ws.Connect();

                ws.Send("CAP REQ :twitch.tv/membership");
                ws.Send("CAP REQ :twitch.tv/tags");
                ws.Send("CAP REQ :twitch.tv/commands");
                ws.Send("PASS SCHMOOPIIE");
                ws.Send("NICK " + TwitchUsername);

                ws.Send("JOIN #" + ChannelName);

                Console.WriteLine("JOINING");

                while (Enabled)
                {

                }
            }
        }

        private void ReconnectIfDisconnected() 
        {
            if (ReconnectOnDisconnect)
            {
                if (Connected == false)
                {
                    if (webSocketThread != null && webSocketThread.IsAlive)
                    {
                        StopThread();
                    }

                    if (webSocketThread == null)
                    {
                        webSocketThread = new Thread(StartWebSocket);
                        webSocketThread.Start();
                    }
                }
            }
        }

        private void Ws_OnError(object sender, WebSocketSharp.ErrorEventArgs e)
        {
            Console.WriteLine("Error! " + e.Message);
            Connected = false;
        }

        private void Ws_OnClose(object sender, CloseEventArgs e)
        {
            Console.WriteLine("WebSocket closed!");

            CrowControlModule.OnDisconnect();
            Connected = false;

            ReconnectIfDisconnected();
        }

        private void Ws_OnOpen(object sender, EventArgs e)
        {
            Console.WriteLine("WebSocket open!");
            CrowControlModule.OnConnect();
            Connected = true;
        }

        private void Ws_OnMessage(object sender, MessageEventArgs e)
        {
            IrcParser parser = new IrcParser();
            IrcMessage msg = parser.ParseIrcMessage(e.Data);
            ChatMessage chatMsg = new ChatMessage(TwitchUsername, msg);

            if (msg.Command == IrcCommand.Ping) 
            {
                Console.WriteLine("PING IS NOW");
            }

            if (chatMsg.IsCustomReward && chatMsg.Bits < MinimumBitsToSkip)
            {
                CrowControlModule.OnCustomRewardMessage(chatMsg);
            }
            else if (chatMsg.Bits >= MinimumBitsToSkip)
            {
                CrowControlModule.OnMessageWithBits(chatMsg);
            }
        }
    }
}
