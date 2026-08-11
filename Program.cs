using System;
using System.IO;
using System.Runtime.InteropServices;

class ShellProgram
{
    static void Main()
    {
         while (true)
        {
            Console.Write("$ ");
            string command = Console.ReadLine();
            if (command == "exit")
            {
                break;
            }
            if (command.StartsWith("echo "))
            {
                Console.WriteLine(command[5..]);
                continue;
            }
            if (command.StartsWith("type"))
            {
                if (command[5..] == "echo" || command[5..] == "type" || command[5..] =="exit")
                {
                    Console.WriteLine(command[5..] + " is a shell builtin");
                }
                else
                {
                    string target = command[5..];
                    string? executable = FindExecutable(target);
                    if(executable !=null)
                        Console.WriteLine($"{target} is {executable}");
                    else
                        Console.WriteLine($"{target}: not found");
                }
            }  
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
}

//REPL - Read Eval Print Loop is interactive loop forming core of the shell
// Read; Displau a prompt and wait for user input 
// Eval: parse and executre the command 
// PrintL display output or error message 
// Loop: return to step 1 and wait for next command 

// Cycle is continues indef until shell process is terminated 
