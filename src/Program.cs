using System.Data;
using System.Reflection.Metadata.Ecma335;

class ShellProgram
{
    public static List<string> builtins = new List<string> {"exit", "type", "echo"};

    public static bool runshell = true;
    static void Main()
    {
         while (runshell is true)
        {
            Console.Write("$ ");
            string text = Console.ReadLine().Trim();
            string[] args = text.Split("");
            runshell = Dispatch(args);
        }
    }
    static string? FindExecutable(string target)
    {
        string path = Environment.GetEnvironmentVariable("PATH");
        char separator = Path.PathSeparator;
        string [] directories = path!.Split(separator);
        foreach (var dir in directories)
        {
            var filePath = Path.Combine(dir, target);
            if (File.Exists(filePath))
            {
                var mode = File.GetUnixFileMode(filePath);
                var executePermissions =
                UnixFileMode.UserExecute |
                UnixFileMode.GroupExecute |
                UnixFileMode.OtherExecute;
                
                if((mode & executePermissions) != 0)
                {
                    return filePath;
                }            
            }
        }
        return null;
    }
    static bool Echo(string [] commandArgs)
    {
        Console.WriteLine(string.Join("",commandArgs));
        return true;
    }

    static bool GetType(string [] commandArgs)
    {
        string typeArg = commandArgs[1];
        if (builtins.Contains(typeArg))
        {
            Console.WriteLine(typeArg + " is a shell builtin");
        }
        else
        {
            string? executable = FindExecutable(typeArg);
            if(executable !=null)
            Console.WriteLine($"{typeArg} is {executable}");
            else
            Console.WriteLine($"{typeArg}: not found");
        }
        return true;
    }
    static bool Dispatch(string [] args)
    {   
        string command = args[0];
        string [] commandArgs = args[1..];
        if(command == "exit")
        {
            return false;
        }
        if (command == "echo")
        {
            Echo(commandArgs);
            return true;
        } 
        if (command == "type")
        {
            GetType(commandArgs);
            return true;
        }
        else
        {
            Console.WriteLine($"{command}: command not found");
            return true;
        }
    }
}
