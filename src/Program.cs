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
            if (command.StartsWith("type"))
            {
                if (command[5..] == "echo" || command[5..] == "type" || command[5..] =="exit")
                {
                    Console.Writeline(command[5..] + "is a shell builtin");
                } 
                else{
                    Console.WriteLine(command[5..]+": not found");
                }
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
