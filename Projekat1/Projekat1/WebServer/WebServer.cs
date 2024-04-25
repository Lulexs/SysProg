using System.Diagnostics;
using System.Net;

namespace Projekat1.WebServer;

public class WebServer {
    private readonly string[] _prefixes = [
        "http://localhost:8883/", "http://127.0.0.1:8883/",
        "https://localhost:8883/", "https://127.0.0.1:8883/"
    ];
    
    private static HawkeyeCache.HawkeyeCache _cache = new();
    private readonly HttpListener _listener = new();
    
    public WebServer() {
        foreach (var prefix in _prefixes) {
            _listener.Prefixes.Add(prefix);
        }
    }

    public void Init() {
        ThreadPool.SetMaxThreads(10, 100);
        
        try {
            _listener.Start();
        }
        catch (Exception ec) {
            Console.WriteLine("Listener failed to start due to " + ec.Message);
            return;
        }

        Console.WriteLine($"Listening at...\n{String.Join("\n", _listener.Prefixes)}");

        while (true) {
            HttpListenerContext context;
            try {
                context = _listener.GetContext();
            }
            catch (Exception ec) {
                Console.WriteLine("Failed to accept context due to " + ec.Message);
                continue;
            }
            ThreadPool.QueueUserWorkItem(ProcessRequest, context);
        }
    }

    private void ProcessRequest(object? context) {
        HttpListenerRequest request = ((HttpListenerContext)context!).Request;
        Console.WriteLine($"Processing {request.HttpMethod} request from {request.UserHostAddress}");
        
        HttpListenerResponse response = ((HttpListenerContext)context!).Response;

        Stopwatch stopwatch = new();
        stopwatch.Start();
        string[]? segments = request?.Url?.Segments;
        string[]? searchWords = segments?[^1].Trim('/').Split('&');
        
        
        // byte[] buffer = HtmlGen.GenerateSearchWordAppearanceTable(dict);
        // response.ContentLength64 = buffer.Length;
        // Stream output = response.OutputStream;
        // output.Write(buffer, 0, buffer.Length);
        // output.Close();
        stopwatch.Stop();
        Console.WriteLine($"{request!.HttpMethod} from {request.UserHostAddress} successfully processed in ${stopwatch.Elapsed.TotalSeconds} seconds");
    }
}