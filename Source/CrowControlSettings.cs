using System;
using YamlDotNet.Serialization;
using WebSocketSharp;
using System.Threading;
using Celeste.Mod.UI;
using Monocle;
using System.Collections.Generic;
using System.Linq;

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
        [SettingName(DialogIds.RequireChannelPoints)] public bool RequireChannelPoints { get; set; } = true;
        [SettingRange(1, 120)] [SettingName(DialogIds.EffectTime)] public int EffectTime { get; set; } = 30;
        [SettingName(DialogIds.MuteCrowSounds)] public bool MuteCrowSounds { get; set; } = false;
        [SettingName(DialogIds.RequireUniqueUsers)] public bool RequireUniqueUsers { get; set; } = false;
        [SettingName(DialogIds.ClearSpawnsOnDeath)] public bool ClearSpawnsOnDeath { get; set; } = true;
        [SettingName(DialogIds.ReconnectOnDisconnect)] public bool ReconnectOnDisconnect { get; set; } = true;
        [SettingName(DialogIds.RequireChatCommandPrefix)] public bool RequireCommandPrefixOnChat { get; set; } = false;
        [SettingName(DialogIds.OneActiveSpawnPerUser)] public bool OnlyOneActiveSpawnPerUser { get; set; } = false;
        [SettingName(DialogIds.ChannelName)] public string ChannelName { get; set; } = "";
        [SettingName(DialogIds.Connect)] public string Connect { get; set; } = "";
        [SettingName(DialogIds.Disconnect)] public string Disconnect { get; set; } = "";

        [SettingName(DialogIds.CommandSettings)] public string CommandSettings { get; set; }

        [YamlIgnore] [SettingIgnore] public bool Connected { get; set; } = false;
        [YamlIgnore] [SettingIgnore] public bool IsReconnecting { get; set; } = false;
        [YamlIgnore] [SettingIgnore] public int TotalRequests { get; set; } = 0;

        //command options
        //DIE
        [SettingIgnore] public bool Die { get; set; } = true;
        [SettingIgnore] public int DieVoteLimit { get; set; } = 10;

        //BLUR
        [SettingIgnore] public bool Blur { get; set; } = true;
        [SettingIgnore] public int BlurVoteLimit { get; set; } = 8;

        [YamlIgnore] [SettingIgnore] public int BlurLevel { get; set; } = 1;
        [YamlIgnore] [SettingIgnore] public bool BlurEnabled { get; set; } = false;

        //BUMP
        [SettingIgnore] public bool Bump { get; set; } = true;
        [SettingIgnore] public int BumpVoteLimit { get; set; } = 1;

        //SEEKER
        [SettingIgnore] public bool Seeker { get; set; } = true;
        [SettingIgnore] public int SeekerVoteLimit { get; set; } = 1;
        [SettingIgnore] public bool ShowSeekerNames { get; set; } = true;

        //MIRROR
        [SettingIgnore] public bool Mirror { get; set; } = true;
        [SettingIgnore] public int MirrorVoteLimit { get; set; } = 8;

        [YamlIgnore] [SettingIgnore] public bool MirrorEnabled { get; set; } = false;

        //KEVIN
        [SettingIgnore] public bool Kevin { get; set; } = true;
        [SettingIgnore] public int KevinVoteLimit { get; set; } = 2;

        //DISABLE GRAB
        [SettingIgnore] public bool DisableGrab { get; set; } = true;
        [SettingIgnore] public int DisableGrabVoteLimit { get; set; } = 8;

        [YamlIgnore] [SettingIgnore] public bool DisableGrabEnabled { get; set; } = false;

        //INVISIBLE
        [SettingIgnore] public bool Invisible { get; set; } = true;
        [SettingIgnore] public int InvisibleVoteLimit { get; set; } = 8;

        [YamlIgnore] [SettingIgnore] public bool InvisibleEnabled { get; set; } = false;

        //INVERT
        [SettingIgnore] public bool Invert { get; set; } = true;
        [SettingIgnore] public int InvertVoteLimit { get; set; } = 12;

        [YamlIgnore] [SettingIgnore] public bool InvertEnabled { get; set; } = false;

        //LOW FRICTION
        [SettingIgnore] public bool LowFriction { get; set; } = true;
        [SettingIgnore] public int LowFrictionVoteLimit { get; set; } = 8;

        [YamlIgnore] [SettingIgnore] public bool LowFrictionEnabled { get; set; } = false;

        //OSHIRO
        [SettingIgnore] public bool Oshiro { get; set; } = true;
        [SettingIgnore] public int OshiroVoteLimit { get; set; } = 2;

        //SNOWBALL
        [SettingIgnore] public bool Snowball { get; set; } = true;
        [SettingIgnore] public int SnowballVoteLimit { get; set; } = 1;
        [SettingIgnore] public bool ShowSnowballNames { get; set; } = true;

        //DOUBLEDASH
        [SettingIgnore] public bool DoubleDash { get; set; } = true;
        [SettingIgnore] public int DoubleDashVoteLimit { get; set; } = 1;

        //GODMODE
        [SettingIgnore] public bool GodMode { get; set; } = true;
        [SettingIgnore] public int GodModeVoteLimit { get; set; } = 20;

        [YamlIgnore] [SettingIgnore] public bool GodModeEnabled { get; set; } = false;

        //FISH
        [SettingIgnore] public bool Fish { get; set; } = true;
        [SettingIgnore] public int FishVoteLimit { get; set; } = 1;

        //WIND
        [SettingIgnore] public bool Wind { get; set; } = true;
        [SettingIgnore] public int WindVoteLimit { get; set; } = 1;
        [YamlIgnore] [SettingIgnore] public bool WindEnabled { get; set; } = false;

        //FEATHER
        [SettingIgnore] public bool Feather { get; set; } = true;
        [SettingIgnore] public int FeatherVoteLimit { get; set; } = 1;

        //ARCHIE
        [YamlIgnore] [SettingIgnore] public bool ArchieEnabled { get; set; } = false;

        //TWITCH AUTH
        [YamlIgnore] [SettingIgnore] public string TwitchUsername { get; private set; } = "justinfan";

        private Thread webSocketThread;
        private Random rand = new Random();

        // better handle socket disconnections
        [YamlIgnore] public bool IsExiting = false;
        [YamlIgnore] private CancellationTokenSource cancelToken;
        [YamlIgnore] private bool clickedDisconnect = false;

        [YamlIgnore] public Dictionary<MessageType, List<string>> currentVoteCounts = new Dictionary<MessageType, List<string>>();

        public CrowControlSettings() 
        {
            int randNum = rand.Next(10000, 99999);
            TwitchUsername += randNum.ToString();

            // Dynamically populate the dictionary.
            var MsgEnumVals = Enum.GetValues(typeof(MessageType)).Cast<MessageType>();
            foreach (var MsgEnum in MsgEnumVals) {
                currentVoteCounts[MsgEnum] = new List<string>();
            }
        }

        public void EffectTimeChange()
        {
            CrowControlModule.GetTimerHelper().ChangeTimerIntervals();
        }

        public void CreateChannelNameEntry(TextMenu textMenu, bool inGame) 
        {
            if (!inGame)
            {
                textMenu.Add(new TextMenu.Button(Dialog.Clean(DialogIds.ChannelName) + ": " + ChannelName).Pressed(() =>
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
            textMenu.Add(new TextMenu.Button(Dialog.Clean(DialogIds.Connect)).Pressed(() =>
            {
                if (webSocketThread == null && Enabled)
                {
                    IsReconnecting = false;
                    clickedDisconnect = false;
                    cancelToken = new CancellationTokenSource();
                    webSocketThread = new Thread(StartWebSocket);
                    webSocketThread.Start();
                }
            }));
        }

        public void CreateDisconnectEntry(TextMenu textMenu, bool inGame)
        {
            textMenu.Add(new TextMenu.Button(Dialog.Clean(DialogIds.Disconnect)).Pressed(() =>
            {
                if (webSocketThread != null && webSocketThread.IsAlive)
                {
                    IsReconnecting = false;
                    clickedDisconnect = true;
                    StopThread();
                }
            }));
        }

        public void CreateCommandSettingsEntry(TextMenu textMenu, bool inGame) 
        {
            TextMenu.Button button = AbstractSubmenu.BuildOpenMenuButton<OuiCrowControlSubmenu>(textMenu, inGame, () => OuiModOptions.Instance.Overworld.Goto<OuiModOptions>(), new object[] { DialogIds.CommandSettings });
            textMenu.Add(button);
        }

        public void StopThread()
        {
            if (webSocketThread != null && webSocketThread.IsAlive)
            {
                cancelToken.Cancel();
                webSocketThread.Join();
                webSocketThread = null;
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

                ws.Send("JOIN #" + ChannelName.ToLower());

                Console.WriteLine("JOINING");

                IsReconnecting = false;
                cancelToken.Token.WaitHandle.WaitOne();
            }
        }

        private void ReconnectIfDisconnected() 
        {
            if (ReconnectOnDisconnect && !IsExiting && !clickedDisconnect)
            {
                if (Connected == false)
                {
                    if (webSocketThread != null && webSocketThread.IsAlive)
                    {
                        StopThread();
                    }

                    if (webSocketThread == null)
                    {
                        cancelToken = new CancellationTokenSource();
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
            IsReconnecting = true;
        }

        private void Ws_OnClose(object sender, CloseEventArgs e)
        {
            Console.WriteLine("WebSocket closed!");
            if (ReconnectOnDisconnect)
            {
                IsReconnecting = true;
            }

            CrowControlModule.OnDisconnect();
            Connected = false;
            if (!clickedDisconnect)
                ReconnectIfDisconnected();
            // Reset this flag
            clickedDisconnect = false;
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
            ChatMessage chatMsg = new ChatMessage(TwitchUsername, msg, RequireCommandPrefixOnChat);

            if (msg.Command == IrcCommand.Ping) 
            {
                Console.WriteLine("PING IS NOW");
            }

            if (chatMsg.Bits >= MinimumBitsToSkip && MinimumBitsToSkip > 0)
            {
                CrowControlModule.OnMessageWithBits(chatMsg);
            }
            else if ((chatMsg.IsCustomReward && RequireChannelPoints) || !RequireChannelPoints)
            {
                CrowControlModule.OnCustomRewardMessage(chatMsg);
            }
        }
    }
}
