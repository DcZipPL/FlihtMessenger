using FlihtMesseger.Chat;
using FlihtMesseger.Classes;
using FlihtMesseger.Classes.ShellHelpers;
using FlihtMesseger.Themes;
using FlihtMesseger.Windows;
using Microsoft.WindowsAPICodePack.Shell.PropertySystem;
using MS.WindowsAPICodePack.Internal;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shapes;

namespace FlihtMesseger
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        //TODO: REPAIR ERROR WITH RUNNING IN MENU START
        static string APP_ID = "DcZip.FlihtMessenger";
        Toaster toaster = new Toaster(APP_ID);

        #region Varables
        //Vars
        private static string address = "http://127.0.0.1/Server";
        private string datafolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\FlihtMessenger";
        private string readed = "";
        private string oldreaded = "";
        private bool Resized = false;
        private bool StartResize = false;
        private Random rand = new Random();
        private Sender sender = new Sender(address);
        private Reader reader = new Reader(address);
        private MessagesBox msgBox = new MessagesBox();
        private User user = new User();

        internal Brush FrameBrush = new SolidColorBrush(Colors.Red);
        internal Brush AlphaFrameBrush = new SolidColorBrush(Colors.Red);
        internal Brush TransparentBrush = new SolidColorBrush(Colors.Transparent);
        internal Brush DarkBrush = new SolidColorBrush(Colors.Red);

        public ThemeType currentTheme = ThemeType.Light;
        public ChatStyle currChatMode = ChatStyle.Modern;
        public int currentTimeSpan = 1400;
        public int messagesUnreaded = 0;

        private ComboBox factoryRefreshTimeBox = null;

        private bool devMode = false;
        private bool lastError = false;
        #endregion

        #region Shortcut Init
        private bool TryCreateShortcut()
        {
            String shortcutPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Microsoft\\Windows\\Start Menu\\Programs\\FlihtMessenger.lnk";
            if (!File.Exists(shortcutPath))
            {
                InstallShortcut(shortcutPath);
                return true;
            }
            return false;
        }

        private void InstallShortcut(String shortcutPath)
        {
            // Find the path to the current executable
            String exePath = Process.GetCurrentProcess().MainModule.FileName;
            IShellLinkW newShortcut = (IShellLinkW)new CShellLink();

            // Create a shortcut to the exe
            ErrorHelper.VerifySucceeded(newShortcut.SetPath(exePath));
            ErrorHelper.VerifySucceeded(newShortcut.SetArguments(""));

            // Open the shortcut property store, set the AppUserModelId property
            IPropertyStore newShortcutProperties = (IPropertyStore)newShortcut;

            using (PropVariant appId = new PropVariant(APP_ID))
            {
                ErrorHelper.VerifySucceeded(newShortcutProperties.SetValue(SystemProperties.System.AppUserModel.ID, appId));
                ErrorHelper.VerifySucceeded(newShortcutProperties.Commit());
            }

            // Commit the shortcut to disk
            IPersistFile newShortcutSave = (IPersistFile)newShortcut;

            ErrorHelper.VerifySucceeded(newShortcutSave.Save(shortcutPath, true));
        }
        #endregion

        #region Imports
        //Get Color Theme
        [DllImport("uxtheme.dll", EntryPoint = "#95")]
        public static extern uint GetImmersiveColorFromColorSetEx(uint dwImmersiveColorSet, uint dwImmersiveColorType, bool bIgnoreHighContrast, uint dwHighContrastCacheMode);
        [DllImport("uxtheme.dll", EntryPoint = "#96")]
        public static extern uint GetImmersiveColorTypeFromName(IntPtr pName);
        [DllImport("uxtheme.dll", EntryPoint = "#98")]
        public static extern int GetImmersiveUserColorSetPreference(bool bForceCheckRegistry, bool bSkipCheckOnFail);
        #endregion

        #region Events
        private void Window_IsLoaded(object sender, RoutedEventArgs e) => WindowLoaded();
        private void CloseButton_Click(object sender, RoutedEventArgs e) => CloseApplication();
        private void ResizeButton_Click(object sender, RoutedEventArgs e) => ShowResizeRectangle();
        private void Fullscreen_Click(object sender, RoutedEventArgs e) => ToggleFullscreen();
        private void SettingsButton(object sender, RoutedEventArgs e) => ToggleSettings();
        private void ApplySettings_Click(object sender, RoutedEventArgs e) => SyncSettings();
        private void LoginButton_Click(object sender, RoutedEventArgs e) => DoLogin();
        private void RefreshButton(object sender, RoutedEventArgs e) => RefreshChat();
        private async void SendButton_Click(object s, RoutedEventArgs e) => await SendMessageAsync();
        private void HideButton_Click(object sender, RoutedEventArgs e) => this.WindowState = WindowState.Minimized;
        private void ResizeBox_MouseDown(object sender, MouseButtonEventArgs e) => ResizeStart(sender as Rectangle);
        private void ResizeBox_MouseMove(object sender, MouseEventArgs e) => ResizeWindow(sender as Rectangle, e);
        private void ResizeBox_MouseUp(object sender, MouseButtonEventArgs e) => ResizeEnd(sender as Rectangle);
        private void ChangeTheme_Click(object sender, RoutedEventArgs e) => ChengeTheme((sender as Button).Tag.ToString());
        private void CheckChanged(object sender, RoutedEventArgs e) => ToggleButtonToRadioButton(((ToggleButton)sender));

        private void Applyandexitbutton_Click(object sender, RoutedEventArgs e) { SyncSettings(); ToggleSettings(); }
        private void MoveWindow(object sender, MouseButtonEventArgs e) { if (e.LeftButton == MouseButtonState.Pressed) { DragMove(); } }
        #endregion

        #region Inits
        public MainWindow()
        {
            InitializeComponent();
            TryCreateShortcut();

            uint colour1 = GetImmersiveColorFromColorSetEx((uint)GetImmersiveUserColorSetPreference(false, false), GetImmersiveColorTypeFromName(Marshal.StringToHGlobalUni("ImmersiveStartSelectionBackground")), false, 0);
            Color colour = Color.FromArgb((byte)((0xFF000000 & colour1) >> 24), (byte)(0x000000FF & colour1), (byte)((0x0000FF00 & colour1) >> 8), (byte)((0x00FF0000 & colour1) >> 16));
            Color colourAlpha = Color.FromArgb((byte)(0x3F), (byte)(0x000000FF & colour1), (byte)((0x0000FF00 & colour1) >> 8), (byte)((0x00FF0000 & colour1) >> 16));
            FrameBrush = new SolidColorBrush(colour);
            AlphaFrameBrush = new SolidColorBrush(colourAlpha);
            RoundBorder.BorderBrush = FrameBrush;
            ResizeBox.Fill = AlphaFrameBrush;
            rightResize.Fill = TransparentBrush;
            leftResize.Fill = TransparentBrush;
            bottomResize.Fill = TransparentBrush;
            ThemeTestWin.Fill = FrameBrush;

            Color c = new Color
            {
                A = 255,
                R = 35,
                G = 35,
                B = 35
            };
            DarkBrush = new SolidColorBrush(c);
        }
        #endregion

        #region Functions
        public void CloseApplication()
        {
            Environment.Exit(1);
        }

        public void SyncSettings()
        {
            Badge.Clear(badge);
            //Value from CheckBoxes saved to varable
            devMode = CheckBoxEDO.IsChecked == true ? true : false;

            //Change or Reload Theme
            ChangeTheme();

            //Developer Mode Features
            if (devMode)
            {
                titlebar.Text = "Fliht Messenger --Version {version} --Debug";
                resizeShadow.Visibility = Visibility.Visible;
                resizeButton.Visibility = Visibility.Visible;
            }
            else
            {
                titlebar.Text = "Fliht Messenger";
                resizeShadow.Visibility = Visibility.Collapsed;
                resizeButton.Visibility = Visibility.Collapsed;
            }

            //Setting Nickname
            if (NickBox.Text != "")
            {
                user.username = NickBox.Text;
                user.usertoken = rand.Next(1853, 9364).ToString();
            }
            else
            {
                user.username = "UnnamedUser" + rand.Next(1853, 9364);
                user.usertoken = rand.Next(1000, 9999).ToString();
            }

            //Address Checking
            try
            {
                if (AddressBox.Text != "")
                {
                    Uri addressUri = new Uri(AddressBox.Text);
                    address = addressUri.ToString();

                    AddressInfoBlock.Foreground = Brushes.Green;
                    AddressInfoBlock.Text = "Address successfully changed!";
                }
                else
                {
                    AddressInfoBlock.Foreground = currentTheme == ThemeType.Dark ? Brushes.White : Brushes.Black;
                    AddressInfoBlock.Text = "Server addres is empty. Default: \"http://127.0.0.1/Server\"";
                }
            }
            catch (UriFormatException)
            {
                AddressInfoBlock.Foreground = Brushes.Red;
                AddressInfoBlock.Text = "Url Format Are wrong! ex. http(s)://example.com";
            }
            catch (Exception)
            {
                AddressInfoBlock.Text = "Unknown Error!";
            }

            //Chat Mode Changing
            currChatMode = MmTb.IsChecked == true ? ChatStyle.Modern : MiTb.IsChecked == true ? ChatStyle.IRC : ChatStyle.ALL;

            //Chat Reading Time Changing
            currentTimeSpan = RefreshTimeBox.SelectedItem != null ? int.Parse(((ComboBoxItem)RefreshTimeBox.SelectedItem).Tag.ToString()) : 1600;
            InitCombobox(RefreshTimeBox.SelectedIndex);

            //Setting Address
            reader.ChangeAddress(address);
            sender.ChangeAddress(address);

            byte utheme = 0x0000;

            switch (currentTheme)
            {
                case ThemeType.Light:
                    utheme = 0x0001;
                    break;
                case ThemeType.Window:
                    utheme = 0x0002;
                    break;
                case ThemeType.Dark:
                    utheme = 0x0003;
                    break;
                case ThemeType.FlatDark:
                    utheme = 0x0004;
                    break;
            }

            //Saving Values
            byte[] savedBytes =
            {

                utheme,
                devMode ? (byte)0x0001 : (byte)0x0000,
                currChatMode == ChatStyle.Modern ? (byte)0x0000 : currChatMode == ChatStyle.IRC ? (byte)0x0001 : (byte)0x0002
            };
            File.WriteAllBytes(datafolder + "\\user.profile", savedBytes);
            File.WriteAllText(datafolder + "\\addresses.dat", Base64.Encode(address));
        }

        private void WindowLoaded()
        {
            statusDef.Height = new GridLength(0, GridUnitType.Pixel);

            factoryRefreshTimeBox = RefreshTimeBox;

            //
            //TempClass.CreateInject();
            //
        }

        private void InitCombobox(int index)
        {
            RefreshTimeBox.Items.Clear();

            //Setting Combobox (Dropdown) items
            int[] Rtb_Timespans = { 200, 800, 1400, 1600, 1800, 2000, 2500, 3000, 3500, 4000, 5000, 6000, 8000, 10000, 15000, 20000, 30000, 40000, 50000, 60000 };

            ComboBoxItem Rtb_item = new ComboBoxItem();

            if (devMode == true)
            {
                Rtb_item.Content = "0.05s --Testing only--";
                Rtb_item.Tag = 50;
                RefreshTimeBox.Items.Add(Rtb_item);
            }

            foreach (int Rtb_Timespan in Rtb_Timespans)
            {
                Rtb_item = new ComboBoxItem
                {
                    Content = (Rtb_Timespan / 1000) + "." + ((Rtb_Timespan / 100).ToString().Remove(0, (Rtb_Timespan / 1000 == 0 ? 0 : 1)).Replace("00", "0")/*Temp*/.Replace("50", "0")/*Temp*/) + "s",
                    Tag = Rtb_Timespan
                };
                RefreshTimeBox.Items.Add(Rtb_item);
            }
            RefreshTimeBox.SelectedIndex = index;
        }

        private void ToggleButtonToRadioButton(ToggleButton senderButton)
        {
            switch (senderButton.Name.ToLower())
            {
                case "mmtb":
                    MiTb.IsChecked = false;
                    MaTb.IsChecked = false;
                    break;
                case "mitb":
                    MmTb.IsChecked = false;
                    MaTb.IsChecked = false;
                    break;
                case "matb":
                    MiTb.IsChecked = false;
                    MmTb.IsChecked = false;
                    break;
            }
        }

        private async void HideStatus()
        {
            for (int i = 30; i >= 0; i--)
            {
                statusDef.Height = new GridLength(i, GridUnitType.Pixel);
                await Task.Delay(10);
            }
            statusDef.Height = new GridLength(0, GridUnitType.Pixel);
        }
        #endregion

        #region Login Functions
        private async void DoLogin()
        {
            try
            {
                if (File.Exists(datafolder + "\\user.profile") && File.Exists(datafolder + "\\addresses.dat") && Directory.Exists(datafolder))
                {
                    byte[] readedBytes = File.ReadAllBytes(datafolder + "\\user.profile");
                    //currentTheme = readedBytes[0] == 0x0001 ? ThemeType.Light : readedBytes[0] == 0x0002 ? ThemeType.Window : ThemeType.Dark;
                    switch (readedBytes[0])
                    {
                        case 0x0001:
                            currentTheme = ThemeType.Light;
                            break;
                        case 0x0002:
                            currentTheme = ThemeType.Window;
                            break;
                        case 0x0003:
                            currentTheme = ThemeType.Dark;
                            break;
                        case 0x0004:
                            currentTheme = ThemeType.FlatDark;
                            break;
                    }
                    CheckBoxEDO.IsChecked = readedBytes[1] == 0x0001 ? true : false;

                    if (readedBytes[2] == 0x0000)
                        MmTb.IsChecked = true;
                    if (readedBytes[2] == 0x0001)
                        MiTb.IsChecked = true;
                    if (readedBytes[2] == 0x0002)
                        MaTb.IsChecked = true;

                    AddressBox.Text = Base64.Decode(File.ReadAllText(datafolder + "\\addresses.dat"));
                }
                else
                {
                    if (!Directory.Exists(datafolder))
                        Directory.CreateDirectory(datafolder);
                }
            }
            catch (Exception)
            {
                aboutBox.Visibility = Visibility.Collapsed;
                loginerrorBox.Visibility = Visibility.Visible;
                float f = 0;
                for (int i = 40; i >= 0; i--)
                {
                    f = i;
                    loginError.Text = "File Version Error! Creating new settings file... Please Wait " + ((f / 2) + 0.5f).ToString() + ((((f / 2) + 0.5f).ToString().Length) <= 3 && ((f / 2) + 0.5f) >= 10 ? ".0" : "");
                    await Task.Delay(100);
                }
            }

            smainGrid.Effect = null;
            bottomLoginGrid.Visibility = Visibility.Collapsed;
            loginBox.Visibility = Visibility.Collapsed;
            SyncSettings();
            InitCombobox(2);

            messagesViwer.ScrollToEnd();

            await StartReadTask();
        }
        #endregion

        #region Theme Functions
        private void ChengeTheme(string type)
        {
            switch (type.ToLower())
            {
                case "light":
                    currentTheme = ThemeType.Light;
                    break;
                case "dark":
                    currentTheme = ThemeType.Dark;
                    break;
                case "window":
                    currentTheme = ThemeType.Window;
                    break;
                case "flatdark":
                    currentTheme = ThemeType.FlatDark;
                    break;
            }
        }

        private void ChangeTheme()
        {
            topBar.Background = TransparentBrush;
            statusGrid.Background = TransparentBrush;
            bpanel.Background = TransparentBrush;
            if (currentTheme == ThemeType.Dark)
            {
                Color c = new Color
                {
                    A = 255,
                    R = 50,
                    G = 50,
                    B = 50
                };
                LinearGradientBrush linear = new LinearGradientBrush(c, Colors.Transparent, new Point(0, 5), new Point(0, 0));
                downShadow.Fill = linear;
                topShadow.Fill = linear;
                var transformGroup = new TransformGroup();
                var rotateTransform = new RotateTransform(180);
                transformGroup.Children.Add(rotateTransform);
                topShadow.RenderTransform = transformGroup;

                Foreground = Brushes.White;
                AddressBox.Foreground = Brushes.White;
                sendButton.Foreground = Brushes.White;
                mainText.Foreground = Brushes.White;

                RectChanger(DarkBrush, ThemeColorType.Black, true);

                bottomSettingsGrid.Background = DarkBrush;
                SettingsBorder.Background = DarkBrush;
                mainGrid.Background = DarkBrush;
            }
            else if (currentTheme == ThemeType.FlatDark)
            {

                Color cb = new Color
                {
                    A = 255,
                    R = 35,
                    G = 35,
                    B = 35
                };
                Color cb1 = new Color
                {
                    A = 255,
                    R = 25,
                    G = 25,
                    B = 25
                };
                Brush b = new SolidColorBrush(cb);
                Brush b1 = new SolidColorBrush(cb1);

                Color c = new Color
                {
                    A = 255,
                    R = 50,
                    G = 50,
                    B = 50
                };
                LinearGradientBrush linear = new LinearGradientBrush(Colors.Transparent, Colors.Transparent, new Point(0, 5), new Point(0, 0));
                downShadow.Fill = linear;
                topShadow.Fill = linear;
                var transformGroup = new TransformGroup();
                var rotateTransform = new RotateTransform(180);
                transformGroup.Children.Add(rotateTransform);
                topShadow.RenderTransform = transformGroup;

                Foreground = Brushes.White;
                AddressBox.Foreground = Brushes.White;
                sendButton.Foreground = Brushes.White;
                mainText.Foreground = Brushes.White;

                topBar.Background = b;
                statusGrid.Background = b;

                RectChanger(b, ThemeColorType.Black, false);

                bottomSettingsGrid.Background = b;
                SettingsBorder.Background = b1;
                mainGrid.Background = b1;
                bpanel.Background = b;
            }
            else if (currentTheme == ThemeType.Window)
            {
                Color c = new Color
                {
                    A = 255,
                    R = 180,
                    G = 180,
                    B = 180
                };
                LinearGradientBrush linear = new LinearGradientBrush(c, Colors.Transparent, new Point(0, 1), new Point(0, 0));
                downShadow.Fill = linear;
                topShadow.Fill = linear;
                var transformGroup = new TransformGroup();
                var rotateTransform = new RotateTransform(180);
                transformGroup.Children.Add(rotateTransform);
                topShadow.RenderTransform = transformGroup;

                Foreground = Brushes.Black;
                AddressBox.Foreground = Brushes.Black;
                sendButton.Foreground = Brushes.Black;
                mainText.Foreground = Brushes.Black;

                topBar.Background = FrameBrush;
                statusGrid.Background = FrameBrush;

                RectChanger(FrameBrush, ThemeColorType.White, true);

                bottomSettingsGrid.Background = FrameBrush;
                SettingsBorder.Background = Brushes.White;
                mainGrid.Background = Brushes.White;
            }
            else
            {
                Color c = new Color
                {
                    A = 255,
                    R = 180,
                    G = 180,
                    B = 180
                };
                LinearGradientBrush linear = new LinearGradientBrush(c, Colors.Transparent, new Point(0, 1), new Point(0, 0));
                downShadow.Fill = linear;
                topShadow.Fill = linear;
                var transformGroup = new TransformGroup();
                var rotateTransform = new RotateTransform(180);
                transformGroup.Children.Add(rotateTransform);
                topShadow.RenderTransform = transformGroup;

                Foreground = Brushes.Black;
                AddressBox.Foreground = Brushes.Black;
                sendButton.Foreground = Brushes.Black;
                mainText.Foreground = Brushes.Black;

                RectChanger(Brushes.White, ThemeColorType.White, true);

                bottomSettingsGrid.Background = Brushes.White;
                SettingsBorder.Background = Brushes.White;
                mainGrid.Background = Brushes.White;
            }
        }

        private void RectChanger(Brush mainBrush, ThemeColorType themeColor, bool visible)
        {
            //Change Brush
            shadowRectangle.Fill = mainBrush;
            shadowRectangle1.Fill = mainBrush;
            shadowRectangle2.Fill = mainBrush;
            shadowRectangle3.Fill = mainBrush;
            shadowRectangle4.Fill = mainBrush;
            shadowRectangle5.Fill = mainBrush;
            shadowRectangle6.Fill = mainBrush;
            shadowRectangle7.Fill = mainBrush;
            shadowRectangle8.Fill = mainBrush;
            resizeShadow.Fill = mainBrush;

            //Toggle Dropshadow Visibility
            ((DropShadowEffect)shadowRectangle.Effect).Opacity = visible == true ? 1 : 0;
            ((DropShadowEffect)shadowRectangle1.Effect).Opacity = visible == true ? 1 : 0;
            ((DropShadowEffect)shadowRectangle2.Effect).Opacity = visible == true ? 1 : 0;
            ((DropShadowEffect)shadowRectangle3.Effect).Opacity = visible == true ? 1 : 0;
            ((DropShadowEffect)shadowRectangle4.Effect).Opacity = visible == true ? 1 : 0;
            ((DropShadowEffect)shadowRectangle5.Effect).Opacity = visible == true ? 1 : 0;
            ((DropShadowEffect)shadowRectangle6.Effect).Opacity = visible == true ? 1 : 0;
            ((DropShadowEffect)shadowRectangle7.Effect).Opacity = visible == true ? 1 : 0;
            ((DropShadowEffect)shadowRectangle8.Effect).Opacity = visible == true ? 1 : 0;
            ((DropShadowEffect)resizeShadow.Effect).Opacity = visible == true ? 1 : 0;

            //Change Style Of Button
            applyandexitbutton.Style = themeColor == ThemeColorType.White ? (Style)Application.Current.Resources["TitleBarModernButton"] : (Style)Application.Current.Resources["ModernDarkButton"];
            applySettingsButton.Style = themeColor == ThemeColorType.White ? (Style)Application.Current.Resources["TitleBarModernButton"] : (Style)Application.Current.Resources["ModernDarkButton"];
            closeSettingsButton.Style = themeColor == ThemeColorType.White ? (Style)Application.Current.Resources["TitleBarModernButton"] : (Style)Application.Current.Resources["ModernDarkButton"];
            hideButton.Style = themeColor == ThemeColorType.White ? (Style)Application.Current.Resources["TitleBarModernButton"] : (Style)Application.Current.Resources["ModernDarkButton"];
            resizeButton.Style = themeColor == ThemeColorType.White ? (Style)Application.Current.Resources["TitleBarModernButton"] : (Style)Application.Current.Resources["ModernDarkButton"];
            sendButton.Style = themeColor == ThemeColorType.White ? (Style)Application.Current.Resources["TitleBarModernButton"] : (Style)Application.Current.Resources["ModernDarkButton"];
            MmTb.Style = themeColor == ThemeColorType.White ? (Style)Application.Current.Resources["ToggleModernButton"] : (Style)Application.Current.Resources["DarkToggleModernButton"];
            MiTb.Style = themeColor == ThemeColorType.White ? (Style)Application.Current.Resources["ToggleModernButton"] : (Style)Application.Current.Resources["DarkToggleModernButton"];
            MaTb.Style = themeColor == ThemeColorType.White ? (Style)Application.Current.Resources["ToggleModernButton"] : (Style)Application.Current.Resources["DarkToggleModernButton"];
        }
        #endregion

        #region Title Bar Functions
        private void RefreshChat()
        {
            mainMessages.Text = "Chat are refreshed!";
            reader.Refresh();
        }

        private void ShowResizeRectangle()
        {
            if (Resized == false)
            {
                Resized = true;
                ResizeBox.Fill = FrameBrush;
                rightResize.Fill = AlphaFrameBrush;
                leftResize.Fill = AlphaFrameBrush;
                bottomResize.Fill = AlphaFrameBrush;
            }
            else
            {
                Resized = false;
                ResizeBox.Fill = AlphaFrameBrush;
                rightResize.Fill = Brushes.Transparent;
                leftResize.Fill = Brushes.Transparent;
                bottomResize.Fill = Brushes.Transparent;
            }
        }

        private void ToggleFullscreen()
        {
            if (WindowState == WindowState.Normal)
            {
                WindowState = WindowState.Maximized;
                DropSh.Opacity = 0;
                Width = SystemParameters.PrimaryScreenWidth;
                Height = SystemParameters.PrimaryScreenHeight - 40;
                //RoundBorder.Height = SystemParameters.PrimaryScreenHeight - 40;
                //RoundBorder.Width = SystemParameters.PrimaryScreenWidth;
            }
            else
            {
                WindowState = WindowState.Normal;
                DropSh.Opacity = 0.4f;
            }
        }

        private void ToggleSettings()
        {
            if (SettingsBorder.Visibility == Visibility.Collapsed)
            {
                bottomSettingsGrid.Visibility = Visibility.Visible;
                SettingsBorder.Visibility = Visibility.Visible;
            }
            else
            {
                bottomSettingsGrid.Visibility = Visibility.Collapsed;
                SettingsBorder.Visibility = Visibility.Collapsed;
            }
        }
        #endregion

        #region Resize Window Functions
        private void ResizeWindow(Rectangle sender, MouseEventArgs e)
        {
            if (StartResize == true)
            {
                Rectangle senderRectangle = sender;
                if (senderRectangle != null)
                {
                    double boxesSizeWidth = Width;
                    double boxesSizeHeight = Height;
                    double width = e.GetPosition(this).X;
                    double height = e.GetPosition(this).Y;
                    senderRectangle.CaptureMouse();
                    if (senderRectangle.Name.ToLower().Contains("rightresize"))
                    {
                        width += 5;
                        if (width > 0)
                            boxesSizeWidth = width;

                    }
                    if (senderRectangle.Name.ToLower().Contains("leftresize"))
                    {
                        width -= 5;
                        this.Left += width;
                        width = boxesSizeWidth - width;
                        if (width > 0)
                        {
                            boxesSizeWidth = width;
                        }

                    }
                    if (senderRectangle.Name.ToLower().Contains("bottomresize"))
                    {
                        height += 5;
                        if (height > 0)
                            boxesSizeHeight = height;

                    }
                    Width = boxesSizeWidth;
                    Height = boxesSizeHeight;
                }
            }
        }

        private void ResizeStart(Rectangle sender)
        {
            Rectangle senderRectangle = sender;
            if (senderRectangle != null)
            {
                StartResize = true;
                senderRectangle.CaptureMouse();
            }
        }

        private void ResizeEnd(Rectangle sender)
        {
            Rectangle senderRectangle = sender;
            if (senderRectangle != null)
            {
                StartResize = false;
                senderRectangle.ReleaseMouseCapture();
            }
        }
        #endregion

        #region Tasks
        private async Task StartReadTask()
        {
            //Read Message Code
            oldreaded = readed;
            await reader.Read(currChatMode);
            string rusername = reader.GetUser();
            string rmessage = reader.GetMessage();
            readed = reader.GetString();
            mainMessages.Text += readed;

            if (this.IsActive == false)
            {
                if (readed != "")
                {
                    if (oldreaded != readed)
                    {
                        messagesUnreaded++;
                        Badge.SetNumber(messagesUnreaded, badge);
                        toaster.Toast(rmessage, rusername);
                        //Toaster.Toast(readed);
                    }
                }
            }

            //Get Errors
            if (reader.GetError(out Exception ex))
            {
                mainMessages.Text += devMode ? "\r\nError 0x0001.  " + ex.Message + "\r\n" + ex.ToString() : "";
                statusDef.Height = new GridLength(30, GridUnitType.Pixel);
                errorStatus.Text = ex.Message;
                lastError = true;
            }
            else
            {
                if (lastError == true)
                {
                    errorStatus.Text = "";
                    HideStatus();
                    errorStatus.Text = "SOMETHING GOING WRONG!!! REPORT THIS BUG IMMEDIATELY!!!";
                    lastError = false;
                }
            }

            //Wait
            await Task.Delay(currentTimeSpan);

            //Make Loop
            await StartReadTask();
        }

        public async Task SendMessageAsync()
        {
            if (mainText.Text != "")
                sender.Send(mainText.Text, user);
            else
            {
                statusDef.Height = new GridLength(30, GridUnitType.Pixel);
                errorStatus.Text = "Message is Empty!";
                await Task.Delay(4000);
                HideStatus();
            }
        }
        #endregion

        private void Window_Activated(object sender, EventArgs e)
        {
            Badge.Clear(badge);
            messagesUnreaded = 0;
        }
    }
}