using System;
using YamlDotNet.Serialization;
using WebSocketSharp;
using System.Threading;
using Celeste.Mod.UI;
using Monocle;

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
        [SettingName(DialogIds.ReconnectOnDisconnect)] public bool ReconnectOnDisconnect { get; set; } = true;
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

        [YamlIgnore] [SettingIgnore] public int CurrentDieVote { get; set; } = 0;

        //BLUR
        [SettingIgnore] public bool Blur { get; set; } = true;
        [SettingIgnore] public int BlurVoteLimit { get; set; } = 8;

        [YamlIgnore] [SettingIgnore] public int BlurLevel { get; set; } = 1;
        [YamlIgnore] [SettingIgnore] public int CurrentBlurVote { get; set; } = 0;
        [YamlIgnore] [SettingIgnore] public bool BlurEnabled { get; set; } = false;

        //BUMP
        [SettingIgnore] public bool Bump { get; set; } = true;
        [SettingIgnore] public int BumpVoteLimit { get; set; } = 1;

        [YamlIgnore] [SettingIgnore] public int CurrentBumpVote { get; set; } = 0;

        //SEEKER
        [SettingIgnore] public bool Seeker { get; set; } = true;
        [SettingIgnore] public int SeekerVoteLimit { get; set; } = 1;
        [SettingIgnore] public bool ShowSeekerNames { get; set; } = true;

        [YamlIgnore] [SettingIgnore] public int CurrentSeekerVote { get; set; } = 0;

        //MIRROR
        [SettingIgnore] public bool Mirror { get; set; } = true;
        [SettingIgnore] public int MirrorVoteLimit { get; set; } = 8;

        [YamlIgnore] [SettingIgnore] public int CurrentMirrorVote { get; set; } = 0;
        [YamlIgnore] [SettingIgnore] public bool MirrorEnabled { get; set; } = false;

        //KEVIN
        [SettingIgnore] public bool Kevin { get; set; } = true;
        [SettingIgnore] public int KevinVoteLimit { get; set; } = 2;

        [YamlIgnore] [SettingIgnore] public int CurrentKevinVote { get; set; } = 0;

        //DISABLE GRAB
        [SettingIgnore] public bool DisableGrab { get; set; } = true;
        [SettingIgnore] public int DisableGrabVoteLimit { get; set; } = 8;

        [YamlIgnore] [SettingIgnore] public int CurrentDisableGrabVote { get; set; } = 0;
        [YamlIgnore] [SettingIgnore] public bool DisableGrabEnabled { get; set; } = false;

        //INVISIBLE
        [SettingIgnore] public bool Invisible { get; set; } = true;
        [SettingIgnore] public int InvisibleVoteLimit { get; set; } = 8;

        [YamlIgnore] [SettingIgnore] public int CurrentInvisibleVote { get; set; } = 0;
        [YamlIgnore] [SettingIgnore] public bool InvisibleEnabled { get; set; } = false;

        //INVERT
        [SettingIgnore] public bool Invert { get; set; } = true;
        [SettingIgnore] public int InvertVoteLimit { get; set; } = 12;

        [YamlIgnore] [SettingIgnore] public int CurrentInvertVote { get; set; } = 0;
        [YamlIgnore] [SettingIgnore] public bool InvertEnabled { get; set; } = false;

        //LOW FRICTION
        [SettingIgnore] public bool LowFriction { get; set; } = true;
        [SettingIgnore] public int LowFrictionVoteLimit { get; set; } = 8;

        [YamlIgnore] [SettingIgnore] public int CurrentLowFrictionVote { get; set; } = 0;
        [YamlIgnore] [SettingIgnore] public bool LowFrictionEnabled { get; set; } = false;

        //OSHIRO
        [SettingIgnore] public bool Oshiro { get; set; } = true;
        [SettingIgnore] public int OshiroVoteLimit { get; set; } = 2;

        [YamlIgnore] [SettingIgnore] public int CurrentOshiroVote { get; set; } = 0;

        //SNOWBALL
        [SettingIgnore] public bool Snowball { get; set; } = true;
        [SettingIgnore] public int SnowballVoteLimit { get; set; } = 1;
        [SettingIgnore] public bool ShowSnowballNames { get; set; } = true;

        [YamlIgnore] [SettingIgnore] public int CurrentSnowballVote { get; set; } = 0;

        //DOUBLEDASH
        [SettingIgnore] public bool DoubleDash { get; set; } = true;
        [SettingIgnore] public int DoubleDashVoteLimit { get; set; } = 1;

        [YamlIgnore] [SettingIgnore] public int CurrentDoubleDashVote { get; set; } = 0;

        //GODMODE
        [SettingIgnore] public bool GodMode { get; set; } = true;
        [SettingIgnore] public int GodModeVoteLimit { get; set; } = 20;

        [YamlIgnore] [SettingIgnore] public int CurrentGodModeVote { get; set; } = 0;
        [YamlIgnore] [SettingIgnore] public bool GodModeEnabled { get; set; } = false;

        //FISH
        [SettingIgnore] public bool Fish { get; set; } = true;
        [SettingIgnore] public int FishVoteLimit { get; set; } = 1;

        [YamlIgnore] [SettingIgnore] public int CurrentFishVote { get; set; } = 0;

        //WIND
        [SettingIgnore] public bool Wind { get; set; } = true;
        [SettingIgnore] public int WindVoteLimit { get; set; } = 1;
        [YamlIgnore] [SettingIgnore] public bool WindEnabled { get; set; } = false;
        [YamlIgnore] [SettingIgnore] public int CurrentWindVote { get; set; } = 0;

        //FEATHER
        [SettingIgnore] public bool Feather { get; set; } = true;
        [SettingIgnore] public int FeatherVoteLimit { get; set; } = 1;
        [YamlIgnore] [SettingIgnore] public int CurrentFeatherVote { get; set; } = 0;

        //ARCHIE
        [YamlIgnore] [SettingIgnore] public bool ArchieEnabled { get; set; } = false;

        //TWITCH AUTH
        [YamlIgnore] [SettingIgnore] public string TwitchUsername { get; private set; } = "justinfan";

        private Thread webSocketThread;
        private Random rand = new Random();

        public CrowControlSettings() 
        {
            int randNum = rand.Next(10000, 99999);
            TwitchUsername += randNum;
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
                webSocketThread.Abort();
                webSocketThread = null;
            }
        }

        public void StartWebSocket()
        {
            using (var ws = new WebSocket("ws://irc-ws.chat.twitch.tv:80"))
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

            if (chatMsg.IsCustomReward && chatMsg.Bits < MinimumBitsToSkip && RequireChannelPoints)
            {
                CrowControlModule.OnCustomRewardMessage(chatMsg);
            }
            else if (chatMsg.Bits >= MinimumBitsToSkip)
            {
                CrowControlModule.OnMessageWithBits(chatMsg);
            }
            else if(!RequireChannelPoints)
            {
                CrowControlModule.OnCustomRewardMessage(chatMsg);
            }
        }
    }
}
