using Projekat2.Utils;

namespace Projekat2.HawkeyeCache;

public class HawkeyePredictor {
    private readonly Dictionary<int, int> _wordMap;
    private readonly int _numberOfEntries = 65536; // 2 ^ 16
    private readonly int _maxPredictorValue;

    public HawkeyePredictor(int predictorBits) {
        _wordMap = new Dictionary<int, int>(_numberOfEntries);
        _maxPredictorValue = 2 << predictorBits - 1;
    }

    public bool FriendlyOrAverse(string searchWord) {
        return !_wordMap.TryGetValue(Hashing.Get16BitHash(searchWord), out int predictor)
               || predictor > _maxPredictorValue >> 2;
    }

    public void TrainPositive(string searchWord) {
        int hashedValue = Hashing.Get16BitHash(searchWord);
        if (_wordMap.TryGetValue(hashedValue, out int predictor)) {
            _wordMap[hashedValue] = (predictor < _maxPredictorValue) ? predictor + 1 : predictor;
        }
        else {
            _wordMap.Add(Hashing.Get16BitHash(searchWord), 2);
        }
    }

    public void TrainNegative(string searchWord) {
        int hashedValue = Hashing.Get16BitHash(searchWord);
        if (_wordMap.TryGetValue(hashedValue, out int predictor)) {
            _wordMap[hashedValue] = predictor != 0 ? predictor - 1 : predictor;
        }
        else {
            _wordMap.Add(Hashing.Get16BitHash(searchWord), 2);
        }
    }
}