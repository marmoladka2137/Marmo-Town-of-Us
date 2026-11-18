using TouMarmo.Modules.OriginalAnimations;
int count=0;
void Check(bool ok,string name){count++;if(!ok)throw new Exception(name);}
foreach(float fps in new[]{15f,30f,60f,120f,240f}){
 float maxRecoil=0,maxFlash=0;
 for(int i=0;i<=fps*3;i++){
  float t=i/fps;
  foreach(float value in new[]{AnimationTiming.DoctorPulse(t),AnimationTiming.DeputyOpacity(t),AnimationTiming.Recoil(t),AnimationTiming.Flash(t)})
   Check(float.IsFinite(value)&&value>=0&&value<=1.01f,"Bounded interpolation");
  maxRecoil=Math.Max(maxRecoil,AnimationTiming.Recoil(t));maxFlash=Math.Max(maxFlash,AnimationTiming.Flash(t));
 }
 Check(maxRecoil>.4f&&maxFlash>.05f,"Transient remains visible at low frame rates");
}
foreach(float aspect in new[]{.75f,1f,4f/3,16f/9,21f/9,32f/9})
foreach(float parent in new[]{.05f,.5f,1f,2.75f,10f}){
 float s=AnimationTiming.Fit(6,aspect,parent,parent);
 Check(float.IsFinite(s)&&s>0,"Finite fit");
 Check(8*s*parent<=6*aspect*.82001f&&4.5f*s*parent<=6*.72001f,"Viewport bounds at inherited scales");
}
Check(AnimationTiming.DoctorPulse(0)==0&&AnimationTiming.DoctorPulse(2.1f)==0,"Medical effect starts and ends invisible");
Check(AnimationTiming.DeputyOpacity(0)==0&&AnimationTiming.DeputyOpacity(1.15f)==0,"Shot starts and ends invisible");
Check(AnimationTiming.Recoil(.29f)==0&&AnimationTiming.Recoil(.54f)==0,"Recoil has no lingering drift");
Check(AnimationTiming.Finished(float.NaN,1)&&AnimationTiming.Finished(float.PositiveInfinity,1),"Invalid clock terminates");
Check(!AnimationTiming.Finished(.5f,1)&&AnimationTiming.Finished(1,1),"Deadline exact");
Console.WriteLine($"PASS: {count} assertions on production timelines, frame rates, and viewport scaling.");

