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
            if (command.StartsWith("echo"))
            {
                Console.WriteLine(command[5..]);
            }
            else {
                Console.WriteLine($"{command}: command not found");
            }
        }
    }

}

//REPL - Read Eval Print Loop is interactive loop forming core of the shell
// Read; Displau a prompt and wait for user input 
// Eval: parse and executre the command 
// PrintL display output or error message 
// Loop: return to step 1 and wait for next command 

// Cycle is continues indef until shell process is terminated 
