// GPL-3.0. Windows launcher interface for Marmo Town of Us.
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using Marmo.SteamInstaller;

namespace Marmo.Launcher
{
    public sealed class LauncherForm : Form
    {
        readonly string root, saved, preferences, localData;
        readonly Func<bool,string[]> discover;
        readonly ComboBox sourceGame = new ComboBox();
        readonly Button detect = new Button(), chooseSource = new Button();
        readonly LinkLabel useDefault = new LinkLabel();
        readonly ToolTip tips = new ToolTip();
        readonly ComboBox versions = new ComboBox();
        bool fillingVersions;
        VersionChoice pinned;
        readonly ComboBox platform = new ComboBox();
        readonly TextBox gamePath = new TextBox();
        readonly Label status = new Label(), installed = new Label();
        readonly Button play = new Button(), install = new Button(), updates = new Button(), browse = new Button();
        readonly CheckBox autoUpdate = new CheckBox();
        bool busy;
        PublicRelease latest;
        ReleaseManifest release;
        public LauncherForm(string appRoot, bool checkForUpdates = true, string profileDirectory = null, Func<bool,string[]> installations = null)
        {
            root = appRoot;
            localData = profileDirectory ?? Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            preferences = profileDirectory ?? Path.Combine(localData,"MTOU","Launcher");
            saved = Path.Combine(preferences,"game-path.txt");
            discover = installations ?? (epic => epic ? InstallerCore.FindEpicGames(InstallerCore.EpicManifestRoot) : InstallerCore.FindSteamGames());
            Text = "MTOU Launcher " + LauncherVersion.DisplayVersion; ClientSize = new Size(1040, 710); MinimumSize = new Size(940, 700);
            FormBorderStyle = FormBorderStyle.FixedSingle; MaximizeBox = false;
            AutoScaleMode = AutoScaleMode.Dpi; StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("Segoe UI", 10); BackColor = Color.FromArgb(17, 20, 29); ForeColor = Color.WhiteSmoke;
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            var hero = new PictureBox { SizeMode = PictureBoxSizeMode.Zoom, Image = Asset("Marmo.Banner.png"),
                BackColor = BackColor, Bounds = new Rectangle(24, 16, 640, 360), Anchor = AnchorStyles.Left | AnchorStyles.Top };
            Controls.Add(hero);
            var mark = new PictureBox { Image = Asset("Marmo.DiscordLogo.png"), SizeMode = PictureBoxSizeMode.Zoom,
                Bounds = new Rectangle(750, 46, 188, 140) }; Controls.Add(mark);
            AddLabel("MARMO", 724, 204, 280, 44, 26, true);
            AddLabel("TOWN OF US", 727, 252, 280, 32, 17, true);
            AddLabel("Launcher " + LauncherVersion.DisplayVersion, 729, 295, 260, 28, 12, false).ForeColor = Color.FromArgb(242, 96, 110);
            AddLabel("Steam  ·  Epic Games", 729, 338, 280, 27, 11, false);
            var line = new Panel { Bounds = new Rectangle(28, 390, 984, 1), BackColor = Color.FromArgb(53, 58, 75) };
            line.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top; Controls.Add(line);
            AddLabel("ORIGINAL AMONG US", 30, 405, 270, 25, 10, true).ForeColor = Color.FromArgb(153,164,188);
            platform.Items.AddRange(new object[] { "Steam", "Epic Games" }); platform.DropDownStyle=ComboBoxStyle.DropDownList;
            platform.SetBounds(648,402,182,30); Controls.Add(platform); platform.SelectedIndex=0;
            Configure(detect,"Auto-detect",847,399,163); detect.Height=36;
            detect.Click += delegate { LoadPlatform(false); };
            sourceGame.DropDownStyle=ComboBoxStyle.DropDownList; sourceGame.SetBounds(30,444,800,31);
            sourceGame.DropDownWidth=960; Controls.Add(sourceGame);
            sourceGame.SelectedIndexChanged += delegate { string source=SelectedSource();tips.SetToolTip(sourceGame,source ?? "Choose Among Us.exe from your installed game.");if(source!=null)WritePreference(StoreFile("source"),source);ShowRevision(); };
            Configure(chooseSource,"Choose .exe…",847,441,163); chooseSource.Height=36; chooseSource.Click += ChooseSource;
            AddLabel("MTOU COPY — original game stays unchanged",30,480,600,24,10,true).ForeColor=Color.FromArgb(153,164,188);
            useDefault.Text="Use default"; useDefault.LinkColor=Color.FromArgb(147,165,255); useDefault.SetBounds(715,479,115,24);
            useDefault.LinkClicked += delegate { SetTarget(pinned==null?LauncherCore.DefaultDestination(localData,IsEpic):VersionLibrary.Destination(localData,IsEpic,pinned.Tag)); }; Controls.Add(useDefault);
            gamePath.SetBounds(30,509,800,31);gamePath.BackColor=Color.FromArgb(29,34,47);gamePath.ForeColor=ForeColor;
            gamePath.ReadOnly=true;gamePath.BorderStyle=BorderStyle.FixedSingle;gamePath.Enter += delegate { gamePath.SelectAll(); };Controls.Add(gamePath);
            Configure(browse,"Choose folder…",847,506,163);browse.Height=36;browse.Click+=Browse;
            installed.SetBounds(30,548,980,23);installed.ForeColor=Color.FromArgb(156,171,197);Controls.Add(installed);
            autoUpdate.Text = "Check and install public MTOU updates automatically"; autoUpdate.Checked = true;
            autoUpdate.SetBounds(30, 575, 740, 28); Controls.Add(autoUpdate);
            Configure(play, "PLAY MTOU", 30, 613, 280); play.BackColor = Color.FromArgb(183, 31, 60);
            Configure(install, "INSTALL MTOU", 326, 613, 236);
            Configure(updates, "Check updates", 579, 613, 220);
            play.Click += Play; install.Click += Install; updates.Click += async delegate { await LoadVersions(); };
            var discord = new LinkLabel { Text = "dc.gg/mtou  ↗", LinkColor = Color.FromArgb(147, 165, 255),
                ActiveLinkColor = Color.White, VisitedLinkColor = Color.FromArgb(147, 165, 255),
                Bounds = new Rectangle(831, 673, 180, 27), TextAlign = ContentAlignment.MiddleRight };
            discord.LinkClicked += delegate { Open("https://dc.gg/mtou"); }; Controls.Add(discord);
            status.SetBounds(30, 673, 780, 28); status.AutoEllipsis = true; Controls.Add(status);
            gamePath.TextChanged += delegate { ShowRevision(); };
            // Keep the native installation controls intact; add a dedicated version row.
            foreach(Control control in Controls)if(control.Top>=399)control.Top+=48;
            ClientSize=new Size(1040,758);MinimumSize=new Size(940,748);
            AddLabel("MTOU VERSION",30,402,175,27,10,true);
            versions.DropDownStyle=ComboBoxStyle.DropDownList;versions.SetBounds(220,400,790,32);
            versions.Items.Add(new VersionChoice {Automatic=true});versions.SelectedIndex=0;Controls.Add(versions);
            versions.SelectedIndexChanged+=delegate {if(!fillingVersions && !busy)BeginInvoke((Action)(async delegate {if(!fillingVersions && !busy)await SelectVersion();}));};
            string preferred=ReadPreference("platform.txt");
            if(LauncherCore.IsModdedGame(root))preferred=LauncherCore.MatchesStore(root,true)?"Epic":"Steam";
            if(preferred=="Epic" || (preferred==null && SafeDiscover(false).Length==0 && SafeDiscover(true).Length>0)) platform.SelectedIndex=1;
            LoadPlatform(true);
            platform.SelectedIndexChanged += delegate { LoadPlatform(true); if(pinned!=null)SetTarget(VersionLibrary.Destination(localData,IsEpic,pinned.Tag)); WritePreference("platform.txt",IsEpic?"Epic":"Steam"); };
            Shown += async delegate { ShowRevision(); if(checkForUpdates)await LoadVersions(); };
            FormClosed += delegate { tips.Dispose(); };
            FormClosing += delegate(object sender, FormClosingEventArgs e) { if (busy) e.Cancel = true; };
        }
        static Image Asset(string name)
        {
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(name))
                return stream == null ? null : new Bitmap(Image.FromStream(stream));
        }
        Label AddLabel(string text, int x, int y, int w, int h, int size, bool bold)
        {
            var label = new Label { Text = text, Bounds = new Rectangle(x, y, w, h), Font = new Font("Segoe UI", size, bold ? FontStyle.Bold : FontStyle.Regular) };
            Controls.Add(label); return label;
        }
        void Configure(Button button, string text, int x, int y, int width)
        {
            button.Text = text; button.SetBounds(x, y, width, 44); button.BackColor = Color.FromArgb(40, 47, 66);
            button.ForeColor = Color.White; button.FlatStyle = FlatStyle.Flat; button.FlatAppearance.BorderSize = 0;
            button.Cursor = Cursors.Hand; Controls.Add(button);
        }
        static void Open(string url)
        {
            try { Process.Start(new ProcessStartInfo(url) { UseShellExecute = true }); }
            catch (System.ComponentModel.Win32Exception) { MessageBox.Show("Open this link in your browser: " + url, "Discord"); }
        }
        string Revision()
        {
            try { return AssemblyName.GetAssemblyName(Path.Combine(gamePath.Text, "BepInEx", "plugins", "TOU-Marmo.dll")).Version.ToString(); }
            catch (IOException) { return ""; } catch (BadImageFormatException) { return ""; } catch (ArgumentException) { return ""; }
            catch (UnauthorizedAccessException) { return ""; }
        }
        bool IsEpic { get { return platform.SelectedIndex==1; } }
        string StoreFile(string name) { return name+(name=="destination" && pinned!=null?"-"+pinned.Tag:"")+"-"+(IsEpic?"epic":"steam")+".txt"; }
        string ReadPreference(string name)
        {
            try { string file=Path.Combine(preferences,name);return File.Exists(file)?File.ReadAllText(file).Trim():null; }
            catch(IOException){return null;}catch(UnauthorizedAccessException){return null;}
        }
        void WritePreference(string name,string value)
        {
            try { Directory.CreateDirectory(preferences);File.WriteAllText(Path.Combine(preferences,name),value); }
            catch(IOException){}catch(UnauthorizedAccessException){}
        }
        string[] SafeDiscover(bool epic)
        {
            try { return discover(epic).Where(LauncherCore.IsOriginalGame).Distinct(StringComparer.OrdinalIgnoreCase).ToArray(); }
            catch(IOException){return new string[0];}catch(UnauthorizedAccessException){return new string[0];}
            catch(ArgumentException){return new string[0];}catch(System.Security.SecurityException){return new string[0];}
        }
        string SelectedSource(){return LauncherCore.FolderFromSelection(sourceGame.SelectedItem as string);}
        void LoadPlatform(bool changeTarget)
        {
            string previous=SelectedSource();var choices=SafeDiscover(IsEpic);
            string remembered=LauncherCore.FolderFromSelection(ReadPreference(StoreFile("source")));
            if(remembered!=null&&!choices.Contains(remembered,StringComparer.OrdinalIgnoreCase))choices=choices.Concat(new[]{remembered}).ToArray();
            sourceGame.Items.Clear();sourceGame.Items.AddRange(choices.Cast<object>().ToArray());
            if(choices.Length==0)sourceGame.Items.Add("Not detected — click Choose .exe…");
            sourceGame.SelectedIndex=0;
            string selected=changeTarget?remembered:previous;
            if(selected!=null&&choices.Contains(selected,StringComparer.OrdinalIgnoreCase))sourceGame.SelectedItem=choices.First(x=>x.Equals(selected,StringComparison.OrdinalIgnoreCase));
            if(changeTarget)
            {
                string target=ReadPreference(StoreFile("destination"));
                if(String.IsNullOrEmpty(target))target=LauncherCore.DefaultDestination(localData,IsEpic);
                if(LauncherCore.MatchesStore(root,IsEpic))target=root;
                else if(!IsEpic && String.IsNullOrEmpty(ReadPreference(StoreFile("destination"))))
                {
                    string existing=LauncherCore.FindGame(root,Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),saved);
                    if(existing!=null&&LauncherCore.MatchesStore(existing,false)&&profileDirectoryIsDefault())target=existing;
                }
                SetTarget(target);
            }
            status.Text=LauncherCore.IsModdedGame(gamePath.Text)?"MTOU selected. Ready to play."
                :SelectedSource()==null?"Among Us was not found. Choose its Among Us.exe once.":"Original game detected. Install MTOU into the separate folder below.";
            ShowRevision();
        }
        bool profileDirectoryIsDefault(){return preferences==Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"MTOU","Launcher");}
        void SetTarget(string target)
        {
            try
            {
                if(Directory.Exists(target)&&!LauncherCore.IsModdedGame(target))target=LauncherCore.DestinationFromSelection(target,IsEpic);
                gamePath.Text=Path.GetFullPath(target);tips.SetToolTip(gamePath,gamePath.Text);
                WritePreference(StoreFile("destination"),gamePath.Text);
                status.Text=LauncherCore.IsModdedGame(gamePath.Text)?"MTOU selected. Ready to play.":"New MTOU copy: "+gamePath.Text;
            }
            catch(Exception error){MessageBox.Show(this,error.Message,"Choose an MTOU folder");}
            ShowRevision();
        }
        void ShowRevision()
        {
            bool ready=LauncherCore.IsModdedGame(gamePath.Text);
            installed.Text=ready?"MTOU "+LauncherCore.DisplayVersion(gamePath.Text)+"  ·  Ready to play":"Not installed yet  ·  Choose INSTALL MTOU to create your separate copy.";
            tips.SetToolTip(installed,ready?"Build "+Revision():"");
            play.Enabled=!busy&&ready;
            install.Enabled=!busy&&!ready&&SelectedSource()!=null;
        }
        void Busy(bool value)
        {
            bool previousFill=fillingVersions;fillingVersions=true;
            try
            {
                busy=value;
                foreach(Control control in new Control[]{play,install,updates,browse,platform,gamePath,autoUpdate,sourceGame,detect,chooseSource,useDefault,versions})control.Enabled=!value;
                ShowRevision();autoUpdate.Enabled=!value && pinned==null;
            }
            finally {fillingVersions=previousFill;}
        }
        void ChooseSource(object sender,EventArgs e)
        {
            using(var dialog=new OpenFileDialog { Title="Select the original Among Us.exe ("+(IsEpic?"Epic Games":"Steam")+")",Filter="Among Us executable|Among Us.exe",CheckFileExists=true,Multiselect=false,RestoreDirectory=true,FileName="Among Us.exe" })
            {
                string current=SelectedSource();
                if(current!=null)dialog.InitialDirectory=current;
                else
                {
                    string location=IsEpic?Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),"Epic Games")
                        :Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),"Steam","steamapps","common");
                    if(Directory.Exists(location))dialog.InitialDirectory=location;
                }
                if(dialog.ShowDialog(this)!=DialogResult.OK)return;
                string folder=LauncherCore.FolderFromSelection(dialog.FileName);
                if(folder==null){MessageBox.Show(this,"Choose Among Us.exe inside the installed game's folder.","Among Us not found");return;}
                if(LauncherCore.IsModdedGame(folder)){SetTarget(folder);status.Text="Existing MTOU found. Ready to play.";return;}
                if(!sourceGame.Items.Contains(folder))sourceGame.Items.Add(folder);
                sourceGame.SelectedItem=folder;WritePreference(StoreFile("source"),folder);
                status.Text="Original game selected. Your MTOU copy uses the folder below.";ShowRevision();
            }
        }
        void Browse(object sender,EventArgs e)
        {
            string selected=FolderPicker.Choose(this,"Choose an existing MTOU folder or a parent folder for a new copy",gamePath.Text);
            if(selected==null)return;
            try { SetTarget(LauncherCore.DestinationFromSelection(selected,IsEpic)); }
            catch(IOException error){MessageBox.Show(this,error.Message,"Choose a separate MTOU folder");}
        }
        void RememberGame()
        {
            LauncherCore.Remember(gamePath.Text,saved);
            WritePreference(StoreFile("destination"),gamePath.Text);
            string source=SelectedSource();if(source!=null)WritePreference(StoreFile("source"),source);
        }
        async Task LoadVersions()
        {
            if(busy)return;Busy(true);
            try
            {
                string cache=Path.Combine(preferences,"release-catalog.json");
                string json;
                try{json=await Task.Run(()=>System.Text.Encoding.UTF8.GetString(ReleaseUpdate.Download(ReleaseUpdate.Feed)));Directory.CreateDirectory(preferences);File.WriteAllText(cache,json);}
                catch{if(!File.Exists(cache))throw;json=File.ReadAllText(cache);}
                var choices=VersionLibrary.Parse(json);
                string keep=pinned==null?ReadPreference("selected-version.txt"):pinned.Tag;
                fillingVersions=true;versions.Items.Clear();versions.Items.Add(new VersionChoice {Automatic=true});
                foreach(var choice in choices)versions.Items.Add(choice);
                versions.SelectedIndex=0;
                for(int i=1;i<versions.Items.Count;i++)if(((VersionChoice)versions.Items[i]).Tag==keep)versions.SelectedIndex=i;
                fillingVersions=false;
            }
            catch(Exception error){status.Text="Version list unavailable: "+error.Message;}
            finally{fillingVersions=false;Busy(false);}
            await SelectVersion();
        }
        async Task SelectVersion()
        {
            if(busy)return;
            var selected=versions.SelectedItem as VersionChoice;
            pinned=selected==null||selected.Automatic?null:selected;release=null;latest=null;
            WritePreference("selected-version.txt",pinned==null?"latest":pinned.Tag);
            if(pinned==null){LoadPlatform(true);autoUpdate.Enabled=true;await CheckUpdates(false);return;}
            SetTarget(VersionLibrary.Destination(localData,IsEpic,pinned.Tag));
            Busy(true);status.Text="Loading MTOU "+pinned.Name+"…";
            try
            {
                var selectedVersion=pinned;
                release=await Task.Run(()=>VersionLibrary.Load(selectedVersion,Path.Combine(localData,"MTOU","VersionCache")));
                latest=new PublicRelease {Tag=pinned.Tag,ManifestUrl=pinned.ManifestUrl,ArchiveUrl=pinned.ArchiveUrl};
                status.Text="Selected MTOU "+release.Version+". Installed separately; automatic upgrades are disabled.";
            }
            catch(Exception error){status.Text="Could not load version: "+error.Message;}
            finally{Busy(false);}
        }
        async Task CheckUpdates(bool explicitClick)
        {
            if (busy || pinned!=null) return; Busy(true); status.Text = "Checking public GitHub releases…";
            try
            {
                latest = await Task.Run(() => ReleaseUpdate.Latest());
                release = await Task.Run(() => new JavaScriptSerializer().Deserialize<ReleaseManifest>(
                    System.Text.Encoding.UTF8.GetString(ReleaseUpdate.Download(latest.ManifestUrl))));
                ReleaseUpdate.Validate(release);
                if (!LauncherCore.IsModdedGame(gamePath.Text)) { status.Text = "Available  " + release.Version; return; }
                if (!ReleaseUpdate.Newer(release.Revision, Revision())) { status.Text = "Your MTOU is up to date."; return; }
                status.Text = "New MTOU version  " + release.Version;
                if (LauncherCore.IsRunning(gamePath.Text)) { status.Text = "Update ready. Close MTOU to install it."; return; }
                if (autoUpdate.Checked || explicitClick)
                {
                    string folder = gamePath.Text; var manifest = release; string address = latest.ArchiveUrl;
                    status.Text = "Downloading and installing MTOU " + release.Version + "…";
                    await Task.Run(() => ReleaseUpdate.Apply(folder, manifest, ReleaseUpdate.DownloadArchive(manifest, address)));
                    ShowRevision(); status.Text = "Updated. Ready to play.";
                }
            }
            catch (Exception error) { status.Text = "Update check failed. You can still play. " + error.Message; }
            finally { Busy(false); }
        }
        async void Play(object sender, EventArgs e)
        {
            if (busy) return;
            if (autoUpdate.Checked && pinned==null) await CheckUpdates(false);
            try
            {
                if(pinned!=null && (release==null || Revision()!=release.Revision))throw new IOException("Install the selected version first. Other versions will stay unchanged.");
                if (!LauncherCore.IsModdedGame(gamePath.Text)) throw new IOException("Select a completed MTOU installation first.");
                if (!LauncherCore.IsRunning(gamePath.Text))
                {
                    RememberGame();
                    if (LauncherCore.MatchesStore(gamePath.Text, true))
                    {
                        status.Text = "Preparing Epic sign-in starter…";
                        string folder = gamePath.Text; Busy(true);
                        try { await Task.Run(() => LauncherCore.EnsureEpicStarter(folder)); }
                        finally { Busy(false); }
                    }
                    Process.Start(LauncherCore.StartInfo(gamePath.Text));
                }
                status.Text = LauncherCore.MatchesStore(gamePath.Text, true) ? "Epic starter opened. Complete its sign-in prompt if requested." : "MTOU is running.";
            }
            catch (Exception error) { MessageBox.Show(this, error.Message, "Cannot start MTOU"); }
        }
        async void Install(object sender, EventArgs e)
        {
            string manifestPath = Path.Combine(root, "installer-manifest.json");
            if (!File.Exists(manifestPath)) { status.Text = "Use the complete launcher package from GitHub Releases."; return; }
            string source=SelectedSource();
            if(source==null){ChooseSource(sender,e);source=SelectedSource();if(source==null)return;}
            bool epic=IsEpic;string destination=gamePath.Text;
            Busy(true);
            try
            {
                var manifest = InstallerManifest.Read(manifestPath); manifest.Platform = epic ? "Epic" : "Steam";
                var core = new InstallerCore(manifest, root);
                core.Progress = delegate(int percent, string message) { BeginInvoke((Action)(() => status.Text = percent + "%  " + message)); };
                core.ValidateSource(source); InstallerCore.ValidateDestination(source, destination, root);
                var updateManifest = new JavaScriptSerializer().Deserialize<ReleaseManifest>(File.ReadAllText(Path.Combine(root, "mtou-release.json")));
                if (pinned!=null) {if(release==null)throw new IOException("Load the selected version before installing.");updateManifest=release;}
                else if (release != null && ReleaseUpdate.Newer(release.Revision, updateManifest.Revision)) updateManifest = release;
                ReleaseUpdate.Validate(updateManifest);
                VersionLibrary.PrepareInstaller(manifest, updateManifest);
                status.Text = "Preparing verified mod files…";
                string versionCache=Path.Combine(localData,"MTOU","VersionCache");
                string legacyArchive=Path.Combine(versionCache,updateManifest.ArchiveSha256+"-mods.zip");
                string localArchive=File.Exists(legacyArchive)?legacyArchive:Path.Combine(root,"MTOU-Mods-Windows.zip");
                byte[] modArchive = await Task.Run(() => File.Exists(localArchive) && String.Equals(InstallerCore.Hash(localArchive), updateManifest.ArchiveSha256, StringComparison.OrdinalIgnoreCase)
                    ? File.ReadAllBytes(localArchive) : ReleaseUpdate.DownloadArchive(updateManifest, updateManifest == release && latest != null ? latest.ArchiveUrl : ReleaseUpdate.Channel().ArchiveUrl));
                if (!String.Equals(ReleaseUpdate.Hash(modArchive), updateManifest.ArchiveSha256, StringComparison.OrdinalIgnoreCase))
                    throw new IOException("Mod package failed SHA-256 verification.");
                string cache = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MTOU", "InstallerCache");
                Directory.CreateDirectory(cache); InstallerCore.NoLinks(cache);
                string cachedArchive = Path.Combine(cache, updateManifest.ArchiveSha256 + "-mods.zip");
                InstallerCore.NoLinks(cachedArchive);
                if (!File.Exists(cachedArchive))
                {
                    string temporary = cachedArchive + "." + Guid.NewGuid().ToString("N") + ".tmp";
                    try
                    {
                        File.WriteAllBytes(temporary, modArchive);
                        try { File.Move(temporary, cachedArchive); }
                        catch (IOException) { if (!File.Exists(cachedArchive)) throw; }
                    }
                    finally { if (File.Exists(temporary)) File.Delete(temporary); }
                }
                if (!String.Equals(InstallerCore.Hash(cachedArchive), updateManifest.ArchiveSha256, StringComparison.OrdinalIgnoreCase))
                    throw new IOException("Cached mod package failed verification.");
                core.ModArchivePath = cachedArchive;
                string game = await Task.Run(() => core.Install(source, destination, null));
                await Task.Run(() => ReleaseUpdate.Apply(game, updateManifest, modArchive));
                gamePath.Text = game; RememberGame(); ShowRevision(); status.Text = "Installed. Ready to play.";
            }
            catch (Exception error) { MessageBox.Show(this, error.Message, "Installation failed"); status.Text = error.Message; }
            finally { Busy(false); }
        }
    }
}
