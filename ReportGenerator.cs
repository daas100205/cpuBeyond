namespace CPUZClone
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;


    public static class ReportGenerator
    {
        public static string GenerateTextReport(HardwareInfo info)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("===============================================================================");
            sb.AppendLine("                    CPU BEYOND - HARDWARE SPECIFICATION REPORT                 ");
            sb.AppendLine("                       Developed by Diego A                                   ");
            sb.AppendLine("                       Generated on: " + DateTime.Now.ToString("F"));
            sb.AppendLine("===============================================================================");
            sb.AppendLine();


            // CPU
            sb.AppendLine("[ PROCESSOR (CPU) ]");
            sb.AppendLine("Name                  : " + info.CpuName);
            sb.AppendLine("Manufacturer          : " + info.CpuManufacturer);
            sb.AppendLine("Socket / Package      : " + info.CpuSocket);
            sb.AppendLine("Architecture          : " + info.CpuArchitecture);
            sb.AppendLine("Cores / Threads       : " + info.CpuCores + " Physical Cores / " + info.CpuThreads + " Threads");
            sb.AppendLine("Max Clock Speed       : " + info.CpuMaxClockMHz + " MHz");
            sb.AppendLine("Current Clock Speed   : " + info.CpuCurrentClockMHz + " MHz");
            sb.AppendLine("L2 Cache              : " + info.CpuL2Cache);
            sb.AppendLine("L3 Cache              : " + info.CpuL3Cache);
            sb.AppendLine("Virtualization        : " + (info.CpuVirtualization ? "Supported / Enabled" : "Disabled / Unsupported"));
            sb.AppendLine();

            // Motherboard
            sb.AppendLine("[ MOTHERBOARD & BIOS ]");
            sb.AppendLine("Manufacturer          : " + info.BoardManufacturer);
            sb.AppendLine("Product Model         : " + info.BoardProduct);
            sb.AppendLine("Board Version         : " + info.BoardVersion);
            sb.AppendLine("Serial Number         : " + info.BoardSerialNumber);
            sb.AppendLine("System Model / Laptop : " + info.SystemManufacturer + " " + info.SystemModel);
            sb.AppendLine("BIOS Vendor           : " + info.BiosVendor);
            sb.AppendLine("BIOS Version          : " + info.BiosVersion);
            sb.AppendLine("BIOS Release Date     : " + info.BiosReleaseDate);
            sb.AppendLine();

            // RAM
            sb.AppendLine("[ MEMORY (RAM) ]");
            sb.AppendLine("Total System RAM      : " + info.TotalRamGB);
            sb.AppendLine("Used RAM              : " + info.UsedRamGB + " (" + string.Format("{0:F1}", info.RamUsagePercent) + "%)");
            sb.AppendLine("Available RAM         : " + info.FreeRamGB);
            sb.AppendLine("Installed RAM Modules :");
            if (info.RamSlots.Count > 0)
            {
                foreach (var slot in info.RamSlots)
                {
                    sb.AppendLine(string.Format("  - [{0}] {1} | {2} | {3} | {4} | Part: {5}",
                        slot.SlotLabel, slot.Capacity, slot.Speed, slot.FormFactor, slot.Manufacturer, slot.PartNumber));
                }
            }
            else
            {
                sb.AppendLine("  - No detailed RAM slot info available.");
            }
            sb.AppendLine();

            // GPU
            sb.AppendLine("[ GRAPHICS (GPU) ]");
            if (info.Gpus.Count > 0)
            {
                foreach (var gpu in info.Gpus)
                {
                    sb.AppendLine("GPU Name              : " + gpu.Name);
                    sb.AppendLine("VRAM                  : " + gpu.Vram);
                    sb.AppendLine("Driver Version        : " + gpu.DriverVersion + " (" + gpu.DriverDate + ")");
                    sb.AppendLine("Display Resolution    : " + gpu.Resolution);
                    sb.AppendLine("-------------------------------------------------------------------------------");
                }
            }
            else
            {
                sb.AppendLine("No GPU detected.");
            }
            sb.AppendLine();

            // STORAGE
            sb.AppendLine("[ STORAGE & DRIVES ]");
            if (info.Drives.Count > 0)
            {
                foreach (var drive in info.Drives)
                {
                    sb.AppendLine("Drive Model           : " + drive.Model);
                    sb.AppendLine("Interface / Media     : " + drive.InterfaceType + " / " + drive.MediaType);
                    sb.AppendLine("Capacity              : " + drive.Size);
                    if (drive.Partitions.Count > 0)
                    {
                        sb.AppendLine("Volumes:");
                        foreach (var part in drive.Partitions)
                        {
                            sb.AppendLine("  * " + part);
                        }
                    }
                    sb.AppendLine("-------------------------------------------------------------------------------");
                }
            }
            sb.AppendLine();

            // BATTERY
            sb.AppendLine("[ POWER & BATTERY ]");
            sb.AppendLine("Battery Detected      : " + (info.HasBattery ? "Yes" : "No"));
            sb.AppendLine("Status                : " + info.BatteryStatus);
            if (info.HasBattery)
            {
                sb.AppendLine("Charge Level          : " + info.BatteryPercentage);
                sb.AppendLine("Est. Time Remaining   : " + info.EstimatedTimeRemaining);
            }
            sb.AppendLine();

            // SYSTEM & OS
            sb.AppendLine("[ OPERATING SYSTEM & NETWORK ]");
            sb.AppendLine("OS Name               : " + info.OsName);
            sb.AppendLine("OS Version            : " + info.OsVersion + " (Build " + info.OsBuild + ")");
            sb.AppendLine("OS Architecture       : " + info.OsArchitecture);
            sb.AppendLine("Computer Name         : " + info.ComputerName);
            sb.AppendLine("System Uptime         : " + info.SystemUptime);
            sb.AppendLine("Network Adapters      :");
            foreach (var net in info.NetworkAdapters)
            {
                sb.AppendLine("  - " + net);
            }
            sb.AppendLine();

            sb.AppendLine("===============================================================================");
            sb.AppendLine("                              END OF SPEC REPORT                               ");
            sb.AppendLine("===============================================================================");

            return sb.ToString();
        }

        public static string GenerateQuickSummary(HardwareInfo info)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("--- PC SUMMARY ---");
            sb.AppendLine("Equipo / Nombre: " + (string.IsNullOrEmpty(info.CustomMachineName) ? info.ComputerName : info.CustomMachineName));
            sb.AppendLine("CPU: " + info.CpuName + " (" + info.CpuCores + "C/" + info.CpuThreads + "T)");
            sb.AppendLine("RAM: " + info.TotalRamGB);
            if (info.Gpus.Count > 0)
                sb.AppendLine("GPU: " + info.Gpus[0].Name + " (" + info.Gpus[0].Vram + ")");
            sb.AppendLine("Motherboard: " + info.BoardManufacturer + " " + info.BoardProduct);
            sb.AppendLine("OS: " + info.OsName + " " + info.OsArchitecture);
            return sb.ToString();
        }

        public static string GenerateJsonPayload(HardwareInfo info)
        {
            string machineName = string.IsNullOrEmpty(info.CustomMachineName) ? info.ComputerName : info.CustomMachineName;
            string gpuStr = info.Gpus.Count > 0 ? info.Gpus[0].Name + " (" + info.Gpus[0].Vram + ")" : "N/A";
            string diskStr = GetFormattedStorageSummary(info);
            string netStr = info.NetworkAdapters.Count > 0 ? info.NetworkAdapters[0] : "N/A";

            StringBuilder json = new StringBuilder();
            json.Append("{");
            json.AppendFormat("\"machineName\":\"{0}\",", EscapeJson(machineName));
            json.AppendFormat("\"computerName\":\"{0}\",", EscapeJson(info.ComputerName));
            json.AppendFormat("\"timestamp\":\"{0}\",", EscapeJson(DateTime.Now.ToString("g")));
            json.AppendFormat("\"cpu\":\"{0}\",", EscapeJson(info.CpuName + " (" + info.CpuCores + "C/" + info.CpuThreads + "T @ " + info.CpuMaxClockMHz + "MHz)"));
            json.AppendFormat("\"ram\":\"{0}\",", EscapeJson(info.TotalRamGB));
            json.AppendFormat("\"gpu\":\"{0}\",", EscapeJson(gpuStr));
            json.AppendFormat("\"motherboard\":\"{0}\",", EscapeJson(info.BoardManufacturer + " " + info.BoardProduct + " (" + info.SystemModel + ")"));
            json.AppendFormat("\"storage\":\"{0}\",", EscapeJson(diskStr));
            json.AppendFormat("\"os\":\"{0}\",", EscapeJson(info.OsName + " " + info.OsArchitecture + " (Build " + info.OsBuild + ")"));
            json.AppendFormat("\"battery\":\"{0}\",", EscapeJson(info.BatteryStatus + (info.HasBattery ? " (" + info.BatteryPercentage + ")" : "")));
            json.AppendFormat("\"network\":\"{0}\"", EscapeJson(netStr));
            json.Append("}");

            return json.ToString();
        }

        public static string GenerateTabSeparatedRow(HardwareInfo info)
        {
            string machineName = string.IsNullOrEmpty(info.CustomMachineName) ? info.ComputerName : info.CustomMachineName;
            string gpuStr = info.Gpus.Count > 0 ? info.Gpus[0].Name + " (" + info.Gpus[0].Vram + ")" : "N/A";
            string diskStr = GetFormattedStorageSummary(info);
            string netStr = info.NetworkAdapters.Count > 0 ? info.NetworkAdapters[0] : "N/A";

            return string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\t{8}",
                machineName,
                DateTime.Now.ToString("g"),
                info.CpuName + " (" + info.CpuCores + "C/" + info.CpuThreads + "T)",
                info.TotalRamGB,
                gpuStr,
                info.BoardManufacturer + " " + info.BoardProduct,
                diskStr,
                info.OsName + " " + info.OsArchitecture,
                netStr);
        }

        public static string GetCsvHeader()
        {
            return "\"Nombre de Equipo\",\"Fecha y Hora\",\"Procesador (CPU)\",\"Memoria RAM\",\"Tarjeta de Video (GPU)\",\"Tarjeta Madre / Laptop\",\"Almacenamiento (Discos / C: Principal)\",\"Sistema Operativo\",\"Red / IP / MAC\"\r\n";
        }

        public static string GenerateCsvRow(HardwareInfo info)
        {
            string machineName = string.IsNullOrEmpty(info.CustomMachineName) ? info.ComputerName : info.CustomMachineName;
            string gpuStr = info.Gpus.Count > 0 ? info.Gpus[0].Name + " (" + info.Gpus[0].Vram + ")" : "N/A";
            string diskStr = GetFormattedStorageSummary(info);
            string netStr = info.NetworkAdapters.Count > 0 ? info.NetworkAdapters[0] : "N/A";

            return string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\",\"{6}\",\"{7}\",\"{8}\"\r\n",
                EscapeCsv(machineName),
                EscapeCsv(DateTime.Now.ToString("g")),
                EscapeCsv(info.CpuName + " (" + info.CpuCores + "C/" + info.CpuThreads + "T)"),
                EscapeCsv(info.TotalRamGB),
                EscapeCsv(gpuStr),
                EscapeCsv(info.BoardManufacturer + " " + info.BoardProduct),
                EscapeCsv(diskStr),
                EscapeCsv(info.OsName + " " + info.OsArchitecture),
                EscapeCsv(netStr));
        }

        public static string GetFormattedStorageSummary(HardwareInfo info)
        {
            StringBuilder sb = new StringBuilder();
            string systemDriveRoot = "C:\\";
            try
            {
                systemDriveRoot = Path.GetPathRoot(Environment.GetFolderPath(Environment.SpecialFolder.System)).ToUpper();
            }
            catch { }

            List<string> primaryList = new List<string>();
            List<string> secondaryList = new List<string>();

            try
            {
                foreach (DriveInfo drive in DriveInfo.GetDrives())
                {
                    if (drive.IsReady)
                    {
                        string driveRoot = drive.Name.ToUpper();
                        bool isSystem = driveRoot.StartsWith(systemDriveRoot.Substring(0, 1));
                        
                        string label = string.IsNullOrEmpty(drive.VolumeLabel) ? "Disco Local" : drive.VolumeLabel;
                        string freeStr = FormatBytesToSize((ulong)drive.AvailableFreeSpace);
                        string totalStr = FormatBytesToSize((ulong)drive.TotalSize);
                        
                        string entry = string.Format("{0} {1} ({2}) - {3} Libre / {4} Total [{5}]",
                            isSystem ? "[PRINCIPAL C:\\]" : "[SECUNDARIO]",
                            drive.Name.TrimEnd('\\'),
                            label,
                            freeStr,
                            totalStr,
                            drive.DriveFormat);

                        if (isSystem)
                            primaryList.Add(entry);
                        else
                            secondaryList.Add(entry);
                    }
                }
            }
            catch { }

            string modelInfo = info.Drives.Count > 0 ? info.Drives[0].Model : "";

            foreach (string p in primaryList)
            {
                if (sb.Length > 0) sb.Append("\n");
                sb.Append(p + (!string.IsNullOrEmpty(modelInfo) ? " (" + modelInfo + ")" : ""));
            }

            foreach (string s in secondaryList)
            {
                if (sb.Length > 0) sb.Append("\n");
                sb.Append(s);
            }

            if (sb.Length == 0)
            {
                sb.Append(info.Drives.Count > 0 ? info.Drives[0].Model + " (" + info.Drives[0].Size + ")" : "Sin datos de almacenamiento");
            }

            return sb.ToString();
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

        private static string EscapeCsv(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return s.Replace("\"", "\"\"");
        }

        private static string EscapeJson(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "");
        }
    }
}



