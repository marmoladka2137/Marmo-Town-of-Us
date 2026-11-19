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
        readonly string root, saved;
        readonly ComboBox platform = new ComboBox();
        readonly TextBox gamePath = new TextBox();
        readonly Label status = new Label(), installed = new Label();
        readonly Button play = new Button(), install = new Button(), updates = new Button(), browse = new Button();
        readonly CheckBox autoUpdate = new CheckBox();
        bool busy;
        PublicRelease latest;
        ReleaseManifest release;
        public LauncherForm(string appRoot)
        {
            root = appRoot;
            saved = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MTOU", "Launcher", "game-path.txt");
            Text = "Marmo Town of Us"; ClientSize = new Size(1040, 710); MinimumSize = new Size(940, 700);
            FormBorderStyle = FormBorderStyle.FixedSingle; MaximizeBox = false;
            AutoScaleMode = AutoScaleMode.Dpi; StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("Segoe UI", 10); BackColor = Color.FromArgb(17, 20, 29); ForeColor = Color.WhiteSmoke;
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            var hero = new PictureBox { SizeMode = PictureBoxSizeMode.Zoom, Image = Asset("Marmo.Banner.png"),
                BackColor = BackColor, Bounds = new Rectangle(24, 22, 670, 377), Anchor = AnchorStyles.Left | AnchorStyles.Top };
            Controls.Add(hero);
            var mark = new PictureBox { Image = Asset("Marmo.DiscordLogo.png"), SizeMode = PictureBoxSizeMode.Zoom,
                Bounds = new Rectangle(750, 46, 188, 140) }; Controls.Add(mark);
            AddLabel("MARMO", 724, 204, 280, 44, 26, true);
            AddLabel("TOWN OF US", 727, 252, 280, 32, 17, true);
            AddLabel("1.1.0 Beta", 729, 295, 260, 28, 12, false).ForeColor = Color.FromArgb(242, 96, 110);
            AddLabel("Steam  ·  Epic Games", 729, 338, 280, 27, 11, false);
            var line = new Panel { Bounds = new Rectangle(28, 414, 984, 1), BackColor = Color.FromArgb(53, 58, 75) };
            line.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top; Controls.Add(line);
            AddLabel("YOUR INSTALLATION", 30, 437, 230, 26, 10, true).ForeColor = Color.FromArgb(153, 164, 188);
            platform.Items.AddRange(new object[] { "Steam", "Epic Games" }); platform.DropDownStyle = ComboBoxStyle.DropDownList;
            platform.SetBounds(815, 437, 194, 29); Controls.Add(platform); platform.SelectedIndex = 0;
            gamePath.SetBounds(30, 479, 800, 31); gamePath.BackColor = Color.FromArgb(29, 34, 47);
            gamePath.ForeColor = ForeColor; gamePath.BorderStyle = BorderStyle.FixedSingle; Controls.Add(gamePath);
            Configure(browse, "Browse", 847, 476, 163); browse.Click += Browse;
            installed.SetBounds(30, 519, 760, 26); installed.ForeColor = Color.FromArgb(156, 171, 197); Controls.Add(installed);
            autoUpdate.Text = "Check and install public MTOU updates automatically"; autoUpdate.Checked = true;
            autoUpdate.SetBounds(30, 554, 740, 28); Controls.Add(autoUpdate);
            Configure(play, "PLAY MTOU", 30, 602, 280); play.BackColor = Color.FromArgb(183, 31, 60);
            Configure(install, "Install a new copy", 326, 602, 236);
            Configure(updates, "Check updates", 579, 602, 220);
            play.Click += Play; install.Click += Install; updates.Click += async delegate { await CheckUpdates(true); };
            var discord = new LinkLabel { Text = "dc.gg/mtou  ↗", LinkColor = Color.FromArgb(147, 165, 255),
                ActiveLinkColor = Color.White, VisitedLinkColor = Color.FromArgb(147, 165, 255),
                Bounds = new Rectangle(831, 659, 180, 27), TextAlign = ContentAlignment.MiddleRight };
            discord.LinkClicked += delegate { Open("https://dc.gg/mtou"); }; Controls.Add(discord);
            status.SetBounds(30, 659, 780, 28); status.AutoEllipsis = true; Controls.Add(status);
            gamePath.Text = LauncherCore.FindGame(root, Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), saved) ?? "";
            gamePath.TextChanged += delegate { ShowRevision(); };
            Shown += async delegate { ShowRevision(); await CheckUpdates(false); };
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
        void ShowRevision()
        {
            string revision = Revision();
            installed.Text = String.IsNullOrEmpty(revision) ? "Choose an MTOU installation, or install a new copy." : "Installed revision  " + revision;
        }
        void Busy(bool value)
        {
            busy = value;
            foreach (Control control in new Control[] { play, install, updates, browse, platform, gamePath, autoUpdate }) control.Enabled = !value;
        }
        void Browse(object sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog { Description = "Choose your installed MTOU folder", ShowNewFolderButton = false })
                if (dialog.ShowDialog(this) == DialogResult.OK) { gamePath.Text = dialog.SelectedPath; ShowRevision(); }
        }
        async Task CheckUpdates(bool explicitClick)
        {
            if (busy) return; Busy(true); status.Text = "Checking public GitHub releases…";
            try
            {
                latest = await Task.Run(() => ReleaseUpdate.Latest());
                release = await Task.Run(() => new JavaScriptSerializer().Deserialize<ReleaseManifest>(
                    System.Text.Encoding.UTF8.GetString(ReleaseUpdate.Download(latest.ManifestUrl))));
                ReleaseUpdate.Validate(release);
                if (!LauncherCore.IsModdedGame(gamePath.Text)) { status.Text = "Available  " + release.Version; return; }
                if (!ReleaseUpdate.Newer(release.Revision, Revision())) { status.Text = "Your MTOU is up to date."; return; }
                status.Text = "New public release  " + release.Revision;
                if (LauncherCore.IsRunning(gamePath.Text)) { status.Text = "Update ready. Close MTOU to install it."; return; }
                if (autoUpdate.Checked || explicitClick)
                {
                    string folder = gamePath.Text; var manifest = release; string address = latest.ArchiveUrl;
                    status.Text = "Downloading and installing " + release.Revision + "…";
                    await Task.Run(() => ReleaseUpdate.Apply(folder, manifest, ReleaseUpdate.Download(address)));
                    ShowRevision(); status.Text = "Updated. Ready to play.";
                }
            }
            catch (Exception error) { status.Text = "Update check failed. You can still play. " + error.Message; }
            finally { Busy(false); }
        }
        async void Play(object sender, EventArgs e)
        {
            if (busy) return;
            if (autoUpdate.Checked) await CheckUpdates(false);
            try
            {
                if (!LauncherCore.IsModdedGame(gamePath.Text)) throw new IOException("Select a completed MTOU installation first.");
                if (!LauncherCore.IsRunning(gamePath.Text))
                {
                    LauncherCore.Remember(gamePath.Text, saved);
                    Process.Start(LauncherCore.StartInfo(gamePath.Text));
                }
                status.Text = "MTOU is running.";
            }
            catch (Exception error) { MessageBox.Show(this, error.Message, "Cannot start MTOU"); }
        }
        async void Install(object sender, EventArgs e)
        {
            string manifestPath = Path.Combine(root, "installer-manifest.json");
            if (!File.Exists(manifestPath)) { status.Text = "Use the complete launcher package from GitHub Releases."; return; }
            string source = null;
            bool epic = platform.SelectedIndex == 1;
            var candidates = epic ? InstallerCore.FindEpicGames(InstallerCore.EpicManifestRoot) : InstallerCore.FindSteamGames();
            using (var dialog = new FolderBrowserDialog { Description = "Choose your installed " + (epic ? "Epic Games" : "Steam") + " Among Us",
                SelectedPath = candidates.FirstOrDefault() ?? "", ShowNewFolderButton = false })
            { if (dialog.ShowDialog(this) != DialogResult.OK) return; source = dialog.SelectedPath; }
            string destination;
            using (var dialog = new FolderBrowserDialog { Description = "Choose a parent folder for a NEW MTOU game copy" })
            { if (dialog.ShowDialog(this) != DialogResult.OK) return; destination = Path.Combine(dialog.SelectedPath, epic ? "Marmo Town of Us Epic" : "Marmo Town of Us"); }
            Busy(true);
            try
            {
                var manifest = InstallerManifest.Read(manifestPath); manifest.Platform = epic ? "Epic" : "Steam";
                var core = new InstallerCore(manifest, root);
                core.Progress = delegate(int percent, string message) { BeginInvoke((Action)(() => status.Text = percent + "%  " + message)); };
                string game = await Task.Run(() => core.Install(source, destination, null));
                var updateManifest = new JavaScriptSerializer().Deserialize<ReleaseManifest>(File.ReadAllText(Path.Combine(root, "mtou-release.json")));
                await Task.Run(() => ReleaseUpdate.Apply(game, updateManifest, File.ReadAllBytes(Path.Combine(root, "MTOU-Mods-Windows.zip"))));
                gamePath.Text = game; LauncherCore.Remember(game, saved); ShowRevision(); status.Text = "Installed. Ready to play.";
            }
            catch (Exception error) { MessageBox.Show(this, error.Message, "Installation failed"); status.Text = error.Message; }
            finally { Busy(false); }
        }
    }
}
