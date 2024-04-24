using System.Collections;

namespace Projekat1.HawkeyeCache;

public class HawkeyePredictor {
    private readonly Dictionary<int, BitArray> _wordMap;
    private readonly int _numberOfEntries = 65536; // 2 ^ 16
    
    public HawkeyePredictor() {
        _wordMap = new Dictionary<int, BitArray>(_numberOfEntries);
    }
    
    
}