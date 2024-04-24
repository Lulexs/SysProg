namespace Projekat1.Utils;

public class Pair(int hashedString) {
    public int HashedString { get; } = hashedString;
    public int LivenessIntervals { get; set; } = 0;
}
