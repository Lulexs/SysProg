using Nito.Collections;

namespace Projekat1.Cache;

public class SRripCache {
    private readonly int _size;
    private readonly int _maxAge;
    private readonly TimeSpan _lifeSpan;
    private readonly Dictionary<string, CacheEntry> _data;
    private readonly Deque<string> _keysDeque;

    public SRripCache(int size = 5, int m = 3, int lifeSpan = 180) {
        _size = size;
        _maxAge = (2 << m) - 1;
        _lifeSpan = TimeSpan.FromSeconds(lifeSpan);
        _data = new Dictionary<string, CacheEntry>(_size);
        _keysDeque = new Deque<string>(_size);
    }

    public void InsertValue(string searchWord, string searchResult) {
        if (_size == _data.Count) {
            if (_data[_keysDeque.Last()].SRripAge != _maxAge) {
                int diff = _maxAge - _data[_keysDeque.Last()].SRripAge;
                foreach (CacheEntry entry in _data.Values) {
                    entry.SRripAge += diff;
                }
            }
            _data.Remove(_keysDeque.Last());
            _keysDeque.RemoveFromBack();
        }
        CacheEntry newEntry = new CacheEntry(searchResult, _maxAge, DateTime.Now);
        _data.Add(searchWord, newEntry);
        _keysDeque.AddToBack(searchWord);
    }

    public string? GetValue(string searchWord) {
        if (_data.TryGetValue(searchWord, out CacheEntry? entry) && DateTime.Now - entry.Timeout < _lifeSpan) {
            entry.SRripAge = 0;
            entry.Timeout = DateTime.Now;
            _keysDeque.Remove(searchWord);
            _keysDeque.AddToFront(searchWord);
            return entry.Payload;
        }

        return null;
    }

    public void PrintCash() {
        foreach (KeyValuePair<string, CacheEntry> pair in _data) {
            Console.WriteLine(pair.Key + " " + pair.Value.Payload + " " + pair.Value.SRripAge);
        }
        Console.WriteLine();

        foreach (string a in _keysDeque) {
            Console.Write(a + " ");
        }
        Console.WriteLine();
    }
}