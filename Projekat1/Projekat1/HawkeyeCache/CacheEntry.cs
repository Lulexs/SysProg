namespace Projekat1.HawkeyeCache;

public class CacheEntry(Dictionary<string, int> payload, int srripAge, DateTime lastAccessed, LinkedListNode<string> associatedNode) {
    public readonly Dictionary<string, int> Payload = payload;
    public int SRripAge = srripAge;
    public DateTime LastAccessed = lastAccessed;
    public readonly LinkedListNode<string> AssociatedNode = associatedNode;
}