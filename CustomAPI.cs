using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace gw2_launcher
{
    public static class CustomApi
    {
        private const int CNST_SYSTEM_HANDLE_INFORMATION = 16;
        private const uint STATUS_INFO_LENGTH_MISMATCH = 0xc0000004;

        private static IntPtr processHwnd = IntPtr.Zero;

        public static List<Process> getProcesses(string name)
        {
            List<Process> processes = new List<Process>();
            foreach (Process process in Process.GetProcessesByName(name))
            {
                processes.Add(process);
            }
            return processes;
        }

        public static bool CloseHandle(Process process, string HandleRegexPattern)
        {
            string handleName = "";
            bool ret = false;
            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(HandleRegexPattern);

            processHwnd = Win32Api.OpenProcess(Win32Api.ProcessAccessFlags.All, false, process.Id);
            List<Win32Api.SystemHandleInformation> handles = CustomApi.GetHandles(process);
            foreach (Win32Api.SystemHandleInformation handle in handles)
            {
                handleName = GetHandleName(handle);
                if (handleName != null && regex.IsMatch(handleName))
                {
                    IntPtr lol = IntPtr.Zero;
                    Win32Api.DuplicateHandle(processHwnd, handle.Handle, new IntPtr(handle.Handle), out lol, 0, false, 0x1);
                    ret = true;
                }
            }
            Win32Api.CloseHandle(processHwnd);
            return ret;
        }

        private static string GetHandleName(Win32Api.SystemHandleInformation sYSTEM_HANDLE_INFORMATION)
        {
            IntPtr ipHandle = IntPtr.Zero;
            Win32Api.ObjectBasicInformation objBasic = new Win32Api.ObjectBasicInformation();
            IntPtr ipBasic = IntPtr.Zero;
            Win32Api.ObjectTypeInformation objObjectType = new Win32Api.ObjectTypeInformation();
            IntPtr ipObjectType = IntPtr.Zero;
            Win32Api.ObjectNameInformation objObjectName = new Win32Api.ObjectNameInformation();
            IntPtr ipObjectName = IntPtr.Zero;
            string strObjectTypeName = "";
            string strObjectName = "";
            int nLength = 0;
            //int nReturn = 0;
            IntPtr ipTemp = IntPtr.Zero;

            //OpenProcessForHandle(sYSTEM_HANDLE_INFORMATION.ProcessID);
            if (!Win32Api.DuplicateHandle(processHwnd, sYSTEM_HANDLE_INFORMATION.Handle, Win32Api.GetCurrentProcess(), out ipHandle, 0, false, Win32Api.DuplicateSameAccess)) return null;


            ipBasic = Marshal.AllocHGlobal(Marshal.SizeOf(objBasic));
            Win32Api.NtQueryObject(ipHandle, (int)Win32Api.ObjectInformationClass.ObjectBasicInformation, ipBasic, Marshal.SizeOf(objBasic), ref nLength);
            objBasic = (Win32Api.ObjectBasicInformation)Marshal.PtrToStructure(ipBasic, objBasic.GetType());
            Marshal.FreeHGlobal(ipBasic);


            ipObjectType = Marshal.AllocHGlobal(objBasic.TypeInformationLength);
            nLength = objBasic.TypeInformationLength;
            while ((uint)(/*nReturn = */Win32Api.NtQueryObject(ipHandle, (int)Win32Api.ObjectInformationClass.ObjectTypeInformation, ipObjectType, nLength, ref nLength)) == Win32Api.StatusInfoLengthMismatch)
            {
                Marshal.FreeHGlobal(ipObjectType);
                ipObjectType = Marshal.AllocHGlobal(nLength);
            }

            objObjectType = (Win32Api.ObjectTypeInformation)Marshal.PtrToStructure(ipObjectType, objObjectType.GetType());
            if (Is64Bits())
            {
                ipTemp = new IntPtr(Convert.ToInt64(objObjectType.Name.Buffer.ToString(), 10) >> 32);
            }
            else
            {
                ipTemp = objObjectType.Name.Buffer;
            }

            strObjectTypeName = Marshal.PtrToStringUni(ipTemp, objObjectType.Name.Length >> 1);
            Marshal.FreeHGlobal(ipObjectType);
            //if (strObjectTypeName != "File") return null;
            if (strObjectTypeName != "Mutant")
            {
                Win32Api.CloseHandle(ipHandle);
                return null;
            }

            nLength = objBasic.NameInformationLength;
            ipObjectName = Marshal.AllocHGlobal(nLength);
            while ((uint)(/*nReturn = */Win32Api.NtQueryObject(ipHandle, (int)Win32Api.ObjectInformationClass.ObjectNameInformation, ipObjectName, nLength, ref nLength)) == Win32Api.StatusInfoLengthMismatch)
            {
                Marshal.FreeHGlobal(ipObjectName);
                ipObjectName = Marshal.AllocHGlobal(nLength);
            }
            objObjectName = (Win32Api.ObjectNameInformation)Marshal.PtrToStructure(ipObjectName, objObjectName.GetType());

            if (Is64Bits())
            {
                ipTemp = new IntPtr(Convert.ToInt64(objObjectName.Name.Buffer.ToString(), 10) >> 32);
            }
            else
            {
                ipTemp = objObjectName.Name.Buffer;
            }

            //byte[] baTemp = new byte[nLength];
            //Win32Api.CopyMemory(baTemp, ipTemp, (uint)nLength);

            if (Is64Bits())
            {
                strObjectName = Marshal.PtrToStringUni(new IntPtr(ipTemp.ToInt64()));
            }
            else
            {
                strObjectName = Marshal.PtrToStringUni(new IntPtr(ipTemp.ToInt32()));
            }

            Marshal.FreeHGlobal(ipObjectName);
            Win32Api.CloseHandle(ipHandle);

            return strObjectName;
        }
        
        private static List<Win32Api.SystemHandleInformation> GetHandles(Process process)
        {
            //uint nStatus;
            int nHandleInfoSize = 0x10000;
            IntPtr ipHandlePointer = Marshal.AllocHGlobal(nHandleInfoSize);
            int nLength = 0;
            IntPtr ipHandle = IntPtr.Zero;

            while (/*(nStatus = */Win32Api.NtQuerySystemInformation(CNST_SYSTEM_HANDLE_INFORMATION, ipHandlePointer, nHandleInfoSize, ref nLength/*)*/) == STATUS_INFO_LENGTH_MISMATCH)
            {
                nHandleInfoSize = nLength;
                Marshal.FreeHGlobal(ipHandlePointer);
                ipHandlePointer = Marshal.AllocHGlobal(nLength);
            }

            byte[] baTemp = new byte[nLength];
            Win32Api.CopyMemory(baTemp, ipHandlePointer, (uint)nLength);

            long lHandleCount = 0;
            if (Is64Bits())
            {
                lHandleCount = Marshal.ReadInt64(ipHandlePointer);
                ipHandle = new IntPtr(ipHandlePointer.ToInt64() + 8);
            }
            else
            {
                lHandleCount = Marshal.ReadInt32(ipHandlePointer);
                ipHandle = new IntPtr(ipHandlePointer.ToInt32() + 4);
            }

            Win32Api.SystemHandleInformation shHandle;
            List<Win32Api.SystemHandleInformation> lstHandles = new List<Win32Api.SystemHandleInformation>();

            for (long lIndex = 0; lIndex < lHandleCount; lIndex++)
            {
                shHandle = new Win32Api.SystemHandleInformation();
                if (Is64Bits())
                {
                    shHandle = (Win32Api.SystemHandleInformation)Marshal.PtrToStructure(ipHandle, shHandle.GetType());
                    ipHandle = new IntPtr(ipHandle.ToInt64() + Marshal.SizeOf(shHandle) + 8);
                }
                else
                {
                    ipHandle = new IntPtr(ipHandle.ToInt64() + Marshal.SizeOf(shHandle));
                    shHandle = (Win32Api.SystemHandleInformation)Marshal.PtrToStructure(ipHandle, shHandle.GetType());
                }
                if (shHandle.ProcessID != process.Id)
                    continue;
                lstHandles.Add(shHandle);
            }
            return lstHandles;

        }

        private static bool Is64Bits()
        {
            return Marshal.SizeOf(typeof(IntPtr)) == 8 ? true : false;
        }
    }
}
