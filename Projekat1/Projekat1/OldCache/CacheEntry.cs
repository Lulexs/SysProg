namespace Projekat1.OldCache;

public class CacheEntry(string payload, int srripAge, DateTime timeout) {
    public readonly string Payload = payload;
    public int SRripAge = srripAge;
    public DateTime Timeout = timeout;
}