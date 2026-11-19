using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Marmo.Launcher;
static class SetupTests
{
    static int checks;
    static void Check(bool ok,string text){if(!ok)throw new Exception(text);checks++;Console.WriteLine("PASS "+text);}
    static T Field<T>(object form,string name){return (T)form.GetType().GetField(name,BindingFlags.Instance|BindingFlags.NonPublic).GetValue(form);}
    [STAThread] static int Main()
    {
        try
        {
            string root=Path.Combine(Path.GetTempPath(),"MTOU-setup-"+Guid.NewGuid().ToString("N"));
            string steam=Path.Combine(root,"Steam library","Among Us"),epic=Path.Combine(root,"Epic library","AmongUs");
            foreach(string folder in new[]{steam,epic}){Directory.CreateDirectory(Path.Combine(folder,"Among Us_Data"));File.WriteAllText(Path.Combine(folder,"Among Us.exe"),"fixture");File.WriteAllText(Path.Combine(folder,"GameAssembly.dll"),"fixture");}
            string profile=Path.Combine(root,"preferences");
            Func<bool,string[]> detect=isEpic=>new[]{isEpic?epic:steam};
            using(var form=new LauncherForm(root,false,profile,detect))
            {
                var source=Field<ComboBox>(form,"sourceGame");var platform=Field<ComboBox>(form,"platform");
                var path=Field<TextBox>(form,"gamePath");
                Check(source.SelectedItem as string==steam,"Steam is selected automatically");
                Check(!Field<Button>(form,"play").Enabled,"Play is disabled until MTOU exists");
                Check(Field<Button>(form,"install").Enabled,"A detected original game enables one-step install");
                Check(path.ReadOnly,"The MTOU destination cannot be accidentally replaced by typed text");
                string steamTarget=path.Text;
                platform.SelectedIndex=1;
                Check(source.SelectedItem as string==epic,"Switching to Epic selects the Epic game");
                Check(path.Text!=steamTarget&&path.Text.EndsWith("Epic"),"Switching stores uses a separate default destination");
                platform.SelectedIndex=0;
                Check(path.Text==steamTarget,"Switching back restores the Steam destination");
            }
            using(var form=new LauncherForm(root,false,Path.Combine(root,"missing-profile"),isEpic=>new string[0]))
            {
                Check(!Field<Button>(form,"install").Enabled,"No game detected keeps Install disabled");
                Check(!Field<Button>(form,"play").Enabled,"No game detected keeps Play disabled");
                Check(Field<ComboBox>(form,"sourceGame").Text.Contains("Choose"),"Missing games have an actionable selection hint");
            }
            using(var form=new LauncherForm(root,false,Path.Combine(root,"epic-only-profile"),isEpic=>isEpic?new[]{epic}:new string[0]))
                Check(Field<ComboBox>(form,"platform").SelectedIndex==1&&Field<ComboBox>(form,"sourceGame").SelectedItem as string==epic,
                    "An Epic-only computer automatically chooses the Epic installation");
            Console.WriteLine("PASS "+checks+" isolated setup assertions. No downloads or game processes.");return 0;
        }
        catch(Exception error){Console.Error.WriteLine(error);return 1;}
    }
}
