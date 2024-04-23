using Projekat1.Cache;

namespace Projekat1;

class Program  {
    public static void Main(string[] args) {
        SRripCache cash = new SRripCache(3, 2, 180);
        cash.InsertValue("a", "1");
        cash.GetValue("a");
        cash.InsertValue("b", "2");
        cash.InsertValue("c", "3");
        
        cash.PrintCash();
    }
}