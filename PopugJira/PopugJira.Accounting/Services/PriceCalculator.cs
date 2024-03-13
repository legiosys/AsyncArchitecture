namespace PopugJira.Accounting.Services;

public static class PriceCalculator
{
    public static (int assign, int complete) Calc()
        => (Random.Shared.Next(10, 20), Random.Shared.Next(20, 40));
}