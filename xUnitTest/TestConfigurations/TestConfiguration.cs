
public class TestConfiguration
{
    public readonly Dictionary<string, string> Configs;
    public TestConfiguration()
    {
        Configs = new Dictionary<string, string>
        {
            // Add more environment variables as needed
            { "BusConnectionString", "Endpoint=sb://buspoc.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=bUuFpmAsPLGFuK/R+j9PuHr8nr8L13T7c+ASbPdWfls=" }, 
        }; 
    }
}