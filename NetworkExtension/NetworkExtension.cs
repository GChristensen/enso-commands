using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.Runtime.InteropServices;

namespace EnsoExtension
{
    public class NetworkExtension : IEnsoExtension
    {
        private static readonly String CONNECT_ERROR = "Connection error";
        private static readonly String HANGUP_ERROR = "Error during hangup";

        private Dictionary<String, CommandDesc> commandActions =
            new Dictionary<String, CommandDesc>()
            {
                {
                    "dial", 
                    new CommandDesc 
                    {
                        action = DialInternet, 
                        desc = "Connect to the Internet using a dialup "
                             + "connection",
                        postfix = "connection name",
                        postfixType = EnsoPostfixType.Bounded,
                        getPostfixes = GetDialupConnections
                    }
                },
                {
                    "hangup", 
                    new CommandDesc 
                    {
                        action = HangupInternet, 
                        desc = "Close an Internet connection",
                        postfix = "connection name",
                        postfixType = EnsoPostfixType.Bounded,
                        getPostfixes = GetDialupConnections
                    }
                }
            };

        private static String[] GetDialupConnections()
        {
            RAS.EntryName[] connections = RAS.GetRasEntries();
            String[] result = new String[connections.Count()];

            for (int i = 0; i < connections.Count(); ++i)
                result[i] = connections[i].Name.ToLower();

            return result;
        }

        private static void DialInternet(String postfix, IEnsoService service)
        {
            try
            {
                if (!"".Equals(postfix))
                    RAS.Dial(postfix);
            }
            catch (Exception)
            {
                service.DisplayMessage(new EnsoMessage(CONNECT_ERROR));
            }
        }

        private static void HangupInternet(String postfix, 
            IEnsoService service)
        {
            try
            {
                if (!"".Equals(postfix))
                {
                    IntPtr hConn = RAS.GetConnection(postfix);
                    if (hConn != IntPtr.Zero)
                        RAS.HangUp(hConn);
                }
            }
            catch (Exception)
            {
                service.DisplayMessage(new EnsoMessage(HANGUP_ERROR));
            }
        }

        private List<EnsoCommand> commands;
        private IEnsoService service;

        public NetworkExtension()
        {
            commands = new List<EnsoCommand>(commandActions.Count);

            foreach (KeyValuePair<String, CommandDesc> cmd in commandActions)
            {
                CommandDesc desc = cmd.Value;
                commands.Add(new EnsoCommand(cmd.Key, desc.postfix, desc.desc,
                    desc.desc, desc.postfixType));
            }
        }

        public void Load(IEnsoService service)
        {
            Debug.Assert(service != null);
            this.service = service;

            String uri = this.GetType().Name + ".rem";

            foreach (EnsoCommand command in commands)
            {
                service.RegisterCommand(this, uri, command);

                CommandDesc desc = commandActions[command.Name];
                if (desc.getPostfixes != null)
                    service.SetCommandValidPostfixes(command, 
                        desc.getPostfixes());
            }
        }

        public void OnCommand(EnsoCommand command, string postfix)
        {
            CommandDesc desc = commandActions[command.Name];
            new Thread(() => desc.action(postfix, service)).Start();    
        }

        public void Unload()
        {
            foreach (EnsoCommand command in commands)
                service.UnregisterCommand(command);
        }
    }
}
