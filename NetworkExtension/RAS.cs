using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.ComponentModel;

namespace EnsoExtension
{
    public static class RAS
    {
        internal static class RAW
        {
            [DllImport("rasapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
            public static extern int RasEnumEntries(
                IntPtr reserved,
                IntPtr lpszPhonebook,
                [In, Out] RASENTRYNAME[] lprasentryname,
                ref int lpcb,
                ref int lpcEntries);

            [DllImport("Rasdlg.dll", SetLastError = true, CharSet = CharSet.Auto)]
            public static extern bool RasDialDlg(
                IntPtr phoneBook,
                string entryName,
                IntPtr phoneNumber,
                ref RASDIALDLG info);

            [DllImport("rasapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
            public static extern int RasEnumConnections(
                [In, Out] RASCONN[] rasconn,
                [In, Out] ref int cb,
                [Out] out int connections);

            [DllImport("rasapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
            public static extern int RasHangUp(IntPtr hrasconn);

            public const int RAS_MaxEntryName = 256;
            const int RAS_MaxDeviceType = 16;
            const int RAS_MaxDeviceName = 128;
            public const int MAX_PATH = 260;

            public const int ERROR_SUCCESS = 0;
            public const int ERROR_BUFFER_TOO_SMALL = 603;

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
            public struct RASENTRYNAME
            {
                public int dwSize;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = RAS_MaxEntryName + 1)]
                public string szEntryName;
                public int dwFlags;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_PATH + 1)]
                public string szPhonebook;
            }

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
            public struct RASDIALDLG
            {
                public int dwSize;
                public IntPtr hwndOwner;
                public int dwFlags;
                public int xDlg;
                public int yDlg;
                public int dwSubEntry;
                public int dwError;
                public IntPtr reserved;
                public IntPtr reserved2;
            }

            [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Auto)]
            public struct RASCONN
            {
                public int dwSize;
                public IntPtr hrasconn;

                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = RAS_MaxEntryName)]
                public string szEntryName;

                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = RAS_MaxDeviceType)]
                public string szDeviceType;

                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = RAS_MaxDeviceName)]
                public string szDeviceName;

                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_PATH)]
                public string szPhonebook;

                public int dwSubEntry;
                public Guid guidEntry;
                public int dwFlags;
                public Guid luid;
            }
        }

        public class EntryName
        {
            public EntryName(string name)
            {
                Name = name;
            }
            public string Name;
        }

        public static EntryName[] GetRasEntries()
        {
            int cb = Marshal.SizeOf(typeof(RAW.RASENTRYNAME));
            int entries = 0;
            RAW.RASENTRYNAME[] entryNames = new RAW.RASENTRYNAME[1];
            entryNames[0].dwSize = Marshal.SizeOf(typeof(RAW.RASENTRYNAME));

            int nRet = RAW.RasEnumEntries(IntPtr.Zero, IntPtr.Zero, entryNames, ref cb, ref entries);
            if (nRet != RAW.ERROR_SUCCESS && nRet != RAW.ERROR_BUFFER_TOO_SMALL)
                throw new Win32Exception((int)nRet);

            if (entries == 0)
                return new EntryName[0];

            entryNames = new RAW.RASENTRYNAME[entries];
            for (int i = 0; i < entryNames.Length; i++)
            {
                entryNames[i].dwSize = Marshal.SizeOf(typeof(RAW.RASENTRYNAME));
            }

            nRet = RAW.RasEnumEntries(IntPtr.Zero, IntPtr.Zero, entryNames, ref cb, ref entries);
            if (nRet != RAW.ERROR_SUCCESS)
                throw new Win32Exception((int)nRet);

            return Array.ConvertAll<RAW.RASENTRYNAME, EntryName>(entryNames,
                delegate(RAW.RASENTRYNAME entry)
                {
                    return new EntryName(entry.szEntryName);
                }
            );
        }

        public static bool Dial(string entryName)
        {
            RAW.RASDIALDLG info = new RAW.RASDIALDLG();
            info.dwSize = Marshal.SizeOf(info);

            bool ret = RAW.RasDialDlg(IntPtr.Zero, entryName, IntPtr.Zero, ref info);
            if (ret == false && info.dwError != RAW.ERROR_SUCCESS)
            {
                throw new Win32Exception(info.dwError);
            }
            return ret;
        }

        public static IntPtr GetConnection(string entryName)
        {
            RAW.RASCONN[] connections = new RAW.RASCONN[1];
            connections[0].dwSize = Marshal.SizeOf(typeof(RAW.RASCONN));

            int connectionsCount = 0;
            int cb = Marshal.SizeOf(typeof(RAW.RASCONN));
            int nRet = RAW.RasEnumConnections(connections, ref cb, out connectionsCount);
            if (nRet != RAW.ERROR_SUCCESS && nRet != RAW.ERROR_BUFFER_TOO_SMALL)
                throw new Win32Exception(nRet);
            if (connectionsCount == 0)
                return IntPtr.Zero;

            connections = new RAW.RASCONN[connectionsCount];
            for (int i = 0; i < connections.Length; i++)
            {
                connections[i].dwSize = Marshal.SizeOf(typeof(RAW.RASCONN));
            }

            nRet = RAW.RasEnumConnections(connections, ref cb, out connectionsCount);
            if (nRet != RAW.ERROR_SUCCESS)
                throw new Win32Exception((int)nRet);

            int index = Array.FindIndex<RAW.RASCONN>(connections,
                delegate(RAW.RASCONN cnn)
                {
                    return (cnn.szEntryName.ToLower() == entryName.ToLower());
                }
            );

            if (index == -1)
                return IntPtr.Zero;

            return connections[index].hrasconn;
        }

        public static void HangUp(IntPtr hrasconn)
        {
            int err = RAW.RasHangUp(hrasconn);
            if (err != RAW.ERROR_SUCCESS)
                throw new Win32Exception(err);
        }
    }
}
