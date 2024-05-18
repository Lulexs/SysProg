using Nito.Collections;
using Projekat2.Utils;

namespace Projekat2.HawkeyeCache;

public class OptGen {
    private readonly Deque<IntPair> _livenessIntervals;
    private readonly int _cacheSize;
    private readonly int _optGenFactor = 8;
    private readonly int _historyLength;

    public OptGen(int cacheSize) {
        _cacheSize = cacheSize;
        _historyLength = _optGenFactor * _cacheSize;
        _livenessIntervals = new Deque<IntPair>(_historyLength);
    }

    public bool HitOrMiss(string searchWord) {
        int hashedWord = searchWord.GetHashCode();
        int count = _livenessIntervals.Count - 1;
        bool optHit = true;
        
        while (count >= 0 
                && _livenessIntervals[count].LivenessIntervals < _cacheSize
                && _livenessIntervals[count].HashedString != hashedWord) {
            count -= 1;
        }

        if (count < 0 || _livenessIntervals[count].LivenessIntervals >= _cacheSize) {
            optHit = false;
        }
        else {
            for (int i = _livenessIntervals.Count - 1; i >= count; --i)
                _livenessIntervals[i].LivenessIntervals += 1;
        }

        if (_livenessIntervals.Count >= _historyLength) {
            _livenessIntervals.RemoveFromFront();
        }
        _livenessIntervals.AddToBack(new IntPair(hashedWord));

        return optHit;
    }
}