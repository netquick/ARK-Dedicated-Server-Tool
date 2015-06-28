﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34209
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ARK_Server_Manager {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "12.0.0.0")]
    public sealed partial class Config : global::System.Configuration.ApplicationSettingsBase {
        
        private static Config defaultInstance = ((Config)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Config())));
        
        public static Config Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("https://www.dropbox.com/s/a6v1obnqigu2bpu/version.txt?dl=1")]
        public string DefaultVersionCheckUrl {
            get {
                return ((string)(this["DefaultVersionCheckUrl"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("http://hellsguard.site11.com/ARK_Server_Tool/updater.zip")]
        public string DefaultUpdatePackageUrl {
            get {
                return ((string)(this["DefaultUpdatePackageUrl"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("https://steamcdn-a.akamaihd.net/client/installer/steamcmd.zip")]
        public string SteamCmdUrl {
            get {
                return ((string)(this["SteamCmdUrl"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("SteamCMD.exe")]
        public string SteamCmdExe {
            get {
                return ((string)(this["SteamCmdExe"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("SteamCMD")]
        public string SteamCmdDir {
            get {
                return ((string)(this["SteamCmdDir"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("SteamCMD.zip")]
        public string SteamCmdZip {
            get {
                return ((string)(this["SteamCmdZip"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("+login anonymous +quit")]
        public string SteamCmdInstallArgs {
            get {
                return ((string)(this["SteamCmdInstallArgs"]));
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string ConfigDirectory {
            get {
                return ((string)(this["ConfigDirectory"]));
            }
            set {
                this["ConfigDirectory"] = value;
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("+login anonymous +force_install_dir \"{0}\"  \"+app_update 376030\" +quit")]
        public string SteamCmdInstallServerArgsFormat {
            get {
                return ((string)(this["SteamCmdInstallServerArgsFormat"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Servers")]
        public string ServersInstallDir {
            get {
                return ((string)(this["ServersInstallDir"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Unnamed Server")]
        public string DefaultServerProfileName {
            get {
                return ((string)(this["DefaultServerProfileName"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Unnamed Server")]
        public string DefaultServerName {
            get {
                return ((string)(this["DefaultServerName"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("ShooterGame\\Binaries\\Win64")]
        public string ServerBinaryRelativePath {
            get {
                return ((string)(this["ServerBinaryRelativePath"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("ShooterGame\\Saved\\Config\\WindowsServer")]
        public string ServerConfigRelativePath {
            get {
                return ((string)(this["ServerConfigRelativePath"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("ShooterGameServer.exe")]
        public string ServerExe {
            get {
                return ((string)(this["ServerExe"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("?QueryPort={0}")]
        public string ServerCommandLineArgsMatchFormat {
            get {
                return ((string)(this["ServerCommandLineArgsMatchFormat"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("-nosteamclient -game -server -log")]
        public string ServerCommandLineStandardArgs {
            get {
                return ((string)(this["ServerCommandLineStandardArgs"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Profiles")]
        public string ProfilesDir {
            get {
                return ((string)(this["ProfilesDir"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute(".profile")]
        public string ProfileExtension {
            get {
                return ((string)(this["ProfileExtension"]));
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("TheIsland")]
        public string DefaultServerMap {
            get {
                return ((string)(this["DefaultServerMap"]));
            }
            set {
                this["DefaultServerMap"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string DataDir {
            get {
                return ((string)(this["DataDir"]));
            }
            set {
                this["DataDir"] = value;
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Ark Server Manager")]
        public string DefaultDataDir {
            get {
                return ((string)(this["DefaultDataDir"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("ShooterGameServer")]
        public string ServerProcessName {
            get {
                return ((string)(this["ServerProcessName"]));
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("GameUserSettings.ini")]
        public string ServerGameUserSettingsFile {
            get {
                return ((string)(this["ServerGameUserSettingsFile"]));
            }
            set {
                this["ServerGameUserSettingsFile"] = value;
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute(".ini")]
        public string IniExtension {
            get {
                return ((string)(this["IniExtension"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("*.profile,*.ini")]
        public string LoadProfileExtensionList {
            get {
                return ((string)(this["LoadProfileExtensionList"]));
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string MachinePublicIP {
            get {
                return ((string)(this["MachinePublicIP"]));
            }
            set {
                this["MachinePublicIP"] = value;
            }
        }
    }
}
