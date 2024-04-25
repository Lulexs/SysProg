namespace Projekat1;

class Program  {
    public static void Main(string[] args) {
        WebServer.WebServer webServer = new();
        webServer.Init();

        // AhoCorasick ac = new AhoCorasick("ACC", "ATC", "CAT", "GCG");
        // string[] words = [];
        // string pattern = "GCATCG";
        // var results = ac.Search(pattern).GroupBy(w => w.Word).ToDictionary(g => g.Key, g => g.Count());
        // foreach (var result in results) {
        //     Console.WriteLine(result.Key + " " + result.Value);
        // }
    }


}