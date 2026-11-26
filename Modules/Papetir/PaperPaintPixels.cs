using UnityEngine;
namespace TouMarmo.Modules.Papetir;

// An otherwise transparent overlay. Black outlines and transparent areas stay intact.
public static class PaperPaintPixels
{
    public static Color32[] Create(Color32[] source,int width,int height)
    {
        if(width<=0 || height<=0 || source.Length!=(long)width*height) return Array.Empty<Color32>();
        var result=new Color32[source.Length];
        var minX=width;var maxX=-1;var minY=height;var maxY=-1;
        for(var y=0;y<height;y++) for(var x=0;x<width;x++)
        {
            if(source[y*width+x].a<220) continue;
            minX=Math.Min(minX,x);maxX=Math.Max(maxX,x);
            minY=Math.Min(minY,y);maxY=Math.Max(maxY,y);
        }
        if(maxX<minX) return result;
        var opaqueWidth=maxX-minX+1;var opaqueHeight=maxY-minY+1;
        for(var chip=0;chip<3;chip++)
        {
            var side=chip%2;
            var bottom=minY+(int)(opaqueHeight*(chip==2 ? .46f : side==0 ? .16f : .24f));
            var chipHeight=Math.Max(2,(int)(opaqueHeight*(chip==2 ? .16f : .22f)));
            var depth=Math.Max(2,(int)(opaqueWidth*.18f));
            for(var row=0;row<chipHeight && bottom+row<height;row++)
            {
                var y=bottom+row;var edge=side==0 ? minX : maxX;var step=side==0 ? 1 : -1;
                while(edge>=minX && edge<=maxX && source[y*width+edge].a<220) edge+=step;
                if(edge<minX || edge>maxX) continue;
                var reach=Math.Max(2,depth*(chipHeight-row)/chipHeight-(row%3==0 ? 1 : 0));
                for(var distance=1;distance<=reach;distance++)
                {
                    var x=edge+step*distance;
                    if(x<minX || x>maxX) break;
                    var pixel=source[y*width+x];
                    if(pixel.a<220 || Math.Max(pixel.r,Math.Max(pixel.g,pixel.b))<55) continue;
                    result[y*width+x]=new Color32(242,238,219,225);
                }
            }
        }
        return result;
    }
}
