using TouMarmo.Modules.Profile;
var root=Path.Combine(Path.GetTempPath(),"mtou-store-tests-"+Guid.NewGuid());Directory.CreateDirectory(root);
int passed=0;
void Check(bool ok,string label){if(!ok)throw new Exception(label);Console.WriteLine("PASS "+label);passed++;}
var path=Path.Combine(root,"selection.json");var s=new CosmeticSelectionStore(path);
s.Seed("""{"customization":{"skin":"skin_A","hat":"hat_A","pet":"","name":"private"}}""");
Check(s.Values.Count==2,"seed only nonempty cosmetic ids");
s.Choose("skin","skin_B");s.Choose("visor","visor_B");
Check(s.Values["hat"]=="hat_A","per-slot change preserves other slots");
s=new(path);Check(s.Values["skin"]=="skin_B"&&s.Values["visor"]=="visor_B","restart loads choices");
s.Seed("""{"customization":{"skin":"","hat":"default"}}""");Check(s.Values["skin"]=="skin_B"&&s.Values["hat"]=="hat_A","startup reset cannot replace choices");
s.Choose("skin","");s=new(path);s.Seed("""{"customization":{"skin":"skin_A"}}""");Check(s.Values["skin"]=="","explicit unequip persists across restart and seed");
s.Choose("pet",null);Check(s.Values["pet"]=="","explicit null means unequip");
try{s.Choose("name","private");throw new Exception("accepted unknown slot");}catch(ArgumentException){Check(true,"reject noncosmetic keys");}
try{s.Choose("skin",new string('x',1025));throw new Exception("accepted long id");}catch(ArgumentException){Check(true,"reject oversized id");}
File.WriteAllText(path,"{corrupt");s=new(path);Check(s.Values.ContainsKey("skin"),"corrupt primary recovers backup");s.Choose("visor","visor_C");Check(new CosmeticSelectionStore(path).Values["visor"]=="visor_C","repair primary after recovery");
Check(!File.ReadAllText(path).Contains("private"),"preference file contains no unrelated profile data");
Check(Directory.GetFiles(root,"*.tmp").Length==0,"atomic writes leave no temporary file");
Console.WriteLine($"{passed} assertions passed");
