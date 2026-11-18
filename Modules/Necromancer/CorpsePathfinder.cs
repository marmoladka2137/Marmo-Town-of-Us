using System.Numerics;
namespace TouMarmo.Modules.Necromancer;
// Bounded A*; every edge is collision-tested, including diagonals, so it never cuts corners.
public static class CorpsePathfinder
{
    public static List<Vector2> Find(Vector2 start,Vector2 goal,Func<Vector2,Vector2,bool> clear,int budget=3500,float step=0.35f)
    {
        if(clear(start,goal)) return new(){goal};
        var destination=((int)MathF.Round((goal.X-start.X)/step),(int)MathF.Round((goal.Y-start.Y)/step));
        var origin=(0,0);
        var open=new PriorityQueue<(int X,int Y),float>();
        var cost=new Dictionary<(int,int),float>{{origin,0}};
        var previous=new Dictionary<(int,int),(int,int)>();
        var closed=new HashSet<(int,int)>();
        Vector2 World((int X,int Y) n)=>start+new Vector2(n.X*step,n.Y*step);
        var margin=24;
        open.Enqueue(origin,0);
        while(open.Count>0 && budget-->0)
        {
            var current=open.Dequeue(); if(!closed.Add(current)) continue;
            var position=World(current);
            if(Vector2.Distance(position,goal)<step*1.5f && clear(position,goal))
            {
                var path=new List<Vector2>{goal}; var n=current;
                while(n!=origin){path.Add(World(n)); n=previous[n];}
                path.Reverse(); return path;
            }
            for(var dx=-1;dx<=1;dx++) for(var dy=-1;dy<=1;dy++)
            {
                if(dx==0 && dy==0) continue;
                var next=(current.X+dx,current.Y+dy);
                if(next.Item1<Math.Min(0,destination.Item1)-margin || next.Item1>Math.Max(0,destination.Item1)+margin ||
                    next.Item2<Math.Min(0,destination.Item2)-margin || next.Item2>Math.Max(0,destination.Item2)+margin || closed.Contains(next)) continue;
                var point=World(next); if(!clear(position,point)) continue;
                var newCost=cost[current]+Vector2.Distance(position,point);
                if(cost.TryGetValue(next,out var known) && known<=newCost) continue;
                cost[next]=newCost; previous[next]=current;
                open.Enqueue(next,newCost+Vector2.Distance(point,goal));
            }
        }
        return new(); // unreachable or budget exceeded: stop, never teleport through geometry.
    }
}
