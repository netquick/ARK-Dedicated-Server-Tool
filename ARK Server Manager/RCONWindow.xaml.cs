﻿using ARK_Server_Manager.Lib;
using ARK_Server_Manager.Lib.ViewModel;
using ARK_Server_Manager.Lib.ViewModel.RCON;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interactivity;
using WPFSharp.Globalizer;
using System.Collections.Specialized;

namespace ARK_Server_Manager
{
    public enum PlayerSortType
    {
        Online = 0,
        Name = 1,
        Tribe = 2,
        LastUpdated = 3,
    }

    [Flags]
    public enum PlayerFilterType
    {
        None            = 0,
        Offline         = 0x1,
        Online          = 0x2,
        Banned          = 0x4,
        Whitelisted     = 0x8
    }

    public enum InputMode
    {
        Command,
        Global,
        Broadcast,
    }

    public enum InputWindowMode
    {
        None,
        ServerChatTo,
        RenamePlayer,
        RenameTribe,
    }

    public class ScrollToBottomAction : TriggerAction<RichTextBox>
    {
        protected override void Invoke(object parameter)
        {
            AssociatedObject.ScrollToEnd();
        }
    }

    public class RCONOutput_CommandTime : Run
    {
        public RCONOutput_CommandTime() : this(DateTime.Now) { }
        public RCONOutput_CommandTime(DateTime time) : base($"[{time.ToString("g")}] ") { }
    }

    public class RCONOutput_TimedCommand : Span
    {
        protected RCONOutput_TimedCommand() : base()
        {
            base.Inlines.Add(new RCONOutput_CommandTime());
        }

        public RCONOutput_TimedCommand(Inline output) : this()
        {            
            base.Inlines.Add(output);
        }

        public RCONOutput_TimedCommand(string output) : this(new Run(output)) { }

    }

    public class RCONOutput_Comment : Run
    {
        public RCONOutput_Comment(string value) : base(value) { }
    }

    public class RCONOutput_ChatSend : RCONOutput_TimedCommand
    {
        public RCONOutput_ChatSend(string target, string output) : base($"[{target}] {output}") { }
    }

    public class RCONOutput_Broadcast : RCONOutput_ChatSend
    {
        public RCONOutput_Broadcast(string output) : base("ALL", output) { }
    }

    public class RCONOutput_ConnectionChanged : RCONOutput_TimedCommand
    {
        public RCONOutput_ConnectionChanged(bool isConnected) : base(isConnected ? GlobalizedApplication.Instance.GetResourceString("RCON_ConnectionEstablishedLabel") : GlobalizedApplication.Instance.GetResourceString("RCON_ConnectionLostLabel")) { }
    }

    public class RCONOutput_Command : RCONOutput_TimedCommand
    {
        public RCONOutput_Command(string text) : base(text) { }
    };

    public class RCONOutput_NoResponse : RCONOutput_TimedCommand
    {
        public RCONOutput_NoResponse() : base(GlobalizedApplication.Instance.GetResourceString("RCON_NoCommandResponseLabel")) { }
    };

    public class RCONOutput_CommandOutput : RCONOutput_TimedCommand
    {
        public RCONOutput_CommandOutput(string text) : base(text) { }
    };

    /// <summary>
    /// Interaction logic for RCON.xaml
    /// </summary>
    public partial class RCONWindow : Window
    {
        private static Dictionary<Server, RCONWindow> RCONWindows = new Dictionary<Server, RCONWindow>();

        private GlobalizedApplication _globalizer = GlobalizedApplication.Instance;

        public static readonly DependencyProperty CurrentConfigProperty = DependencyProperty.Register(nameof(CurrentConfig), typeof(Config), typeof(RCONWindow), new PropertyMetadata(Config.Default));
        public static readonly DependencyProperty CurrentInputModeProperty = DependencyProperty.Register(nameof(CurrentInputMode), typeof(InputMode), typeof(RCONWindow), new PropertyMetadata(InputMode.Command));
        public static readonly DependencyProperty PlayerFilteringProperty = DependencyProperty.Register(nameof(PlayerFiltering), typeof(PlayerFilterType), typeof(RCONWindow), new PropertyMetadata(PlayerFilterType.Online | PlayerFilterType.Offline | PlayerFilterType.Banned | PlayerFilterType.Whitelisted));
        public static readonly DependencyProperty PlayerFilterStringProperty = DependencyProperty.Register(nameof(PlayerFilterString), typeof(string), typeof(RCONWindow), new PropertyMetadata(string.Empty));
        public static readonly DependencyProperty PlayerSortingProperty = DependencyProperty.Register(nameof(PlayerSorting), typeof(PlayerSortType), typeof(RCONWindow), new PropertyMetadata(PlayerSortType.Online));
        public static readonly DependencyProperty PlayersViewProperty = DependencyProperty.Register(nameof(PlayersView), typeof(ICollectionView), typeof(RCONWindow), new PropertyMetadata(null));
        public static readonly DependencyProperty RCONParametersProperty = DependencyProperty.Register(nameof(RCONParameters), typeof(RCONParameters), typeof(RCONWindow), new PropertyMetadata(null));
        public static readonly DependencyProperty ScrollOnNewInputProperty = DependencyProperty.Register(nameof(ScrollOnNewInput), typeof(bool), typeof(RCONWindow), new PropertyMetadata(true));
        public static readonly DependencyProperty ServerRCONProperty = DependencyProperty.Register(nameof(ServerRCON), typeof(ServerRCON), typeof(RCONWindow), new PropertyMetadata(null));    

        public RCONWindow(RCONParameters parameters)
        {
            InitializeComponent();
            WindowUtils.RemoveDefaultResourceDictionary(this);

            this.CurrentInputWindowMode = InputWindowMode.None;
            this.PlayerFiltering = (PlayerFilterType)Config.Default.RCON_PlayerListFilter;
            this.PlayerSorting = (PlayerSortType)Config.Default.RCON_PlayerListSort;
            this.RCONParameters = parameters;
            this.ServerRCON = new ServerRCON(parameters);
            this.ServerRCON.RegisterCommandListener(RenderRCONCommandOutput);
            this.ServerRCON.Players.CollectionChanged += Players_CollectionChanged;

            this.PlayersView = CollectionViewSource.GetDefaultView(this.ServerRCON.Players);
            this.PlayersView.Filter = new Predicate<object>(PlayerFilter);

            var notifier = new PropertyChangeNotifier(this.ServerRCON, ServerRCON.StatusProperty, (s, a) =>
            {
                this.RenderConnectionStateChange(a);
            });

            this.DataContext = this;

            AddCommentsBlock(
                _globalizer.GetResourceString("RCON_Comments_Line1"),
                _globalizer.GetResourceString("RCON_Comments_Line2"),
                _globalizer.GetResourceString("RCON_Comments_Line3"),
                _globalizer.GetResourceString("RCON_Comments_Line4"),
                _globalizer.GetResourceString("RCON_Comments_Line5"),
                String.Format(_globalizer.GetResourceString("RCON_Comments_Line6"), 
                _globalizer.GetResourceString("RCON_Help_Keyword")));

            if (this.RCONParameters.RCONWindowExtents.Width > 50 && this.RCONParameters.RCONWindowExtents.Height > 50)
            {
                this.Left = this.RCONParameters.RCONWindowExtents.Left;
                this.Top = this.RCONParameters.RCONWindowExtents.Top;
                this.Width = this.RCONParameters.RCONWindowExtents.Width;
                this.Height = this.RCONParameters.RCONWindowExtents.Height;

                //
                // Fix issues where the console was saved while offscreen.
                if(this.Left == -32000)
                {
                    this.Left = 0;
                }

                if(this.Top == -32000)
                {
                    this.Top = 0;
                }
            }

            this.ConsoleInput.Focus();
        }


        public Config CurrentConfig
        {
            get { return (Config)GetValue(CurrentConfigProperty); }
            set { SetValue(CurrentConfigProperty, value); }
        }

        public InputMode CurrentInputMode
        {
            get { return (InputMode)GetValue(CurrentInputModeProperty); }
            set { SetValue(CurrentInputModeProperty, value); }
        }

        public PlayerFilterType PlayerFiltering
        {
            get { return (PlayerFilterType)GetValue(PlayerFilteringProperty); }
            set { SetValue(PlayerFilteringProperty, value); }
        }

        public string PlayerFilterString
        {
            get { return (string)GetValue(PlayerFilterStringProperty); }
            set { SetValue(PlayerFilterStringProperty, value); }
        }

        public PlayerSortType PlayerSorting
        {
            get { return (PlayerSortType)GetValue(PlayerSortingProperty); }
            set { SetValue(PlayerSortingProperty, value); }
        }

        public ICollectionView PlayersView
        {
            get { return (ICollectionView)GetValue(PlayersViewProperty); }
            set { SetValue(PlayersViewProperty, value); }
        }

        public RCONParameters RCONParameters
        {
            get { return (RCONParameters)GetValue(RCONParametersProperty); }
            set { SetValue(RCONParametersProperty, value); }
        }

        public bool ScrollOnNewInput
        {
            get { return (bool)GetValue(ScrollOnNewInputProperty); }
            set { SetValue(ScrollOnNewInputProperty, value); }
        }

        public ServerRCON ServerRCON
        {
            get { return (ServerRCON)GetValue(ServerRCONProperty); }
            set { SetValue(ServerRCONProperty, value); }
        }


        private InputWindowMode CurrentInputWindowMode
        {
            get;
            set;
        }


        protected override void OnClosing(CancelEventArgs e)
        {
            this.ServerRCON.Players.CollectionChanged -= Players_CollectionChanged;
            this.ServerRCON.DisposeAsync().DoNotWait();
            base.OnClosing(e);
        }

        public static RCONWindow GetRCONForServer(Server server)
        {
            RCONWindow window;
            if(!RCONWindows.TryGetValue(server, out window) || !window.IsLoaded)
            {
                window = new RCONWindow(new RCONParameters()
                {
                    AdminPassword = server.Runtime.ProfileSnapshot.AdminPassword,
                    InstallDirectory = server.Profile.InstallDirectory,
                    AltSaveDirectoryName = server.Profile.AltSaveDirectoryName,
                    ProfileName = server.Profile.ProfileName,
                    RCONHost = server.Runtime.ProfileSnapshot.ServerIP,
                    RCONPort = server.Runtime.ProfileSnapshot.RCONPort,
                    RCONWindowExtents = server.Profile.RCONWindowExtents,
                    MaxPlayers = server.Runtime.MaxPlayers,
                    Server = server
                });
                RCONWindows[server] = window;
            }

            return window;
        }

        public static RCONWindow GetRCON(RCONParameters parameters)
        {
            return new RCONWindow(parameters);
        }

        public static void CloseAllWindows()
        {
            foreach(var window in RCONWindows.Values)
            {
                if(window.IsLoaded)
                {
                    window.Close();
                }
            }
        }

        public ICommand Button1Command
        {
            get
            {
                return new RelayCommand<object>(
                    execute: (_) =>
                    {
                        inputBox.Visibility = Visibility.Collapsed;
                        dockPanel.IsEnabled = true;

                        PlayerInfo player;
                        var inputText = inputTextBox.Text;

                        switch (this.CurrentInputWindowMode)
                        {
                            case InputWindowMode.ServerChatTo:
                                player = inputBox.Tag as PlayerInfo;
                                if (player != null)
                                    this.ServerRCON.IssueCommand($"ServerChatTo \"{player.SteamId}\" {inputText}");
                                break;

                            case InputWindowMode.RenamePlayer:
                                player = inputBox.Tag as PlayerInfo;
                                if (player != null && player.ArkData != null)
                                    this.ServerRCON.IssueCommand($"RenamePlayer \"{player.CharacterName}\" {inputText}");
                                break;

                            case InputWindowMode.RenameTribe:
                                player = inputBox.Tag as PlayerInfo;
                                if (player != null)
                                    this.ServerRCON.IssueCommand($"RenameTribe \"{player.TribeName}\" {inputText}");
                                break;

                            default:
                                break;
                        }

                        // Clear InputBox.
                        inputTextBox.Text = String.Empty;
                        this.CurrentInputWindowMode = InputWindowMode.None;
                    },
                    canExecute: (_) => true
                );
            }
        }

        public ICommand Button2Command
        {
            get
            {
                return new RelayCommand<object>(
                    execute: (_) =>
                    {
                        inputBox.Visibility = Visibility.Collapsed;
                        dockPanel.IsEnabled = true;

                        switch (this.CurrentInputWindowMode)
                        {
                            default:
                                break;
                        }

                        // Clear InputBox.
                        inputTextBox.Text = String.Empty;
                        this.CurrentInputWindowMode = InputWindowMode.None;
                    },
                    canExecute: (_) => true
                );
            }
        }

        public ICommand ClearLogsCommand
        {
            get
            {
                return new RelayCommand<object>(
                    execute: (_) =>
                    {
                        string logsDir = String.Empty;
                        try
                        {
                            logsDir = App.GetProfileLogDir(this.RCONParameters.ProfileName);
                            Directory.Delete(logsDir, true);
                        }
                        catch (Exception)
                        {
                            // Ignore any failures here, best effort only.
                        }

                        MessageBox.Show(String.Format(_globalizer.GetResourceString("RCON_ClearLogs_Label"), logsDir), _globalizer.GetResourceString("RCON_ClearLogs_Title"), MessageBoxButton.OK, MessageBoxImage.Information);
                    },
                    canExecute: (_) => this.RCONParameters.Server != null
                );
            }
        }

        public ICommand ViewLogsCommand
        {
            get
            {
                return new RelayCommand<object>(
                    execute: (_) =>
                    {
                        string logsDir = String.Empty;
                        try
                        {
                            logsDir = App.GetProfileLogDir(this.RCONParameters.ProfileName);
                            Process.Start(logsDir);
                        }
                        catch(Exception ex)
                        {
                            MessageBox.Show(String.Format(_globalizer.GetResourceString("RCON_ViewLogs_ErrorLabel"), logsDir, ex.Message), _globalizer.GetResourceString("RCON_ViewLogs_ErrorTitle"), MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    },
                    canExecute: (_) => this.RCONParameters.Server != null
                );
            }
        }

        public ICommand SaveWorldCommand
        {
            get
            {
                return new RelayCommand<object>(
                    execute: (_) =>
                    {
                        var message = _globalizer.GetResourceString("RCON_SaveWorldLabel");
                        this.ServerRCON.IssueCommand($"broadcast {message}");

                        this.ServerRCON.IssueCommand("saveworld");
                    },
                    canExecute: (_) => true
                );
            }
        }

        public ICommand DestroyWildDinosCommand
        {
            get
            {
                return new RelayCommand<object>(
                    execute: (_) =>
                    {
                        var message = _globalizer.GetResourceString("RCON_DestroyWildDinosLabel");
                        this.ServerRCON.IssueCommand($"broadcast {message}");

                        this.ServerRCON.IssueCommand("DestroyWildDinos");
                    },
                    canExecute: (_) => true
                );
            }
        }

        public ICommand SortPlayersCommand
        {
            get
            {
                return new RelayCommand<PlayerSortType>(
                    execute: (sort) => 
                    {
                        Config.Default.RCON_PlayerListSort = (int)this.PlayerSorting;
                        SortPlayers();
                    },
                    canExecute: (sort) => true
                );
            }
        }       

        public ICommand FilterPlayersCommand
        {
            get
            {
                return new RelayCommand<PlayerFilterType>(
                    execute: (filter) => 
                    {
                        this.PlayerFiltering ^= filter;
                        Config.Default.RCON_PlayerListFilter = (int)this.PlayerFiltering;
                        this.PlayersView.Refresh();
                    },
                    canExecute: (filter) => true
                );
            }
        }

        public ICommand ChatPlayerCommand
        {
            get
            {
                return new RelayCommand<PlayerInfo>(
                    execute: (player) => 
                    {
                        dockPanel.IsEnabled = false;

                        CurrentInputWindowMode = InputWindowMode.ServerChatTo;
                        inputBox.Tag = player;
                        inputTitle.Text = $"{_globalizer.GetResourceString("RCON_ChatPlayerLabel")} {player.SteamName ?? _globalizer.GetResourceString("RCON_UnnamedLabel")}";
                        inputTextBox.Text = string.Empty;
                        button1.Content = _globalizer.GetResourceString("RCON_Button_Send");
                        button2.Content = _globalizer.GetResourceString("RCON_Button_Cancel");
                        inputBox.Visibility = Visibility.Visible;
                    },
                    canExecute: (player) => true
                );
            }
        }

        public ICommand RenamePlayerCommand
        {
            get
            {
                return new RelayCommand<PlayerInfo>(
                    execute: (player) => 
                    {
                        dockPanel.IsEnabled = false;

                        CurrentInputWindowMode = InputWindowMode.RenamePlayer;
                        inputBox.Tag = player;
                        inputTitle.Text = $"{_globalizer.GetResourceString("RCON_RenamePlayerLabel")} {player.SteamName ?? _globalizer.GetResourceString("RCON_UnnamedLabel")}";
                        inputTextBox.Text = string.Empty;
                        button1.Content = _globalizer.GetResourceString("RCON_Button_Change");
                        button2.Content = _globalizer.GetResourceString("RCON_Button_Cancel");
                        inputBox.Visibility = System.Windows.Visibility.Visible;
                    },
                    canExecute: (player) => player != null && player.ArkData != null && player.IsOnline
                );
            }
        }

        public ICommand RenameTribeCommand
        {
            get
            {
                return new RelayCommand<PlayerInfo>(
                    execute: (player) => 
                    {
                        dockPanel.IsEnabled = false;

                        CurrentInputWindowMode = InputWindowMode.RenameTribe;
                        inputBox.Tag = player;
                        inputTitle.Text = $"{_globalizer.GetResourceString("RCON_RenameTribeLabel")} {player.SteamName ?? _globalizer.GetResourceString("RCON_UnnamedLabel")}";
                        inputTextBox.Text = string.Empty;
                        button1.Content = _globalizer.GetResourceString("RCON_Button_Change");
                        button2.Content = _globalizer.GetResourceString("RCON_Button_Cancel");
                        inputBox.Visibility = System.Windows.Visibility.Visible;
                    },
                    canExecute: (player) => player != null && player.IsOnline
                );
            }
        }

        public ICommand KillPlayerCommand
        {
            get
            {
                return new RelayCommand<PlayerInfo>(
                    execute: (player) => { this.ServerRCON.IssueCommand($"KillPlayer {player.ArkData.Id}"); },
                    canExecute: (player) => false //player != null && player.IsOnline
                );
            }
        }

        public ICommand KickPlayerCommand
        {
            get
            {
                return new RelayCommand<PlayerInfo>(
                    execute: (player) => { this.ServerRCON.IssueCommand($"KickPlayer {player.SteamId}"); },
                    canExecute: (player) => player != null && player.IsOnline
                    );
            }
        }

        public ICommand BanPlayerCommand
        {
            get
            {
                return new RelayCommand<PlayerInfo>(
                    execute: (player) => { var command = player.IsBanned ? "unbanplayer" : "banplayer" ;  this.ServerRCON.IssueCommand($"{command} {player.SteamId}"); },
                    canExecute: (player) => true
                    );
            }
        }

        public ICommand WhitelistPlayerCommand
        {
            get
            {
                return new RelayCommand<PlayerInfo>(
                    execute: (player) => { var command = player.IsWhitelisted ? "DisallowPlayerToJoinNoCheck" : "AllowPlayerToJoinNoCheck"; this.ServerRCON.IssueCommand($"{command} {player.SteamId}"); },
                    canExecute: (player) => true
                );
            }
        }

        public ICommand ViewPlayerSteamProfileCommand
        {
            get
            {
                return new RelayCommand<PlayerInfo>(
                    execute: (player) => 
                    {
                        if (player.ArkData == null || string.IsNullOrWhiteSpace(player.ArkData.ProfileUrl))
                            Process.Start($"http://steamcommunity.com/profiles/{player.SteamId}");
                        else
                            Process.Start(player.ArkData.ProfileUrl);
                    },
                    canExecute: (player) => player != null
                );
            }
        }

        public ICommand ViewPlayerProfileCommand
        {
            get
            {
                return new RelayCommand<PlayerInfo>(
                    execute: (player) => {
                        var savedArksPath = !string.IsNullOrWhiteSpace(this.RCONParameters.AltSaveDirectoryName)
                                                ? Path.Combine(this.RCONParameters.InstallDirectory, Config.Default.SavedRelativePath, this.RCONParameters.AltSaveDirectoryName)
                                                : Path.Combine(this.RCONParameters.InstallDirectory, Config.Default.SavedArksRelativePath);
                        var window = new PlayerProfileWindow(player, savedArksPath);
                        window.Owner = this;
                        window.ShowDialog();
                    },
                    canExecute: (player) => player != null && player.ArkData != null
                    );
            }
        }

        public ICommand ViewPlayerTribeCommand
        {
            get
            {
                return new RelayCommand<PlayerInfo>(
                    execute: (player) => {
                        var savedArksPath = !string.IsNullOrWhiteSpace(this.RCONParameters.AltSaveDirectoryName)
                                                ? Path.Combine(this.RCONParameters.InstallDirectory, Config.Default.SavedRelativePath, this.RCONParameters.AltSaveDirectoryName)
                                                : Path.Combine(this.RCONParameters.InstallDirectory, Config.Default.SavedArksRelativePath);
                        var window = new TribeProfileWindow(player, this.ServerRCON.Players, savedArksPath);
                        window.Owner = this;
                        window.ShowDialog();
                    },
                    canExecute: (player) => player != null && player.ArkData != null && !String.IsNullOrWhiteSpace(player.TribeName)
                    );
            }
        }

        public ICommand CopySteamIDCommand
        {
            get
            {
                return new RelayCommand<PlayerInfo>(
                    execute: (player) => 
                    {
                        try
                        {
                            System.Windows.Clipboard.SetText(player.SteamId.ToString());
                            MessageBox.Show($"{_globalizer.GetResourceString("RCON_CopySteamIdLabel")} {player.SteamName}", _globalizer.GetResourceString("RCON_CopySteamIdTitle"), MessageBoxButton.OK);
                        }
                        catch
                        {
                            MessageBox.Show($"{_globalizer.GetResourceString("RCON_ClipboardErrorLabel")}", _globalizer.GetResourceString("RCON_ClipboardErrorTitle"), MessageBoxButton.OK);
                        }
                    },
                    canExecute: (player) => player != null
                    );

            }
        }

        public ICommand CopyPlayerIDCommand
        {
            get
            {
                return new RelayCommand<PlayerInfo>(
                    execute: (player) => 
                    {
                        if (player.ArkData != null)
                        {
                            try
                            {
                                System.Windows.Clipboard.SetText(player.ArkData.Id.ToString());
                                MessageBox.Show($"{_globalizer.GetResourceString("RCON_CopyPlayerIdLabel")} {player.SteamName}", _globalizer.GetResourceString("RCON_CopyPlayerIdTitle"), MessageBoxButton.OK);
                            }
                            catch
                            {
                                MessageBox.Show($"{_globalizer.GetResourceString("RCON_ClipboardErrorLabel")}", _globalizer.GetResourceString("RCON_ClipboardErrorTitle"), MessageBoxButton.OK);
                            }
                        }
                    },
                    canExecute: (player) => player != null && player.ArkData != null
                    );

            }
        }

        private void RenderConnectionStateChange(DependencyPropertyChangedEventArgs a)
        {
            var oldStatus = (ServerRCON.ConsoleStatus)a.OldValue;
            var newStatus = (ServerRCON.ConsoleStatus)a.NewValue;
            if(oldStatus != newStatus)
            {
                Paragraph p = new Paragraph();
                if (newStatus == ServerRCON.ConsoleStatus.Connected)
                {
                    p.Inlines.Add(new RCONOutput_ConnectionChanged(true));
                }
                else
                {
                    p.Inlines.Add(new RCONOutput_ConnectionChanged(false));
                }

                AddBlockContent(p);
            }
        }

        private void RenderRCONCommandOutput(ServerRCON.ConsoleCommand command)
        {
            //
            // Format output
            //
            Paragraph p = new Paragraph();

            if (!command.suppressCommand)
            {
                foreach (var element in FormatCommandInput(command))
                {
                    p.Inlines.Add(element);
                }
            }

            if (!command.suppressOutput)
            {
                foreach (var element in FormatCommandOutput(command))
                {
                    p.Inlines.Add(element);
                }
            }

            if (!(command.suppressCommand && command.suppressOutput))
            {
                if (p.Inlines.Count > 0)
                {
                    AddBlockContent(p);
                }
            }
        }

        private void AddBlockContent(Block b)
        {
            ConsoleContent.Blocks.Add(b);            
        }

        private IEnumerable<Inline> FormatCommandInput(ServerRCON.ConsoleCommand command)
        {
            if (command.command.Equals("broadcast", StringComparison.OrdinalIgnoreCase))
            {
                yield return new RCONOutput_Broadcast(command.args);
            }
            else
            {
                yield return new RCONOutput_Command($"> {command.rawCommand}");
            }

            if(!command.suppressOutput && command.lines.Count() > 0)
            {
                yield return new LineBreak();
            }
        }

        private void AddCommentsBlock(params string[] lines)
        {
            var p = new Paragraph();
            bool firstLine = true;

            foreach (var output in lines)
            {
                var trimmed = output.TrimEnd();
                if (!firstLine)
                {
                    p.Inlines.Add(new LineBreak());                    
                }

                firstLine = false;

                p.Inlines.Add(new RCONOutput_Comment(output));
            }

            AddBlockContent(p);
        }

        private IEnumerable<Inline> FormatCommandOutput(ServerRCON.ConsoleCommand command)
        {
            bool firstLine = true;
            
            foreach (var output in command.lines)
            {
                var trimmed = output.TrimEnd();
                if(!firstLine)
                {
                    yield return new LineBreak();
                }
                firstLine = false;

                if (output == ServerRCON.NoResponseOutput)
                {
                    yield return new RCONOutput_NoResponse();
                }
                else
                {
                    yield return new RCONOutput_CommandOutput(trimmed);
                }
            }
        }

        private void ConsoleInput_KeyUp(object sender, KeyEventArgs e)
        {            
            if(e.Key == Key.Enter)
            {
                var textBox = (TextBox)sender;
                var effectiveMode = this.CurrentInputMode;
                var commandText = textBox.Text.Trim();
                if (commandText.StartsWith(_globalizer.GetResourceString("RCON_Help_Keyword")))
                {
                    AddCommentsBlock(
                        _globalizer.GetResourceString("RCON_Help_Line1"),
                        "   " + _globalizer.GetResourceString("RCON_Help_Line2"),
                        "   " + _globalizer.GetResourceString("RCON_Help_Line3"),
                        "   " + _globalizer.GetResourceString("RCON_Help_Line4"),
                        "   " + _globalizer.GetResourceString("RCON_Help_Line5"),
                        "   " + _globalizer.GetResourceString("RCON_Help_Line6"),
                        "   " + _globalizer.GetResourceString("RCON_Help_Line7"),
                        "   " + _globalizer.GetResourceString("RCON_Help_Line8"),
                        "   " + _globalizer.GetResourceString("RCON_Help_Line9"),
                        "   " + _globalizer.GetResourceString("RCON_Help_Line10"),
                        "   " + _globalizer.GetResourceString("RCON_Help_Line11"),
                        "   " + _globalizer.GetResourceString("RCON_Help_Line12"),
                        "   " + _globalizer.GetResourceString("RCON_Help_Line13"),
                        "   " + _globalizer.GetResourceString("RCON_Help_Line14"),
                        "   " + _globalizer.GetResourceString("RCON_Help_Line15"),
                        "   " + _globalizer.GetResourceString("RCON_Help_Line16"),
                        "   " + _globalizer.GetResourceString("RCON_Help_Line17"),
                        "   " + _globalizer.GetResourceString("RCON_Help_Line18"),
                        "   " + _globalizer.GetResourceString("RCON_Help_Line19"),
                        "   " + _globalizer.GetResourceString("RCON_Help_Line20"),
                        "   " + _globalizer.GetResourceString("RCON_Help_Line21"),
                        "   " + _globalizer.GetResourceString("RCON_Help_Line22"),
                        "   " + _globalizer.GetResourceString("RCON_Help_Line23"),
                        "   " + _globalizer.GetResourceString("RCON_Help_Line24"),
                        "   " + _globalizer.GetResourceString("RCON_Help_Line25"),
                        "   " + _globalizer.GetResourceString("RCON_Help_Line26"),
                        "   " + _globalizer.GetResourceString("RCON_Help_Line27"),
                        "   " + _globalizer.GetResourceString("RCON_Help_Line28"),
                        _globalizer.GetResourceString("RCON_Help_Line29"),
                        "   " + _globalizer.GetResourceString("RCON_Help_Line30"),
                        "   " + _globalizer.GetResourceString("RCON_Help_Line31"),
                        "   " + _globalizer.GetResourceString("RCON_Help_Line32")
                        );
                }
                else
                {
                    if (commandText.StartsWith("/"))
                    {
                        effectiveMode = InputMode.Command;
                        commandText = commandText.Substring(1);
                    }

                    switch (effectiveMode)
                    {
                        case InputMode.Broadcast:
                            this.ServerRCON.IssueCommand($"broadcast {commandText}");
                            break;

                        case InputMode.Global:
                            if (!String.IsNullOrWhiteSpace(Config.Default.RCON_AdminName))
                            {
                                this.ServerRCON.IssueCommand($"serverchat [{Config.Default.RCON_AdminName}] {commandText}");
                            }
                            else
                            {
                                this.ServerRCON.IssueCommand($"serverchat {commandText}");
                            }
                            break;

                        case InputMode.Command:
                            this.ServerRCON.IssueCommand(commandText);
                            break;

#if false
                    case InputMode.Chat:
                        this.ServerRCON.IssueCommand(textBox.Text);
                        break;
#endif
                    }
                }

                textBox.Text = String.Empty;
            }
        }

        private void RCON_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (this.WindowState != WindowState.Minimized)
            {
                Rect savedRect = this.RCONParameters.RCONWindowExtents;
                this.RCONParameters.RCONWindowExtents = new Rect(savedRect.Location, e.NewSize);
            }
        }

        private void RCON_LocationChanged(object sender, EventArgs e)
        {
            if (this.WindowState != WindowState.Minimized && this.Left != -32000 && this.Top != -32000)
            {
                Rect savedRect = this.RCONParameters.RCONWindowExtents;
                this.RCONParameters.RCONWindowExtents = new Rect(new Point(this.Left, this.Top), savedRect.Size);
                if (this.RCONParameters.Server != null)
                {
                    this.RCONParameters.Server.Profile.RCONWindowExtents = this.RCONParameters.RCONWindowExtents;
                }
            }
        }

        private void Players_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            SortPlayers();
            PlayersView?.Refresh();
        }

        private void PlayerFilter_SourceUpdated(object sender, DataTransferEventArgs e)
        {
            PlayersView?.Refresh();
        }

        public bool PlayerFilter(object obj)
        {
            var player = obj as PlayerInfo;
            if (player == null)
                return false;

            var result = (this.PlayerFiltering.HasFlag(PlayerFilterType.Online) && player.IsOnline) ||
                         (this.PlayerFiltering.HasFlag(PlayerFilterType.Offline) && !player.IsOnline) ||
                         (this.PlayerFiltering.HasFlag(PlayerFilterType.Banned) && player.IsBanned) ||
                         (this.PlayerFiltering.HasFlag(PlayerFilterType.Whitelisted) && player.IsWhitelisted);
            if (!result)
                return false;

            var filterString = PlayerFilterString.ToLower();

            if (string.IsNullOrWhiteSpace(filterString))
                return true;

            result = player.SteamNameFilterString != null && player.SteamNameFilterString.Contains(filterString) || 
                    player.TribeNameFilterString != null && player.TribeNameFilterString.Contains(filterString) ||
                    player.CharacterNameFilterString !=null && player.CharacterNameFilterString.Contains(filterString);

            return result;
        }

        public void SortPlayers()
        {
            this.PlayersView.SortDescriptions.Clear();

            switch (this.PlayerSorting)
            {
                case PlayerSortType.Name:
                    this.PlayersView.SortDescriptions.Add(new SortDescription(nameof(PlayerInfo.SteamName), ListSortDirection.Ascending));
                    break;

                case PlayerSortType.Online:
                    this.PlayersView.SortDescriptions.Add(new SortDescription(nameof(PlayerInfo.IsOnline), ListSortDirection.Descending));
                    this.PlayersView.SortDescriptions.Add(new SortDescription(nameof(PlayerInfo.SteamName), ListSortDirection.Ascending));
                    break;

                case PlayerSortType.Tribe:
                    this.PlayersView.SortDescriptions.Add(new SortDescription(nameof(PlayerInfo.TribeName), ListSortDirection.Ascending));
                    this.PlayersView.SortDescriptions.Add(new SortDescription(nameof(PlayerInfo.SteamName), ListSortDirection.Ascending));
                    break;

                case PlayerSortType.LastUpdated:
                    this.PlayersView.SortDescriptions.Add(new SortDescription(nameof(PlayerInfo.LastUpdated), ListSortDirection.Descending));
                    this.PlayersView.SortDescriptions.Add(new SortDescription(nameof(PlayerInfo.SteamName), ListSortDirection.Ascending));
                    break;
            }
        }
    }
}
