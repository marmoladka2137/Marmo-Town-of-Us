using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

[assembly: AssemblyTitle("Marmo Town of Us")]
[assembly: AssemblyProduct("Marmo Town of Us Launcher")]
[assembly: AssemblyVersion("2.1.0.0")]
[assembly: AssemblyFileVersion("2.1.0.0")]
[assembly: AssemblyCompany("Marmo")]

namespace Marmo.Launcher
{
    static class LauncherApp
    {
        [STAThread]
        static int Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            try
            {
                string app = AppDomain.CurrentDomain.BaseDirectory;
                if (!args.Contains("--wait") && !args.Contains("--game-log"))
                {
                    Application.Run(new LauncherForm(app));
                    return 0;
                }
                string saved = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MTOU", "Launcher", "game-path.txt");
                string game = LauncherCore.FindGame(app, Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), saved);
                if (game == null)
                {
                    string installer = Path.Combine(app, "MTOU Installer.exe");
                    if (File.Exists(installer))
                    {
                        Process.Start(new ProcessStartInfo(installer) { WorkingDirectory = app, UseShellExecute = false });
                        return 0;
                    }
                    using (var dialog = new FolderBrowserDialog { Description = "Select your installed Marmo Town of Us folder (contains Among Us.exe)", ShowNewFolderButton = false })
                    {
                        if (dialog.ShowDialog() != DialogResult.OK) return 0;
                        game = dialog.SelectedPath;
                        if (!LauncherCore.IsModdedGame(game)) throw new IOException("MTOU is not installed in this folder. Run the Steam installer first, then select its completed game folder.");
                    }
                }
                if (LauncherCore.IsRunning(game)) return 0;
                // Remembering a choice is optional; read-only profile storage must not prevent launching.
                if (!args.Contains("--no-remember"))
                    try { LauncherCore.Remember(game, saved); } catch (IOException) { } catch (UnauthorizedAccessException) { }
                int logOption = Array.IndexOf(args, "--game-log");
                string logFile = logOption >= 0 && logOption + 1 < args.Length ? args[logOption + 1] : null;
                using (var process = Process.Start(LauncherCore.StartInfo(game, logFile)))
                {
                    if (process == null) throw new IOException("Windows could not start MTOU.");
                    if (args.Contains("--wait")) { process.WaitForExit(); return process.ExitCode; }
                }
                return 0;
            }
            catch (Exception error)
            {
                MessageBox.Show(error.Message, "Marmo Town of Us", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return 1;
            }
        }
    }
}

