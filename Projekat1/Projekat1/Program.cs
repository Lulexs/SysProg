using Projekat1.HawkeyeCache;

namespace Projekat1;

class Program  {
    public static void Main(string[] args) {
        OptGen optGen = new(2);
        string[] letters = ["A", "B", "B", "C", "D", "E", "A", "F", "D", "E", "F", "C"];

        foreach (var l in letters) {
            Console.WriteLine(optGen.HitOrMiss(l));
            optGen.PrintHistory();
            Console.WriteLine();
        }
    }
}