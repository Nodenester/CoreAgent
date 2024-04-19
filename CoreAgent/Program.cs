using CoreAgent.Agent;
using CoreAgent.Data;
using System;
using System.Diagnostics;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Write down the agent storage location:");
        string input = Console.ReadLine();
        string folderName = string.IsNullOrEmpty(input) ? "Agent1" : input;

        Agent agent = new Agent(folderName);
        if (agent.Goal == null || agent.Goal == "")
        {
            Console.WriteLine("No existing agent data found. Let's set up your agent.");

            Console.Write("Enter Goal for the Agent: ");
            agent.Goal = Console.ReadLine();

            Console.Write("Enter Extra Info for the Agent: ");
            agent.ExtraInfo = Console.ReadLine();

            agent.Save();
            Console.WriteLine("Agent created and saved successfully.");
        }
        else
        {
            Console.WriteLine("Existing agent loaded successfully.");
        }
        agent.PrintAgentInfo();

        Console.WriteLine("Press enter to start the agent:");
        Console.ReadLine();
        agent.Run();
        Console.ReadLine();
    }
}
