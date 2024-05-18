namespace Projekat2.Utils;

public static class Hashing {
    public static unsafe int Get16BitHash(string str) {
        int hash = 0;
        int len = str.Length;

        fixed (char* ch = str)
        {
            for (int i = 0; i < len; i++)
            {
                hash = hash + ((hash) << 5) + *(ch + i) + ((*(ch + i)) << 7);
            }
        }

        return ((hash) ^ (hash >> 16)) & 0xffff;
    }
}