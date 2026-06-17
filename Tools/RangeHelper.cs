namespace Tools;

public class RangeHelper
{

    /// <summary>
    /// Génère la liste des mois (year, month) couverts par [start, end), bornée à 24 mois
    /// pour éviter une explosion si le filtre "all"/"year" couvre une longue plage.
    /// </summary>
    private static List<(int Year, int Month)> GenerateMonthlyRange(DateTime start, DateTime end)
    {
        var safeStart = start < end.AddYears(-2) ? end.AddYears(-2) : start;

        var months = new List<(int Year, int Month)>();
        var cursor = new DateTime(safeStart.Year, safeStart.Month, 1);
        var last   = new DateTime(end.Year, end.Month, 1);

        while (cursor <= last)
        {
            months.Add((cursor.Year, cursor.Month));
            cursor = cursor.AddMonths(1);
        }

        return months;
    }
}