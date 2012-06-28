using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;
using mshtml;
using Microsoft.WindowsAPICodePack;
using Microsoft.WindowsAPICodePack.Controls;
using Microsoft.WindowsAPICodePack.Taskbar;
using Awesomium.Core;

namespace WinGrooves
{
    /// <summary>
    /// Summary description for FrmMain.
    /// </summary>
    public class FrmMain : System.Windows.Forms.Form
    {
        private System.ComponentModel.IContainer components;
        private bool closingFromTray = false;

        public static KeyboardHook hook = new KeyboardHook();
        bool windowInitialized;

        private NotifyIcon notifyIcon1;
        private ContextMenuStrip contextMenuStrip1;
        private ToolStripMenuItem HideShow;
        private ToolStripMenuItem About;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripMenuItem Exit;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripMenuItem Previous;
        private ToolStripMenuItem Next;
        private ToolStripMenuItem toolStripMenuItem1;
        private ToolStrip toolStrip1;
        private ToolStripButton toolStripButton1;
        private ToolStripButton toolStripButton2;
        private ToolStripButton toolStripButton3;
        private Timer currentSongTimer;
        private ToolStripButton toolStripButton4;
        private Timer alwaysListeningTimer;
        private ToolStripMenuItem Play;

		//These constants are needed to disable the IE click sound
        private const int FEATURE_DISABLE_NAVIGATION_SOUNDS = 21;
        private const int SET_FEATURE_ON_THREAD = 0x00000001;
        private const int SET_FEATURE_ON_PROCESS = 0x00000002;
        private const int SET_FEATURE_IN_REGISTRY = 0x00000004;
        private const int SET_FEATURE_ON_THREAD_LOCALMACHINE = 0x00000008;
        private const int SET_FEATURE_ON_THREAD_INTRANET = 0x00000010;
        private const int SET_FEATURE_ON_THREAD_TRUSTED = 0x00000020;
        private const int SET_FEATURE_ON_THREAD_INTERNET = 0x00000040;
        private ToolStripMenuItem Like;
        private ToolStripMenuItem Dislike;
        private Awesomium.Windows.Forms.WebControl webControl1;
        private const int SET_FEATURE_ON_THREAD_RESTRICTED = 0x00000080;
        [DllImport("urlmon.dll")]
        [PreserveSig]
        [return: MarshalAs(UnmanagedType.Error)]
        static extern int CoInternetSetFeatureEnabled(
        int FeatureEntry,
        [MarshalAs(UnmanagedType.U4)] int dwFlags,
        bool fEnable);

        //Windows 7 features
        private ThumbnailToolbarButton buttonPrev;
        private bool isMusicPlaying;
        private ThumbnailToolbarButton buttonPause;
        private ThumbnailToolbarButton buttonNext;

        public FrmMain()
        {
            WebCoreConfig config = new WebCoreConfig();
            config.UserAgentOverride = "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/536.5 (KHTML, like Gecko) Chrome/19.0.1084.56 Safari/536.5";
            config.EnablePlugins = true;
            WebCore.Initialize(config, true);
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();
            this.Resize += new EventHandler(FrmMain_Resize);

            //code to remember window size and position
            // this is the default
            this.WindowState = FormWindowState.Normal;
            this.StartPosition = FormStartPosition.WindowsDefaultBounds;

            // check if the saved bounds are nonzero and visible on any screen
            if (Properties.Settings.Default.WindowPosition != Rectangle.Empty &&
                IsVisibleOnAnyScreen(Properties.Settings.Default.WindowPosition))
            {
                // first set the bounds
                this.StartPosition = FormStartPosition.Manual;
                this.DesktopBounds = Properties.Settings.Default.WindowPosition;

                // afterwards set the window state to the saved value (which could be Maximized)
                this.WindowState = Properties.Settings.Default.WindowState;
            }
            else
            {
                // this resets the upper left corner of the window to windows standards
                this.StartPosition = FormStartPosition.WindowsDefaultLocation;

                // we can still apply the saved size
                // msorens: added gatekeeper, otherwise first time appears as just a title bar!
                if (Properties.Settings.Default.WindowPosition != Rectangle.Empty)
                {
                    this.Size = Properties.Settings.Default.WindowPosition.Size;
                }
            }

            windowInitialized = true;
		}

        private bool IsVisibleOnAnyScreen(Rectangle rect)
        {
            foreach (Screen screen in Screen.AllScreens)
            {
                if (screen.WorkingArea.IntersectsWith(rect))
                {
                    return true;
                }
            }

            return false;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            // only save the WindowState if Normal or Maximized
            switch (this.WindowState)
            {
                case FormWindowState.Normal:
                case FormWindowState.Maximized:
                    Properties.Settings.Default.WindowState = this.WindowState;
                    break;

                default:
                    Properties.Settings.Default.WindowState = FormWindowState.Normal;
                    break;
            }

            # region msorens: this code does *not* handle minimized/maximized window.

            // reset window state to normal to get the correct bounds
            // also make the form invisible to prevent distracting the user
            //this.Visible = false;
            //this.WindowState = FormWindowState.Normal;
            //Settings.Default.WindowPosition = this.DesktopBounds;

            # endregion

            Properties.Settings.Default.Save();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            TrackWindowState();
        }

        protected override void OnMove(EventArgs e)
        {
            base.OnMove(e);
            TrackWindowState();
        }

        // On a move or resize in Normal state, record the new values as they occur.
        // This solves the problem of closing the app when minimized or maximized.
        private void TrackWindowState()
        {
            // Don't record the window setup, otherwise we lose the persistent values!
            if (!windowInitialized) { return; }

            if (WindowState == FormWindowState.Normal)
            {
                Properties.Settings.Default.WindowPosition = this.DesktopBounds;
            }
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }


        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmMain));
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.HideShow = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.About = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.Previous = new System.Windows.Forms.ToolStripMenuItem();
            this.Next = new System.Windows.Forms.ToolStripMenuItem();
            this.Play = new System.Windows.Forms.ToolStripMenuItem();
            this.Like = new System.Windows.Forms.ToolStripMenuItem();
            this.Dislike = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.Exit = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton2 = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton3 = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton4 = new System.Windows.Forms.ToolStripButton();
            this.currentSongTimer = new System.Windows.Forms.Timer(this.components);
            this.alwaysListeningTimer = new System.Windows.Forms.Timer(this.components);
            this.webControl1 = new Awesomium.Windows.Forms.WebControl();
            this.contextMenuStrip1.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // notifyIcon1
            // 
            this.notifyIcon1.ContextMenuStrip = this.contextMenuStrip1;
            this.notifyIcon1.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon1.Icon")));
            this.notifyIcon1.Text = Constants.APPNAME;
            this.notifyIcon1.Visible = true;
            this.notifyIcon1.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.notifyIcon1_MouseDoubleClick);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.HideShow,
            this.toolStripMenuItem1,
            this.About,
            this.toolStripSeparator2,
            this.Previous,
            this.Next,
            this.Play,
            this.Like,
            this.Dislike,
            this.toolStripSeparator1,
            this.Exit});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(182, 236);
            // 
            // HideShow
            // 
            this.HideShow.Name = "HideShow";
            this.HideShow.Size = new System.Drawing.Size(181, 22);
            this.HideShow.Text = "Show/Hide";
            this.HideShow.Click += new System.EventHandler(this.HideShow_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(181, 22);
            this.toolStripMenuItem1.Text = "Options";
            this.toolStripMenuItem1.Click += new System.EventHandler(this.toolStripMenuItem1_Click_1);
            // 
            // About
            // 
            this.About.Name = "About";
            this.About.Size = new System.Drawing.Size(181, 22);
            this.About.Text = "About";
            this.About.Click += new System.EventHandler(this.About_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(178, 6);
            // 
            // Previous
            // 
            this.Previous.Name = "Previous";
            this.Previous.Size = new System.Drawing.Size(181, 22);
            this.Previous.Text = "Previous Song";
            this.Previous.Click += new System.EventHandler(this.Previous_Click);
            // 
            // Next
            // 
            this.Next.Name = "Next";
            this.Next.Size = new System.Drawing.Size(181, 22);
            this.Next.Text = "Next Song";
            this.Next.Click += new System.EventHandler(this.Next_Click);
            // 
            // Play
            // 
            this.Play.Name = "Play";
            this.Play.Size = new System.Drawing.Size(181, 22);
            this.Play.Text = "Play/Pause";
            this.Play.Click += new System.EventHandler(this.Play_Click);
            // 
            // Like
            // 
            this.Like.Name = "Like";
            this.Like.Size = new System.Drawing.Size(181, 22);
            this.Like.Text = "Like Current Song";
            this.Like.Click += new System.EventHandler(this.Like_Click);
            // 
            // Dislike
            // 
            this.Dislike.Name = "Dislike";
            this.Dislike.Size = new System.Drawing.Size(181, 22);
            this.Dislike.Text = "Dislike Current Song";
            this.Dislike.Click += new System.EventHandler(this.Dislike_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(178, 6);
            // 
            // Exit
            // 
            this.Exit.Name = "Exit";
            this.Exit.Size = new System.Drawing.Size(181, 22);
            this.Exit.Text = "Exit";
            this.Exit.Click += new System.EventHandler(this.Exit_Click);
            // 
            // toolStrip1
            // 
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton1,
            this.toolStripButton2,
            this.toolStripButton3,
            this.toolStripButton4});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(1284, 25);
            this.toolStrip1.TabIndex = 10;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripButton1
            // 
            this.toolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton1.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton1.Image")));
            this.toolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton1.Name = "toolStripButton1";
            this.toolStripButton1.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton1.Text = "Go back one page";
            this.toolStripButton1.Click += new System.EventHandler(this.toolStripButton1_Click);
            // 
            // toolStripButton2
            // 
            this.toolStripButton2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton2.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton2.Image")));
            this.toolStripButton2.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton2.Name = "toolStripButton2";
            this.toolStripButton2.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton2.Text = "Go forward one page";
            // 
            // toolStripButton3
            // 
            this.toolStripButton3.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton3.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton3.Image")));
            this.toolStripButton3.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton3.Name = "toolStripButton3";
            this.toolStripButton3.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton3.Text = "Options";
            this.toolStripButton3.Click += new System.EventHandler(this.toolStripButton3_Click);
            // 
            // toolStripButton4
            // 
            this.toolStripButton4.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton4.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton4.Image")));
            this.toolStripButton4.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton4.Name = "toolStripButton4";
            this.toolStripButton4.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton4.Text = "About";
            this.toolStripButton4.Click += new System.EventHandler(this.toolStripButton4_Click);
            // 
            // currentSongTimer
            // 
            this.currentSongTimer.Interval = 3000;
            this.currentSongTimer.Tick += new System.EventHandler(this.currentSongTimer_Tick);
            // 
            // alwaysListeningTimer
            // 
            this.alwaysListeningTimer.Enabled = true;
            this.alwaysListeningTimer.Interval = 600000;
            this.alwaysListeningTimer.Tick += new System.EventHandler(this.alwaysListeningTimer_Tick);
            //
            // Win 7 toolbar buttons
            //
            this.buttonPrev = new ThumbnailToolbarButton(Properties.Resources.PlayerPrev, "Previous Music");
            this.buttonPause = new ThumbnailToolbarButton(Properties.Resources.PlayerPlay, "Pause/Play Music");
            this.isMusicPlaying = false;
            this.buttonNext = new ThumbnailToolbarButton(Properties.Resources.PlayerNext, "Next Music");
            // 
            // webControl1
            // 
            this.webControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webControl1.Location = new System.Drawing.Point(0, 25);
            this.webControl1.Name = "webControl1";
            this.webControl1.Size = new System.Drawing.Size(1284, 690);
            this.webControl1.TabIndex = 11;
            // 
            // FrmMain
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.CausesValidation = false;
            this.ClientSize = new System.Drawing.Size(1284, 715);
            this.Controls.Add(this.webControl1);
            this.Controls.Add(this.toolStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FrmMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = Constants.APPNAME;
            this.Activated += new System.EventHandler(this.FrmMain_Activated);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FrmMain_FormClosing);
            this.Load += new System.EventHandler(this.FrmMain_Load);
            this.contextMenuStrip1.ResumeLayout(false);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //Check if application is aready running before running it again
            bool result;
            var mutex = new System.Threading.Mutex(true, Constants.APPNAME, out result);
            if (!result)
            {
                MessageBox.Show(Constants.APPNAME + " is already running.");
                return;
            }
            Application.Run(new FrmMain());
            GC.KeepAlive(mutex);
        }

        private void FrmMain_Load(object sender, System.EventArgs e)
        {            
            webControl1.LoadURL("http://www.grooveshark.com");
            // register the event that is fired after a key press.
            hook.KeyPressed += new EventHandler<KeyPressedEventArgs>(hook_KeyPressed);

            //disable the IE "click sound"
            int feature = FEATURE_DISABLE_NAVIGATION_SOUNDS;
            CoInternetSetFeatureEnabled(feature, SET_FEATURE_ON_PROCESS, true);

            if (Properties.Settings.Default.startMinimized)
            {
                showHideWindow();
            }
            //Thumbnail buttons for win7 users
            if (TaskbarManager.IsPlatformSupported)
            {
                buttonPrev.Click += new EventHandler<ThumbnailButtonClickedEventArgs>(Previous_Click);
                buttonPause.Click += new EventHandler<ThumbnailButtonClickedEventArgs>(Play_Click);
                buttonNext.Click += new EventHandler<ThumbnailButtonClickedEventArgs>(Next_Click);

                //Add the buttons (kinda of ugly tough)
                ThumbnailToolbarButton[] buttonList = new ThumbnailToolbarButton[3];
                buttonList[0] = buttonPrev; buttonList[1] = buttonPause; buttonList[2] = buttonNext;
                TaskbarManager.Instance.ThumbnailToolbars.AddButtons(this.Handle, buttonList);
            }
            currentSongTimer.Enabled = true;
        }
        
        private void playerClickOn(string element)
        {
            jQuery("document.getElementById('" + element + "').click();");
        }

        private void jQueryClickOn(string select)
        {
            jQuery("$(" + select + ").click();");
        }

        private void jQuery(string query)
        {
            //if (webControl1.IsDomReady)
            //{
                webControl1.ExecuteJavascript(query);
            //}
        }

        private Awesomium.Core.JSValue jQueryWithResult(string query)
        {
            if (webControl1.IsDomReady)
            {
                return webControl1.ExecuteJavascriptWithResult(query, Constants.ASYNC_TIMEOUT);
            }
            else
            {
                return new Awesomium.Core.JSValue();
            }
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Show();
            WindowState = FormWindowState.Normal;
        }

        private void FrmMain_Resize(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.trayMinimize)
            {
                if (this.WindowState == FormWindowState.Minimized)
                {
                    Hide();
                }
            }
        }

        private void FrmMain_Activated(object sender, EventArgs e)
        {
            SetupGlobalHotkeys();
        }

        private void Exit_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.Save();
            closingFromTray = true;
            Close();
        }

        private void HideShow_Click(object sender, EventArgs e)
        {
            showHideWindow();
        }

        private void showHideWindow()
        {
            if (this.WindowState == FormWindowState.Minimized || this.Visible == false)
            {
                Show();
                WindowState = FormWindowState.Normal;
                TopMost = true;    
            }
            else
            {
                this.WindowState = FormWindowState.Minimized;
                if (Properties.Settings.Default.trayMinimize)
                {
                    Hide();
                }
            }
        }

        private void Play_Click(object sender, EventArgs e)
        {
            if (TaskbarManager.IsPlatformSupported)
            {
                //this only shortens the delay of the button change. Need to have a disabled state!
                if (!isMusicPlaying) { buttonPause.Icon = Properties.Resources.PlayerPause; isMusicPlaying = true; }
                else { buttonPause.Icon = Properties.Resources.PlayerPlay; isMusicPlaying = false; }
            }
          
            playerClickOn("player_play_pause");
        }

        private void Next_Click(object sender, EventArgs e)
        {
            playerClickOn("player_next");
        }

        private void Previous_Click(object sender, EventArgs e)
        {
            playerClickOn("player_previous");
        }

        private string getSongTitle()
        {
            return jQueryWithResult("return $('#playerDetails_nowPlaying .song').text();").ToString();
        }

        private string getSongArtist()
        {
            return jQueryWithResult("return $('#playerDetails_nowPlaying .artist').text();").ToString();
        }

        private bool getMusicState()
        {
            return jQueryWithResult("return $('#player_play_pause').hasClass('pause');").ToBoolean();
        }

        private void moveMouse()
        {
            jQuery("$('#page_wrapper').mousemove();");
        }

        private void SetupGlobalHotkeys()
        {
            hook.unregisterAllHotkeys(); //first unregister everything

            // register the media keys
            try { hook.RegisterHotKey(global::ModifierKeys.None, (Keys)Constants.VK_MEDIA_PLAY_PAUSE); }
            catch (InvalidOperationException exception) { } //MessageBox.Show(exception.Message);
            try { hook.RegisterHotKey(global::ModifierKeys.None, (Keys)Constants.VK_MEDIA_NEXT_TRACK); }
            catch (InvalidOperationException exception) { }
            try { hook.RegisterHotKey(global::ModifierKeys.None, (Keys)Constants.VK_MEDIA_PREV_TRACK); }
            catch (InvalidOperationException exception) { }

            //register other customizable hot keys
            try { hook.RegisterHotKey(hook.Win32ModifiersFromKeys((Keys)Properties.Settings.Default.hotkeyPlay), hook.getKeyWithoutModifier((Keys)Properties.Settings.Default.hotkeyPlay)); }
            catch (InvalidOperationException exception) { }
            try { hook.RegisterHotKey(hook.Win32ModifiersFromKeys((Keys)Properties.Settings.Default.hotkeyNext), hook.getKeyWithoutModifier((Keys)Properties.Settings.Default.hotkeyNext)); }
            catch (InvalidOperationException exception) { }
            try { hook.RegisterHotKey(hook.Win32ModifiersFromKeys((Keys)Properties.Settings.Default.hotkeyPrevious), hook.getKeyWithoutModifier((Keys)Properties.Settings.Default.hotkeyPrevious)); }
            catch (InvalidOperationException exception) { }
            try { hook.RegisterHotKey(hook.Win32ModifiersFromKeys((Keys)Properties.Settings.Default.hotkeyLike), hook.getKeyWithoutModifier((Keys)Properties.Settings.Default.hotkeyLike)); }
            catch (InvalidOperationException exception) { }
            try { hook.RegisterHotKey(hook.Win32ModifiersFromKeys((Keys)Properties.Settings.Default.hotkeyDislike), hook.getKeyWithoutModifier((Keys)Properties.Settings.Default.hotkeyDislike)); }
            catch (InvalidOperationException exception) { }
            try { hook.RegisterHotKey(hook.Win32ModifiersFromKeys((Keys)Properties.Settings.Default.hotkeyFavorite), hook.getKeyWithoutModifier((Keys)Properties.Settings.Default.hotkeyFavorite)); }
            catch (InvalidOperationException exception) { }
            try { hook.RegisterHotKey(hook.Win32ModifiersFromKeys((Keys)Properties.Settings.Default.hotkeyShowHide), hook.getKeyWithoutModifier((Keys)Properties.Settings.Default.hotkeyShowHide)); }
            catch (InvalidOperationException exception) { }
            try { hook.RegisterHotKey(hook.Win32ModifiersFromKeys((Keys)Properties.Settings.Default.hotkeyMute), hook.getKeyWithoutModifier((Keys)Properties.Settings.Default.hotkeyMute)); }
            catch (InvalidOperationException exception) { }
            try { hook.RegisterHotKey(hook.Win32ModifiersFromKeys((Keys)Properties.Settings.Default.hotkeyShuffle), hook.getKeyWithoutModifier((Keys)Properties.Settings.Default.hotkeyShuffle)); }
            catch (InvalidOperationException exception) { }
        }

        void hook_KeyPressed(object sender, KeyPressedEventArgs e)
        {
            switch (e.Key.ToString())
            {
                case "MediaPlayPause":
                    playerClickOn("player_play_pause");
                    return;
                case "MediaNextTrack":
                    playerClickOn("player_next");
                    return;
                case "MediaPreviousTrack":
                    playerClickOn("player_previous");
                    return;
                default:
                    break;
            }

            uint KeyAsInt = (uint)(e.Key | hook.keyToModifierKey(e.Modifier));
            if (KeyAsInt == Properties.Settings.Default.hotkeyPlay)
            {
                playerClickOn("player_play_pause");
            }
            else if (KeyAsInt == Properties.Settings.Default.hotkeyNext)
            {
                playerClickOn("player_next");
            }
            else if (KeyAsInt == Properties.Settings.Default.hotkeyPrevious)
            {
                playerClickOn("player_previous");
            }
            else if (KeyAsInt == Properties.Settings.Default.hotkeyLike)
            {
                LikeCurrentSong();
            }
            else if (KeyAsInt == Properties.Settings.Default.hotkeyDislike)
            {
                DislikeCurrentSong();
            }
            else if (KeyAsInt == Properties.Settings.Default.hotkeyFavorite)
            {
                jQueryClickOn("#playerDetails_nowPlaying_options");
                jQueryClickOn("#jjmenu_main .jj_menu_item_favorites");
            }
            else if (KeyAsInt == Properties.Settings.Default.hotkeyMute)
            {
                playerClickOn("player_volume");
            }
            else if (KeyAsInt == Properties.Settings.Default.hotkeyShowHide)
            {
                showHideWindow();
            }
            else if (KeyAsInt == Properties.Settings.Default.hotkeyShuffle)
            {
                playerClickOn("player_shuffle");
            }
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            OptionsForm optionsDialog = new OptionsForm();
            optionsDialog.ShowDialog(this);
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            webControl1.GoBack();
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            webControl1.GoForward();
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            OptionsForm optionsDialog = new OptionsForm();
            optionsDialog.ShowDialog(this);
        }

        private void About_Click(object sender, EventArgs e)
        {
            AboutBox1 aboutDialog = new AboutBox1();
            aboutDialog.ShowDialog(this);
        }

        private void currentSongTimer_Tick(object sender, EventArgs e)
        {
            if (webControl1.IsDomReady)
            {
                try
                {
                    string songTitle = getSongTitle();
                    string songArtist = getSongArtist();
                    //set the Windows title
                    if (songTitle.Length > 0)
                    {
                        this.Text = string.Format("%s - %s - %s", songTitle, songArtist, Constants.APPNAME);
                        //set the tray icon text if it is less than 63 characters (the max allowed)
                        if ((songTitle.Length + songArtist.Length + 3) < 63)
                        {
                            notifyIcon1.Text = string.Format("%s - %s", songTitle, songArtist);
                        }
                        else
                        {
                            try  // Get what you can up to max length.
                            {
                                notifyIcon1.Text = string.Format("%s - %s", songTitle, songArtist).Substring(0, 62);
                            }
                            catch // Possible you land right on and under, throwing exception.  handle with old fallback.
                            {
                                notifyIcon1.Text = Constants.APPNAME;
                            }
                        }
                    }

                    //control thumbail icons
                    if (TaskbarManager.IsPlatformSupported)
                    {
                        //the element class of the play button on grooveshark changes according to the music state (contains play/paused/nothing)
                        //I can't figure a better way to control the thumbnail states.
                        if (getMusicState())
                        {
                            buttonPause.Icon = Properties.Resources.PlayerPause;
                            isMusicPlaying = true;
                        }
                        else
                        {
                            buttonPause.Icon = Properties.Resources.PlayerPlay;
                            isMusicPlaying = false;
                        }
                    }


                   // HandleThumbnailButtons();
                }
                catch (NullReferenceException)
                {
                    //this should avoid weird errors
                }
            }
        }

        private void toolStripMenuItem1_Click_1(object sender, EventArgs e)
        {
            OptionsForm optionsDialog = new OptionsForm();
            optionsDialog.ShowDialog(this);
        }

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            AboutBox1 aboutDialog = new AboutBox1();
            aboutDialog.ShowDialog(this);
        }

        private void alwaysListeningTimer_Tick(object sender, EventArgs e)
        {
            if (webControl1.IsDomReady)
            {
                //this will simulate moving the mouse so that the player doesn't stop playing music after a few minutes of not interacting with the page
                moveMouse();
            }
        }

        private void FrmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!closingFromTray && Properties.Settings.Default.trayClose)
            {
                e.Cancel = true;
                Hide();
            }
        }

        private void Like_Click(object sender, EventArgs e)
        {
            LikeCurrentSong();
        }

        private void LikeCurrentSong()
        {
            jQueryClickOn("#queue_list_window .queue-item-active .smile");
        }

        private void DislikeCurrentSong()
        {
            jQueryClickOn("#queue_list_window .queue-item-active .frown");
        }

        private void Dislike_Click(object sender, EventArgs e)
        {
            DislikeCurrentSong();
        }

    }
}