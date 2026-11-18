using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Security.Cryptography;

if(args.Length!=2)throw new ArgumentException("contracts.json output-folder");
var contracts=JsonSerializer.Deserialize<Contracts>(File.ReadAllText(args[0]),new JsonSerializerOptions{PropertyNameCaseInsensitive=true})!;
string output=Path.GetFullPath(args[1]);Directory.CreateDirectory(output);
var records=new List<object>();int images=0;
foreach(var clip in contracts.Clips){
 int count=Math.Clamp((int)Math.Ceiling(clip.Duration*24),2,72);
 string folder=Path.Combine(output,clip.Name);Directory.CreateDirectory(folder);
 for(int i=0;i<count;i++){
  var canvas=new Canvas(192,192);Draw(canvas,clip.Name,(float)i/(clip.Loop?count:count-1));
  var bytes=canvas.Png();string path=Path.Combine(folder,i.ToString("D3")+".png");File.WriteAllBytes(path,bytes);
  records.Add(new {clip=clip.Name,frame=i,path=Path.GetRelativePath(output,path).Replace('\\','/'),sha256=Convert.ToHexString(SHA256.HashData(bytes))});images++;
 }
}
File.WriteAllText(Path.Combine(output,"frame-manifest.json"),JsonSerializer.Serialize(records,new JsonSerializerOptions{WriteIndented=true}));
Console.WriteLine($"Generated {images} entirely procedural PNG frames for {contracts.Clips.Length} contracts; no input pixels or motion curves read.");
return;

static void Draw(Canvas c,string name,float p){
 string n=name.ToLowerInvariant();float wave=MathF.Sin(2*MathF.PI*p);
 var ink=new Rgba(20,28,43);var cyan=new Rgba(71,216,235);var gold=new Rgba(255,194,69);
 if(n.Contains("shield")||n.Contains("warden")||n.Contains("barrier")||n.Contains("mirror")){
  var color=n.Contains("medic")?new Rgba(74,238,148):n.Contains("warden")?new Rgba(184,119,252):cyan;
  float radius=55+3*wave;
  if(n.Contains("mirror")){
   c.Poly(new[]{(96f,28f),(150f,96f),(96f,164f),(42f,96f)},ink);
   c.Poly(new[]{(96f,35f),(143f,96f),(96f,157f),(49f,96f)},color);
   c.Poly(new[]{(96f,47f),(132f,96f),(96f,145f),(60f,96f)},new Rgba(54,89,127));
   c.Line(78,112,116,66,4,new Rgba(213,250,255));
  }else{
   for(int i=0;i<8;i++){float a=i*MathF.PI/4;
    c.Line(96+radius*MathF.Cos(a),96+radius*MathF.Sin(a),96+radius*MathF.Cos(a+MathF.PI/4),96+radius*MathF.Sin(a+MathF.PI/4),5,color);}
   c.Ring(96,96,radius+9,2,new Rgba(color.R,color.G,color.B,110));
   if(n.Contains("medic")||n.Contains("barrier")){c.Round(86,66,20,60,3,color);c.Round(66,86,60,20,3,color);}
   else for(int i=0;i<3;i++)c.Round(66+i*24,75,13,41+5*wave,3,color);
  }
  return;
 }
 if(n.Contains("tort")||n.Contains("flop")||n.Contains("blackimp")){
  if(n.Contains("tort")){
   float walk=n.Contains("walk")?wave*6:0;
   for(int i=0;i<4;i++)c.Round(49+i%2*65,118+i/2*18+((i%2==0)?walk:-walk),27,16,7,new Rgba(68,190,148));
   c.Ellipse(91,99,50,37,ink);c.Ellipse(91,97,44,31,new Rgba(87,170,98));
   c.Poly(new[]{(91f,77f),(111f,88f),(111f,108f),(91f,119f),(71f,108f),(71f,88f)},new Rgba(161,224,129));
   c.Ellipse(148,106,22,19,ink);c.Ellipse(149,104,18,15,new Rgba(77,205,167));
   c.Disc(157,99,4,ink);
  }else if(n.Contains("flop")){
   for(int i=0;i<3;i++){float dx=wave*(i%2==0?5:-5);c.Line(64,110,32+i*8,120+i*9+dx,5,new Rgba(233,134,67));c.Line(125,110,158-i*8,120+i*9-dx,5,new Rgba(233,134,67));}
   c.Ellipse(96,100,40,29,ink);c.Ellipse(96,98,35,24,new Rgba(245,153,82));
   c.Line(78,85,73,60,6,new Rgba(245,153,82));c.Line(113,85,118,60,6,new Rgba(245,153,82));
   c.Disc(73,59,8,ink);c.Disc(118,59,8,ink);c.Disc(74,57,3,new Rgba(255,255,255));c.Disc(119,57,3,new Rgba(255,255,255));
   c.Line(86,110,106,110,3,ink);
  }else{
   Avatar(c,96,102+(n.Contains("walk")?Math.Abs(wave)*3:0),.68f,new Rgba(47,52,75),n.Contains("walk")?wave:0);
   c.Line(79,63,76,48,5,ink);c.Line(109,63,115,48,5,ink);c.Disc(76,47,5,cyan);c.Disc(115,47,5,cyan);
  }
  if(n.Contains("sad"))c.Line(85,137,102,133,2,cyan);
  if(n.Contains("scared"))c.Ring(96,98,67,2,new Rgba(255,204,82,100));
  if(n.Contains("petting")){c.Disc(89+wave*8,36,4,gold);c.Disc(103-wave*8,27,3,gold);}
  return;
 }
 if(n.Contains("heartbeat")){
  c.Line(25,100,60,100,4,cyan);c.Line(60,100,76,79,4,cyan);c.Line(76,79,91,127,4,cyan);
  c.Line(91,127,108,59,4,cyan);c.Line(108,59,125,100,4,cyan);c.Line(125,100,166,100,4,cyan);
  c.Disc(25+140*p,100,5,new Rgba(237,255,255));return;
 }
 if(n.Contains("sentry")){
  float open=n.Contains("off")?1-p:p;
  c.Round(46,59,101,69,10,ink);c.Round(51,64,91,59,7,new Rgba(78,103,140));
  c.Disc(96,93,26,ink);c.Disc(96,93,20,cyan);c.Ellipse(96,93,14,3+11*open,new Rgba(24,61,85));
  c.Line(96,129,96,146,8,ink);c.Line(68,148,124,148,6,ink);return;
 }
 if(n.Contains("gavel")||n.Contains("pointhand")||n.Contains("mayorrevealhands")){
  float y=69+20*MathF.Sin(p*MathF.PI);
  c.Round(88,y+8,15,65,5,ink);c.Round(92,y+12,7,56,3,new Rgba(214,159,95));
  c.Round(58,y-13,75,31,7,ink);c.Round(63,y-8,65,21,4,new Rgba(189,126,76));
  if(n.Contains("hammer")&&p>.55f){float a=(p-.55f)/.45f;c.Ring(96,148,12+31*a,3,new Rgba(255,199,93,(byte)(255*(1-a))));}
  return;
 }
 if(n.Contains("outfit")){
  c.Poly(new[]{(63f,87f),(129f,87f),(142f,145f),(50f,145f)},ink);
  c.Poly(new[]{(69f,92f),(123f,92f),(134f,139f),(58f,139f)},new Rgba(65,110,162));
  c.Line(96,94,96,137,3,gold);c.Disc(113,107,7,gold);return;
 }
 if(n.Contains("deathmeeting")&&n.Contains("blood")){
  Sparks(c,p,new Rgba(238,84,117));return; // Newly drawn impact sparks, no inherited blood frames.
 }
 if(n.Contains("miner")||n.Contains("hexbomb")||n.Contains("shatter")){
  Sparks(c,p,n.Contains("hex")?new Rgba(211,104,246):gold);return;
 }
 if(n.Contains("escapist")){
  c.Ring(96,105,46+4*wave,5,cyan);c.Line(96,128,96,59,7,ink);c.Line(96,59,76,82,7,ink);c.Line(96,59,116,82,7,ink);return;
 }
 if(n.Contains("parasite")||n.Contains("ambush")){
  var color=n.Contains("parasite")?new Rgba(239,97,131):new Rgba(112,107,192);
  for(int i=0;i<6;i++){float a=i*MathF.PI/3+.18f*wave;c.Line(96,104,96+(42+5*wave)*MathF.Cos(a),104+(42+5*wave)*MathF.Sin(a),11,ink);}
  c.Ellipse(96,100,36,38,ink);c.Ellipse(96,100,29,31,color);c.Ellipse(85,94,5,9,cyan);c.Ellipse(107,94,5,9,cyan);return;
 }
 bool stone=n.Contains("medusa"),death=n.Contains("deathmeeting"),ghost=n.Contains("ghost");
 float collapse=death?Math.Clamp((p-.25f)/.75f,0,1):0;
 Avatar(c,96,102+collapse*30+(ghost?wave*5:0),1-collapse*.48f,
  stone?new Rgba(139,160,170):ghost?new Rgba(91,204,225,150):new Rgba(231,86,99),death?0:wave*.1f);
 if(stone&&(n.Contains("crack")||n.Contains("move"))){c.Line(95,63,85,91,3,ink);c.Line(85,91,105,109,3,ink);c.Line(105,109,94,132,3,ink);}
 if(n.Contains("mayor")||n.Contains("prosbody")){c.Round(62,42,69,15,5,ink);c.Round(72,21,49,28,5,new Rgba(60,89,142));c.Line(78,43,115,43,3,gold);}
 if(death&&p>.18f&&p<.55f){c.Ring(125,94,7+23*p,3,gold);}
}
static void Sparks(Canvas c,float p,Rgba color){
 float a=MathF.Sin(p*MathF.PI);float r=12+62*p;
 for(int i=0;i<9;i++){float t=i*2*MathF.PI/9;c.Line(96+MathF.Cos(t)*(r-12),96+MathF.Sin(t)*(r-12),96+MathF.Cos(t)*r,96+MathF.Sin(t)*r,3,new Rgba(color.R,color.G,color.B,(byte)(255*a)));}
 c.Ring(96,96,r*.65f,3,new Rgba(color.R,color.G,color.B,(byte)(180*a)));
}
static void Avatar(Canvas c,float x,float y,float size,Rgba color,float step){
 float w=64*size,h=85*size;var ink=new Rgba(20,28,43,color.A);
 c.Round(x-w*.73f,y-h*.19f,w*.29f,h*.7f,8*size,ink);
 c.Round(x-w*.70f,y-h*.14f,w*.22f,h*.57f,5*size,new Rgba(122,85,113,color.A));
 c.Round(x-w*.46f,y-h*.58f,w*.92f,h*1.1f,24*size,ink);
 c.Round(x-w*.39f,y-h*.52f,w*.78f,h*.96f,20*size,color);
 c.Round(x-w*.4f,y+h*.20f+step*6,w*.37f,h*.48f,8*size,ink);
 c.Round(x+w*.04f,y+h*.20f-step*6,w*.37f,h*.48f,8*size,ink);
 c.Round(x-w*.34f,y+h*.21f+step*6,w*.25f,h*.39f,5*size,color);
 c.Round(x+w*.1f,y+h*.21f-step*6,w*.25f,h*.39f,5*size,color);
 c.Round(x-w*.13f,y-h*.40f,w*.69f,h*.34f,11*size,ink);
 c.Round(x-w*.06f,y-h*.35f,w*.55f,h*.24f,8*size,new Rgba(130,220,238,color.A));
 c.Line(x+w*.06f,y-h*.30f,x+w*.32f,y-h*.30f,4*size,new Rgba(235,255,255,color.A));
}
record Contracts(Clip[] Clips);
record Clip(string Name,float Duration,bool Loop,string[] SpritePaths,string SourceBundlePathId);
readonly record struct Rgba(byte R,byte G,byte B,byte A=255);
sealed class Canvas(int width,int height){
 readonly byte[] pixels=new byte[width*height*4];
 void Blend(int x,int y,Rgba color,float coverage=1){
  if(x<0||y<0||x>=width||y>=height)return;int i=(y*width+x)*4;
  float a=color.A/255f*Math.Clamp(coverage,0,1),b=pixels[i+3]/255f,result=a+b*(1-a);if(result<=0)return;
  pixels[i]=(byte)Math.Clamp((color.R*a+pixels[i]*b*(1-a))/result,0,255);
  pixels[i+1]=(byte)Math.Clamp((color.G*a+pixels[i+1]*b*(1-a))/result,0,255);
  pixels[i+2]=(byte)Math.Clamp((color.B*a+pixels[i+2]*b*(1-a))/result,0,255);
  pixels[i+3]=(byte)Math.Clamp(result*255,0,255);
 }
 void Area(float l,float t,float r,float b,Func<float,float,float> sdf,Rgba color){
  for(int y=Math.Max(0,(int)MathF.Floor(t-1));y<Math.Min(height,(int)MathF.Ceiling(b+1));y++)
   for(int x=Math.Max(0,(int)MathF.Floor(l-1));x<Math.Min(width,(int)MathF.Ceiling(r+1));x++)Blend(x,y,color,.5f-sdf(x+.5f,y+.5f));
 }
 public void Disc(float x,float y,float radius,Rgba color)=>Area(x-radius,y-radius,x+radius,y+radius,(a,b)=>MathF.Sqrt((a-x)*(a-x)+(b-y)*(b-y))-radius,color);
 public void Ellipse(float x,float y,float rx,float ry,Rgba color)=>Area(x-rx,y-ry,x+rx,y+ry,(a,b)=>(MathF.Sqrt((a-x)*(a-x)/(rx*rx)+(b-y)*(b-y)/(ry*ry))-1)*Math.Min(rx,ry),color);
 public void Ring(float x,float y,float radius,float thickness,Rgba color)=>Area(x-radius-thickness,y-radius-thickness,x+radius+thickness,y+radius+thickness,(a,b)=>Math.Abs(MathF.Sqrt((a-x)*(a-x)+(b-y)*(b-y))-radius)-thickness/2,color);
 public void Line(float x1,float y1,float x2,float y2,float thickness,Rgba color){
  float dx=x2-x1,dy=y2-y1,length=dx*dx+dy*dy;
  Area(Math.Min(x1,x2)-thickness,Math.Min(y1,y2)-thickness,Math.Max(x1,x2)+thickness,Math.Max(y1,y2)+thickness,(x,y)=>{
   float t=length==0?0:Math.Clamp(((x-x1)*dx+(y-y1)*dy)/length,0,1);float a=x-x1-t*dx,b=y-y1-t*dy;return MathF.Sqrt(a*a+b*b)-thickness/2;},color);
 }
 public void Round(float x,float y,float w,float h,float r,Rgba color){
  r=Math.Min(r,Math.Min(w,h)/2);
  Area(x,y,x+w,y+h,(a,b)=>{float dx=Math.Abs(a-x-w/2)-w/2+r,dy=Math.Abs(b-y-h/2)-h/2+r;return MathF.Sqrt(Math.Max(0,dx)*Math.Max(0,dx)+Math.Max(0,dy)*Math.Max(0,dy))+Math.Min(Math.Max(dx,dy),0)-r;},color);
 }
 public void Poly((float x,float y)[] p,Rgba color){
  float l=p.Min(v=>v.x),r=p.Max(v=>v.x),t=p.Min(v=>v.y),b=p.Max(v=>v.y);
  Area(l,t,r,b,(x,y)=>{bool inside=false;for(int i=0,j=p.Length-1;i<p.Length;j=i++)if((p[i].y>y)!=(p[j].y>y)&&x<(p[j].x-p[i].x)*(y-p[i].y)/(p[j].y-p[i].y)+p[i].x)inside=!inside;return inside?-1:1;},color);
 }
 public byte[] Png(){
  using var result=new MemoryStream();result.Write(new byte[]{137,80,78,71,13,10,26,10});
  using var header=new MemoryStream();Big(header,(uint)width);Big(header,(uint)height);header.Write(new byte[]{8,6,0,0,0});Chunk(result,"IHDR",header.ToArray());
  using var raw=new MemoryStream();using(var z=new ZLibStream(raw,CompressionLevel.SmallestSize,true))for(int y=0;y<height;y++){z.WriteByte(0);z.Write(pixels,y*width*4,width*4);}
  Chunk(result,"IDAT",raw.ToArray());Chunk(result,"IEND",Array.Empty<byte>());return result.ToArray();
 }
 static void Big(Stream s,uint v){s.WriteByte((byte)(v>>24));s.WriteByte((byte)(v>>16));s.WriteByte((byte)(v>>8));s.WriteByte((byte)v);}
 static void Chunk(Stream s,string name,byte[] bytes){
  Big(s,(uint)bytes.Length);byte[] tag=Encoding.ASCII.GetBytes(name);s.Write(tag);s.Write(bytes);uint crc=0xffffffff;
  foreach(byte v in tag.Concat(bytes)){crc^=v;for(int i=0;i<8;i++)crc=(crc&1)!=0?(crc>>1)^0xedb88320:crc>>1;}Big(s,~crc);
 }
}

