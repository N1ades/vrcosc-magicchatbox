﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using vrcosc_magicchatbox.Classes;
using vrcosc_magicchatbox.Classes.DataAndSecurity;

namespace vrcosc_magicchatbox.ViewModels
{
    public class ViewModel : INotifyPropertyChanged
    {
        public static readonly ViewModel Instance = new ViewModel();

        private HeartRateConnector _heartRateConnector;

        #region ICommand's
        public ICommand ActivateStatusCommand { get; set; }
        public ICommand ToggleVoiceCommand { get; }
        public ICommand SortScannedAppsByProcessNameCommand { get; }
        public ICommand SortScannedAppsByFocusCountCommand { get; }
        public ICommand SortScannedAppsByUsedNewMethodCommand { get; }
        public ICommand SortScannedAppsByIsPrivateAppCommand { get; }
        public ICommand SortScannedAppsByApplyCustomAppNameCommand { get; }
        public RelayCommand<string> ActivateSettingCommand { get; }

        #endregion

        public Dictionary<Timezone, string> TimezoneFriendlyNames { get; }
        public Dictionary<string, Action<bool>> SettingsMap;
        public ViewModel()
        {
            ActivateStatusCommand = new RelayCommand(ActivateStatus);
            ToggleVoiceCommand = new RelayCommand(ToggleVoice);
            SortScannedAppsByProcessNameCommand = new RelayCommand(() => SortScannedApps(SortProperty.ProcessName));
            SortScannedAppsByFocusCountCommand = new RelayCommand(() => SortScannedApps(SortProperty.FocusCount));
            SortScannedAppsByUsedNewMethodCommand = new RelayCommand(() => SortScannedApps(SortProperty.UsedNewMethod));
            SortScannedAppsByIsPrivateAppCommand = new RelayCommand(() => SortScannedApps(SortProperty.IsPrivateApp));
            SortScannedAppsByApplyCustomAppNameCommand = new RelayCommand(() => SortScannedApps(SortProperty.ApplyCustomAppName));
            ActivateSettingCommand = new RelayCommand<string>(ActivateSetting);
            TimezoneFriendlyNames = new Dictionary<Timezone, string>
        {
            { Timezone.UTC, "Coordinated Universal Time (UTC)" },
            { Timezone.EST, "Eastern Standard Time (EST)" },
            { Timezone.CST, "Central Standard Time (CST)" },
            { Timezone.PST, "Pacific Standard Time (PST)" },
            { Timezone.CET, "European Central Time (CET)" },
            { Timezone.AEST, "Australian Eastern Standard Time (AEST)" },
        };
            SettingsMap = new Dictionary<string, Action<bool>>
    {
        { nameof(Settings_WindowActivity), value => Settings_WindowActivity = value },
        { nameof(Settings_IntelliChat), value => Settings_IntelliChat = value },
        { nameof(Settings_Spotify), value => Settings_Spotify = value },
        { nameof(Settings_Chatting), value => Settings_Chatting = value },
        { nameof(Settings_AppOptions), value => Settings_AppOptions = value },
        { nameof(Settings_TTS), value => Settings_TTS = value },
        { nameof(Settings_Time), value => Settings_Time = value },
        { nameof(Settings_HeartRate), value => Settings_HeartRate = value },
        { nameof(Settings_Status), value => Settings_Status = value }
    };
            _heartRateConnector = new HeartRateConnector();
            PropertyChanged += _heartRateConnector.PropertyChangedHandler;
        }

        public void ActivateSetting(string settingName)
        {
            if (SettingsMap.ContainsKey(settingName))
            {
                foreach (var setting in SettingsMap)
                {
                    setting.Value(setting.Key == settingName);
                }
                MainWindow.ChangeMenuItem(3);
            }
        }

        public void SortScannedApps(SortProperty sortProperty)
        {
            var isAscending = _sortDirection[sortProperty];
            _sortDirection[sortProperty] = !isAscending;

            IOrderedEnumerable<ProcessInfo> sortedScannedApps;

            switch (sortProperty)
            {
                case SortProperty.ProcessName:
                    sortedScannedApps = isAscending
                        ? _ScannedApps.OrderBy(process => process.ProcessName)
                        : _ScannedApps.OrderByDescending(process => process.ProcessName);
                    break;

                case SortProperty.UsedNewMethod:
                    sortedScannedApps = isAscending
                        ? _ScannedApps.OrderBy(process => process.UsedNewMethod)
                        : _ScannedApps.OrderByDescending(process => process.UsedNewMethod);
                    break;

                case SortProperty.ApplyCustomAppName:
                    sortedScannedApps = isAscending
                        ? _ScannedApps.OrderBy(process => process.ApplyCustomAppName)
                        : _ScannedApps.OrderByDescending(process => process.ApplyCustomAppName);
                    break;

                case SortProperty.FocusCount:
                    sortedScannedApps = isAscending
                        ? _ScannedApps.OrderBy(process => process.FocusCount)
                        : _ScannedApps.OrderByDescending(process => process.FocusCount);
                    break;
                case SortProperty.IsPrivateApp:
                    sortedScannedApps = isAscending
                        ? _ScannedApps.OrderBy(process => process.IsPrivateApp)
                        : _ScannedApps.OrderByDescending(process => process.IsPrivateApp);
                    break;

                default:
                    throw new ArgumentException($"Invalid sort property: {sortProperty}");
            }

            ScannedApps = new ObservableCollection<ProcessInfo>(sortedScannedApps);
        }

        private void ToggleVoice()
        {
            if (Instance.ToggleVoiceWithV)
                OSCController.ToggleVoice(true);
        }


        private ObservableCollection<MediaSessionInfo> _MediaSessions = new ObservableCollection<MediaSessionInfo>();
        public ObservableCollection<MediaSessionInfo> MediaSessions
        {
            get { return _MediaSessions; }
            set
            {
                _MediaSessions = value;
                NotifyPropertyChanged(nameof(MediaSessions));
            }
        }

        private bool _IntgrWindowActivity_DESKTOP = true;
        public bool IntgrWindowActivity_DESKTOP
        {
            get { return _IntgrWindowActivity_DESKTOP; }
            set
            {
                _IntgrWindowActivity_DESKTOP = value;
                NotifyPropertyChanged(nameof(IntgrWindowActivity_DESKTOP));
            }
        }


        private bool _IntgrSpotifyStatus_DESKTOP = true;
        public bool IntgrSpotifyStatus_DESKTOP
        {
            get { return _IntgrSpotifyStatus_DESKTOP; }
            set
            {
                _IntgrSpotifyStatus_DESKTOP = value;
                NotifyPropertyChanged(nameof(IntgrSpotifyStatus_DESKTOP));
            }
        }

        private bool _IntgrSpotifyStatus_VR = true;
        public bool IntgrSpotifyStatus_VR
        {
            get { return _IntgrSpotifyStatus_VR; }
            set
            {
                _IntgrSpotifyStatus_VR = value;
                NotifyPropertyChanged(nameof(IntgrSpotifyStatus_VR));
            }
        }

        private bool _IntgrCurrentTime_DESKTOP = false;
        public bool IntgrCurrentTime_DESKTOP
        {
            get { return _IntgrCurrentTime_DESKTOP; }
            set
            {
                _IntgrCurrentTime_DESKTOP = value;
                NotifyPropertyChanged(nameof(IntgrCurrentTime_DESKTOP));
            }
        }

        private bool _IntgrCurrentTime_VR = true;
        public bool IntgrCurrentTime_VR
        {
            get { return _IntgrCurrentTime_VR; }
            set
            {
                _IntgrCurrentTime_VR = value;
                NotifyPropertyChanged(nameof(IntgrCurrentTime_VR));
            }
        }

        private bool _IntgrHeartRate_VR = true;
        public bool IntgrHeartRate_VR
        {
            get { return _IntgrHeartRate_VR; }
            set
            {
                _IntgrHeartRate_VR = value;
                NotifyPropertyChanged(nameof(IntgrHeartRate_VR));
            }
        }


        private bool _IntgrHeartRate_DESKTOP = false;
        public bool IntgrHeartRate_DESKTOP
        {
            get { return _IntgrHeartRate_DESKTOP; }
            set
            {
                _IntgrHeartRate_DESKTOP = value;
                NotifyPropertyChanged(nameof(IntgrHeartRate_DESKTOP));
            }
        }


        private bool _IntgrScanMediaLink = false;
        public bool IntgrScanMediaLink
        {
            get { return _IntgrScanMediaLink; }
            set
            {
                _IntgrScanMediaLink = value;
                NotifyPropertyChanged(nameof(IntgrScanMediaLink));
            }
        }

        private bool _IntgrWindowActivity_VR = false;
        public bool IntgrWindowActivity_VR
        {
            get { return _IntgrWindowActivity_VR; }
            set
            {
                _IntgrWindowActivity_VR = value;
                NotifyPropertyChanged(nameof(IntgrWindowActivity_VR));
            }
        }

        private bool _IntgrStatus_VR = true;
        public bool IntgrStatus_VR
        {
            get { return _IntgrStatus_VR; }
            set
            {
                _IntgrStatus_VR = value;
                NotifyPropertyChanged(nameof(IntgrStatus_VR));
            }
        }

        private bool _IntgrStatus_DESKTOP = true;
        public bool IntgrStatus_DESKTOP
        {
            get { return _IntgrStatus_DESKTOP; }
            set
            {
                _IntgrStatus_DESKTOP = value;
                NotifyPropertyChanged(nameof(IntgrStatus_DESKTOP));
            }
        }

        public static void ActivateStatus(object parameter)
        {
            try
            {
                var item = parameter as StatusItem;
                foreach (var i in ViewModel.Instance.StatusList)
                {
                    if (i == item)
                    {
                        i.IsActive = true;
                        i.LastUsed = DateTime.Now;

                    }
                    else
                    {
                        i.IsActive = false;
                    }
                }
                SaveStatusList();
            }
            catch (Exception ex)
            {
                Logging.WriteException(ex, makeVMDump: true, MSGBox: false);
            }

        }

        public void ScannedAppsItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "FocusCount")
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    CollectionViewSource.GetDefaultView(ScannedApps).Refresh();
                });
            }
        }

        public static void SaveStatusList()
        {
            try
            {
                if (CreateIfMissing(ViewModel.Instance.DataPath) == true)
                {
                    string json = JsonConvert.SerializeObject(ViewModel.Instance.StatusList);
                    File.WriteAllText(Path.Combine(ViewModel.Instance.DataPath, "StatusList.xml"), json);
                }

            }
            catch (Exception ex)
            {
                Logging.WriteException(ex, makeVMDump: false, MSGBox: false);
            }

        }
        public static bool CreateIfMissing(string path)
        {
            try
            {
                if (!Directory.Exists(path))
                {
                    DirectoryInfo di = Directory.CreateDirectory(path);
                    return true;
                }
                return true;
            }
            catch (IOException ex)
            {
                Logging.WriteException(ex, makeVMDump: false, MSGBox: false);
                return false;
            }

        }
        private void UpdateToggleVoiceText()
        {
            ToggleVoiceText = ToggleVoiceWithV ? "Toggle voice (V)" : "Toggle voice";
        }

        #region Properties     

        private ObservableCollection<StatusItem> _StatusList = new ObservableCollection<StatusItem>();
        private ObservableCollection<ChatItem> _LastMessages = new ObservableCollection<ChatItem>();
        private string _aesKey = "g5X5pFei6G8W6UwK6UaA6YhC6U8W6ZbP";
        private string _PlayingSongTitle = "";
        private bool _ScanPause = false;
        private bool _Topmost = false;
        private int _ScanPauseTimeout = 25;
        private int _ScanPauseCountDown = 0;
        private string _NewStatusItemTxt = "";
        private string _NewChattingTxt = "";
        private string _ChatFeedbackTxt = "";
        private string _FocusedWindow = "";
        private string _StatusTopBarTxt = "";
        private string _ChatTopBarTxt = "";
        private bool _SpotifyActive = false;
        private bool _SpotifyPaused = false;
        private bool _IsVRRunning = false;
        private bool _MasterSwitch = true;
        private bool _PrefixTime = false;
        private bool _PrefixChat = true;
        private bool _ChatFX = true;
        private bool _TypingIndicator = false;
        private bool _PrefixIconMusic = true;
        private bool _PauseIconMusic = true;
        private bool _PrefixIconStatus = true;
        private bool _CountDownUI = true;
        private bool _Time24H = false;
        private string _OSCtoSent = "";
        private string _ApiStream = "b2t8DhYcLcu7Nu0suPcvc8lO27wztrjMPbb + 8hQ1WPba2dq / iRyYpBEDZ0NuMNKR5GRrF2XdfANLud0zihG / UD + ewVl1p3VLNk1mrNdrdg88rguzi6RJ7T1AA7hyBY + F";
        private Version _AppVersion = new("0.7.1");
        private Version _GitHubVersion;
        private string _VersionTxt = "Check for updates";
        private string _VersionTxtColor = "#FF8F80B9";
        private string _StatusBoxCount = "0/140";
        private string _StatusBoxColor = "#FF504767";
        private string _ChatBoxCount = "0/140";
        private string _ChatBoxColor = "#FF504767";
        private string _CurrentTime = "";
        private string _ActiveChatTxt = "";
        private bool _IntgrStatus = true;
        private bool _IntgrScanWindowActivity = false;
        private bool _IntgrScanWindowTime = true;
        private bool _IntgrScanSpotify = true;
        private int _ScanInterval = 4;
        private int _CurrentMenuItem = 0;
        private string _MenuItem_0_Visibility = "Hidden";
        private string _MenuItem_1_Visibility = "Hidden";
        private string _MenuItem_2_Visibility = "Hidden";
        private string _MenuItem_3_Visibility = "Visible";
        private int _OSCmsg_count = 0;
        private string _OSCmsg_countUI = "";
        private string _OSCIP = "127.0.0.1";
        private string _Char_Limit = "Hidden";
        private string _Spotify_Opacity = "1";
        private string _Status_Opacity = "1";
        private string _Window_Opacity = "1";
        private string _Time_Opacity = "1";
        private string _HeartRate_Opacity = "1";
        private int _OSCPortOut = 9000;
        private string _DataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Vrcosc-MagicChatbox");
        private List<Voice> _TikTokTTSVoices;
        private Voice _SelectedTikTokTTSVoice;
        private bool _TTSTikTokEnabled = false;
        private AudioDevice _selectedAuxOutputDevice;
        private AudioDevice _selectedPlaybackOutputDevice;
        private List<AudioDevice> _playbackOutputDevices = new List<AudioDevice>();
        private List<AudioDevice> _auxOutputDevices = new List<AudioDevice>();
        private bool _TTSCutOff = true;
        private string _LogPath = @"C:\temp\Vrcosc-MagicChatbox";
        private string _RecentPlayBackOutput;
        private bool _VrcConnected;
        private string _NewVersionURL;
        private bool _CanUpdate;
        private string _toggleVoiceText = "Toggle voice (V)";
        private bool _AutoUnmuteTTS = true;
        private bool _ToggleVoiceWithV = true;
        private bool _TTSBtnShadow = false;
        private float _TTSVolume = 0.2f;

        private ProcessInfo _LastProcessFocused = new ProcessInfo();
        private Dictionary<SortProperty, bool> _sortDirection = new Dictionary<SortProperty, bool>
        {
            { SortProperty.ProcessName, true },
            { SortProperty.UsedNewMethod, true },
            { SortProperty.ApplyCustomAppName, true },
            { SortProperty.IsPrivateApp, true },
            { SortProperty.FocusCount, true }
        };
        public enum SortProperty
        {
            ProcessName,
            UsedNewMethod,
            ApplyCustomAppName,
            IsPrivateApp,
            FocusCount
        }




        private int _HeartRateScanInterval = 3;
        public int HeartRateScanInterval
        {
            get { return _HeartRateScanInterval; }
            set
            {
                _HeartRateScanInterval = value;
                NotifyPropertyChanged(nameof(HeartRateScanInterval));
            }
        }

        private DateTime _HeartRateLastUpdate = DateTime.Now;
        public DateTime HeartRateLastUpdate
        {
            get { return _HeartRateLastUpdate; }
            set
            {
                _HeartRateLastUpdate = value;
                NotifyPropertyChanged(nameof(HeartRateLastUpdate));
            }
        }

        private bool _AutoSetDaylight = true;
        public bool AutoSetDaylight
        {
            get { return _AutoSetDaylight; }
            set
            {
                _AutoSetDaylight = value;
                NotifyPropertyChanged(nameof(AutoSetDaylight));
            }
        }


        private bool _UseDaylightSavingTime = false;
        public bool UseDaylightSavingTime
        {
            get { return _UseDaylightSavingTime; }
            set
            {
                _UseDaylightSavingTime = value;
                NotifyPropertyChanged(nameof(UseDaylightSavingTime));
            }
        }

        private bool _Settings_WindowActivity = false;
        public bool Settings_WindowActivity
        {
            get { return _Settings_WindowActivity; }
            set
            {
                _Settings_WindowActivity = value;
                NotifyPropertyChanged(nameof(Settings_WindowActivity));
            }
        }

        private bool _Settings_IntelliChat = false;
        public bool Settings_IntelliChat
        {
            get { return _Settings_IntelliChat; }
            set
            {
                _Settings_IntelliChat = value;
                NotifyPropertyChanged(nameof(Settings_IntelliChat));
            }
        }

        private bool _Settings_Spotify = false;
        public bool Settings_Spotify
        {
            get { return _Settings_Spotify; }
            set
            {
                _Settings_Spotify = value;
                NotifyPropertyChanged(nameof(Settings_Spotify));
            }
        }

        private bool _Settings_Chatting = false;
        public bool Settings_Chatting
        {
            get { return _Settings_Chatting; }
            set
            {
                _Settings_Chatting = value;
                NotifyPropertyChanged(nameof(Settings_Chatting));
            }
        }


        private bool _Settings_AppOptions = false;
        public bool Settings_AppOptions
        {
            get { return _Settings_AppOptions; }
            set
            {
                _Settings_AppOptions = value;
                NotifyPropertyChanged(nameof(Settings_AppOptions));
            }
        }

        private bool _Settings_TTS = false;
        public bool Settings_TTS
        {
            get { return _Settings_TTS; }
            set
            {
                _Settings_TTS = value;
                NotifyPropertyChanged(nameof(Settings_TTS));
            }
        }

        private bool _Settings_Time = false;
        public bool Settings_Time
        {
            get { return _Settings_Time; }
            set
            {
                _Settings_Time = value;
                NotifyPropertyChanged(nameof(Settings_Time));
            }
        }

        private bool _Settings_HeartRate = false;
        public bool Settings_HeartRate
        {
            get { return _Settings_HeartRate; }
            set
            {
                _Settings_HeartRate = value;
                NotifyPropertyChanged(nameof(Settings_HeartRate));
            }
        }
        private bool _Settings_Status = false;
        public bool Settings_Status
        {
            get { return _Settings_Status; }
            set
            {
                _Settings_Status = value;
                NotifyPropertyChanged(nameof(Settings_Status));
            }
        }

        public enum Timezone
        {
            UTC,
            EST,
            CST,
            PST,
            CET,
            AEST
        }


        private int _HeartRate;
        public int HeartRate
        {
            get { return _HeartRate; }
            set
            {
                _HeartRate = value;
                NotifyPropertyChanged(nameof(HeartRate));
            }
        }


        private bool _ShowBPMSuffix = false;
        public bool ShowBPMSuffix
        {
            get { return _ShowBPMSuffix; }
            set
            {
                _ShowBPMSuffix = value;
                NotifyPropertyChanged(nameof(ShowBPMSuffix));
            }
        }

        private string _PulsoidAccessToken;
        public string PulsoidAccessToken
        {
            get { return _PulsoidAccessToken; }
            set
            {
                _PulsoidAccessToken = value;
                NotifyPropertyChanged(nameof(PulsoidAccessToken));
            }
        }


        private bool _timeShowTimeZone = false;
        public bool TimeShowTimeZone
        {
            get => _timeShowTimeZone;
            set
            {
                _timeShowTimeZone = value;
                NotifyPropertyChanged(nameof(TimeShowTimeZone));
            }
        }

        private Timezone _selectedTimeZone;
        public Timezone SelectedTimeZone
        {
            get => _selectedTimeZone;
            set
            {
                _selectedTimeZone = value;
                NotifyPropertyChanged(nameof(SelectedTimeZone));
            }
        }

        private string _lastUsedSortDirection;
        public string LastUsedSortDirection
        {
            get { return _lastUsedSortDirection; }
            set
            {
                _lastUsedSortDirection = value;
                NotifyPropertyChanged(nameof(LastUsedSortDirection));
            }
        }


        public ProcessInfo LastProcessFocused
        {
            get { return _LastProcessFocused; }
            set
            {
                _LastProcessFocused = value;
                NotifyPropertyChanged(nameof(LastProcessFocused));
            }
        }


        private string _DeletedAppslabel;
        public string DeletedAppslabel
        {
            get { return _DeletedAppslabel; }
            set
            {
                _DeletedAppslabel = value;
                NotifyPropertyChanged(nameof(DeletedAppslabel));
            }
        }

        private ObservableCollection<ProcessInfo> _ScannedApps = new ObservableCollection<ProcessInfo>();
        public ObservableCollection<ProcessInfo> ScannedApps
        {
            get { return _ScannedApps; }
            set
            {
                _ScannedApps = value;
                NotifyPropertyChanged(nameof(ScannedApps));
            }
        }


        private bool _ApplicationHookV2 = true;
        public bool ApplicationHookV2
        {
            get { return _ApplicationHookV2; }
            set
            {
                _ApplicationHookV2 = value;
                NotifyPropertyChanged(nameof(ApplicationHookV2));
            }
        }

        private bool _IntgrIntelliWing = false;
        public bool IntgrIntelliWing
        {
            get { return _IntgrIntelliWing; }
            set
            {
                _IntgrIntelliWing = value;
                NotifyPropertyChanged(nameof(IntgrIntelliWing));
            }
        }

        private bool _AppIsEnabled = true;
        public bool AppIsEnabled
        {
            get { return _AppIsEnabled; }
            set
            {
                _AppIsEnabled = value;
                NotifyPropertyChanged(nameof(AppIsEnabled));
            }
        }

        private double _AppOpacity = 0.98;
        public double AppOpacity
        {
            get { return _AppOpacity; }
            set
            {
                _AppOpacity = value;
                NotifyPropertyChanged(nameof(AppOpacity));
            }
        }


        private ObservableCollection<ChatModelMsg> _OpenAIAPIBuiltInActions;
        public ObservableCollection<ChatModelMsg> OpenAIAPIBuiltInActions
        {
            get { return _OpenAIAPIBuiltInActions; }
            set
            {
                _OpenAIAPIBuiltInActions = value;
                NotifyPropertyChanged(nameof(OpenAIAPIBuiltInActions));
            }
        }

        private string _OpenAIAPITestResponse;
        public string OpenAIAPITestResponse
        {
            get { return _OpenAIAPITestResponse; }
            set
            {
                _OpenAIAPITestResponse = value;
                NotifyPropertyChanged(nameof(OpenAIAPITestResponse));
            }
        }
        private int _OpenAIUsedTokens;
        public int OpenAIUsedTokens
        {
            get { return _OpenAIUsedTokens; }
            set
            {
                _OpenAIUsedTokens = value;
                NotifyPropertyChanged(nameof(OpenAIUsedTokens));
            }
        }


        private bool _IntelliChatModeration = true;
        public bool IntelliChatModeration
        {
            get { return _IntelliChatModeration; }
            set
            {
                _IntelliChatModeration = value;
                NotifyPropertyChanged(nameof(IntelliChatModeration));
            }
        }

        private string _OpenAIModerationUrl;
        public string OpenAIModerationUrl
        {
            get { return _OpenAIModerationUrl; }
            set
            {
                _OpenAIModerationUrl = value;
                NotifyPropertyChanged(nameof(OpenAIModerationUrl));
            }
        }

        private bool _IntgrIntelliChat = false;
        public bool IntgrIntelliChat
        {
            get { return _IntgrIntelliChat; }
            set
            {
                _IntgrIntelliChat = value;
                NotifyPropertyChanged(nameof(IntgrIntelliChat));
            }
        }

        private string _OpenAIAPISelectedModel;
        public string OpenAIAPISelectedModel
        {
            get { return _OpenAIAPISelectedModel; }
            set
            {
                _OpenAIAPISelectedModel = value;
                NotifyPropertyChanged(nameof(OpenAIAPISelectedModel));
            }
        }

        private ObservableCollection<string> _OpenAIAPIModels;
        public ObservableCollection<string> OpenAIAPIModels
        {
            get { return _OpenAIAPIModels; }
            set
            {
                _OpenAIAPIModels = value;
                NotifyPropertyChanged(nameof(OpenAIAPIModels));
            }
        }

        private string _OpenAIAPIUrl;
        public string OpenAIAPIUrl
        {
            get { return _OpenAIAPIUrl; }
            set
            {
                _OpenAIAPIUrl = value;
                NotifyPropertyChanged(nameof(OpenAIAPIUrl));
            }
        }

        private string _OpenAIAPIKey;
        public string OpenAIAPIKey
        {
            get { return _OpenAIAPIKey; }
            set
            {
                _OpenAIAPIKey = value;
                NotifyPropertyChanged(nameof(OpenAIAPIKey));
            }
        }

        public string ToggleVoiceText
        {
            get { return _toggleVoiceText; }
            set
            {
                _toggleVoiceText = value;
                NotifyPropertyChanged(nameof(ToggleVoiceText));
            }
        }
        public bool ToggleVoiceWithV
        {
            get { return _ToggleVoiceWithV; }
            set
            {
                _ToggleVoiceWithV = value;
                NotifyPropertyChanged(nameof(ToggleVoiceWithV));
                UpdateToggleVoiceText();
            }
        }
        public bool TTSBtnShadow
        {
            get { return _TTSBtnShadow; }
            set
            {
                _TTSBtnShadow = value;
                NotifyPropertyChanged(nameof(TTSBtnShadow));
                MainWindow.ShadowOpacity = value ? 1 : 0;
            }
        }
        public bool AutoUnmuteTTS
        {
            get { return _AutoUnmuteTTS; }
            set
            {
                _AutoUnmuteTTS = value;
                NotifyPropertyChanged(nameof(AutoUnmuteTTS));
            }
        }



        public float TTSVolume
        {
            get { return _TTSVolume; }
            set
            {
                _TTSVolume = value;
                NotifyPropertyChanged(nameof(TTSVolume));
            }
        }

        private string _tagURL;
        public string tagURL
        {
            get { return _tagURL; }
            set
            {
                _tagURL = value;
                NotifyPropertyChanged(nameof(tagURL));
            }
        }


        private string _UpdateStatustxt;
        public string UpdateStatustxt
        {
            get { return _UpdateStatustxt; }
            set
            {
                _UpdateStatustxt = value;
                NotifyPropertyChanged(nameof(UpdateStatustxt));
            }
        }

        private string _AppLocation;
        public string AppLocation
        {
            get { return _AppLocation; }
            set
            {
                _AppLocation = value;
                NotifyPropertyChanged(nameof(AppLocation));
            }
        }



        public bool CanUpdate
        {
            get { return _CanUpdate; }
            set
            {
                _CanUpdate = value;
                NotifyPropertyChanged(nameof(CanUpdate));
            }
        }
        public string NewVersionURL
        {
            get { return _NewVersionURL; }
            set
            {
                _NewVersionURL = value;
                NotifyPropertyChanged(nameof(NewVersionURL));
            }
        }
        public bool VrcConnected
        {
            get { return _VrcConnected; }
            set
            {
                _VrcConnected = value;
                NotifyPropertyChanged(nameof(VrcConnected));
            }
        }
        public string LogPath
        {
            get { return _LogPath; }
            set
            {
                _LogPath = value;
                NotifyPropertyChanged(nameof(LogPath));
            }
        }
        public string RecentPlayBackOutput
        {
            get { return _RecentPlayBackOutput; }
            set
            {
                _RecentPlayBackOutput = value;
                NotifyPropertyChanged(nameof(RecentPlayBackOutput));
            }
        }
        public bool TTSCutOff
        {
            get { return _TTSCutOff; }
            set
            {
                _TTSCutOff = value;
                NotifyPropertyChanged(nameof(TTSCutOff));
            }
        }
        public List<AudioDevice> AuxOutputDevices
        {
            get { return _auxOutputDevices; }
            set { _auxOutputDevices = value; NotifyPropertyChanged(nameof(AuxOutputDevices)); }
        }
        public List<AudioDevice> PlaybackOutputDevices
        {
            get { return _playbackOutputDevices; }
            set { _playbackOutputDevices = value; NotifyPropertyChanged(nameof(PlaybackOutputDevices)); }
        }
        public AudioDevice SelectedAuxOutputDevice
        {
            get { return _selectedAuxOutputDevice; }
            set { _selectedAuxOutputDevice = value; NotifyPropertyChanged(nameof(SelectedAuxOutputDevice)); }
        }
        public AudioDevice SelectedPlaybackOutputDevice
        {
            get { return _selectedPlaybackOutputDevice; }
            set { _selectedPlaybackOutputDevice = value; NotifyPropertyChanged(nameof(SelectedPlaybackOutputDevice)); }
        }
        public bool TTSTikTokEnabled
        {
            get { return _TTSTikTokEnabled; }
            set
            {
                _TTSTikTokEnabled = value;
                NotifyPropertyChanged(nameof(TTSTikTokEnabled));
            }
        }
        private string _RecentTikTokTTSVoice = "en_au_001";
        public string RecentTikTokTTSVoice
        {
            get { return _RecentTikTokTTSVoice; }
            set
            {
                _RecentTikTokTTSVoice = value;
                NotifyPropertyChanged(nameof(RecentTikTokTTSVoice));
            }
        }
        public Voice SelectedTikTokTTSVoice
        {
            get { return _SelectedTikTokTTSVoice; }
            set
            {
                _SelectedTikTokTTSVoice = value;
                NotifyPropertyChanged(nameof(SelectedTikTokTTSVoice));
            }
        }
        public List<Voice> TikTokTTSVoices
        {
            get { return _TikTokTTSVoices; }
            set
            {
                _TikTokTTSVoices = value;
                NotifyPropertyChanged(nameof(TikTokTTSVoices));
            }
        }
        public string ApiStream
        {
            get { return _ApiStream; }
            set
            {
                _ApiStream = value;
                NotifyPropertyChanged(nameof(ApiStream));
            }
        }
        public ObservableCollection<ChatItem> LastMessages
        {
            get { return _LastMessages; }
            set
            {
                _LastMessages = value;
                NotifyPropertyChanged(nameof(LastMessages));
            }
        }
        public bool TypingIndicator
        {
            get { return _TypingIndicator; }
            set
            {
                _TypingIndicator = value;
                NotifyPropertyChanged(nameof(TypingIndicator));
            }
        }
        public bool Topmost
        {
            get { return _Topmost; }
            set
            {
                _Topmost = value;
                NotifyPropertyChanged(nameof(Topmost));
            }
        }
        public bool PauseIconMusic
        {
            get { return _PauseIconMusic; }
            set
            {
                _PauseIconMusic = value;
                NotifyPropertyChanged(nameof(PauseIconMusic));
            }
        }
        public bool ChatFX
        {
            get { return _ChatFX; }
            set
            {
                _ChatFX = value;
                NotifyPropertyChanged(nameof(ChatFX));
            }
        }
        public bool CountDownUI
        {
            get { return _CountDownUI; }
            set
            {
                _CountDownUI = value;
                NotifyPropertyChanged(nameof(CountDownUI));
            }
        }
        public bool PrefixChat
        {
            get { return _PrefixChat; }
            set
            {
                _PrefixChat = value;
                NotifyPropertyChanged(nameof(PrefixChat));
            }
        }
        public bool ScanPause
        {
            get { return _ScanPause; }
            set
            {
                _ScanPause = value;
                NotifyPropertyChanged(nameof(ScanPause));
            }
        }
        public int ScanPauseTimeout
        {
            get { return _ScanPauseTimeout; }
            set
            {
                _ScanPauseTimeout = value;
                NotifyPropertyChanged(nameof(ScanPauseTimeout));
            }
        }
        public int ScanPauseCountDown
        {
            get { return _ScanPauseCountDown; }
            set
            {
                _ScanPauseCountDown = value;
                NotifyPropertyChanged(nameof(ScanPauseCountDown));
            }
        }
        public string aesKey
        {
            get { return _aesKey; }
            set
            {
                _aesKey = value;
                NotifyPropertyChanged(nameof(aesKey));
            }
        }
        public string ChatTopBarTxt
        {
            get { return _ChatTopBarTxt; }
            set
            {
                _ChatTopBarTxt = value;
                NotifyPropertyChanged(nameof(ChatTopBarTxt));
            }
        }
        public string ChatFeedbackTxt
        {
            get { return _ChatFeedbackTxt; }
            set
            {
                _ChatFeedbackTxt = value;
                NotifyPropertyChanged(nameof(ChatFeedbackTxt));
            }
        }
        public string ActiveChatTxt
        {
            get { return _ActiveChatTxt; }
            set
            {
                _ActiveChatTxt = value;
                NotifyPropertyChanged(nameof(ActiveChatTxt));
            }
        }
        public string StatusTopBarTxt
        {
            get { return _StatusTopBarTxt; }
            set
            {
                _StatusTopBarTxt = value;
                NotifyPropertyChanged(nameof(StatusTopBarTxt));
            }
        }
        public string NewChattingTxt
        {
            get { return _NewChattingTxt; }
            set
            {
                _NewChattingTxt = value;
                NotifyPropertyChanged(nameof(NewChattingTxt));
            }
        }
        public string NewStatusItemTxt
        {
            get { return _NewStatusItemTxt; }
            set
            {
                _NewStatusItemTxt = value;
                NotifyPropertyChanged(nameof(NewStatusItemTxt));
            }
        }
        public string ChatBoxCount
        {
            get { return _ChatBoxCount; }
            set
            {
                _ChatBoxCount = value;
                NotifyPropertyChanged(nameof(ChatBoxCount));
            }
        }
        public string StatusBoxCount
        {
            get { return _StatusBoxCount; }
            set
            {
                _StatusBoxCount = value;
                NotifyPropertyChanged(nameof(StatusBoxCount));
            }
        }
        public string ChatBoxColor
        {
            get { return _ChatBoxColor; }
            set
            {
                _ChatBoxColor = value;
                NotifyPropertyChanged(nameof(ChatBoxColor));
            }
        }
        public string StatusBoxColor
        {
            get { return _StatusBoxColor; }
            set
            {
                _StatusBoxColor = value;
                NotifyPropertyChanged(nameof(StatusBoxColor));
            }
        }
        public bool PrefixIconStatus
        {
            get { return _PrefixIconStatus; }
            set
            {
                _PrefixIconStatus = value;
                NotifyPropertyChanged(nameof(PrefixIconStatus));
            }
        }
        public bool PrefixIconMusic
        {
            get { return _PrefixIconMusic; }
            set
            {
                _PrefixIconMusic = value;
                NotifyPropertyChanged(nameof(PrefixIconMusic));
            }
        }
        public ObservableCollection<StatusItem> StatusList
        {
            get { return _StatusList; }
            set
            {
                _StatusList = value;
                NotifyPropertyChanged(nameof(StatusList));


            }
        }
        public string MenuItem_3_Visibility
        {
            get { return _MenuItem_3_Visibility; }
            set
            {
                _MenuItem_3_Visibility = value;
                NotifyPropertyChanged(nameof(MenuItem_3_Visibility));
            }
        }
        public string MenuItem_2_Visibility
        {
            get { return _MenuItem_2_Visibility; }
            set
            {
                _MenuItem_2_Visibility = value;
                NotifyPropertyChanged(nameof(MenuItem_2_Visibility));
            }
        }
        public string MenuItem_1_Visibility
        {
            get { return _MenuItem_1_Visibility; }
            set
            {
                _MenuItem_1_Visibility = value;
                NotifyPropertyChanged(nameof(MenuItem_1_Visibility));
            }
        }
        public string MenuItem_0_Visibility
        {
            get { return _MenuItem_0_Visibility; }
            set
            {
                _MenuItem_0_Visibility = value;
                NotifyPropertyChanged(nameof(MenuItem_0_Visibility));
            }
        }
        public int CurrentMenuItem
        {
            get { return _CurrentMenuItem; }
            set
            {
                _CurrentMenuItem = value;
                NotifyPropertyChanged(nameof(CurrentMenuItem));
            }
        }
        public bool Time24H
        {
            get { return _Time24H; }
            set
            {
                _Time24H = value;
                NotifyPropertyChanged(nameof(Time24H));
            }
        }
        public bool PrefixTime
        {
            get { return _PrefixTime; }
            set
            {
                _PrefixTime = value;
                NotifyPropertyChanged(nameof(PrefixTime));
            }
        }
        public string Spotify_Opacity
        {
            get { return _Spotify_Opacity; }
            set
            {
                _Spotify_Opacity = value;
                NotifyPropertyChanged(nameof(Spotify_Opacity));
            }
        }


        private int _HeartRateAdjustment = 0;
        public int HeartRateAdjustment
        {
            get { return _HeartRateAdjustment; }
            set
            {
                _HeartRateAdjustment = value;
                NotifyPropertyChanged(nameof(HeartRateAdjustment));
            }
        }

        public string HeartRate_Opacity
        {
            get { return _HeartRate_Opacity; }
            set
            {
                _HeartRate_Opacity = value;
                NotifyPropertyChanged(nameof(HeartRate_Opacity));
            }
        }


        private bool _IntgrHeartRate = false;
        public bool IntgrHeartRate
        {
            get { return _IntgrHeartRate; }
            set
            {
                _IntgrHeartRate = value;
                NotifyPropertyChanged(nameof(IntgrHeartRate));
            }
        }

        public string Status_Opacity
        {
            get { return _Status_Opacity; }
            set
            {
                _Status_Opacity = value;
                NotifyPropertyChanged(nameof(Status_Opacity));
            }
        }

        public string Time_Opacity
        {
            get { return _Time_Opacity; }
            set
            {
                _Time_Opacity = value;
                NotifyPropertyChanged(nameof(Time_Opacity));
            }
        }

        public string Window_Opacity
        {
            get { return _Window_Opacity; }
            set
            {
                _Window_Opacity = value;
                NotifyPropertyChanged(nameof(Window_Opacity));
            }
        }
        public bool IntgrStatus
        {
            get { return _IntgrStatus; }
            set
            {
                _IntgrStatus = value;
                NotifyPropertyChanged(nameof(IntgrStatus));
            }
        }

        public bool MasterSwitch
        {
            get { return _MasterSwitch; }
            set
            {
                _MasterSwitch = value;
                NotifyPropertyChanged(nameof(MasterSwitch));
            }
        }

        public string Char_Limit
        {
            get { return _Char_Limit; }
            set
            {
                _Char_Limit = value;
                NotifyPropertyChanged(nameof(Char_Limit));
            }
        }

        public string DataPath
        {
            get { return _DataPath; }
            set
            {
                _DataPath = value;
                NotifyPropertyChanged(nameof(DataPath));
            }
        }

        public string OSCmsg_countUI
        {
            get { return _OSCmsg_countUI; }
            set
            {
                _OSCmsg_countUI = value;
                NotifyPropertyChanged(nameof(OSCmsg_countUI));
            }
        }
        public int OSCmsg_count
        {
            get { return _OSCmsg_count; }
            set
            {
                _OSCmsg_count = value;
                NotifyPropertyChanged(nameof(OSCmsg_count));
            }
        }

        public bool IntgrScanWindowTime
        {
            get { return _IntgrScanWindowTime; }
            set
            {
                _IntgrScanWindowTime = value;
                NotifyPropertyChanged(nameof(IntgrScanWindowTime));
            }
        }

        public string OSCIP
        {
            get { return _OSCIP; }
            set
            {
                _OSCIP = value;
                NotifyPropertyChanged(nameof(OSCIP));
            }
        }

        public bool IntgrScanWindowActivity
        {
            get { return _IntgrScanWindowActivity; }
            set
            {
                _IntgrScanWindowActivity = value;
                NotifyPropertyChanged(nameof(IntgrScanWindowActivity));
            }
        }

        public int OSCPortOut
        {
            get { return _OSCPortOut; }
            set
            {
                _OSCPortOut = value;
                NotifyPropertyChanged(nameof(OSCPortOut));
            }
        }

        public bool IntgrScanSpotify
        {
            get { return _IntgrScanSpotify; }
            set
            {
                _IntgrScanSpotify = value;
                NotifyPropertyChanged(nameof(IntgrScanSpotify));
            }
        }

        public int ScanInterval
        {
            get { return _ScanInterval; }
            set
            {
                _ScanInterval = value;
                NotifyPropertyChanged(nameof(ScanInterval));
            }
        }

        public string CurrentTime
        {
            get { return _CurrentTime; }
            set
            {
                _CurrentTime = value;
                NotifyPropertyChanged(nameof(CurrentTime));
            }
        }



        public string VersionTxt
        {
            get { return _VersionTxt; }
            set
            {
                _VersionTxt = value;
                NotifyPropertyChanged(nameof(VersionTxt));
            }
        }

        public string VersionTxtColor
        {
            get { return _VersionTxtColor; }
            set
            {
                _VersionTxtColor = value;
                NotifyPropertyChanged(nameof(VersionTxtColor));
            }
        }

        public Version AppVersion
        {
            get { return _AppVersion; }
            set
            {
                _AppVersion = value;
                NotifyPropertyChanged(nameof(AppVersion));
            }
        }

        public Version GitHubVersion
        {
            get { return _GitHubVersion; }
            set
            {
                _GitHubVersion = value;
                NotifyPropertyChanged(nameof(GitHubVersion));
            }
        }


        public bool IsVRRunning
        {
            get { return _IsVRRunning; }
            set
            {
                _IsVRRunning = value;
                NotifyPropertyChanged(nameof(IsVRRunning));
            }
        }


        public string OSCtoSent
        {
            get { return _OSCtoSent; }
            set
            {
                _OSCtoSent = value;
                NotifyPropertyChanged(nameof(OSCtoSent));
            }
        }
        public string FocusedWindow
        {
            get { return _FocusedWindow; }
            set
            {
                _FocusedWindow = value;
                NotifyPropertyChanged(nameof(FocusedWindow));
            }
        }
        public string PlayingSongTitle
        {
            get { return _PlayingSongTitle; }
            set
            {
                _PlayingSongTitle = value;
                NotifyPropertyChanged(nameof(PlayingSongTitle));
            }
        }
        public bool SpotifyActive
        {
            get { return _SpotifyActive; }
            set
            {
                _SpotifyActive = value;
                NotifyPropertyChanged(nameof(SpotifyActive));
            }
        }
        public bool SpotifyPaused
        {
            get { return _SpotifyPaused; }
            set
            {
                _SpotifyPaused = value;
                NotifyPropertyChanged(nameof(SpotifyPaused));
            }
        }


        #endregion

        #region PropChangedEvent
        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        #endregion
    }
}
