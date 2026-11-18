// Marmo Town of Us installer. GPL-3.0; see LICENSE.
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

[assembly: AssemblyTitle("Marmo Town of Us Steam Installer")]
[assembly: AssemblyProduct("Marmo Town of Us")]
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]

namespace Marmo.SteamInstaller
{
    static class InstallerApp
    {
        [STAThread]
        static int Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            try
            {
                string root = AppDomain.CurrentDomain.BaseDirectory;
                var manifest = InstallerManifest.Read(Path.Combine(root, "installer-manifest.json"));
                var core = new InstallerCore(manifest, root);
                if (args.Length > 0 && args[0] == "--install")
                {
                    string source = Arg(args, "--source");
                    string destination = Arg(args, "--destination");
                    core.Install(source, destination, Arg(args, "--dependency-cache"));
                    return 0;
                }
                Application.Run(new InstallerForm(core)); return 0;
            }
            catch (Exception error)
            {
                MessageBox.Show(error.Message, "MTOU installation failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return 1;
            }
        }
        static string Arg(string[] args, string name)
        {
            for (int i = 0; i + 1 < args.Length; i++) if (args[i] == name) return args[i + 1];
            return null;
        }
    }

    public sealed class InstallerForm : Form
    {
        readonly InstallerCore core;
        readonly TextBox source = new TextBox();
        readonly TextBox destination = new TextBox();
        readonly Button start = new Button();
        readonly Button browseSource = new Button();
        readonly Button browseDestination = new Button();
        readonly Button play = new Button();
        readonly CheckBox shortcut = new CheckBox();
        readonly Label status = new Label();
        readonly ProgressBar progress = new ProgressBar();
        readonly BackgroundWorker worker = new BackgroundWorker();
        bool busy;
        string installed;
        string logFile;
        int lastProgress = -1;

        public InstallerForm(InstallerCore engine)
        {
            core = engine;
            Text = "Marmo Town of Us — Steam installer";
            Font = new Font("Segoe UI", 10);
            BackColor = Color.FromArgb(27, 29, 34); ForeColor = Color.WhiteSmoke;
            ClientSize = new Size(740, 440); MinimumSize = new Size(700, 480);
            StartPosition = FormStartPosition.CenterScreen;
            AutoScaleMode = AutoScaleMode.Font;
            var title = Label("Marmo Town of Us", 24, 18, 685, 40);
            title.Font = new Font("Segoe UI", 22, FontStyle.Bold); title.ForeColor = Color.FromArgb(245, 80, 80);
            Label("1.1.0 Beta  |  Steam / Windows x64", 26, 62, 680, 24);
            Label("Creates a separate modded copy from your installed Steam game.", 26, 96, 685, 24);
            Label("Steam Among Us folder", 26, 139, 630, 23);
            ConfigurePath(source, 166); ConfigureBrowse(browseSource, "Browse...", 165);
            browseSource.Click += delegate
            {
                using (var dialog = new FolderBrowserDialog { Description = "Select Steam's Among Us game folder", SelectedPath = source.Text })
                    if (dialog.ShowDialog(this) == DialogResult.OK) source.Text = dialog.SelectedPath;
            };
            Label("New folder for MTOU", 26, 207, 630, 23);
            ConfigurePath(destination, 234); ConfigureBrowse(browseDestination, "Choose...", 233);
            destination.Text = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "Marmo Town of Us");
            browseDestination.Click += delegate
            {
                using (var dialog = new FolderBrowserDialog { Description = "Select a parent folder; MTOU will create a new subfolder" })
                    if (dialog.ShowDialog(this) == DialogResult.OK) destination.Text = Path.Combine(dialog.SelectedPath, "Marmo Town of Us");
            };
            shortcut.Text = "Create a desktop shortcut"; shortcut.Checked = true;
            shortcut.SetBounds(26, 278, 400, 26); Controls.Add(shortcut);
            progress.SetBounds(26, 317, 686, 18); progress.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            Controls.Add(progress);
            status.SetBounds(26, 341, 686, 25); status.Text = "Internet is required to download official dependencies.";
            status.AutoEllipsis = true; status.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right; Controls.Add(status);
            start.Text = "Install MTOU"; start.SetBounds(535, 383, 177, 36); start.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            start.BackColor = Color.FromArgb(190, 45, 55); start.FlatStyle = FlatStyle.Flat; Controls.Add(start);
            play.Text = "Play MTOU"; play.SetBounds(345, 383, 177, 36); play.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            play.Visible = false; Controls.Add(play);
            play.Click += delegate
            {
                try { Process.Start(new ProcessStartInfo(Path.Combine(installed, "Among Us.exe")) { WorkingDirectory = installed }); }
                catch (Exception error) { MessageBox.Show(this, error.Message, "Cannot start MTOU"); }
            };
            start.Click += StartInstall;
            worker.DoWork += delegate(object sender, DoWorkEventArgs e)
            {
                var paths = (string[])e.Argument;
                e.Result = core.Install(paths[0], paths[1], null);
            };
            worker.RunWorkerCompleted += Finished;
            core.Progress = delegate(int value, string message)
            {
                if (IsDisposed || value == lastProgress) return;
                lastProgress = value;
                BeginInvoke((Action)delegate { progress.Value = Math.Max(0, Math.Min(100, value)); status.Text = message; });
            };
            core.Log = WriteLog;
            Shown += delegate
            {
                try
                {
                    string[] games = InstallerCore.FindSteamGames();
                    if (games.Length > 0) { source.Text = games[0]; status.Text = "Steam installation found. Choose a new destination and install."; }
                    else status.Text = "Select Among Us using Steam > Properties > Installed Files > Browse.";
                }
                catch (Exception error) { status.Text = "Automatic detection failed; use Browse. " + error.Message; }
            };
            FormClosing += delegate(object sender, FormClosingEventArgs e)
            {
                if (busy) { e.Cancel = true; MessageBox.Show(this, "Please wait for installation to finish.", "MTOU installer"); }
            };
        }
        Label Label(string text, int x, int y, int width, int height)
        {
            var label = new Label { Text = text, AutoEllipsis = true };
            label.SetBounds(x, y, width, height); label.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            Controls.Add(label); return label;
        }
        void ConfigurePath(TextBox control, int top)
        {
            control.SetBounds(26, top, 570, 28); control.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            Controls.Add(control);
        }
        void ConfigureBrowse(Button button, string text, int top)
        {
            button.Text = text; button.SetBounds(606, top, 106, 30); button.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            Controls.Add(button);
        }
        void SetBusy(bool value)
        {
            busy = value; source.Enabled = destination.Enabled = browseSource.Enabled = browseDestination.Enabled = shortcut.Enabled = start.Enabled = !value;
        }
        void StartInstall(object sender, EventArgs e)
        {
            try
            {
                core.ValidateSource(source.Text); InstallerCore.ValidateDestination(source.Text, destination.Text, core.PackageRoot);
                string logs = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MTOU", "InstallerLogs");
                Directory.CreateDirectory(logs); logFile = Path.Combine(logs, DateTime.Now.ToString("yyyyMMdd-HHmmss") + ".log");
                WriteLog("MTOU installer " + core.Manifest.AssemblyRevision);
                SetBusy(true); lastProgress = -1; progress.Value = 0;
                worker.RunWorkerAsync(new[] { source.Text, destination.Text });
            }
            catch (Exception error) { MessageBox.Show(this, error.Message, "Cannot install", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
        }
        void Finished(object sender, RunWorkerCompletedEventArgs e)
        {
            SetBusy(false);
            if (e.Error != null)
            {
                WriteLog(e.Error.ToString()); status.Text = "Installation failed. Your Steam game was not modified.";
                MessageBox.Show(this, e.Error.Message + "\r\n\r\nLog: " + logFile, "Installation failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            installed = (string)e.Result;
            if (shortcut.Checked)
                try { CreateShortcut(installed); }
                catch (Exception error) { WriteLog("Shortcut not created: " + error.Message); }
            progress.Value = 100; status.Text = "Ready. Keep Steam running. The first launch may take several minutes.";
            start.Text = "Close"; start.Click -= StartInstall; start.Click += delegate { Close(); }; play.Visible = true;
            source.Enabled = destination.Enabled = browseSource.Enabled = browseDestination.Enabled = shortcut.Enabled = false;
        }
        void WriteLog(string text)
        {
            if (logFile != null) File.AppendAllText(logFile, DateTime.Now.ToString("O") + " " + text + Environment.NewLine);
        }
        static void CreateShortcut(string folder)
        {
            string filename = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "Marmo Town of Us.lnk");
            if (File.Exists(filename)) return;
            object shell = null, link = null;
            try
            {
                shell = Activator.CreateInstance(Type.GetTypeFromProgID("WScript.Shell"));
                dynamic dynamicShell = shell; link = dynamicShell.CreateShortcut(filename);
                dynamic dynamicLink = link;
                dynamicLink.TargetPath = Path.Combine(folder, "Among Us.exe");
                dynamicLink.WorkingDirectory = folder; dynamicLink.Description = "Marmo Town of Us — keep Steam running";
                dynamicLink.Save();
            }
            finally
            {
                if (link != null) Marshal.FinalReleaseComObject(link);
                if (shell != null) Marshal.FinalReleaseComObject(shell);
            }
        }
    }
}
