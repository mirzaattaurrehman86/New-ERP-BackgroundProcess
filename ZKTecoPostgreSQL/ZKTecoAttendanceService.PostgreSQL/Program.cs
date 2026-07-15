using ZKTecoAttendanceService.PostgreSQL.Services;
class Postgreprogram
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("************ ..::.. STARTED ..::.. ************");

        ProcessService processService = new ProcessService();
        await processService.loadCurrentMonthPunchesAsync();

        Console.WriteLine("************ ..::.. COMPLETED ..::.. ************");
    }
}