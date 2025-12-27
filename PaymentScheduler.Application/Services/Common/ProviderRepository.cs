using PaymentScheduler.Application.Interface.Common;
using PaymentScheduler.Domain.Enums;

namespace PaymentScheduler.Application.Services.Common;

public class ProviderRepository : IProviderRepository
{
    public DateTime AdjustForHolidaysAndWeekends(DateTime date)
    {
        while (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
            date = date.AddDays(1);

        var holidays = GetHolidays(date.Year);
        while (holidays.Contains(date.Date))
        {
            date = date.AddDays(1);
            while (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                date = date.AddDays(1);
        }

        return date;
    }

    public HashSet<DateTime> GetHolidays(int year)
    {
        return new HashSet<DateTime>
        {
            new DateTime(year, 1, 1),
            new DateTime(year, 7, 4),
            new DateTime(year, 12, 25)
        };
    }

    public DateTime CalculateNextExecutionDate(DateTime currentDate, PaymentFrequency frequency)
    {
        DateTime nextDate = frequency switch
        {
            PaymentFrequency.Daily => currentDate.AddDays(1),
            PaymentFrequency.Weekly => currentDate.AddDays(7),
            PaymentFrequency.BiWeekly => currentDate.AddDays(14),
            PaymentFrequency.Monthly => currentDate.AddMonths(1),
            PaymentFrequency.Quarterly => currentDate.AddMonths(3),
            PaymentFrequency.Annually => currentDate.AddYears(1),
            _ => throw new ArgumentException($"Unsupported frequency: {frequency}")
        };

        return AdjustForHolidaysAndWeekends(nextDate);
    }
}