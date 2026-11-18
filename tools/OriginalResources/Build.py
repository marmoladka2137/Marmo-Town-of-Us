"""Reproducible resource build. No Unity Editor, installation, credentials or old motion required.
bootstrap is a one-time private import; build reads only checked-in sanitized sources."""
import sys,json,base64,copy,math,struct,hashlib,collections
from pathlib import Path
sys.path.insert(0,str(Path(__file__).resolve().parents[4]/'MTOU-LICENSE-RECHECK-2026-11-17/tools/python-deps'))
import UnityPy
from UnityPy.streams import EndianBinaryReader,EndianBinaryWriter
from UnityPy.helpers import TypeTreeHelper
from UnityPy.files.ObjectReader import ObjectReader
from UnityPy.enums import ClassIDType
from UnityPy.export.Texture2DConverter import image_to_texture2d
from PIL import Image
tool=Path(__file__).resolve().parent
source=tool.parents[1]
sources=tool/'Sources'
def encode(v):
 if isinstance(v,(bytes,bytearray,memoryview)):return {'$bytes':base64.b64encode(v).decode()}
 if isinstance(v,dict):return {k:encode(x) for k,x in v.items()}
 if isinstance(v,(list,tuple)):return [encode(x) for x in v]
 return v
def decode(v):
 if isinstance(v,dict):
  if set(v)=={'$bytes'}:return base64.b64decode(v['$bytes'])
  if set(v)=={'$texture'}:
   png=sources/v['$texture']
   return image_to_texture2d(Image.open(png).convert('RGBA'),4)[0]
  return {k:decode(x) for k,x in v.items()}
 if isinstance(v,list):return [decode(x) for x in v]
 return v
def ptrs(v,path=''):
 if isinstance(v,dict):
  if 'm_FileID' in v and 'm_PathID' in v:yield path,v
  for k,x in v.items():yield from ptrs(x,path+'.'+k)
 elif isinstance(v,(list,tuple)):
  for i,x in enumerate(v):yield from ptrs(x,path+'['+str(i)+']')
def block_size(obj,key,data):
 writer=EndianBinaryWriter(endian=obj.assets_file.header.endian)
 node=next(n for n in obj.serialized_type.node.m_Children if n.m_Name==key)
 TypeTreeHelper.write_typetree(data,node,writer,obj.assets_file)
 return writer.Length
def clear_clip(obj,d):
 for key in ['m_RotationCurves','m_CompressedRotationCurves','m_EulerCurves','m_PositionCurves','m_ScaleCurves','m_FloatCurves','m_PPtrCurves','m_Events']:d[key]=[]
 d['m_Legacy']=True;d['m_Compressed']=False;d['m_SampleRate']=24
 d['m_WrapMode']=2 if d['m_MuscleClip']['m_LoopTime'] else 1
 d['m_ClipBindingConstant']['genericBindings']=[];d['m_ClipBindingConstant']['pptrCurveMapping']=[]
 packed=d['m_MuscleClip']['m_Clip']['data']
 packed['m_StreamedClip'].update(data=[],curveCount=0,discreteCurveCount=0)
 packed['m_DenseClip'].update(m_FrameCount=0,m_CurveCount=0,m_SampleRate=24,m_BeginTime=0,m_SampleArray=[])
 packed['m_ConstantClip']['data']=[]
 d['m_MuscleClip']['m_IndexArray']=[];d['m_MuscleClip']['m_ValueArrayDelta']=[];d['m_MuscleClip']['m_ValueArrayReferencePose']=[]
 d['m_MuscleClipSize']=block_size(obj,'m_MuscleClip',d['m_MuscleClip'])
def quad(sprite):
 rd=sprite['m_RD'];v=rd['m_VertexData'];channels=v['m_Channels'];pos=channels[0]
 assert pos['stream']==0 and pos['format']==0 and pos['dimension']==3
 sizes={0:4,1:2,2:1,3:1,4:2,5:2,6:1,7:1,8:2,9:2,10:4,11:4}
 stride=max((c['offset']+c['dimension']*sizes[c['format']] for c in channels if c['dimension'] and c['stream']==0),default=12)
 points=[struct.unpack_from('<fff',v['m_DataSize'],i*stride+pos['offset']) for i in range(v['m_VertexCount'])]
 assert points
 xmin=min(p[0] for p in points);xmax=max(p[0] for p in points)
 ymin=min(p[1] for p in points);ymax=max(p[1] for p in points)
 vertices=[(xmin,ymin,0,0,0),(xmin,ymax,0,0,1),(xmax,ymax,0,1,1),(xmax,ymin,0,1,0)]
 # Retain a rectangular layout interface, not the original artist's silhouette.
 v['m_VertexCount']=4
 v['m_DataSize']=b''.join(struct.pack('<fffff',*p) for p in vertices)
 for c in channels:c.update(stream=0,offset=0,format=0,dimension=0)
 channels[0].update(dimension=3);channels[4].update(offset=12,dimension=2)
 rd['m_IndexBuffer']=list(struct.pack('<6H',0,1,2,0,2,3))
 mesh=copy.deepcopy(rd['m_SubMeshes'][0]);mesh.update(firstByte=0,indexCount=6,topology=0,baseVertex=0,firstVertex=0,vertexCount=4)
 mesh['localAABB']={'m_Center':{'x':(xmin+xmax)/2,'y':(ymin+ymax)/2,'z':0},'m_Extent':{'x':(xmax-xmin)/2,'y':(ymax-ymin)/2,'z':0}}
 rd['m_SubMeshes']=[mesh];rd['settingsRaw']=0
 rd['textureRect']={'x':0.,'y':0.,'width':192.,'height':192.}
 rd['textureRectOffset']={'x':0.,'y':0.}
 if 'atlasRectOffset' in rd:rd['atlasRectOffset']={'x':0.,'y':0.}
 if 'downscaleMultiplier' in rd:rd['downscaleMultiplier']=1.
 rd['alphaTexture']={'m_FileID':0,'m_PathID':0}
 if 'secondaryTextures' in rd:rd['secondaryTextures']=[]
 for key in ['m_Bindpose','m_SourceSkin','m_Bones']:
  if key in rd:rd[key]=[]
 sprite['m_IsPolygon']=False;sprite['m_PhysicsShape']=[];sprite['m_Bones']=[]
 sprite['m_AtlasTags']=[];sprite['m_SpriteAtlas']={'m_FileID':0,'m_PathID':0}
def bootstrap(old_folder):
 sources.mkdir(parents=True,exist_ok=True)
 overall=None;report=[]
 for platform in ['win','android']:
  original=old_folder/('tou-assets-'+platform+'.bundle')
  env=UnityPy.load(str(original));f=env.file
  cab=next(k for k,v in f.files.items() if type(v).__name__=='SerializedFile')
  asset=f.files[cab];objects=asset.objects
  data={k:o.parse_as_dict() for k,o in objects.items()}
  removed={k for k,o in objects.items() if o.type.name in ['AudioClip','AnimatorController','Shader']}
  audio=[{'name':d['m_Name'],'duration':d['m_Length']} for k,d in data.items() if objects[k].type.name=='AudioClip']
  names={k:d['m_Name'] for k,d in data.items() if objects[k].type.name=='AnimationClip'}
  spec=[];frame_to_name={};textures={}
  for k,name in names.items():
   d=data[k]
   assert not d['m_Events']
   assert all(x['path']==0 for x in d['m_ClipBindingConstant']['genericBindings'])
   duration=d['m_MuscleClip']['m_StopTime']-d['m_MuscleClip']['m_StartTime']
   count=len(list((source/'Resources/OriginalMotion'/name).glob('*.png')))
   assert count>=2
   spec.append({'name':name,'duration':duration,'loop':d['m_MuscleClip']['m_LoopTime'],'frames':count,'spritePaths':['']})
   for p in d['m_ClipBindingConstant']['pptrCurveMapping']:
    sid=p['m_PathID']
    if sid:
     assert objects[sid].type.name=='Sprite'
     frame_to_name.setdefault(sid,name)
   clear_clip(objects[k],d)
  for sid,name in frame_to_name.items():textures.setdefault(data[sid]['m_RD']['texture']['m_PathID'],name)
  for k,d in data.items():
   if objects[k].type.name=='Sprite' and d['m_RD']['texture']['m_PathID'] in textures:
    assert k in frame_to_name,'A static image shares this animation sheet; explicit split required.'
    quad(d)
  # The replacement playback owns visual state; retain Animator components for native API compatibility.
  for k,d in data.items():
   if objects[k].type.name=='Animator':d['m_Controller']={'m_FileID':0,'m_PathID':0}
  # Fonts resolve the game's installed TMP shader; custom surfaces use owned raster fills.
  # Use the same built-in placeholder reference already used by this format's sprite materials.
  default_shader=next(d['m_Shader'] for k,d in data.items() if objects[k].type.name=='Material' and d['m_Name']=='Sprites-Default')
  assert default_shader['m_FileID']!=0
  for k,d in data.items():
   if objects[k].type.name=='Material' and d['m_Shader']['m_PathID'] in removed and d['m_Shader']['m_FileID']==0:
    d['m_Shader']=copy.deepcopy(default_shader)
  # All original audio/controller object refs must be isolated from actual prefab behaviour.
  for k,d in data.items():
   if k in removed or objects[k].type.name=='AssetBundle':continue
   for path,p in ptrs(d):
    assert not (p['m_FileID']==0 and p['m_PathID'] in removed),(objects[k].type.name,path)
  for k,d in data.items():
   if objects[k].type.name=='Texture2D':
    if not d.get('image data') and not d.get('m_StreamData',{}).get('size',0):continue # Native dynamic font atlas, no serialized pixels.
    folder=sources/'Textures'/platform;folder.mkdir(parents=True,exist_ok=True)
    image_name=str(k)+'.png';target=folder/image_name
    if k in textures:
     target.write_bytes((source/'Resources/OriginalMotion'/textures[k]/'000.png').read_bytes())
    else:
     try:objects[k].parse_as_object().image.save(target)
     except Exception:
      print('TEXTURE FAILED',k,d['m_Name'],d.get('m_StreamData'),len(d.get('image data',b'')),flush=True);raise
    img=Image.open(target)
    d.update(m_Width=img.width,m_Height=img.height,m_CompleteImageSize=img.width*img.height*4,m_TextureFormat=4,m_MipCount=1,m_MipsStripped=0,m_IsPreProcessed=False,m_PlatformBlob=[])
    d['m_StreamData']={'offset':0,'size':0,'path':''}
    d['image data']={'$texture':target.relative_to(sources).as_posix()}
   if objects[k].type.name=='AssetBundle':
    d['m_Container']=[pair for pair in d['m_Container'] if pair[1]['asset']['m_PathID'] not in removed]
    for _,p in ptrs(d):
     if p['m_FileID']==0 and p['m_PathID'] in removed:p['m_PathID']=0
    # LoadAllAssets(type) enumerates exported assets, not all serialized dependencies.
    # Export font materials too, so their installed-game shader can be rebound before UI creation.
    exported={pair[1]['asset']['m_PathID'] for pair in d['m_Container']}
    for material_id,material in data.items():
     if objects[material_id].type.name!='Material' or material_id in exported:continue
     index=len(d['m_PreloadTable']);p={'m_FileID':0,'m_PathID':material_id};d['m_PreloadTable'].append(p)
     d['m_Container'].append(('assets/mtou-runtime-materials/'+str(material_id)+'.mat',{'preloadIndex':index,'preloadSize':1,'asset':p.copy()}))
  export=[]
  for k,o in objects.items():
   if k in removed:continue
   export.append({'pathId':k,'typeId':o.type_id,'classId':o.class_id,'data':encode(data[k])})
  (sources/(platform+'.objects.json')).write_text(json.dumps(export,separators=(',',':')),encoding='utf-8')
  own='CAB-'+hashlib.md5(('MTOU-owned-resources-v1-'+platform).encode()).hexdigest()
  f.files={own:asset};asset.name=own;asset.objects={}
  (sources/(platform+'.schema.bundle')).write_bytes(f.save(packer='lz4'))
  if overall is None:overall={'clips':sorted(spec,key=lambda x:x['name']),'audio':sorted(audio,key=lambda x:x['name'])}
  else:
   assert overall=={'clips':sorted(spec,key=lambda x:x['name']),'audio':sorted(audio,key=lambda x:x['name'])}
  report.append({'platform':platform,'cleared_clips':len(spec),'replaced_animation_sprites':len(frame_to_name),'replaced_animation_textures':len(textures),'removed_old_audio':len(audio),'removed_audio_and_controller_objects':len(removed),'remaining_objects':len(export)})
 (source/'Resources/OriginalMotion/catalog.json').write_text(json.dumps(overall,indent=2),encoding='utf-8')
 (sources/'IMPORT_PROVENANCE.json').write_text(json.dumps(report,indent=2),encoding='utf-8')
 print(json.dumps(report,indent=2))
def build():
 for platform in ['win','android']:
  env=UnityPy.load(str(sources/(platform+'.schema.bundle')))
  asset=next(v for v in env.file.files.values() if type(v).__name__=='SerializedFile')
  assert not asset.objects
  rows=json.loads((sources/(platform+'.objects.json')).read_text(encoding='utf-8'))
  for row in rows:
   type_id=row['typeId'];class_id=row['classId']
   obj=ObjectReader(asset,asset.reader,row['pathId'],type_id,asset.types[type_id],class_id,ClassIDType(class_id),0,0,None,None,data=b'')
   asset.objects[obj.path_id]=obj
   obj.save_typetree(decode(row['data']))
  ids=set(asset.objects)
  for row in rows:
   o=asset.objects[row['pathId']];d=decode(row['data'])
   for path,p in ptrs(d):
    assert p['m_FileID']!=0 or not p['m_PathID'] or p['m_PathID'] in ids,(o.type.name,path,p)
  target=source/'IntegratedTownOfUs/Resources'/('tou-assets-'+platform+'.bundle')
  target.write_bytes(env.file.save(packer='lz4'))
  test=UnityPy.load(str(target))
  counts=collections.Counter(o.type.name for o in test.objects)
  assert counts['AnimationClip']==55 and counts['AudioClip']==0 and counts['AnimatorController']==0 and counts['Shader']==0
  for o in test.objects:
   if o.type.name=='AnimationClip':
    d=o.parse_as_dict();assert not d['m_PPtrCurves'] and not d['m_ClipBindingConstant']['pptrCurveMapping']
    assert not d['m_MuscleClip']['m_Clip']['data']['m_StreamedClip']['data']
  print(platform,len(target.read_bytes()),dict(counts))
if __name__=='__main__':
 if len(sys.argv)>1 and sys.argv[1]=='bootstrap':bootstrap(Path(sys.argv[2]).resolve())
 else:build()





