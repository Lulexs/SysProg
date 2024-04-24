namespace Projekat1.HawkeyeCache;

public class CacheEntry(string payload, int srripAge, DateTime lastAccessed, LinkedListNode<string> associatedNode) {
    public readonly string Payload = payload;
    public int SRripAge = srripAge;
    public DateTime LastAccessed = lastAccessed;
    public LinkedListNode<string> AssociatedNode = associatedNode;
}