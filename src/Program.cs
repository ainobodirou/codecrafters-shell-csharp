using System.Data;
using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography.X509Certificates;

class ShellProgram
{
    public static List<string> builtins = new List<string> {"exit", "type", "echo", "pwd", "cd"};
    public static bool runshell = true;

    static string[] ParseInput(string userInput)
    {
        List<string> args = new List<string>();
        string curr = "";
        bool quote = false;
        bool dquote = false;

        foreach (var item in userInput)
        {
            if (item == '\'')
            {
               quote = !quote;
               continue;
            }
            if (item == '\"')
            {
                dquote = !dquote;
                continue;
            }
            
            if (char.IsWhiteSpace(item))
            {
                if (!quote && !dquote)
                {
                    if (curr.Length > 0){
                        args.Add(curr);
                        curr = "";
                        continue;
                    }
                }
                else
                {
                    curr += item;
                }
            }
            else
            {
                curr += item;
            }
        }
        args.Add(curr);
        return args.ToArray();
    }
    static void Main()
    {
         while (runshell is true)
        {
            Console.Write("$ ");
            string text = Console.ReadLine().Trim();
            string[] args = ParseInput(text);
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
    static void Echo(string [] commandArgs)
    {
        Console.WriteLine(string.Join(" ",commandArgs));
        return;
    }

    static void HandleCd(string absPath)
    {
        if (absPath == "~")
        {
            try
            {
                string homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                if (string.IsNullOrWhiteSpace(homeDir))
                {
                    homeDir = Environment.GetEnvironmentVariable("HOME")
                            ?? Environment.GetEnvironmentVariable("USERPROFILE")
                            ?? throw new Exception("UNable to determine HOME directory");
                }
                Environment.CurrentDirectory = homeDir;

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }

        }
        else
        {
            if(Directory.Exists(absPath))
            {
                Environment.CurrentDirectory = absPath;
            }
            else
            {
                Console.WriteLine($"cd: {absPath}: No such file or directory");
            }
        }
        return;
    }

    static void GetType(string [] commandArgs)
    {
        string typeArg = commandArgs[0];
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
        return;
    }

    static void Execute(string command, string [] commandArgs)
    {
        string? executable = FindExecutable(command);
        if(executable != null)
            Process.Start(command, commandArgs).WaitForExit();
        else
            Console.WriteLine($"{command}: command not found");
        return;
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
        if (command == "pwd")
        {
            string workingDirectory = Environment.CurrentDirectory;
            Console.WriteLine(workingDirectory); 
            return true;
        }
        if (command == "cd")
        {
            HandleCd(commandArgs[0]);
            return true;
        }
        else
        {
            Execute(command, commandArgs);
            return true;
        }
    }

    //implementing the cd builtin
    // include the cd builtin in the builtins collection
    //implement the cd method that:
    // handles absolute paths like /usr/local/bin
    // handles relative paths like ./, ../, ./dir
    // the ~ character which represents home directory

}  
