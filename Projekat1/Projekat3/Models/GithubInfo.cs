namespace Projekat3.Models;

public class GithubInfo {
    public required string Name { get; set; }
    public required string Topic { get; set; }
    public int Stars { get; set; }
    public int Forks { get; set; }
    public int Size { get; set; }
}