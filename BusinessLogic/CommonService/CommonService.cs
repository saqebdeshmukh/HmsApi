using System;
using System.Collections.Generic;
using System.Text;

namespace BusinessLogic.CommonService
{
    public class CommonService : ICommonService
    {
        public DateTime GetCurrentIndianTime()
        {
            return TimeZoneInfo.ConvertTimeFromUtc(
                DateTime.UtcNow,
                TimeZoneInfo.FindSystemTimeZoneById("India Standard Time")
            );
        }
    }
}
