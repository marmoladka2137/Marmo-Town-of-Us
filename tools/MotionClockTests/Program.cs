using TouMarmo.Modules.OriginalMotion;
int checks=0;
void Assert(bool ok,string name){if(!ok)throw new Exception(name);checks++;}
foreach(int fps in new[]{24,30,60,144,240}){
 var c=new MotionClock();c.Play(1.75f,false,1);
 int last=-1;
 for(int i=0;i<fps*2;i++){
  c.Advance(1.0/fps);int frame=c.Frame(43);Assert(frame>=last&&frame<43,"Finite monotonic frame index");last=frame;
 }
 Assert(!c.Playing&&c.Frame(43)==42&&Math.Abs(c.Elapsed-1.75)<1e-8,"Nonloop completes at last frame");
 c.Play(1.75f,true,1.25f);for(int i=0;i<fps*8;i++)c.Advance(1.0/fps);
 Assert(c.Playing&&Math.Abs(c.Elapsed-10)<1e-8,"Loop progresses across frame rates");
 c.Paused=true;double saved=c.Elapsed;c.Advance(2);Assert(c.Elapsed==saved,"Pause retains position");
 c.Paused=false;c.Speed=0;c.Advance(2);Assert(c.Elapsed==saved,"Zero speed retains position");
 c.Stopped=true;c.Speed=1;c.Advance(2);Assert(c.Elapsed==saved&&!c.Playing,"Stop retains position");
 c.Play(1,true,1);c.Advance(2.5);Assert(c.Frame(24)==12,"Loop wraps displayed frame");
 c.SetTime(double.NaN);Assert(c.Elapsed==0,"Nonfinite seek sanitized");
 c.Advance(double.PositiveInfinity);c.Advance(-2);Assert(c.Elapsed==0,"Invalid deltas ignored");
 c.SetTime(-2);Assert(c.Elapsed==0,"Negative seek clamped");
 c.Play(2,false,float.NaN);c.Advance(1);Assert(c.Elapsed==0,"Nonfinite speed sanitized");
 c.Play(2,false,2);c.Advance(1);Assert(!c.Playing&&c.Elapsed==2,"Speed multiplier applied");
 c.Play(2,false,1);Assert(c.Playing&&!c.Paused&&!c.Stopped&&c.Elapsed==0,"Replay resets state");
}
foreach(float invalid in new[]{0,-1,float.NaN,float.PositiveInfinity}){
 bool rejected=false;try{new MotionClock().Play(invalid,false,1);}catch(ArgumentOutOfRangeException){rejected=true;}Assert(rejected,"Reject invalid duration");
}
Console.WriteLine($"PASS: {checks} playback-clock assertions; pure production class, no Unity runtime.");
int clockChecks=checks;
for(int mode=0;mode<4;mode++)foreach(bool host in new[]{true,false})
 Assert(RejoinSchedule.Eligible(mode,host)==(mode==0||(mode==1&&host)||(mode==2&&!host)),"Host/client/never preference matrix");
foreach(double delay in new[]{0,.5,4,10}){
 var s=new RejoinSchedule();s.Begin(100,delay);
 Assert(s.Active&&s.Seconds(100)==(int)Math.Ceiling(delay),"Deadline starts");
 Assert(!s.Take(99.99)&&s.Active,"Cannot fire before deadline");
 Assert(s.Take(100+delay)&&!s.Active,"Deadline fires once");
 Assert(!s.Take(100+delay+10)&&s.Seconds(200)==0,"No repeated clicks");
 s.Begin(200,delay);s.Cancel();Assert(!s.Take(1000),"Cancellation prevents click");
 s.Begin(200,delay);Assert(s.Take(200+delay),"New game rearms schedule");
}
Console.WriteLine($"PASS: {checks-clockChecks} return-to-lobby state assertions; new deadline implementation.");
