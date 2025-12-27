using PaymentScheduler.Domain.Enums;

namespace PaymentScheduler.Application.Interface.Common;

public interface IProviderRepository
{
    DateTime AdjustForHolidaysAndWeekends(DateTime date);

    DateTime CalculateNextExecutionDate(DateTime currentDate, PaymentFrequency frequency);

    HashSet<DateTime> GetHolidays(int year);
}