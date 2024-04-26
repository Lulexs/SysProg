namespace Projekat1.HawkeyeCache;

public class HawkeyeCache {
    private readonly int _size;
    private readonly int _maxAge;
    private readonly TimeSpan _timeToLive;
    private readonly Dictionary<string, CacheEntry> _data;
    private readonly LinkedList<string> _keysList;
    private readonly OptGen _optGen;
    private readonly HawkeyePredictor _predictor;
    private readonly object _lockObj = new();
    private int _hits;
    private int _accesses;
    
    public HawkeyeCache(int size = 100, int m = 3, int secondsToLive = 180) {
        _size = size;
        _maxAge = (2 << m) - 1;
        _timeToLive = TimeSpan.FromSeconds(secondsToLive);
        _data = new Dictionary<string, CacheEntry>(_size);
        _keysList = new LinkedList<string>();
        _optGen = new OptGen(_size);
        _predictor = new HawkeyePredictor(3);
    }

    private void _TrainPredictor(string searchWord) {
        if (_optGen.HitOrMiss(searchWord)) {
            _predictor.TrainPositive(searchWord);
        }
        else {
            _predictor.TrainNegative(searchWord);
        }
    }

    public Dictionary<string, int>? GetValue(string searchWord) {
        lock (_lockObj) {
            if (_data.TryGetValue(searchWord, out CacheEntry? entry) &&
                DateTime.Now - entry.LastAccessed < _timeToLive) {
                bool isFriendly = _predictor.FriendlyOrAverse(searchWord);
                _keysList.Remove(entry.AssociatedNode);
                if (isFriendly) {
                    entry.SRripAge = 0;
                    _keysList.AddFirst(entry.AssociatedNode);
                }
                else {
                    entry.SRripAge = _maxAge;
                    _keysList.AddLast(entry.AssociatedNode);
                }

                entry.LastAccessed = DateTime.Now;

                _TrainPredictor(searchWord);

                _hits += 1;
                _accesses += 1;
                Console.WriteLine($"Cache hit; Stats: {_hits}/{_accesses} | {_hits / (float)_accesses * 100}%");
                return entry.Payload;
            }

            _accesses += 1;
            Console.WriteLine($"Cache miss; Stats: {_hits}/{_accesses} | {_hits / (float)_accesses * 100}%");
            return null;
        }
    }

    public void InsertValue(string searchWord, Dictionary<string, int>searchResult) {
        lock (_lockObj) {
            bool isFriendly = _predictor.FriendlyOrAverse(searchWord);
            int srripAge = isFriendly ? 0 : _maxAge;
            if (_size == _data.Count) {
                if (isFriendly && _data[_keysList.Last()].SRripAge != _maxAge) {
                    foreach (CacheEntry entry in _data.Values) {
                        entry.SRripAge += 1;
                    }
                }

                _data.Remove(_keysList.Last());
                _keysList.RemoveLast();
            }

            LinkedListNode<string> associatedNode = new LinkedListNode<string>(searchWord);
            CacheEntry newEntry = new CacheEntry(searchResult, srripAge, DateTime.Now, associatedNode);
            if (srripAge == 0) {
                _keysList.AddFirst(associatedNode);
            }
            else {
                _keysList.AddLast(associatedNode);
            }

            _data.Add(searchWord, newEntry);

            _TrainPredictor(searchWord);
        }
    }
}