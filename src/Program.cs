using System.Diagnostics;
using System.Security.Authentication;
using Microsoft.VisualBasic;

class ShellProgram
{
    public static List<string> builtins = new List<string> {"exit", "type", "echo", "pwd", "cd"};
    public static bool runshell = true;


        enum ParseMode
    {
        Unquoted,
        SingleQuoted,
        DoubleQuoted
    }

    static (string[] Args, string? OutputPath) ParseInput(string userInput)
    {
        List<string> args = new List<string>();
        ParseMode mode = ParseMode.Unquoted;
        string curr = "";
        string redirect = null;
        bool argumentStarted = false;
        bool escapeNextCharacter = false;
        bool outputTarget = false;
    

        

        void FinishArgument()
        {
           
            if (outputTarget)
            {
                redirect += curr;
                outputTarget = false;
            }
            else
            {
                args.Add(curr);
            }
            curr = "";
            argumentStarted = false;
            }

        
        foreach (char character in userInput)
        {
            switch (mode)
            {
                case ParseMode.Unquoted:

                    if (escapeNextCharacter)
                    {
                        argumentStarted = true;
                        curr += character;
                        escapeNextCharacter = false;
                        continue;
                    }
                    if (character == '>')
                    {
                        FinishArgument();
                        outputTarget = true;
                        continue;
                    }
                    if (character == '\'')
                    {
                        argumentStarted = true;
                        mode = ParseMode.SingleQuoted;
                    }
                    else if (character == '"')
                    {
                        argumentStarted = true;
                        mode = ParseMode.DoubleQuoted;
                    }
                    else if (char.IsWhiteSpace(character))
                    {
                        FinishArgument();
                    }
                    else if (character == '\\')
                    {
                        escapeNextCharacter = true;
                        continue;
                    }
                    else
                    {
                        argumentStarted = true;
                        curr += character;
                    }
                    break;
                case ParseMode.SingleQuoted:
                    if (character == '\'')
                    {
                        mode = ParseMode.Unquoted;
                    }
                    else
                    {
                        curr += character;
                    }
                    break;
                case ParseMode.DoubleQuoted:

                    if (escapeNextCharacter)
                    {
                        if (character is '"' or '\\' or '$' or '`')
                        {
                            curr += character;
                        }
                        else
                            {
                                curr += '\\';
                                curr += character;
                            }
                            escapeNextCharacter = false;
                        }
                    
                    else if (character == '\\')
                    {
                        escapeNextCharacter = true;
                    }
                    else if (character == '"')
                    {
                        mode = ParseMode.Unquoted;
                    }
                    else
                    {
                        curr += character;
                    }
                    break;          
            }
        }
        FinishArgument();
        return (args.ToArray(), redirect);
    }

    static void Main()
    {
         while (runshell is true)
        {
            Console.Write("$ ");
            string text = Console.ReadLine().Trim();
            var parsed  = ParseInput(text);
            string[] args = parsed.Args;
            string? outputPath = parsed.OutputPath; 
            runshell = Dispatch(args, outputPath);
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
    static void Echo(string [] commandArgs, TextWriter output)
    {
        output.WriteLine(string.Join(" ",commandArgs));
        return;
    }


    static void HandleCd(string absPath, TextWriter output)
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
                output.WriteLine("Error: " + ex.Message);
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
                output.WriteLine($"cd: {absPath}: No such file or directory");
            }
        }
        return;
    }

    static void GetType(string [] commandArgs, TextWriter output)
    {
        string typeArg = commandArgs[0];
        if (builtins.Contains(typeArg))
        {
            output.WriteLine(typeArg + " is a shell builtin");
        }
        else
        {
            string? executable = FindExecutable(typeArg);
            if(executable !=null)
            output.WriteLine($"{typeArg} is {executable}");
            else
            output.WriteLine($"{typeArg}: not found");
        }
        return;
    }

    static void Execute(string command, string [] commandArgs, TextWriter output)
    {
        string? executable = FindExecutable(command);

        if (executable is null)
        {
            Console.Error.WriteLine($"{command}: command not found");
            return;
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = executable,
            UseShellExecute = false,
            RedirectStandardOutput = true
        };
        foreach (string argument in commandArgs)
        {
            startInfo.ArgumentList.Add(argument);
        }
        using var process = new Process
        {
            StartInfo = startInfo
        };
        
        process.Start();
        string capturedOut = process.StandardOutput.ReadToEnd();
        output.Write(capturedOut);
        process.WaitForExit();
    }
    static bool Dispatch(string [] args, string? outputPath)
    {   

        string command = args[0]; 
        string [] commandArgs = args[1..];
        TextWriter output = Console.Out;
        StreamWriter? fileWriter = null;

        try
        {
            if (outputPath is not null)
            {
                fileWriter = new StreamWriter(outputPath, append: false);
                output = fileWriter;
            }

            if(command == "exit")
            {
                return false;
            }
            if (command == "echo")
            {     
                Echo(commandArgs, output);
                return true;
            } 
            if (command == "type")
            {
                GetType(commandArgs, output);
                return true;
            }
            if (command == "pwd")
            {
                string workingDirectory = Environment.CurrentDirectory;
                output.WriteLine(workingDirectory); 
                return true;
            }
            if (command == "cd")
            {
                HandleCd(commandArgs[0], output);
                return true;
            }
            else
            {
                Execute(command, commandArgs, output);
                return true;
            }     
        }
        finally
        {
            fileWriter?.Dispose();
        }
    }
    }


    //implementing the cd builtin
    // include the cd builtin in the builtins collection
    //implement the cd method that:
    // handles absolute paths like /usr/local/bin
    // handles relative paths like ./, ../, ./dir
    // the ~ character which represents home directory

