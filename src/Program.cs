using System;
using System.Data;
using System.Diagnostics.Tracing;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

class ShellProgram
{

    public static List<string> builtins = new List<string> {"exit", "type", "echo"};

    static void Main()
    {
         while (true)
        {
            Console.Write("$ ");
            string text = Console.ReadLine();
            string command = text.Trim();
            string arg = "";
            foreach (char c in command)
            {
                if (char.IsWhiteSpace(c)){
                    int i = command.IndexOf(c);
                    command = command[..i];
                    arg = command[i..];
                }
                arg = string.Empty;
            }
            Dispatch(command,arg);
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
        if (command.StartsWith("echo"))
        {
            return Echo(arg);
        }
        if (command.StartsWith("type"))
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
