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

        enum outputPathTarget
    {
        None,
        StandardOutput,
        StandardError
    }

    static (string[] Args, string? OutputPath, string? outputPathPath) ParseInput(string userInput)
    {
        List<string> args = new List<string>();
        ParseMode mode = ParseMode.Unquoted;
        string curr = "";
        string? outputPath = null;
        string? errorPath = null;
        bool argumentStarted = false;
        bool escapeNextCharacter = false;
        outputPathTarget pendingoutputPath = outputPathTarget.None;
    

        

        void FinishArgument()
        {
           if (!argumentStarted)
            {
                return;
            }
            if (pendingoutputPath == outputPathTarget.StandardOutput)
            {
                outputPath = curr;
            }
            else if (pendingoutputPath == outputPathTarget.StandardError)
            {
                errorPath = curr;
            }
            else
            {
                args.Add(curr);
            }
            curr = "";
            argumentStarted = false;
            pendingoutputPath = outputPathTarget.None;
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
                        if (curr == "1")
                        {
                            curr = "";
                            argumentStarted = false;
                        }
                        if (curr == "2")
                        {
                            curr ="";
                            argumentStarted = false;
                            pendingoutputPath = outputPathTarget.StandardError;
                        }
                        else
                        {
                        FinishArgument();
                        pendingoutputPath = outputPathTarget.StandardOutput;
                        }
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
        return (args.ToArray(), outputPath, errorPath);
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
            string? outputPathPath = parsed.outputPathPath;
            runshell = Dispatch(args, outputPath, outputPathPath);
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
    static void Echo(string [] commandArgs, TextWriter output, TextWriter error)
    {
        output.WriteLine(string.Join(" ",commandArgs));
        return;
    }


    static void HandleCd(string absPath, TextWriter output, TextWriter error)
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
                            ?? throw new Exception("Unable to determine HOME directory");
                }
                Environment.CurrentDirectory = homeDir;

            }
            catch (Exception ex)
            {
                error.WriteLine("Error: " + ex.Message);
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
                error.WriteLine($"cd: {absPath}: No such file or directory");
            }
        }
        return;
    }

    static void GetType(string [] commandArgs, TextWriter output, TextWriter error)
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
            error.WriteLine($"{typeArg}: not found");
        }
        return;
    }

    static void Execute(string command, string [] commandArgs, TextWriter output, TextWriter error)
    {
        string? executable = FindExecutable(command);

        if (executable is null)
        {
            error.WriteLine($"{command}: command not found");
            return;
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = command,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
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

        Task<string> outputTask =
            process.StandardOutput.ReadToEndAsync();

        Task<string> errorTask = 
            process.StandardError.ReadToEndAsync();
        
        process.WaitForExit();

        string capturedOut = outputTask.GetAwaiter().GetResult();
        string capturedError = errorTask.GetAwaiter().GetResult();

        output.Write(capturedOut);
        error.Write(capturedError);
    }
    static bool Dispatch(string [] args, string? outputPath, string? errorPath)
    {   

        string command = args[0]; 
        string [] commandArgs = args[1..];
        TextWriter output = Console.Out;
        TextWriter error = Console.Error;
        StreamWriter? fileWriter = null;
        StreamWriter? errorWriter = null;

        try
        {
            if (outputPath is not null )
            {
                fileWriter = new StreamWriter(outputPath, append: false);
                output = fileWriter;
                
            }
            if (errorPath is not null)
            {
                errorWriter = new StreamWriter(errorPath, append: false);
                error = errorWriter;
            }
            if(command == "exit")
            {
                return false;
            }
            if (command == "echo")
            {     
                Echo(commandArgs, output, error) ;
                return true;
            } 
            if (command == "type")
            {
                GetType(commandArgs, output, error);
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
                HandleCd(commandArgs[0], output, error);
                return true;
            }
            else
            {
                Execute(command, commandArgs, output, error);
                return true;
            }     
        }
        finally
        {
            fileWriter?.Dispose();
            errorWriter?.Dispose();
        }
    }
    }


    //implementing the cd builtin
    // include the cd builtin in the builtins collection
    //implement the cd method that:
    // handles absolute paths like /usr/local/bin
    // handles relative paths like ./, ../, ./dir
    // the ~ character which represents home directory

