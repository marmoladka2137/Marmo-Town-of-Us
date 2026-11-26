namespace TouMarmo.Modules.Cosmetics;

public static class HeadBackground
{
    // Only remove near-black pixels connected to the image edge. Internal dark
    // hair, glasses and facial details are not a global chroma-key target.
    public static bool[] EdgeMask(int width, int height, Func<int, bool> background)
    {
        var mask = new bool[checked(width * height)];
        var queue = new Queue<int>();
        void Add(int i) { if (!mask[i] && background(i)) { mask[i] = true; queue.Enqueue(i); } }
        for (var x = 0; x < width; x++) { Add(x); Add((height - 1) * width + x); }
        for (var y = 0; y < height; y++) { Add(y * width); Add(y * width + width - 1); }
        while (queue.Count > 0)
        {
            var i = queue.Dequeue(); var x = i % width;
            if (x > 0) Add(i - 1);
            if (x < width - 1) Add(i + 1);
            if (i >= width) Add(i - width);
            if (i + width < mask.Length) Add(i + width);
        }
        return mask;
    }
}
