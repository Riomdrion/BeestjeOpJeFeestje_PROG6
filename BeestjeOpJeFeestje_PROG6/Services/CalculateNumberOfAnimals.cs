namespace BeestjeOpJeFeestje_PROG6.Services;

public static class CalculateNumberOfAnimals
{
    public static int GetMaxAnimals(string cardType)
    {
        switch (cardType.ToLower())
        {
            case "silver":
                return 4;
            case "gold":
                return int.MaxValue;
            case "platinum":
                return int.MaxValue; 
            default:
                return 3;
        }
    }
    
    public static bool GetBookingVipStatus(string cardType)
    {
        if (cardType.ToLower() == "platinum")
        {
            return true;
        }
        return false;
    }
}