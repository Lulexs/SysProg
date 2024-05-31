using System.Net.Http.Headers;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Newtonsoft.Json.Linq;
using Projekat3.Models;

namespace Projekat3.Services;

public class GithubService(string githubApiToken) {
    private HttpClient HttpClient { get; } = new();
    private readonly string _baseUrl = "https://api.github.com/search/repositories?q=topic:";
    private string GithubApiToken { get; } = githubApiToken;
    
    public IObservable<GithubInfo> GetRelatedRepositories(HashSet<string> topics) {
        var observables = topics.Select(topic =>
            Observable.FromAsync(() => FetchReposAsync(topic)).SelectMany(repos => repos));
        return observables.Merge().SubscribeOn(new EventLoopScheduler()).ObserveOn(new EventLoopScheduler());
    }

    private async Task<IEnumerable<GithubInfo>> FetchReposAsync(string topic) {
        string url = _baseUrl + topic;

        Console.WriteLine($"Calling Github api in thread ${Thread.CurrentThread.ManagedThreadId}");
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", GithubApiToken);
        HttpClient.DefaultRequestHeaders.UserAgent.TryParseAdd("request");
        var response = await HttpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();
        JObject jsonResponse = JObject.Parse(responseBody);
        var repositories = jsonResponse["items"];

        if (repositories == null)
            return [];

        return repositories.Select(repo => new GithubInfo() {
            Name = repo["name"] != null ? (string)repo["name"]! : "####",
            Topic = topic,
            Forks = repo["forks_count"] != null ? (int)repo["forks_count"]! : -1,
            Stars = repo["stargazers_count"] != null ? (int)repo["stargazers_count"]! : -1,
            Size = repo["size"] != null ? (int)repo["size"]! : -1
        });
    }
}