namespace BeestjeOpJeFeestje_PROG6.Services;

public static class CalculateNumberOfAnimals
{
    public static int GetMaxAnimals(string cardType)
    {
        switch (cardType.ToLower())
        {
            case "zilveren":
                return 4;
            case "gouden":
                return int.MaxValue;
            case "platina":
                return int.MaxValue; 
            default:
                return 3;
        }
    }
    
    public static bool GetBookingVipStatus(string cardType)
    {
        if (cardType == "platina")
        {
            return true;
        }
        return false;
    }
}