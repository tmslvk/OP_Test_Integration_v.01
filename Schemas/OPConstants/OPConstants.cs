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
    }
}