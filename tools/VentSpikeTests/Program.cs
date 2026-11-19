using TownOfUs.Modules.Plumber;

int checks=0;
void Check(bool ok,string description){if(!ok)throw new Exception(description);checks++;Console.WriteLine("PASS "+description);}
var s=new VentSpikeState();
Check(s.Place(1,10,2)&&s.Place(2,10,2),"Two nails fit the configured limit");
Check(!s.Place(3,10,2),"The same owner cannot exceed the limit");
Check(s.Place(3,11,2),"Another Plumber has its own limit");
Check(s.Trigger(1,20,3,out var owner,out var lethal)&&owner==10&&!lethal,"First entry records the trap owner and a nonlethal hit");
Check(!s.Trigger(1,20,3,out _,out _),"Polling and an RPC for one entry cannot double count");
Check(s.Trigger(2,20,3,out _,out lethal)&&!lethal&&s.Hits[20]==2,"A different vent triggers without a ground exit");
Check(!s.Traps.ContainsKey(2),"A triggered nail disappears");
Check(s.Trigger(3,20,3,out _,out lethal)&&lethal&&s.Hits[20]==0,"The threshold produces one lethal hit and resets hits");
s.Exit(20,1);Check(s.Place(1,10,50)&&s.Trigger(1,20,3,out _,out _),"Leaving and revisiting a rearmed vent counts again");
s.Clear();for(int i=0;i<50;i++)Check(s.Place(i,10,50),"The expanded maximum accepts nail "+(i+1));
Check(!s.Place(50,10,50),"A fifty-first nail is rejected");
s.Clear();Check(s.Traps.Count==0&&s.Hits.Count==0,"Game reset clears traps and accumulated hits");
Console.WriteLine("PASS "+checks+" deterministic nail assertions; no Unity or multiplayer transport.");
