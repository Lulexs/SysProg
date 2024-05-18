using System.Timers;
using System.Diagnostics;
using Timer = System.Timers.Timer;

namespace Projekat2;

public static class Program  {
    public static async Task Main(string[] args) {
        Timer timer = new Timer(20000);
        timer.Elapsed += TimerElapsed!;
        timer.AutoReset = true;
        timer.Start();
        
        WebServer.WebServer webServer = new();
        await webServer.InitAsync();
        
        timer.Stop();
        timer.Dispose();
    }

    private static void TimerElapsed(object sender, ElapsedEventArgs e)
    {
        Process currentProcess = Process.GetCurrentProcess();
        int threadCount = currentProcess.Threads.Count;
        double cpuTime = currentProcess.TotalProcessorTime.TotalSeconds;
        double kernelModeCpuTime = currentProcess.PrivilegedProcessorTime.TotalSeconds;
        long totalAllocatedMem = currentProcess.WorkingSet64 / 1024 / 1024;
        
        Console.WriteLine($"Number of threads running: {threadCount}");
        Console.WriteLine($"Total CPU time: {cpuTime}");
        Console.WriteLine($"Total CPU kernel time: {kernelModeCpuTime}");
        Console.WriteLine($"Physical memory usage: ${totalAllocatedMem}MB");
    }
}