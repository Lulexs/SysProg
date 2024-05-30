using System.Reactive.Linq;
using Newtonsoft.Json.Linq;
using Projekat3.Models;

namespace Projekat3.Services;

public class GithubService(HttpClient httpClient) {
    private HttpClient HttpClient { get; } = httpClient;
    private readonly string _baseUrl = "https://api.github.com/search/repositories?q=topic:";

    public IObservable<GithubInfo> GetRelatedRepositories(string topic) {
        return Observable.FromAsync(() => FetchReposAsync(topic)).SelectMany(repos => repos);
    }

    private async Task<IEnumerable<GithubInfo>> FetchReposAsync(string topic) {
        string url = _baseUrl + topic;

        var response = await HttpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();
        JObject jsonResponse = JObject.Parse(responseBody);
        var repositories = jsonResponse["items"];

        if (repositories == null)
            return [];

        return repositories.Select(repo => new GithubInfo() {
            Name = repo["name"] != null ? (string)repo["name"]! : null,
            Forks = repo["forks_count"] != null ? (int)repo["forks_count"]! : null,
            Stars = repo["stargazers_count"] != null ? (int)repo["stargazers_count"]! : null,
            Size = repo["size"] != null ? (int)repo["size"]! : null
        });
    }
}