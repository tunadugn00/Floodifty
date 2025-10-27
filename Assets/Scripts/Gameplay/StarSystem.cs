using UnityEngine;

public static class StarSystem
{
    public static int CalculateStars(int movesUsed, int movesAllowed)
    {
        if (movesUsed <= movesAllowed) return 3;
        else if (movesUsed == movesAllowed + 1 || movesUsed == movesAllowed + 2)
        {
            return 2;
        }
        else
        {
            return 1;
        }
    }
}
