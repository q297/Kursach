namespace Client;

// Example usage:
public static class Program
{
    private static void Main()
    {
        new View.View().MainAsync().GetAwaiter().GetResult();
    }
}