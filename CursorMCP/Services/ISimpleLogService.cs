namespace CursorMCP.Services;

using Microsoft.Extensions.Configuration;

public interface ISimpleLogService
{
    void Log(string message);
}

public class SimpleLogService(IConfiguration configuration) : ISimpleLogService
{
    private readonly string logFilePath = configuration.GetValue<string>("LogFilePath") ?? "log.txt";

    public void Log(string message)
    {
        File.AppendAllText(logFilePath, message);
        File.AppendAllText(logFilePath, Environment.NewLine);
    }
}