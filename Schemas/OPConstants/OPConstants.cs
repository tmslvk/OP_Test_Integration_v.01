 namespace BPMSoft.Configuration.OPConstants
{
    using System;
    using System.ComponentModel;

    public static class OPResponseStatus
    {
        public static string OK { get; set; } = "OK";
        public static string Error { get; set; } = "Error";
    }

    public static class OPScheduleType
    {
        public static readonly string HOURLY = "HOURLY";
        public static readonly string EVERY_2_HOURS = "EVERY_2_HOURS";
        public static readonly string EVERY_24_HOURS = "EVERY_24_HOURS";
        public static readonly string DAILY = "DAILY";
        public static readonly string DAILY_AT_02 = "DAILY_AT_02";
        public static readonly string EVERY_5_MIN = "EVERY_5_MIN";
    }

    public static class OPSchedulerIntervals
    {
        public const char MinuteSuffix = 'm';
        public const char HourSuffix = 'h';

        public static TimeSpan Parse(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Interval is empty");

            value = value.Trim().ToLowerInvariant();

            if (IsAllDigits(value))
            {
                return TimeSpan.FromMinutes(int.Parse(value));
            }

            char lastChar = value[value.Length - 1];
            string numberPart = value.Substring(0, value.Length - 1);

            if (!IsAllDigits(numberPart))
                throw new FormatException("Invalid interval format: " + value);

            int number = int.Parse(numberPart);

            if (lastChar == MinuteSuffix)
                return TimeSpan.FromMinutes(number);

            if (lastChar == HourSuffix)
                return TimeSpan.FromHours(number);

            throw new FormatException("Unknown interval suffix: " + lastChar);
        }

        private static bool IsAllDigits(string value)
        {
            if (string.IsNullOrEmpty(value))
                return false;

            for (int i = 0; i < value.Length; i++)
            {
                if (value[i] < '0' || value[i] > '9')
                    return false;
            }

            return true;
        }
    }
}