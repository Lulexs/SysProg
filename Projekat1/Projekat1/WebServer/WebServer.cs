using System.Net;

namespace Projekat1.WebServer;

public class WebServer {
    private readonly string[] _prefixes = [
        "http://localhost:8080/", "http://127.0.0.1:8080/",
        "https://localhost:8080/", "https://127.0.0.1:8080/"
    ];
    
    private static HawkeyeCache.HawkeyeCache _cache = new HawkeyeCache.HawkeyeCache();
    private readonly HttpListener _listener = new HttpListener();
    
    public WebServer() {
        foreach (var prefix in _prefixes) {
            _listener.Prefixes.Add(prefix);
        }
    }

    public void Init() {
        _listener.Start();
        Console.WriteLine($"Listening at...\n{String.Join("\n", _listener.Prefixes)}");

        while (true) {
            HttpListenerContext context = _listener.GetContext();
            HttpListenerRequest request = context.Request;
            Console.WriteLine("Here");
            Console.WriteLine(request.HttpMethod + " " + request.Url);
        }
    }
    
}