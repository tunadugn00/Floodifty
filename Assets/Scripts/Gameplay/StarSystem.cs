using UnityEngine;

public static class StarSystem
{
    public static int CalculateStars(int movesUsed, int movesAllowed)
    {
        if (movesUsed <= movesAllowed) return 3;
        else if (movesUsed == movesAllowed + 1) return 2;
        else if (movesUsed == movesAllowed + 2) return 1;
        else return 0; // lose
    }
}
