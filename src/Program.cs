class ShellProgram
{
    static void Main()
    {
         Console.Write("$ ");
         string command = Console.Read();
         Console.WriteLine("{command}: command not found", command);
    }
}

class ErrorMessaage
{
    public static void PrintErrorMessage(string message)
    {
        Console.Writelline("Invalid Input: " + message);
    }
}