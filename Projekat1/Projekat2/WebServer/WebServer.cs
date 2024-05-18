using System.Diagnostics;
using System.Net;
using Ganss.Text;

namespace Projekat2.WebServer;

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

    public async Task InitAsync() {
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
                context = await _listener.GetContextAsync();
            }
            catch (Exception ec) {
                Console.WriteLine("Failed to accept connection due to " + ec.Message);
                continue;
            }

            _ = ProcessRequest(context);
        }
    }

    private async Task ProcessRequest(object? context) {
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
                buffer = await GenerateResponseForValidRequestAsync(searchWords.ToHashSet());
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
        await output.WriteAsync(buffer, 0, buffer.Length);
        output.Close();
        stopwatch.Stop();
        Console.WriteLine($"{request.HttpMethod} from {request.UserHostAddress} successfully processed in ${stopwatch.Elapsed.TotalSeconds} seconds");

    }

    private async Task<byte[]> GenerateResponseForValidRequestAsync(HashSet<string> searchWords) {
        Dictionary<string, Dictionary<string, int>> statisticsDict = searchWords.ToDictionary(w => w, _ => new Dictionary<string, int>());
        IEnumerable<string> searchFiles = [..Directory.GetFiles(_rootDirectory, "*.txt")];
        List<Task<Dictionary<string, Tuple<string, int>>?>> tasks = []; 
        List<string> toBeProcessed = [];

        
        foreach (var word in searchWords) {
            Dictionary<string, int>? cachedVal;

            if ((cachedVal = Cache.GetValue(word)) != null) {
                statisticsDict[word] = cachedVal;
            }
            else {
                toBeProcessed.Add(word);
            }
        }

        AhoCorasick ac = new AhoCorasick(toBeProcessed);

        foreach (var file in searchFiles) {
            tasks.Add(SearchWordsInFile(file, ac));
        }
        await Task.WhenAll(tasks);

        foreach (var task in tasks) {
            if (task.Result != null) {
                foreach (KeyValuePair<string, Tuple<string, int>> pair in task.Result) {
                    statisticsDict[pair.Key].Add(pair.Value.Item1, pair.Value.Item2);
                } 
            }
        }

        foreach (var word in toBeProcessed) {
            Cache.InsertValue(word, statisticsDict[word]);
        }
        
        return HtmlGen.GenerateSearchWordAppearanceTable(statisticsDict.ToDictionary());
    }

    private async Task<Dictionary<string, Tuple<string, int>>?> SearchWordsInFile(string filepath, AhoCorasick ac){        
        if (!File.Exists(filepath))
            return null;
        
        try {
            string content = await File.ReadAllTextAsync(filepath);
            return ac.Search(content).GroupBy(w => w.Word)
                .ToDictionary(g => g.Key, g => new Tuple<string, int>(filepath, g.Count()));
        }
        catch (Exception ec) {
            Console.WriteLine($"Processing of file {filepath} failed due to: {ec.Message}");
            return null;
        }
    }
}