namespace BPMSoft.Configuration.OPCronConverter
{
    using BPMSoft.Core;
    using BPMSoft.Core.Configuration;
    using BPMSoft.Configuration.OPConstants;

    public static class OPCronConverter
    {
        public static string GetCron(UserConnection uc)
        {
            var type = (string)SysSettings.GetValue(uc, "OPCarsIntegrationScheduleType");

            if (type == OPScheduleType.HOURLY)
                return "0 0 * * * ?";

            if (type == OPScheduleType.EVERY_2_HOURS)
                return "0 0 */2 * * ?";

            if (type == OPScheduleType.EVERY_24_HOURS)
                return "0 0 0 * * ?";

            if (type == OPScheduleType.DAILY)
                return "0 0 0 * * ?";

            if (type == OPScheduleType.DAILY_AT_02)
                return "0 0 2 * * ?";

            if (type == OPScheduleType.EVERY_5_MIN)
                return "*/5 * * * * ?";

            return "0 0 * * * ?"; // fallback
        }
    }
}