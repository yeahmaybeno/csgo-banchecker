using Newtonsoft.Json;
using SteamKit2;
using SteamKit2.GC;
using SteamKit2.GC.CSGO.Internal;
using SteamKit2.Internal;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace banchecker
{
    class BanCheck
    {
        public class AccountDetails
        {
            public string login = string.Empty;
            public bool banned = false;
            public int penalty_reason = -1;
            public int penalty_seconds = -1;
            public int wins = -1;
            public int rank = -1;
        }

        private const int SLEEPTIME = 2000;

        private SteamGameCoordinator SteamGameCoordinator;
        private SteamClient steamClient;
        private SteamUser steamUser;
        private CallbackManager manager;

        private static List<AccountDetails> accounts = new List<AccountDetails>();

        private bool isRunning = false;

        private string username = string.Empty;
        private string password = string.Empty;

        private bool AcknowledgedPenalty = false;

        private Stopwatch sw = new Stopwatch();

        public BanCheck(string user, string pass)
        {
            username = user;
            password = pass;

            steamClient = new SteamClient();
            manager = new CallbackManager(steamClient);

            steamUser = steamClient.GetHandler<SteamUser>();
            SteamGameCoordinator = steamClient.GetHandler<SteamGameCoordinator>();

            manager.Subscribe<SteamClient.ConnectedCallback>(OnConnected);
            manager.Subscribe<SteamClient.DisconnectedCallback>(OnDisconnected);

            manager.Subscribe<SteamApps.VACStatusCallback>(OnVACStatus);

            manager.Subscribe<SteamUser.LoggedOnCallback>(OnLoggedOn);
            manager.Subscribe<SteamUser.LoggedOffCallback>(OnLoggedOff);

            manager.Subscribe<SteamGameCoordinator.MessageCallback>(OnMessageCall);
        }

        public void Check()
        {
            isRunning = true;

            // initiate the connection
            steamClient.Connect();

            // create our callback handling loop
            while (isRunning)
            {
                // in order for the callbacks to get routed, they need to be handled by the manager
                manager.RunWaitCallbacks(TimeSpan.FromSeconds(1));
                if(sw.Elapsed.Seconds > 5)
                {
                    Log.WriteLine("Resending CMsgClientHello");

                    var ClientHello = new ClientGCMsgProtobuf<CMsgClientHello>((uint)EGCBaseClientMsg.k_EMsgGCClientHello);
                    SteamGameCoordinator.Send(ClientHello, 730);

                    sw.Restart();
                }
            }
        }

        private void OnConnected(SteamClient.ConnectedCallback callback)
        {
            Log.WriteLine(string.Format("Connected to Steam! Logging in '{0}'...", username));

            steamUser.LogOn(new SteamUser.LogOnDetails
            {
                Username = username,
                Password = password,
            });
        }

        private void OnDisconnected(SteamClient.DisconnectedCallback callback)
        {
            Log.WriteLine("Disconnected from Steam");

            isRunning = false;
        }

        private void OnVACStatus(SteamApps.VACStatusCallback callback)
        {
            if(callback.BannedApps.Contains(730))
            {
                Log.WriteLine(string.Format("Banned: {0}", username));
                steamUser.LogOff();
            }
        }

        private void OnLoggedOn(SteamUser.LoggedOnCallback callback)
        {
            if (callback.Result != EResult.OK)
            {
                if (callback.Result == EResult.AccountLogonDenied)
                {
                    // if we recieve AccountLogonDenied or one of it's flavors (AccountLogonDeniedNoMailSent, etc)
                    // then the account we're logging into is SteamGuard protected
                    // see sample 5 for how SteamGuard can be handled

                    Log.WriteLine("Unable to logon to Steam: This account is SteamGuard protected.", true);

                    isRunning = false;
                    return;
                }

                Log.WriteLine(string.Format("Unable to logon to Steam: {0} / {1}", callback.Result, callback.ExtendedResult), true);

                isRunning = false;
                return;
            }


            Log.WriteLine("Successfully logged on!");

            // at this point, we'd be able to perform actions on Steam

            var Play = new ClientMsgProtobuf<CMsgClientGamesPlayed>(EMsg.ClientGamesPlayed);
            Play.Body.games_played.Add(new CMsgClientGamesPlayed.GamePlayed { game_id = new GameID(730), });
            steamClient.Send(Play);

            Thread.Sleep(SLEEPTIME);

            var ClientHello = new ClientGCMsgProtobuf<CMsgClientHello>((uint)EGCBaseClientMsg.k_EMsgGCClientHello);
            SteamGameCoordinator.Send(ClientHello, 730);

            sw.Start();
        }

        private void OnLoggedOff(SteamUser.LoggedOffCallback callback)
        {
            if (callback.Result.ToString().Contains("LoggedInElsewhere"))
            {
                accounts.Add(new AccountDetails()
                {
                    login = string.Format("{0}:{1}", username, password)
                });
            }
            else
            {
                Log.WriteLine(string.Format("Logged off of Steam: {0}", callback.Result));
            }
        }

        private void OnMessageCall(SteamGameCoordinator.MessageCallback callback)
        {
            Log.WriteLine(callback.EMsg.ToString());

            switch (callback.EMsg)
            {
                case (uint)EGCBaseClientMsg.k_EMsgGCClientWelcome:
                    {
                        sw.Stop();

                        var ClientHello = new ClientGCMsgProtobuf<CMsgGCCStrike15_v2_MatchmakingGC2ClientHello>((uint)ECsgoGCMsg.k_EMsgGCCStrike15_v2_MatchmakingClient2GCHello);
                        SteamGameCoordinator.Send(ClientHello, 730);

                        break;
                    }

                case (uint)ECsgoGCMsg.k_EMsgGCCStrike15_v2_MatchmakingGC2ClientHello:
                    {
                        var details = new ClientGCMsgProtobuf<CMsgGCCStrike15_v2_MatchmakingGC2ClientHello>(callback.Message);

                        LogAccount(details);

                        if (details.Body.vac_banned == 0)
                        {
                            var penalty_seconds = Math.Abs(details.Body.penalty_seconds);
                            if (details.Body.penalty_secondsSpecified && penalty_seconds > 0 && !AcknowledgedPenalty)
                            {
                                AcknowledgedPenalty = true;

                                Log.WriteLine("k_EMsgGCCStrike15_v2_AcknowledgePenalty");

                                var AcknowledgePenalty = new ClientGCMsgProtobuf<CMsgGCCStrike15_v2_AcknowledgePenalty>((uint)ECsgoGCMsg.k_EMsgGCCStrike15_v2_AcknowledgePenalty);
                                AcknowledgePenalty.Body.acknowledged = 1;
                                AcknowledgePenalty.Body.acknowledgedSpecified = true;

                                SteamGameCoordinator.Send(AcknowledgePenalty, 730);

                                Thread.Sleep(SLEEPTIME);

                                var ClientHello = new ClientGCMsgProtobuf<CMsgGCCStrike15_v2_MatchmakingGC2ClientHello>((uint)ECsgoGCMsg.k_EMsgGCCStrike15_v2_MatchmakingClient2GCHello);
                                SteamGameCoordinator.Send(ClientHello, 730);

                                return;
                            }

                            if (details.Body.ranking == null || (!details.Body.penalty_secondsSpecified && penalty_seconds > 0))
                            {
                                Log.WriteLine(string.Format("{0} !details.Body.penalty_secondsSpecified && penalty_seconds > 0", username), true);

                                Thread.Sleep(SLEEPTIME);

                                var ClientHello = new ClientGCMsgProtobuf<CMsgGCCStrike15_v2_MatchmakingGC2ClientHello>((uint)ECsgoGCMsg.k_EMsgGCCStrike15_v2_MatchmakingClient2GCHello);
                                SteamGameCoordinator.Send(ClientHello, 730);

                                return;
                            }

                            accounts.Add(new AccountDetails()
                            {
                                login = string.Format("{0}:{1}", username, password),
                                penalty_reason = (int)details.Body.penalty_reason,
                                penalty_seconds = (int)details.Body.penalty_seconds,
                                wins = (int)details.Body.ranking.wins,
                                rank = details.Body.player_level
                            });
                        }

                        steamUser.LogOff();
                        break;
                    }
                default: break;
            }
        }

        private void LogAccount(ClientGCMsgProtobuf<CMsgGCCStrike15_v2_MatchmakingGC2ClientHello> details)
        {
            Log.WriteLine(string.Format("player_cur_xp {0}", details.Body.player_cur_xp.ToString()));
            Log.WriteLine(string.Format("player_level {0}", details.Body.player_level.ToString()));
            Log.WriteLine(string.Format("vac_banned {0}", details.Body.vac_banned.ToString()));
            Log.WriteLine(string.Format("penalty_reason {0}", details.Body.penalty_reason.ToString()));
            Log.WriteLine(string.Format("penalty_seconds {0}", details.Body.penalty_seconds.ToString()));
            Log.WriteLine(string.Format("wins {0}", (details.Body.ranking != null) ? details.Body.ranking.wins.ToString() : "null"));
        }

        public static void Dump(string output_txt, string output_json)
        {
            File.WriteAllText(output_json, JsonConvert.SerializeObject(accounts, Formatting.Indented));

            string contents = string.Empty;
            foreach(var account in accounts)
            {
                contents += account.login + Environment.NewLine;
            }

            File.WriteAllText(output_txt, contents);
        }
    }
}
