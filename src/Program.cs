using System;
using System.Data;
using System.Diagnostics.Tracing;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

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
            string command = "";
            string arg = "";
            foreach (char c in text)
            {
                if (char.IsWhiteSpace(c)){
                    int i = text.IndexOf(c);
                    command = text[..i].Trim();
                    arg = text[i..].Trim();
                    break;
                }
                else
                {
                    command = text;
                }
            }
            
            runshell = Dispatch(command,arg);
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
    static bool Echo(string arg)
    {
        Console.WriteLine(arg);
        return true;
    }

    static bool GetType(string arg)
    {
        if (builtins.Contains(arg))
        {
            Console.WriteLine(arg + " is a shell builtin");
        }
        else
        {
            string? executable = FindExecutable(arg);
            if(executable !=null)
            Console.WriteLine($"{arg} is {executable}");
            else
            Console.WriteLine($"{arg}: not found");
        }
        return true;
    }
    static bool Dispatch(string command, string arg)
    {
        if(command == "exit")
        {
            return false;
        }
        if (command == "echo")
        {
            return Echo(arg);
        } 
        if (command == "type")
        {
            return GetType(arg);
        }
        else
        {
            Console.WriteLine($"{command}: command not found");
            return true;
        }
    }
}
