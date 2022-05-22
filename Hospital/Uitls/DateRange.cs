using MongoDB.Bson.Serialization.Attributes;

namespace HospitalSystem.Utils;

public class DateRange
{
    // the minimum allowed range (makes scheduling easy)
    [BsonIgnore]
    private static TimeSpan _minDuration = TimeSpan.FromSeconds(60);  
    public DateTime Starts { get; set; }
    public DateTime Ends { get; set; }

    public DateRange(DateTime starts, DateTime ends, bool allowPast = false)
    {
        if (ends < starts)
        {
            throw new ArgumentException("Range can not end before it starts.");
        }

        if (ends - starts < _minDuration)
        {
            throw new ArgumentException("Can not make range that lasts less than the minimum duration: " 
                + _minDuration.Seconds + " seconds.");
        }

        if (!allowPast && starts < DateTime.Now)
        {
            throw new ArgumentException("Can not create range that starts in the past.");
        }

        Starts = starts;
        Ends = ends;
    }

    public override string ToString()
    {
        return Starts + " - " + Ends;
    }

    public bool Contains(DateTime date)
    {
        return Starts <= date && date <= Ends;
    }

    public bool Overlaps(DateRange range)
    {
        return Starts <= range.Ends && range.Starts <= Ends;
    }

    public TimeSpan GetDuration()
    {
        return Ends - Starts;
    }

    public bool HasPassed()
    {
        return Ends < DateTime.Now;
    }

    public bool IsFuture()
    {
        return Starts > DateTime.Now;
    }
}