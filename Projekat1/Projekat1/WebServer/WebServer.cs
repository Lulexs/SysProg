using System.Diagnostics;
using System.Net;
using Ganss.Text;
using Projekat1.Utils;

namespace Projekat1.WebServer;

public class WebServer {
    private const int Port = 10889;

    private readonly string[] _prefixes = [
        $"http://localhost:{Port}/", $"http://127.0.0.1:{Port}/"
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
        
        Dictionary<string, Dictionary<string, int>> statisticsDict = new();
        List<string> toBeProcessed = new();

        Thread loaderThread = new Thread(() => {
            LoadFiles(localSemaphore, fileData);
        });
        loaderThread.Start();
        
        foreach (var word in searchWords) {
            Dictionary<string, int>? cachedVal;
            if ((cachedVal = Cache.GetValue(word)) != null) {
                int counter = 0;
                while (++counter < 10 && !statisticsDict.TryAdd(word, cachedVal)) { }

                if (counter >= 10) {
                    toBeProcessed.Add(word);
                }
            }
            else
                toBeProcessed.Add(word);
        }

        var ac = new AhoCorasick(toBeProcessed);

        while (true) {
            localSemaphore.Wait();
            if (fileData.FileName == "EOF") {
                localSemaphore.Release();
                break;
            }

            if (fileData.FileName == null) {
                localSemaphore.Release();
                continue;
            }
            
            Console.WriteLine("Preuzeto " + fileData.FileName);
            string filename = fileData.FileName;
            string contents = fileData.Content!;
            fileData.FileName = null;
            fileData.Content = null;
            localSemaphore.Release();

            var results = ac.Search(contents).GroupBy(w => w.Word).ToDictionary(g => g.Key, g => g.Count());
            Console.WriteLine("Obradjivanje " + filename);
            foreach (KeyValuePair<string, int> pair in results) {
                if (!statisticsDict.TryGetValue(pair.Key, out var value)) {
                    statisticsDict.Add(pair.Key,
                        new Dictionary<string, int>([new KeyValuePair<string, int>(filename, pair.Value)]));
                }
                else {
                    value.Add(filename, pair.Value);
                }
            }

            foreach (var tbp in toBeProcessed.Where(tbp => !results.ContainsKey(tbp))) {
                if (!statisticsDict.TryGetValue(tbp, out var value)) {
                    statisticsDict.Add(tbp,
                        new Dictionary<string, int>([new KeyValuePair<string, int>(filename, 0)]));
                }
                else {
                    value.Add(filename, 0);
                }
            }
        }
        
        return HtmlGen.GenerateSearchWordAppearanceTable(statisticsDict);
    }


    private void LoadFiles(SemaphoreSlim semaphore, StringPair loadedContent) {
        
        Stack<Tuple<string, string>> buffer = new();
        Stack<string> searchFiles = new Stack<string>(Directory.GetFiles(_rootDirectory, "*.txt"));

        while (searchFiles.Count > 0 || buffer.Count > 0 || loadedContent.FileName != null) {
            if (loadedContent.FileName == null && semaphore.Wait(0)) {
                if (buffer.Count > 0 && loadedContent.FileName == null) {
                    Console.WriteLine("baf " + buffer.Peek().Item1);
                    loadedContent.FileName = buffer.Peek().Item1;
                    loadedContent.Content = buffer.Peek().Item2;
                    buffer.Pop();
                }
                else {
                    string? fileContent = null;
                    while (searchFiles.Count > 0 && (fileContent = LoadFileContent(searchFiles.Peek())) == null) {
                        Console.WriteLine("Popped 1 " + searchFiles.Pop());
                    }

                    if (fileContent != null) {
                        loadedContent.FileName = searchFiles.Pop();
                        Console.Write("newl " + loadedContent.FileName);
                        loadedContent.Content = fileContent;
                    }
                }

                semaphore.Release();
            }
            
            else {
                string? fileContent = null;
                while (searchFiles.Count > 0 && (fileContent = LoadFileContent(searchFiles.Peek())) == null) {
                    Console.WriteLine("Popped 2 " + searchFiles.Pop());
                }
                if (fileContent != null) {
                    Console.WriteLine("Buffered " + searchFiles.Peek());
                    buffer.Push(new Tuple<string, string>(searchFiles.Pop(), fileContent));
                }
            }
        }
        Console.WriteLine("Buff len after processing: " + buffer.Count);
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
            Console.WriteLine("Loaded " + filename);
            return fileContent;
        }
        catch (Exception) {
            return null;
        }
    }
}