﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace InstantSnip {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "14.0.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string SnipName {
            get {
                return ((string)(this["SnipName"]));
            }
            set {
                this["SnipName"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string SnipLocation {
            get {
                return ((string)(this["SnipLocation"]));
            }
            set {
                this["SnipLocation"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public bool AllowSnipOverwriting {
            get {
                return ((bool)(this["AllowSnipOverwriting"]));
            }
            set {
                this["AllowSnipOverwriting"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool AllowDeletingPictureAfterSnipping {
            get {
                return ((bool)(this["AllowDeletingPictureAfterSnipping"]));
            }
            set {
                this["AllowDeletingPictureAfterSnipping"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0")]
        public double TimeBeforeDeletingPicture {
            get {
                return ((double)(this["TimeBeforeDeletingPicture"]));
            }
            set {
                this["TimeBeforeDeletingPicture"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public bool IsCopyImageToClipBoard {
            get {
                return ((bool)(this["IsCopyImageToClipBoard"]));
            }
            set {
                this["IsCopyImageToClipBoard"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public bool IsCopyUriToClipboard {
            get {
                return ((bool)(this["IsCopyUriToClipboard"]));
            }
            set {
                this["IsCopyUriToClipboard"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool IsFirstRun {
            get {
                return ((bool)(this["IsFirstRun"]));
            }
            set {
                this["IsFirstRun"] = value;
            }
        }
    }
}
