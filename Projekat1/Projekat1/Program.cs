using Projekat1.HawkeyeCache;

namespace Projekat1;

class Program  {
    public static void Main(string[] args) {
        string[] letters = ["A", "B", "B", "C", "D", "E", "A", "F", "D", "E", "F", "C"];
        HawkeyeCache.HawkeyeCache cache = new HawkeyeCache.HawkeyeCache();
    
        foreach (var l in letters) {
            if (cache.GetValue(l) != null) {
                Console.WriteLine("HIT " + l);
            }
            else {
                Console.WriteLine("MISS " + l);
                cache.InsertValue(l, l);
            }
        }
        
    }
}