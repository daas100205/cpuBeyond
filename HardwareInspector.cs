namespace CPUZClone
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Management;
    using System.Net;
    using System.Net.NetworkInformation;
    using System.Runtime.InteropServices;
    using System.Text;

    public class RamSlotInfo
    {
        public string SlotLabel { get; set; }
        public string Capacity { get; set; }
        public string Speed { get; set; }
        public string FormFactor { get; set; }
        public string Manufacturer { get; set; }
        public string PartNumber { get; set; }
    }

    public class DriveInfoModel
    {
        public string Model { get; set; }
        public string InterfaceType { get; set; }
        public string MediaType { get; set; }
        public string Size { get; set; }
        public List<string> Partitions { get; set; }

        public DriveInfoModel()
        {
            Partitions = new List<string>();
        }
    }

    public class GpuInfoModel
    {
        public string Name { get; set; }
        public string Vram { get; set; }
        public string DriverVersion { get; set; }
        public string DriverDate { get; set; }
        public string VideoProcessor { get; set; }
        public string Resolution { get; set; }
    }

    public class HardwareInfo
    {
        // CPU
        public string CpuName { get; set; }
        public string CpuManufacturer { get; set; }
        public string CpuSocket { get; set; }
        public string CpuArchitecture { get; set; }
        public uint CpuCores { get; set; }
        public uint CpuThreads { get; set; }
        public uint CpuMaxClockMHz { get; set; }
        public uint CpuCurrentClockMHz { get; set; }
        public string CpuL2Cache { get; set; }
        public string CpuL3Cache { get; set; }
        public bool CpuVirtualization { get; set; }
        public float CpuLoadPercentage { get; set; }

        // Motherboard & BIOS
        public string BoardManufacturer { get; set; }
        public string BoardProduct { get; set; }
        public string BoardVersion { get; set; }
        public string BoardSerialNumber { get; set; }
        public string SystemModel { get; set; }
        public string SystemManufacturer { get; set; }
        public string BiosVendor { get; set; }
        public string BiosVersion { get; set; }
        public string BiosReleaseDate { get; set; }

        // Memory
        public string TotalRamGB { get; set; }
        public string UsedRamGB { get; set; }
        public string FreeRamGB { get; set; }
        public float RamUsagePercent { get; set; }
        public List<RamSlotInfo> RamSlots { get; set; }

        // GPU
        public List<GpuInfoModel> Gpus { get; set; }

        // Storage
        public List<DriveInfoModel> Drives { get; set; }

        // Battery
        public bool HasBattery { get; set; }
        public string BatteryStatus { get; set; }
        public string BatteryPercentage { get; set; }
        public string PowerLineStatus { get; set; }
        public string EstimatedTimeRemaining { get; set; }

        // OS & Network
        public string OsName { get; set; }
        public string OsVersion { get; set; }
        public string OsBuild { get; set; }
        public string OsArchitecture { get; set; }
        public string ComputerName { get; set; }
        public string CustomMachineName { get; set; }
        public string SystemUptime { get; set; }
        public List<string> NetworkAdapters { get; set; }


        public HardwareInfo()
        {
            RamSlots = new List<RamSlotInfo>();
            Gpus = new List<GpuInfoModel>();
            Drives = new List<DriveInfoModel>();
            NetworkAdapters = new List<string>();
        }
    }

    public static class HardwareInspector
    {
        public static HardwareInfo GetSystemInfo()
        {
            HardwareInfo info = new HardwareInfo();

            FetchCpuInfo(info);
            FetchMotherboardInfo(info);
            FetchMemoryInfo(info);
            FetchGpuInfo(info);
            FetchStorageInfo(info);
            FetchBatteryInfo(info);
            FetchOsAndNetworkInfo(info);

            return info;
        }

        private static void FetchCpuInfo(HardwareInfo info)
        {
            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        info.CpuName = GetWmiProp(obj, "Name");
                        info.CpuManufacturer = GetWmiProp(obj, "Manufacturer");
                        info.CpuSocket = GetWmiProp(obj, "SocketDesignation");
                        info.CpuCores = SafeConvertUint(obj["NumberOfCores"]);
                        info.CpuThreads = SafeConvertUint(obj["NumberOfLogicalProcessors"]);
                        info.CpuMaxClockMHz = SafeConvertUint(obj["MaxClockSpeed"]);
                        info.CpuCurrentClockMHz = SafeConvertUint(obj["CurrentClockSpeed"]);
                        info.CpuL2Cache = FormatBytesToSize(SafeConvertUint(obj["L2CacheSize"]) * 1024);
                        info.CpuL3Cache = FormatBytesToSize(SafeConvertUint(obj["L3CacheSize"]) * 1024);
                        
                        ushort arch = (ushort)SafeConvertUint(obj["Architecture"]);
                        info.CpuArchitecture = GetArchitectureString(arch);
                        
                        info.CpuVirtualization = (bool)(obj["VirtualizationFirmwareEnabled"] ?? false);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                info.CpuName = "Error fetching CPU: " + ex.Message;
            }

            info.CpuLoadPercentage = GetCpuLoad();
        }

        public static float GetCpuLoad()
        {
            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT LoadPercentage FROM Win32_Processor"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        return SafeConvertFloat(obj["LoadPercentage"]);
                    }
                }
            }
            catch { }
            return 0f;
        }

        private static void FetchMotherboardInfo(HardwareInfo info)
        {
            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_BaseBoard"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        info.BoardManufacturer = GetWmiProp(obj, "Manufacturer");
                        info.BoardProduct = GetWmiProp(obj, "Product");
                        info.BoardVersion = GetWmiProp(obj, "Version");
                        info.BoardSerialNumber = GetWmiProp(obj, "SerialNumber");
                        break;
                    }
                }

                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_ComputerSystem"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        info.SystemManufacturer = GetWmiProp(obj, "Manufacturer");
                        info.SystemModel = GetWmiProp(obj, "Model");
                        break;
                    }
                }

                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_BIOS"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        info.BiosVendor = GetWmiProp(obj, "Manufacturer");
                        info.BiosVersion = GetWmiProp(obj, "SMBIOSBIOSVersion");
                        if (string.IsNullOrEmpty(info.BiosVersion))
                            info.BiosVersion = GetWmiProp(obj, "Version");
                        
                        string rawDate = GetWmiProp(obj, "ReleaseDate");
                        info.BiosReleaseDate = FormatWmiDate(rawDate);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                info.BoardProduct = "Error: " + ex.Message;
            }
        }

        private static void FetchMemoryInfo(HardwareInfo info)
        {
            try
            {
                ulong totalBytes = 0;
                ulong freeBytes = 0;

                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        totalBytes = (ulong)SafeConvertUint64(obj["TotalVisibleMemorySize"]) * 1024;
                        freeBytes = (ulong)SafeConvertUint64(obj["FreePhysicalMemory"]) * 1024;
                        break;
                    }
                }

                ulong usedBytes = totalBytes > freeBytes ? totalBytes - freeBytes : 0;
                info.TotalRamGB = string.Format("{0:F2} GB", totalBytes / (1024.0 * 1024.0 * 1024.0));
                info.UsedRamGB = string.Format("{0:F2} GB", usedBytes / (1024.0 * 1024.0 * 1024.0));
                info.FreeRamGB = string.Format("{0:F2} GB", freeBytes / (1024.0 * 1024.0 * 1024.0));
                info.RamUsagePercent = totalBytes > 0 ? (float)((double)usedBytes / totalBytes * 100.0) : 0f;

                // Physical RAM slots
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PhysicalMemory"))
                {
                    int slotIndex = 1;
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        RamSlotInfo slot = new RamSlotInfo();
                        string bank = GetWmiProp(obj, "BankLabel");
                        string loc = GetWmiProp(obj, "DeviceLocator");
                        slot.SlotLabel = !string.IsNullOrEmpty(loc) ? loc : (!string.IsNullOrEmpty(bank) ? bank : "Slot " + slotIndex);
                        
                        ulong cap = SafeConvertUint64(obj["Capacity"]);
                        slot.Capacity = FormatBytesToSize(cap);

                        uint speed = SafeConvertUint(obj["Speed"]);
                        slot.Speed = speed > 0 ? speed + " MHz" : "N/A";

                        ushort formFactorCode = (ushort)SafeConvertUint(obj["FormFactor"]);
                        slot.FormFactor = GetFormFactorString(formFactorCode);

                        slot.Manufacturer = GetWmiProp(obj, "Manufacturer");
                        slot.PartNumber = GetWmiProp(obj, "PartNumber").Trim();

                        info.RamSlots.Add(slot);
                        slotIndex++;
                    }
                }
            }
            catch (Exception ex)
            {
                info.TotalRamGB = "Error: " + ex.Message;
            }
        }

        private static void FetchGpuInfo(HardwareInfo info)
        {
            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        GpuInfoModel gpu = new GpuInfoModel();
                        gpu.Name = GetWmiProp(obj, "Name");
                        
                        ulong vramBytes = SafeConvertUint64(obj["AdapterRAM"]);
                        gpu.Vram = vramBytes > 0 ? FormatBytesToSize(vramBytes) : "Dynamic / N/A";

                        gpu.DriverVersion = GetWmiProp(obj, "DriverVersion");
                        gpu.DriverDate = FormatWmiDate(GetWmiProp(obj, "DriverDate"));
                        gpu.VideoProcessor = GetWmiProp(obj, "VideoProcessor");

                        uint hRes = SafeConvertUint(obj["CurrentHorizontalResolution"]);
                        uint vRes = SafeConvertUint(obj["CurrentVerticalResolution"]);
                        uint refresh = SafeConvertUint(obj["CurrentRefreshRate"]);

                        if (hRes > 0 && vRes > 0)
                        {
                            gpu.Resolution = string.Format("{0} x {1} @ {2}Hz", hRes, vRes, refresh);
                        }
                        else
                        {
                            gpu.Resolution = "N/A";
                        }

                        info.Gpus.Add(gpu);
                    }
                }
            }
            catch (Exception ex)
            {
                GpuInfoModel gpu = new GpuInfoModel();
                gpu.Name = "Error fetching GPU: " + ex.Message;
                info.Gpus.Add(gpu);
            }
        }

        private static void FetchStorageInfo(HardwareInfo info)
        {
            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        DriveInfoModel drive = new DriveInfoModel();
                        drive.Model = GetWmiProp(obj, "Model");
                        drive.InterfaceType = GetWmiProp(obj, "InterfaceType");

                        ulong diskSize = SafeConvertUint64(obj["Size"]);
                        drive.Size = FormatBytesToSize(diskSize);

                        drive.MediaType = GetWmiProp(obj, "MediaType");
                        if (string.IsNullOrEmpty(drive.MediaType))
                            drive.MediaType = "Physical Disk";

                        info.Drives.Add(drive);
                    }
                }

                // Append logical volume stats
                foreach (DriveInfo drive in DriveInfo.GetDrives())
                {
                    if (drive.IsReady)
                    {
                        string volInfo = string.Format("{0} ({1}) - {2} Free / {3} Total [{4}]",
                            drive.Name,
                            string.IsNullOrEmpty(drive.VolumeLabel) ? "Local Disk" : drive.VolumeLabel,
                            FormatBytesToSize((ulong)drive.AvailableFreeSpace),
                            FormatBytesToSize((ulong)drive.TotalSize),
                            drive.DriveFormat);

                        if (info.Drives.Count > 0)
                        {
                            info.Drives[0].Partitions.Add(volInfo);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DriveInfoModel drive = new DriveInfoModel();
                drive.Model = "Error: " + ex.Message;
                info.Drives.Add(drive);
            }
        }

        private static void FetchBatteryInfo(HardwareInfo info)
        {
            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Battery"))
                {
                    ManagementObjectCollection collection = searcher.Get();
                    if (collection.Count > 0)
                    {
                        info.HasBattery = true;
                        foreach (ManagementObject obj in collection)
                        {
                            ushort status = (ushort)SafeConvertUint(obj["BatteryStatus"]);
                            info.BatteryStatus = GetBatteryStatusString(status);
                            info.BatteryPercentage = GetWmiProp(obj, "EstimatedChargeRemaining") + "%";
                            
                            uint secs = SafeConvertUint(obj["EstimatedRunTime"]);
                            info.EstimatedTimeRemaining = secs > 0 && secs < 71582788 ? (secs / 60) + " mins" : "Calculating / AC Power";
                            break;
                        }
                    }
                    else
                    {
                        info.HasBattery = false;
                        info.BatteryStatus = "No Battery Installed (Desktop PC)";
                    }
                }
            }
            catch
            {
                info.HasBattery = false;
                info.BatteryStatus = "No Battery / Desktop PC";
            }
        }

        private static void FetchOsAndNetworkInfo(HardwareInfo info)
        {
            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        info.OsName = GetWmiProp(obj, "Caption");
                        info.OsVersion = GetWmiProp(obj, "Version");
                        info.OsBuild = GetWmiProp(obj, "BuildNumber");
                        info.OsArchitecture = GetWmiProp(obj, "OSArchitecture");
                        info.ComputerName = GetWmiProp(obj, "CSName");
                        break;
                    }
                }

                TimeSpan uptime = TimeSpan.FromMilliseconds(Environment.TickCount);
                info.SystemUptime = string.Format("{0}d {1}h {2}m", uptime.Days, uptime.Hours, uptime.Minutes);

                foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (ni.OperationalStatus == OperationalStatus.Up && ni.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                    {
                        IPInterfaceProperties ipProps = ni.GetIPProperties();
                        string ipv4Str = "";
                        foreach (UnicastIPAddressInformation ip in ipProps.UnicastAddresses)
                        {
                            if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                            {
                                ipv4Str = ip.Address.ToString();
                                break;
                            }
                        }

                        string mac = ni.GetPhysicalAddress().ToString();
                        if (mac.Length == 12)
                        {
                            mac = string.Join("-", EnumerableChunk(mac, 2));
                        }

                        info.NetworkAdapters.Add(string.Format("{0} | IP: {1} | MAC: {2}", ni.Name, string.IsNullOrEmpty(ipv4Str) ? "N/A" : ipv4Str, mac));
                    }
                }
            }
            catch (Exception ex)
            {
                info.OsName = "Error: " + ex.Message;
            }
        }

        // Helpers
        private static string GetWmiProp(ManagementObject obj, string propName)
        {
            try
            {
                object val = obj[propName];
                return val != null ? val.ToString().Trim() : "N/A";
            }
            catch
            {
                return "N/A";
            }
        }

        private static uint SafeConvertUint(object obj)
        {
            if (obj == null) return 0;
            uint res;
            if (uint.TryParse(obj.ToString(), out res)) return res;
            return 0;
        }

        private static ulong SafeConvertUint64(object obj)
        {
            if (obj == null) return 0;
            ulong res;
            if (ulong.TryParse(obj.ToString(), out res)) return res;
            return 0;
        }

        private static float SafeConvertFloat(object obj)
        {
            if (obj == null) return 0f;
            float res;
            if (float.TryParse(obj.ToString(), out res)) return res;
            return 0f;
        }

        private static string FormatBytesToSize(ulong bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return string.Format("{0:0.##} {1}", len, sizes[order]);
        }

        private static string FormatWmiDate(string wmiDate)
        {
            if (string.IsNullOrEmpty(wmiDate) || wmiDate.Length < 8) return wmiDate;
            try
            {
                string year = wmiDate.Substring(0, 4);
                string month = wmiDate.Substring(4, 2);
                string day = wmiDate.Substring(6, 2);
                return string.Format("{0}-{1}-{2}", year, month, day);
            }
            catch
            {
                return wmiDate;
            }
        }

        private static string GetArchitectureString(ushort arch)
        {
            switch (arch)
            {
                case 0: return "x86 (32-bit)";
                case 5: return "ARM";
                case 6: return "Itanium";
                case 9: return "x64 (64-bit)";
                case 12: return "ARM64";
                default: return "Unknown (" + arch + ")";
            }
        }

        private static string GetFormFactorString(ushort code)
        {
            switch (code)
            {
                case 7: return "SIMM";
                case 8: return "DIMM";
                case 12: return "SODIMM";
                case 13: return "SRAMM";
                case 14: return "FB-DIMM";
                default: return code > 0 ? "FormFactor #" + code : "Standard";
            }
        }

        private static string GetBatteryStatusString(ushort status)
        {
            switch (status)
            {
                case 1: return "Discharging";
                case 2: return "Connected to AC (Charging/Full)";
                case 3: return "Fully Charged";
                case 4: return "Low Battery";
                case 5: return "Critical Battery";
                case 6: return "Charging";
                default: return "Unknown Status (" + status + ")";
            }
        }

        private static string[] EnumerableChunk(string str, int chunkSize)
        {
            int count = (str.Length + chunkSize - 1) / chunkSize;
            string[] result = new string[count];
            for (int i = 0; i < count; i++)
            {
                int len = Math.Min(chunkSize, str.Length - i * chunkSize);
                result[i] = str.Substring(i * chunkSize, len);
            }
            return result;
        }
    }
}
