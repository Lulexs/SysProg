namespace Projekat1;

public static class Program  {
    public static void Main(string[] args) {
        WebServer.WebServer webServer = new();
        webServer.Init();
    }

}