using System.Diagnostics;

namespace Seismic.ReportsService.Common;

public static class ActivityExtensions
{
    public static void RecordFailure(this Activity? activity, Exception exception)
    {
        if (activity is null) return;
        activity.SetStatus(ActivityStatusCode.Error, exception.Message);
        activity.AddException(exception);
    }
}
