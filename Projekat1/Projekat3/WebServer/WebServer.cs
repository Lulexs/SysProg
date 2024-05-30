using System.Diagnostics;
using System.Net;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace Projekat3.WebServer;

public class WebServer {
    private const int Port = 10881;

    private readonly string[] _prefixes = [
        $"http://localhost:{Port}/", $"http://127.0.0.1:{Port}/"
    ];
    
    private readonly HttpListener _listener = new();
    private IDisposable? _subscription;

    public WebServer() {
        foreach (var prefix in _prefixes) {
            _listener.Prefixes.Add(prefix);
        }

        _listener.Start();
        Console.WriteLine($"Listening at...\n{String.Join("\n", _listener.Prefixes)}");
    }
    
    public void Init() {
        if (_subscription != null)
            return;
        
        _subscription = GetRequestStream().Distinct().Subscribe(
            onNext: ProcessRequest, 
            onError: (err) => Console.WriteLine($"Server shutting down due to {err.Message}"));
    }

    private void ProcessRequest(HttpListenerContext? context) {
        if (context == null)
            return;
        
        HttpListenerRequest request = context.Request;
        Console.WriteLine($"Processing {request.HttpMethod} request from {request.UserHostAddress} in thread" +
                          $"${Thread.CurrentThread.ManagedThreadId}");
        
        HttpListenerResponse response = context.Response;
        response.Headers.Add("Access-Control-Allow-Origin", "*");
        response.Headers.Add("Access-Control-Allow-Methods", "GET");
        response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Accept");

        Stopwatch stopwatch = new();
        stopwatch.Start();
        string[]? segments = request.Url?.Segments;
        string[]? searchWords = segments?[^1].Trim('/').Split('&');

        byte[] buffer;

        if (searchWords == null || searchWords.Length == 0 || searchWords.Length > 10) {
            buffer = HtmlGen.GenerateErrorTemplate([
                "Error occured while parsing search words",
                "Illegal number of arguments"
            ]);
            
        }
        else {
            try {
                buffer = "<h1>Hello world</h1>"u8.ToArray();
            }
            catch (Exception ec) {
                buffer = HtmlGen.GenerateErrorTemplate([
                    "Error occured while processing search words",
                    ec.Message
                ]);
            }
        }

        response.ContentLength64 = buffer.Length;
        Stream output = response.OutputStream;
        output.Write(buffer, 0, buffer.Length);
        output.Close();
        stopwatch.Stop();
        Console.WriteLine($"{request.HttpMethod} from {request.UserHostAddress} successfully processed in ${stopwatch.Elapsed.TotalSeconds} seconds");
    }

    private IObservable<HttpListenerContext?> GetRequestStream() {
        // Here subscribe on would move callback to different thread
        return Observable.Create<HttpListenerContext?>(async (observer) => {
            while (true) {
                try {
                    var context = await _listener.GetContextAsync();
                    observer.OnNext(context);
                }
                catch (HttpListenerException ec) {
                    observer.OnError(ec);
                    return;
                }
                catch (Exception) {
                    observer.OnNext(null);
                }
            }
        }).ObserveOn(TaskPoolScheduler.Default);
    }
}