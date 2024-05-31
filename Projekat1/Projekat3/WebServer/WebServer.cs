using System.Diagnostics;
using System.Net;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Projekat3.Models;
using Projekat3.Services;

namespace Projekat3.WebServer;

public class WebServer {
    private const int Port = 10889;

    private readonly string[] _prefixes = [
        $"http://localhost:{Port}/", $"http://127.0.0.1:{Port}/"
    ];
    
    private readonly HttpListener _listener = new();
    private readonly GithubService _githubService;
    private IDisposable? _subscription;

    public WebServer(GithubService githubService) {
        foreach (var prefix in _prefixes) {
            _listener.Prefixes.Add(prefix);
        }

        _githubService = githubService;
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
        string[]? topics = segments?[^1].Trim('/').Split('&');
        
        if (topics == null || topics.Length == 0 || topics.Length > 10) {
             SendResponse(HtmlGen.GenerateErrorTemplate([
                "Error occured while parsing search words",
                "Illegal number of arguments"
            ]), response, stopwatch, request.HttpMethod, request.UserHostAddress);
        }

        Dictionary<string, List<GithubInfo>> statisticsDict = topics!.ToDictionary(w => w, _ => new List<GithubInfo>());

        _githubService.GetRelatedRepositories(topics!.ToHashSet()).Subscribe(
            info => {
                Console.WriteLine($"Repo ${info.Name} processed in thread ${Thread.CurrentThread.ManagedThreadId}");
                statisticsDict[info.Topic].Add(info);
            },
            exception => {
                Console.WriteLine($"Result sent in thread ${Thread.CurrentThread.ManagedThreadId}");
                SendResponse(HtmlGen.GenerateErrorTemplate([
                    "Error occured while processing topics",
                    exception.Message
                ]), response, stopwatch, request.HttpMethod, request.UserHostAddress);
            },
            () => {
                Console.WriteLine($"Result sent in thread ${Thread.CurrentThread.ManagedThreadId}");
                SendResponse(HtmlGen.GenerateOverviewByTopic(statisticsDict), response, stopwatch, request.HttpMethod,
                    request.UserHostAddress);
            });
    }

    private void SendResponse(byte[] buffer, HttpListenerResponse response, Stopwatch stopwatch, string method,
        string userAddress) {
        response.ContentLength64 = buffer.Length;
        Stream output = response.OutputStream;
        output.Write(buffer, 0, buffer.Length);
        output.Close();
        stopwatch.Stop();
        Console.WriteLine($"{method} from {userAddress} successfully processed in ${stopwatch.Elapsed.TotalSeconds} seconds");
    }

    private IObservable<HttpListenerContext?> GetRequestStream() {
        // Here subscribe on would move callback to different thread
        return Observable.Create<HttpListenerContext?>(async (observer) => {
            while (true) {
                try {
                    var context = await _listener.GetContextAsync();
                    Console.WriteLine($"Request accepted in thread ${Thread.CurrentThread.ManagedThreadId}");
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