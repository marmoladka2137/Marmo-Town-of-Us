param([string]$Root=(Join-Path $PSScriptRoot '..'))
$ErrorActionPreference='Stop'
$code="#nullable enable annotations`nusing System;using System.Linq;using System.Collections.Generic;"
foreach($name in @('MimicSession','MimicCommands')) {
 $part=Get-Content (Join-Path $Root "Modules/LobbyMimic/$name.cs") -Raw
 $code+=$part.Replace('namespace TouMarmo.Modules.LobbyMimic;','namespace TouMarmo.Modules.LobbyMimic {')+'}'
}
$code+=@'
public sealed class Info {public bool Disconnected;public string PlayerName="";public static implicit operator bool(Info p)=>p!=null;}
public sealed class PlayerControl {
 public Info Data=new();public static PlayerControl LocalPlayer=new();
 public static List<PlayerControl> AllPlayerControls=new();
 public static implicit operator bool(PlayerControl p)=>p!=null;
}
public sealed class Field {public int Clears;public void Clear(){Clears++;}}
public sealed class ChatController {
 public Field freeChatField=new(),quickChatMenu=new(),quickChatField=new();
 public int Updates;public void UpdateChatMode(){Updates++;}
}
public static class MiscUtils {public static string Last="";public static void AddSystemChat(Info p,string sender,string text){Last=text;}}
namespace TouMarmo.Modules.LobbyMimic {
 public static class MimicRuntime {
  public static MimicSession Session=new();public static bool Host,Lobby,Started;
  public static bool CanCommand=>MimicSession.CanCommand(Host,Lobby,Started);
  public static int Begins,Stops;public static PlayerControl Selected;
  public static void Begin(PlayerControl p){Begins++;Selected=p;}
  public static void RpcStop(PlayerControl p,int v){Stops++;}
 }
}
public static class MimicTests {
 static int checks;
 static void Check(bool ok,string name){checks++;if(!ok)throw new Exception(name);}
 static TouMarmo.Modules.LobbyMimic.OutfitSnapshot Outfit(string name,int c)=>new(name,c,"hat"+name,"skin"+name,"visor"+name,"pet"+name,"plate"+name);
 public static int Run(){
  var s=new TouMarmo.Modules.LobbyMimic.MimicSession();var a=Outfit("A",1);var b=Outfit("B",2);
  Check(!s.Active&&!s.Capture(1,a),"Inactive effect cannot lock/capture anyone");
  Check(s.Begin(1,b)&&s.Active&&s.Target==b,"Full target outfit captured");
  Check(s.Capture(1,a)&&s.Capture(2,b),"Separate originals retained even for selected player");
  Check(!s.Capture(1,b)&&s.Originals[1]==a,"Repeated capture never overwrites original");
  s.Started=true;Check(s.Begin(1,b)&&s.Started,"Reliable replay does not reset match lifetime");
  Check(s.Begin(2,a)&&s.Originals[1]==a&&s.Originals[2]==b,"Retarget keeps both original profiles");
  Check(!s.Begin(1,b)&&s.Target==a,"Old out-of-order target cannot replace current target");
  Check(s.Capture(1000,b),"New network object gets independent original");
  s.Stop(2);Check(!s.Active&&!s.Started&&s.Target==null,"Stop disables presentation and lock");
  Check(s.Originals[1]==a&&s.Originals[2]==b,"Originals remain available during restoration");
  Check(!s.Begin(1,b)&&!s.Begin(2,b),"In-flight replay cannot reactivate finished game");
  Check(s.Begin(3,b)&&s.Originals.Count==0,"Next game starts with fresh originals");
  s.Stop(3);s.ResetConnection();Check(s.Begin(1,a),"A different lobby can start a fresh host epoch");
  foreach(bool host in new[]{false,true})foreach(bool lobby in new[]{false,true})foreach(bool started in new[]{false,true})
   Check(TouMarmo.Modules.LobbyMimic.MimicSession.CanCommand(host,lobby,started)==(host&&lobby&&!started),"Host/lobby/start permission matrix");
  var target=new PlayerControl{Data=new Info{PlayerName="MaRmO Lada"}};
  PlayerControl.AllPlayerControls.Add(target);var chat=new ChatController();
  Check(!TouMarmo.Modules.LobbyMimic.MimicCommands.Handle(chat,"/mimicker hi"),"Other commands not intercepted");
  foreach(bool host in new[]{false,true})foreach(bool lobby in new[]{false,true})foreach(bool started in new[]{false,true}) {
   TouMarmo.Modules.LobbyMimic.MimicRuntime.Host=host;TouMarmo.Modules.LobbyMimic.MimicRuntime.Lobby=lobby;TouMarmo.Modules.LobbyMimic.MimicRuntime.Started=started;
   var before=TouMarmo.Modules.LobbyMimic.MimicRuntime.Begins;
   Check(TouMarmo.Modules.LobbyMimic.MimicCommands.Handle(chat,"/mimic marmo lada"),"Mimic consumed locally");
   Check(TouMarmo.Modules.LobbyMimic.MimicRuntime.Begins-before==(host&&lobby&&!started?1:0),"Command permission enforced");
   var stops=TouMarmo.Modules.LobbyMimic.MimicRuntime.Stops;
   TouMarmo.Modules.LobbyMimic.MimicCommands.Handle(chat,"/unmimic");
   Check(TouMarmo.Modules.LobbyMimic.MimicRuntime.Stops-stops==(host&&lobby&&!started?1:0),"Unmimic permission enforced");
  }
  TouMarmo.Modules.LobbyMimic.MimicRuntime.Host=true;TouMarmo.Modules.LobbyMimic.MimicRuntime.Lobby=true;TouMarmo.Modules.LobbyMimic.MimicRuntime.Started=false;
  var count=TouMarmo.Modules.LobbyMimic.MimicRuntime.Begins;
  foreach(var text in new[]{"/mimic","/mimic nobody","/mimic MaRmO","/mimic     "})TouMarmo.Modules.LobbyMimic.MimicCommands.Handle(chat,text);
  Check(TouMarmo.Modules.LobbyMimic.MimicRuntime.Begins==count,"Missing/partial/unknown names cannot trigger effect");
  target.Data.Disconnected=true;TouMarmo.Modules.LobbyMimic.MimicCommands.Handle(chat,"/mimic marmo lada");
  Check(TouMarmo.Modules.LobbyMimic.MimicRuntime.Begins==count,"Disconnected source rejected");target.Data.Disconnected=false;
  PlayerControl.AllPlayerControls.Add(new PlayerControl{Data=new Info{PlayerName="marmo lada"}});
  TouMarmo.Modules.LobbyMimic.MimicCommands.Handle(chat,"/mimic marmo lada");
  Check(TouMarmo.Modules.LobbyMimic.MimicRuntime.Begins==count,"Ambiguous original name rejected");PlayerControl.AllPlayerControls.RemoveAt(1);
  TouMarmo.Modules.LobbyMimic.MimicCommands.Handle(chat,"  /MIMIC   marmo lada  ");
  Check(TouMarmo.Modules.LobbyMimic.MimicRuntime.Begins==count+1&&TouMarmo.Modules.LobbyMimic.MimicRuntime.Selected==target,"Full multiword name and case-insensitive command work");
  var stopCount=TouMarmo.Modules.LobbyMimic.MimicRuntime.Stops;
  TouMarmo.Modules.LobbyMimic.MimicCommands.Handle(chat,"/unmimic extra");
  Check(TouMarmo.Modules.LobbyMimic.MimicRuntime.Stops==stopCount,"Unmimic rejects extra arguments");
  Check(chat.freeChatField.Clears==chat.Updates&&chat.quickChatField.Clears==chat.Updates&&chat.quickChatMenu.Clears==chat.Updates,"Commands cleared locally without chat broadcast");
  return checks;
 }
}
'@
Add-Type -TypeDefinition $code
"PASS $([MimicTests]::Run()) production session/command assertions with API doubles; not a multiplayer test."
