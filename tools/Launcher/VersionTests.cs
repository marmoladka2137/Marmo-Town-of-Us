using System;
using System.IO;
using System.Linq;
using System.Web.Script.Serialization;
using System.Reflection;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using Marmo.Launcher;
using Marmo.SteamInstaller;
public static class VersionTests
{
    static int n;
    static void Check(bool value,string name){n++;if(!value){Console.Error.WriteLine("FAIL "+name);throw new Exception(name);}Console.WriteLine("PASS "+name);}
    static void Wait(Task task){Application.DoEvents();var end=DateTime.UtcNow.AddSeconds(30);while(!task.IsCompleted&&DateTime.UtcNow<end){Application.DoEvents();Thread.Sleep(10);}if(!task.IsCompleted)throw new Exception("UI task timeout");task.GetAwaiter().GetResult();}
    static object Field(object o,string name){return o.GetType().GetField(name,BindingFlags.Instance|BindingFlags.NonPublic).GetValue(o);}
    [STAThread] public static int Main(string[] args)
    {
        try
        {
            SynchronizationContext.SetSynchronizationContext(new WindowsFormsSynchronizationContext());
            string root=args[0];Directory.CreateDirectory(root);
            var json=File.ReadAllText(args[1]);var choices=VersionLibrary.Parse(json);
            Check(choices.Any(c=>c.Tag=="v1.1.1"),"Current public version listed");
            Check(choices.Any(c=>c.Tag=="v1.1.0-beta"&&c.ManifestUrl==null&&c.ChecksumUrl!=null),"Legacy Beta listed without a modern manifest");
            Check(VersionLibrary.Parse("[]").Count==0,"Empty release list safe");
            Check(VersionLibrary.Destination(root,false,"v1.1.1")!=VersionLibrary.Destination(root,false,"v1.1.0-beta"),"Versions use separate installation folders");
            Check(VersionLibrary.Destination(root,false,"v1.1.1")!=VersionLibrary.Destination(root,true,"v1.1.1"),"Steam and Epic copies isolated");
            bool rejected=false;try{VersionLibrary.Destination(root,false,"../../bad");}catch(IOException){rejected=true;}Check(rejected,"Unsafe version paths rejected");
            var release=new ReleaseManifest {Schema=1,Version="old",Revision="1.1.0.19",SourceCommit=new string('a',40),ArchiveSha256=new string('b',64),Files=new[]{new ReleaseFile{Path="BepInEx/plugins/TOU-Marmo.dll",Bytes=10,Sha256=new string('c',64)}}};
            var installer=new InstallerManifest {Version="new",AssemblyRevision="1.1.1.0"};VersionLibrary.PrepareInstaller(installer,release);
            Check(installer.AssemblyRevision==release.Revision&&installer.Payload.Sha256==release.Files[0].Sha256,"Explicit old-version install uses old plugin pin instead of newer bundled pin");
            var current=choices.First(c=>c.Tag=="v1.1.1");var manifest=VersionLibrary.Load(current,Path.Combine(root,"cache"));
            Check(manifest.Revision=="1.1.1.0","Current real public updater manifest validates including license files");
            current.ManifestUrl="https://github.com/invalid";
            Check(VersionLibrary.Load(current,Path.Combine(root,"cache")).Revision==manifest.Revision,"Pinned cached manifest works offline without fetching the changed URL");
            var beta=choices.First(c=>c.Tag=="v1.1.0-beta");var old=VersionLibrary.Load(beta,Path.Combine(root,"cache"));
            Check(old.Revision=="1.1.0.19","Real legacy package checksum and DLL metadata verified");
            byte[] bytes=File.ReadAllBytes(Path.Combine(root,"cache",old.ArchiveSha256+"-mods.zip"));
            Check(ReleaseUpdate.Hash(bytes)==old.ArchiveSha256,"Derived legacy mod-only payload has a matching archive pin");
            Check(old.Files.Any(f=>f.Path=="BepInEx/plugins/TOU-Marmo.dll")&&!old.Files.Any(f=>f.Path=="Among Us.exe"),"Legacy payload contains the mod, excludes the game");
            using(var form=new LauncherForm(root,false,Path.Combine(root,"profile-"+Guid.NewGuid()),e=>new string[0]))
            {
                var versionBox=(ComboBox)Field(form,"versions");
                Check(versionBox.DropDownStyle==ComboBoxStyle.DropDownList,"Version choice cannot contain arbitrary typed paths");
                Check(form.Text.Contains("Launcher 1.2.0"),"Launcher version shown independently of MTOU 1.1.1");
                var method=form.GetType().GetMethod("StoreFile",BindingFlags.Instance|BindingFlags.NonPublic);
                string latest=(string)method.Invoke(form,new object[]{"destination"});
                form.GetType().GetField("pinned",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(form,beta);
                string pinned=(string)method.Invoke(form,new object[]{"destination"});
                Check(latest!=pinned,"Pinned destination cannot overwrite latest-version preference");
                form.GetType().GetField("pinned",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(form,null);
                string uiCache=Path.Combine(root,"MTOU","VersionCache");Directory.CreateDirectory(uiCache);
                foreach(var file in Directory.GetFiles(Path.Combine(root,"cache")))File.Copy(file,Path.Combine(uiCache,Path.GetFileName(file)),true);
                form.ShowInTaskbar=false;form.StartPosition=FormStartPosition.Manual;form.Location=new System.Drawing.Point(-5000,-5000);form.Show();Application.DoEvents();
                Wait((Task)form.GetType().GetMethod("LoadVersions",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(form,null));
                Check(versionBox.Items.Count>=3,"Actual version selector loads latest, current release and Beta");
                versionBox.SelectedIndex=Enumerable.Range(1,versionBox.Items.Count-1).First(i=>((VersionChoice)versionBox.Items[i]).Tag=="v1.1.0-beta");
                Application.DoEvents();var end=DateTime.UtcNow.AddSeconds(30);while((bool)Field(form,"busy")&&DateTime.UtcNow<end){Application.DoEvents();Thread.Sleep(10);}
                Check(!((CheckBox)Field(form,"autoUpdate")).Enabled,"Selecting Beta disables automatic upgrades");
                Check(((TextBox)Field(form,"gamePath")).Text.Contains("v1.1.0-beta"),"Real selection event switches to separate Beta destination");
                Check(((Label)Field(form,"status")).Text.Contains("Selected MTOU"),"Pinned release metadata loaded by real UI event");
                Console.WriteLine("STATE play="+((Button)Field(form,"play")).Enabled+" install="+((Button)Field(form,"install")).Enabled+" busy="+Field(form,"busy")+" path="+((TextBox)Field(form,"gamePath")).Text+" source="+((ComboBox)Field(form,"sourceGame")).Text);
                Check(!((Button)Field(form,"play")).Enabled&&!((Button)Field(form,"install")).Enabled,"No game/source cannot be enabled by finishing metadata loading");
                Check(versionBox.Enabled,"Version selector remains available after loading");
                foreach(Control c in form.Controls)Check(c.Bottom<=form.ClientSize.Height&&c.Right<=form.ClientSize.Width,"Control fits form: "+c.GetType().Name);
                using(var preview=new System.Drawing.Bitmap(form.Width,form.Height)){form.DrawToBitmap(preview,new System.Drawing.Rectangle(0,0,form.Width,form.Height));preview.Save(Path.Combine(root,"launcher-preview.png"));}
                versionBox.SelectedIndex=0;Application.DoEvents();end=DateTime.UtcNow.AddSeconds(30);while((bool)Field(form,"busy")&&DateTime.UtcNow<end){Application.DoEvents();Thread.Sleep(10);}
                Check(((CheckBox)Field(form,"autoUpdate")).Enabled,"Switching back to latest restores automatic updates");
                Check(!((TextBox)Field(form,"gamePath")).Text.Contains("v1.1.0-beta"),"Latest mode does not reuse the pinned Beta destination");
                form.Close();
            }
            Console.WriteLine("PASS "+n+" version-selection checks. No game launched.");return 0;
        }
        catch(Exception e){Console.Error.WriteLine(e);return 1;}
    }
}
