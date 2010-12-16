using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace EnsoExtension
{
    internal class FreeFormatMessage : EnsoMessage
    {
        private String text;

        public FreeFormatMessage(String text) : base("")
        {
            this.text = text;
        }

        public override String ToString()
        {
            return text;
        }
    }

    public class SystemExtension : IEnsoExtension
    {
        private Dictionary<String, CommandDesc> commandActions =
            new Dictionary<String, CommandDesc>()
            {
                {
                    "kill", 
                    new CommandDesc 
                    {
                        action = KillProcess, 
                        desc = "Kill process using its executable name",
                        postfix = "process name or id",
                        postfixType = EnsoPostfixType.Arbitrary
                    }
                },
                {
                    "memory", 
                    new CommandDesc 
                    {
                        action = QueryMemoryUsage, 
                        desc = "Query process memory usage (working "
                                + "set/virtual)",
                        postfix = "process name or id",
                        postfixType = EnsoPostfixType.Arbitrary
                    }
                },
                {
                    "process list", 
                    new CommandDesc 
                    {
                        action = ListProcesses, 
                        desc = "List active processes",
                        postfix = "",
                        postfixType = EnsoPostfixType.None
                    }
                }

            };

        private List<EnsoCommand> commands;
        private IEnsoService service;

        public SystemExtension()
        {
            commands = new List<EnsoCommand>(commandActions.Count);

            foreach (KeyValuePair<String, CommandDesc> cmd  in commandActions)
            {
                CommandDesc desc = cmd.Value;
                commands.Add(new EnsoCommand(cmd.Key, desc.postfix, desc.desc,
                    desc.desc, desc.postfixType));
            }
        }

        private static Process[] GetProcesses(String postfix)
        {
            if (Regex.IsMatch(postfix, @"\d+"))
            {
                int pid = Convert.ToInt32(postfix);
                return new Process[] { Process.GetProcessById(pid) };
            }
            else
                return Process.GetProcessesByName(postfix);
        }

        private static void KillProcess(String postfix, IEnsoService service)
        {
            Process [] processes = GetProcesses(postfix);

            if (processes.Count() > 0)
                foreach (Process process in processes)
                    try
                    {
                        process.Kill();
                    }
                    catch (Exception) { }
            else
                service.DisplayMessage(new EnsoMessage("Process " + postfix 
                    + " not found"));
        }

        private static String GetProcessMemoryUsage(Process p, bool printPID)
        {
            String format = printPID
                     ? "{0} ({5:d}): {1:d}{2}/{3:d}{4}"
                     : "{0}: {1:d}{2}/{3:d}{4}";

            String wsUnits = "Mb";
            long workingSet = p.WorkingSet64 / 1024 / 1024;

            if (workingSet == 0)
            {
                wsUnits = "Kb";
                workingSet = p.WorkingSet64 / 1024;
            }

            String vmUnits = "Mb";
            long virtualMemory = p.VirtualMemorySize64 / 1024 / 1024;

            if (virtualMemory == 0)
            {
                vmUnits = "Kb";
                virtualMemory = p.VirtualMemorySize64 / 1024;
            }

            return String.Format(format, p.ProcessName, workingSet, wsUnits,
                virtualMemory, vmUnits, p.Id);
        }

        private static void QueryMemoryUsage(String postfix, 
            IEnsoService service)
        {
            List<Process> processes = new List<Process>(GetProcesses(postfix));
            int processCount = processes.Count();

            if (processCount > 0)
            {
                String message = "";

                if (processCount == 1)
                {
                    message = "<p>"
                            + GetProcessMemoryUsage(processes[0], true) 
                            + "</p>";                   
                }
                else if (processCount <= 5)
                {
                    processes.Sort((Process l, Process r) =>
                        -l.WorkingSet64.CompareTo(r.WorkingSet64));

                    foreach (Process process in processes)
                        message += "<p>"
                                + GetProcessMemoryUsage(process, true)
                                + "</p>";
                }
                else
                {
                    processes.Sort((Process l, Process r) =>
                        -l.WorkingSet64.CompareTo(r.WorkingSet64));

                    foreach (Process process in processes)
                        message += "<caption>"
                                + GetProcessMemoryUsage(process, true)
                                + "</caption>";
                }

                service.DisplayMessage(new FreeFormatMessage(message));
            }
            else
                service.DisplayMessage(new EnsoMessage("Process " + postfix
                    + " not found"));
        }

        private static void ListProcesses(String postfix,
            IEnsoService service)
        {
            Process[] processes = Process.GetProcesses();
            SortedSet<String> uniqueNames = new SortedSet<String>();

            foreach (Process p in processes)
                uniqueNames.Add(p.ProcessName);

            char initial = '\0';
            String message = "";

            foreach (String s in uniqueNames)
            {
                String name = s.ToLower();

                if (name.Length > 0 && !name[0].Equals(initial))
                {
                    initial = name[0];
                    name = char.ToUpper(name[0]) + name.Substring(1);
                }

                message += name + " ";                
            }

            message = "<p>" + message.Trim() + "</p>";

            service.DisplayMessage(new FreeFormatMessage(message));
        }


        public void Load(IEnsoService service)
        {
            Debug.Assert(service != null);
            this.service = service;

            String uri = this.GetType().Name + ".rem";

            foreach (EnsoCommand command in commands)
            {
                service.RegisterCommand(this, uri, command);
            }
        }

        public void OnCommand(EnsoCommand command, string postfix)
        {
            Action<String, IEnsoService> action = 
                commandActions[command.Name].action;

            if (action != null)
                new Thread(() => action(postfix, service)).Start();
        }

        public void Unload()
        {
            foreach (EnsoCommand command in commands)
                service.UnregisterCommand(command);
        }
    }
}
