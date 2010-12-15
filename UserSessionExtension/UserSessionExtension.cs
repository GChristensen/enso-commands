using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace EnsoExtension
{
    [Flags]
    public enum ExitWindows : uint
    {
        LogOff = 0x00,
        ShutDown = 0x01,
        Reboot = 0x02
    }

    [Flags]
    enum ShutdownReason : uint
    {
        FlagPlanned = 0x80000000
    }

    public class UserSessionExtension : IEnsoExtension
    {
        private readonly String COMMAND_DESC = "Power management command";

        private IEnsoService service;

        [DllImport("PowrProf")]
        extern static bool SetSuspendState(bool hibernate, bool force, 
            bool disableWakeup);

        [StructLayout(LayoutKind.Sequential, Pack=1)]
        internal struct TokPriv1Luid
        {
            public int Count;
            public long Luid;
            public int Attr;
        }

        [DllImport("kernel32", ExactSpelling=true)]
        internal static extern IntPtr GetCurrentProcess();

        [DllImport("advapi32", ExactSpelling=true, SetLastError=true)]
        internal static extern bool OpenProcessToken(IntPtr h, int acc, 
            ref IntPtr phtok);

        [DllImport("advapi32", SetLastError=true)]
        internal static extern bool LookupPrivilegeValue(string host, 
            string name, ref long pluid);

        [DllImport("advapi32", ExactSpelling=true, SetLastError=true)]
        internal static extern bool AdjustTokenPrivileges(IntPtr htok, 
            bool disall, ref TokPriv1Luid newst, int len, IntPtr prev, 
            IntPtr relen);

        [DllImport("user32")]
        public static extern int ExitWindowsEx(uint uFlags, uint dwReason);

        internal const int SE_PRIVILEGE_ENABLED = 0x00000002;
        internal const int TOKEN_QUERY = 0x00000008;
        internal const int TOKEN_ADJUST_PRIVILEGES = 0x00000020;
        internal const string SE_SHUTDOWN_NAME = "SeShutdownPrivilege";

        private static void PerformExit(ExitWindows action)
        {
            TokPriv1Luid tp;
            IntPtr hproc = GetCurrentProcess();
            IntPtr htok = IntPtr.Zero;

            OpenProcessToken(hproc, TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, 
                ref htok);

            tp.Count = 1;
            tp.Luid = 0;
            tp.Attr = SE_PRIVILEGE_ENABLED;

            LookupPrivilegeValue(null, SE_SHUTDOWN_NAME, ref tp.Luid);
            AdjustTokenPrivileges(htok, false, ref tp, 0, IntPtr.Zero, 
                IntPtr.Zero);

            ExitWindowsEx((uint)action, (uint)ShutdownReason.FlagPlanned);
        }

        private Dictionary<String, Action> commandActions =
            new Dictionary<String, Action>()
            {
                {"log off", () => PerformExit(ExitWindows.LogOff)},
                {"shut down", () => PerformExit(ExitWindows.ShutDown)},
                {"reboot", () => PerformExit(ExitWindows.Reboot)},
                {"suspend", () => SetSuspendState(false, true, true)},
                {"hibernate", () => SetSuspendState(true, true, true)}
            };

        private List<EnsoCommand> commands;

        public UserSessionExtension()
        {
            commands = new List<EnsoCommand>(commandActions.Count);

            foreach (String cmdName in commandActions.Keys)
            {
                commands.Add(new EnsoCommand(cmdName, null, COMMAND_DESC,
                    COMMAND_DESC, EnsoPostfixType.None));
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
            }
        }

        public void OnCommand(EnsoCommand command, string postfix)
        {
            Action action = commandActions[command.Name];

            if (action != null)
                new Thread(new ThreadStart(action)).Start();
        }

        public void Unload()
        {
            foreach (EnsoCommand command in commands)
                service.UnregisterCommand(command);
        }
    }
}
