using System.Net;
using Projekat1.HawkeyeCache;

namespace Projekat1;

class Program  {
    public static HawkeyeCache.HawkeyeCache cache = new HawkeyeCache.HawkeyeCache(100);
    public static void Main(string[] args) {
        // string[] prefixes = [
        //     "http://localhost:8080/", "http://127.0.0.1:8080/",
        //     "https://localhost:8080/", "https://127.0.0.1:8080/"
        // ];
        // HttpListener listener = new HttpListener();
        // foreach (var prefix in prefixes) {
        //     listener.Prefixes.Add(prefix);
        // }
        // listener.Start();
        // Console.WriteLine($"Listening at...\n{String.Join("\n", listener.Prefixes)}");
    }


}