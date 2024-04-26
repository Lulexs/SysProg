using System.Diagnostics;
using System.Net;
using Projekat1.Utils;

namespace Projekat1.WebServer;

public class WebServer {
    private readonly string[] _prefixes = [
        "http://localhost:8883/", "http://127.0.0.1:8883/",
        "https://localhost:8883/", "https://127.0.0.1:8883/"
    ];
    
    private static readonly HawkeyeCache.HawkeyeCache Cache = new();
    private readonly HttpListener _listener = new();
    private readonly string _rootDirectory = AppDomain.CurrentDomain.BaseDirectory;
    
    public WebServer() {
        foreach (var prefix in _prefixes) {
            _listener.Prefixes.Add(prefix);
        }
        
        DirectoryInfo directory = new DirectoryInfo(_rootDirectory);
        while (directory.GetFiles("*.csproj").Length == 0) {
            directory = directory.Parent!;
            _rootDirectory = directory.FullName;
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
        
        HttpListenerResponse response = ((HttpListenerContext)context).Response;

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
                buffer = GenerateResponseForValidRequest(searchWords);
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

    private byte[] GenerateResponseForValidRequest(string[] searchWords) {
        StringPair fileData = new();
        SemaphoreSlim localSemaphore = new(1);
        Thread loaderThread = new Thread(() => LoadFiles(localSemaphore, fileData));
        loaderThread.Start();
        
        Dictionary<string, Dictionary<string, int>> statisticsDict = new();
        List<string> toBeProcessed = new();
        
        foreach (var word in searchWords) {
            Dictionary<string, int>? cachedVal;
            if ((cachedVal = Cache.GetValue(word)) != null) 
                statisticsDict.Add(word, cachedVal);
            else
                toBeProcessed.Add(word);
        }
        
        
        
        return HtmlGen.GenerateSearchWordAppearanceTable(statisticsDict);
    }

    private void LoadFiles(SemaphoreSlim semaphore, StringPair loadedContent) {
        Stack<Tuple<string, string>> buffer = new();
        Stack<string> searchFiles = new Stack<string>(Directory.GetFiles(_rootDirectory, "*.txt"));

        while (searchFiles.Count > 0) {
            if (loadedContent.FileName == null && semaphore.Wait(0)) {
                if (buffer.Count > 0) {
                    loadedContent.FileName = buffer.Peek().Item1;
                    loadedContent.Content = buffer.Peek().Item2;
                    buffer.Pop();
                }

                string? fileContent = null;
                while (searchFiles.Count > 0 && (fileContent = LoadFileContent(searchFiles.Peek())) == null) {
                    searchFiles.Pop();
                }

                if (fileContent != null) {
                    loadedContent.FileName = buffer.Peek().Item1;
                    loadedContent.Content = buffer.Peek().Item2;
                    buffer.Pop();
                }

                semaphore.Release();
            }
            else {
                string? fileContent = null;
                while (searchFiles.Count > 0 && (fileContent = LoadFileContent(searchFiles.Peek())) == null) {
                    searchFiles.Pop();
                }

                if (fileContent != null) {
                    buffer.Push(new Tuple<string, string>(searchFiles.Pop(), fileContent));
                }
            }
        }

        semaphore.Wait();
        loadedContent.FileName = "EOF";
        loadedContent.Content = "EOF";
        semaphore.Release();
    }

    private string? LoadFileContent(string filename) {
        string path = Path.Combine(_rootDirectory, filename);
        
        if (!File.Exists(path))
            return null;

        try {
            using StreamReader sr = new StreamReader(filename);
            var fileContent = sr.ReadToEnd();
            return fileContent;
        }
        catch (Exception ec) {
            return null;
        }
    }
    
}