namespace CPUZClone
{
    using System;
    using System.Drawing;
    using System.IO;
    using System.Net;
    using System.Text;
    using System.Windows.Forms;

    public class MainForm : Form
    {
        private HardwareInfo currentInfo;
        private TabControl mainTabControl;
        private TextBox txtMachineName;
        
        // Live Monitoring Controls
        private Timer liveTimer;
        private ProgressBar cpuProgressBar;
        private Label cpuLoadLabel;
        private ProgressBar ramProgressBar;
        private Label ramLoadLabel;
        private Label statusLabel;

        private string googleScriptUrlFile = "google_script_url.txt";
        private string defaultGoogleScriptUrl = "";

        // UI Colors (Sleek Dark Theme)
        private Color bgColor = Color.FromArgb(24, 26, 32);
        private Color panelColor = Color.FromArgb(32, 35, 43);
        private Color cardColor = Color.FromArgb(40, 44, 54);
        private Color textColor = Color.FromArgb(230, 235, 245);
        private Color accentColor = Color.FromArgb(0, 168, 255);
        private Color labelHeaderColor = Color.FromArgb(120, 170, 245);
        private Color gridLineColor = Color.FromArgb(55, 60, 72);

        public MainForm()
        {
            this.Text = "CPU Beyond v1.0 - Hardware Inspector | Developed by Diego A";
            this.Size = new Size(880, 720);
            this.MinimumSize = new Size(820, 640);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = bgColor;
            this.ForeColor = textColor;
            this.Font = new Font("Segoe UI", 9.5f, FontStyle.Regular);

            // Window Icon loaded directly from compiled embedded resource
            try
            {
                byte[] iconBytes = Convert.FromBase64String(EmbeddedLogo.Base64Image);
                using (MemoryStream ms = new MemoryStream(iconBytes))
                using (Bitmap bmp = new Bitmap(ms))
                {
                    IntPtr hIcon = bmp.GetHicon();
                    this.Icon = Icon.FromHandle(hIcon);
                }
            }
            catch { }
            this.ShowIcon = true;


            // Load saved Google Script URL if present
            if (File.Exists(googleScriptUrlFile))
            {
                try { defaultGoogleScriptUrl = File.ReadAllText(googleScriptUrlFile).Trim(); } catch { }
            }

            InitializeComponents();
            LoadHardwareData();

            // Setup live refresh timer (1.5s interval)
            liveTimer = new Timer();
            liveTimer.Interval = 1500;
            liveTimer.Tick += LiveTimer_Tick;
            liveTimer.Start();
        }

        private void InitializeComponents()
        {
            // Header Panel
            Panel headerPanel = new Panel();
            headerPanel.Dock = DockStyle.Top;
            headerPanel.Height = 115;
            headerPanel.BackColor = panelColor;
            headerPanel.Padding = new Padding(15, 8, 15, 8);

            // Picture Box for embedded logo
            PictureBox logoBox = new PictureBox();
            logoBox.Size = new Size(58, 58);
            logoBox.Location = new Point(12, 8);
            logoBox.SizeMode = PictureBoxSizeMode.Zoom;
            
            try
            {
                byte[] imgBytes = Convert.FromBase64String(EmbeddedLogo.Base64Image);
                using (MemoryStream ms = new MemoryStream(imgBytes))
                {
                    logoBox.Image = Image.FromStream(ms);
                }
            }
            catch { }

            Label titleLabel = new Label();
            titleLabel.Text = "CPU BEYOND";
            titleLabel.Font = new Font("Segoe UI", 16f, FontStyle.Bold);
            titleLabel.ForeColor = accentColor;
            titleLabel.AutoSize = true;
            titleLabel.Location = new Point(80, 8);

            Label subtitleLabel = new Label();
            subtitleLabel.Text = "Hardware Specification Inspector  •  Developed by Diego A";
            subtitleLabel.Font = new Font("Segoe UI", 9.5f, FontStyle.Italic);
            subtitleLabel.ForeColor = Color.LightGray;
            subtitleLabel.AutoSize = true;
            subtitleLabel.Location = new Point(83, 38);

            // Machine Name Input Panel inside Header
            Label lblNameTitle = new Label();
            lblNameTitle.Text = "Nombre / Identificador de esta PC:";
            lblNameTitle.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            lblNameTitle.ForeColor = labelHeaderColor;
            lblNameTitle.Location = new Point(83, 75);
            lblNameTitle.AutoSize = true;

            txtMachineName = new TextBox();
            txtMachineName.Location = new Point(310, 72);
            txtMachineName.Width = 320;
            txtMachineName.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
            txtMachineName.BackColor = cardColor;
            txtMachineName.ForeColor = Color.Yellow;
            txtMachineName.Text = Environment.MachineName;
            txtMachineName.TextChanged += (s, e) => {
                if (currentInfo != null)
                    currentInfo.CustomMachineName = txtMachineName.Text.Trim();
            };

            headerPanel.Controls.Add(logoBox);
            headerPanel.Controls.Add(titleLabel);
            headerPanel.Controls.Add(subtitleLabel);
            headerPanel.Controls.Add(lblNameTitle);
            headerPanel.Controls.Add(txtMachineName);

            // Bottom Action Bar Panel
            Panel bottomPanel = new Panel();
            bottomPanel.Dock = DockStyle.Bottom;
            bottomPanel.Height = 65;
            bottomPanel.BackColor = panelColor;
            bottomPanel.Padding = new Padding(15, 10, 15, 10);

            Button btnSaveExcelCsv = CreateStyledButton("Guardar en Excel (.csv)", Color.FromArgb(230, 126, 34));
            btnSaveExcelCsv.Location = new Point(12, 15);
            btnSaveExcelCsv.Width = 175;
            btnSaveExcelCsv.Click += BtnSaveExcelCsv_Click;

            Button btnExport = CreateStyledButton("Guardar Reporte (.txt)", accentColor);
            btnExport.Location = new Point(195, 15);
            btnExport.Width = 160;
            btnExport.Click += BtnExport_Click;

            Button btnSendSheets = CreateStyledButton("Guardar en Google Sheets", Color.FromArgb(46, 204, 113));
            btnSendSheets.Location = new Point(363, 15);
            btnSendSheets.Width = 185;
            btnSendSheets.Click += BtnSendSheets_Click;

            Button btnCopyExcel = CreateStyledButton("Copiar Fila", Color.FromArgb(52, 152, 219));
            btnCopyExcel.Location = new Point(556, 15);
            btnCopyExcel.Width = 100;
            btnCopyExcel.Click += BtnCopyExcel_Click;

            Button btnRefresh = CreateStyledButton("Actualizar", Color.FromArgb(155, 89, 182));
            btnRefresh.Location = new Point(664, 15);
            btnRefresh.Width = 95;
            btnRefresh.Click += (s, e) => LoadHardwareData();

            statusLabel = new Label();
            statusLabel.Text = "Listo";
            statusLabel.ForeColor = Color.DarkGray;
            statusLabel.AutoSize = true;
            statusLabel.Location = new Point(768, 22);

            bottomPanel.Controls.Add(btnSaveExcelCsv);
            bottomPanel.Controls.Add(btnExport);
            bottomPanel.Controls.Add(btnSendSheets);
            bottomPanel.Controls.Add(btnCopyExcel);
            bottomPanel.Controls.Add(btnRefresh);
            bottomPanel.Controls.Add(statusLabel);



            // Tab Control
            mainTabControl = new TabControl();
            mainTabControl.Dock = DockStyle.Fill;
            mainTabControl.Padding = new Point(15, 8);
            mainTabControl.Font = new Font("Segoe UI", 10f, FontStyle.Bold);

            this.Controls.Add(mainTabControl);
            this.Controls.Add(headerPanel);
            this.Controls.Add(bottomPanel);
        }


        private Button CreateStyledButton(string text, Color bg)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.BackColor = bg;
            btn.ForeColor = Color.White;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.Height = 34;
            btn.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            btn.Cursor = Cursors.Hand;
            return btn;
        }

        private void LoadHardwareData()
        {
            statusLabel.Text = "Scanning hardware...";
            this.Cursor = Cursors.WaitCursor;

            currentInfo = HardwareInspector.GetSystemInfo();

            mainTabControl.TabPages.Clear();
            mainTabControl.TabPages.Add(BuildCpuTabPage());
            mainTabControl.TabPages.Add(BuildMotherboardTabPage());
            mainTabControl.TabPages.Add(BuildMemoryTabPage());
            mainTabControl.TabPages.Add(BuildGpuTabPage());
            mainTabControl.TabPages.Add(BuildStorageTabPage());
            mainTabControl.TabPages.Add(BuildBatteryTabPage());
            mainTabControl.TabPages.Add(BuildOsTabPage());

            this.Cursor = Cursors.Default;
            statusLabel.Text = "Hardware Scan Complete (" + DateTime.Now.ToString("HH:mm:ss") + ")";
        }

        private TabPage BuildCpuTabPage()
        {
            TabPage page = CreateBaseTabPage("CPU");
            TableLayoutPanel grid = CreatePropertyGrid();

            AddGridRow(grid, "Processor Name", currentInfo.CpuName);
            AddGridRow(grid, "Manufacturer", currentInfo.CpuManufacturer);
            AddGridRow(grid, "Socket / Package", currentInfo.CpuSocket);
            AddGridRow(grid, "Architecture", currentInfo.CpuArchitecture);
            AddGridRow(grid, "Physical Cores", currentInfo.CpuCores.ToString());
            AddGridRow(grid, "Logical Threads", currentInfo.CpuThreads.ToString());
            AddGridRow(grid, "Max Clock Speed", currentInfo.CpuMaxClockMHz + " MHz");
            AddGridRow(grid, "Current Clock Speed", currentInfo.CpuCurrentClockMHz + " MHz");
            AddGridRow(grid, "L2 Cache Size", currentInfo.CpuL2Cache);
            AddGridRow(grid, "L3 Cache Size", currentInfo.CpuL3Cache);
            AddGridRow(grid, "Hardware Virtualization", currentInfo.CpuVirtualization ? "Supported / Enabled" : "Disabled or N/A");

            // Live CPU Load Bar
            Panel loadPanel = new Panel();
            loadPanel.Dock = DockStyle.Bottom;
            loadPanel.Height = 45;
            loadPanel.BackColor = cardColor;
            loadPanel.Padding = new Padding(10);

            Label lblLoadTitle = new Label();
            lblLoadTitle.Text = "CPU Utilization:";
            lblLoadTitle.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            lblLoadTitle.ForeColor = labelHeaderColor;
            lblLoadTitle.Location = new Point(10, 12);
            lblLoadTitle.Size = new Size(110, 20);

            cpuProgressBar = new ProgressBar();
            cpuProgressBar.Location = new Point(125, 10);
            cpuProgressBar.Size = new Size(450, 22);
            cpuProgressBar.Value = Math.Min(100, Math.Max(0, (int)currentInfo.CpuLoadPercentage));

            cpuLoadLabel = new Label();
            cpuLoadLabel.Text = string.Format("{0:F1}%", currentInfo.CpuLoadPercentage);
            cpuLoadLabel.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            cpuLoadLabel.ForeColor = accentColor;
            cpuLoadLabel.Location = new Point(585, 12);
            cpuLoadLabel.Size = new Size(80, 20);

            loadPanel.Controls.Add(lblLoadTitle);
            loadPanel.Controls.Add(cpuProgressBar);
            loadPanel.Controls.Add(cpuLoadLabel);

            page.Controls.Add(grid);
            page.Controls.Add(loadPanel);
            return page;
        }

        private TabPage BuildMotherboardTabPage()
        {
            TabPage page = CreateBaseTabPage("Motherboard");
            TableLayoutPanel grid = CreatePropertyGrid();

            AddGridRow(grid, "Board Manufacturer", currentInfo.BoardManufacturer);
            AddGridRow(grid, "Board Product / Model", currentInfo.BoardProduct);
            AddGridRow(grid, "Board Version", currentInfo.BoardVersion);
            AddGridRow(grid, "Serial Number", currentInfo.BoardSerialNumber);
            AddGridRow(grid, "System Manufacturer", currentInfo.SystemManufacturer);
            AddGridRow(grid, "System Model / Laptop", currentInfo.SystemModel);
            AddGridRow(grid, "BIOS Vendor", currentInfo.BiosVendor);
            AddGridRow(grid, "BIOS Version", currentInfo.BiosVersion);
            AddGridRow(grid, "BIOS Release Date", currentInfo.BiosReleaseDate);

            page.Controls.Add(grid);
            return page;
        }

        private TabPage BuildMemoryTabPage()
        {
            TabPage page = CreateBaseTabPage("Memory (RAM)");
            
            Panel topSummary = new Panel();
            topSummary.Dock = DockStyle.Top;
            topSummary.Height = 85;
            topSummary.BackColor = cardColor;
            topSummary.Padding = new Padding(15, 10, 15, 10);

            Label lblTotal = new Label();
            lblTotal.Text = "Total RAM: " + currentInfo.TotalRamGB + "   |   Used: " + currentInfo.UsedRamGB + "   |   Free: " + currentInfo.FreeRamGB;
            lblTotal.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
            lblTotal.ForeColor = accentColor;
            lblTotal.Location = new Point(15, 10);
            lblTotal.AutoSize = true;

            ramProgressBar = new ProgressBar();
            ramProgressBar.Location = new Point(15, 40);
            ramProgressBar.Size = new Size(560, 22);
            ramProgressBar.Value = Math.Min(100, Math.Max(0, (int)currentInfo.RamUsagePercent));

            ramLoadLabel = new Label();
            ramLoadLabel.Text = string.Format("{0:F1}% Used", currentInfo.RamUsagePercent);
            ramLoadLabel.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            ramLoadLabel.ForeColor = accentColor;
            ramLoadLabel.Location = new Point(585, 42);
            ramLoadLabel.Size = new Size(100, 20);

            topSummary.Controls.Add(lblTotal);
            topSummary.Controls.Add(ramProgressBar);
            topSummary.Controls.Add(ramLoadLabel);

            // RAM Slots ListView
            ListView listView = new ListView();
            listView.Dock = DockStyle.Fill;
            listView.View = View.Details;
            listView.FullRowSelect = true;
            listView.GridLines = true;
            listView.BackColor = panelColor;
            listView.ForeColor = textColor;
            listView.Font = new Font("Segoe UI", 9f);

            listView.Columns.Add("Slot / Locator", 120);
            listView.Columns.Add("Capacity", 100);
            listView.Columns.Add("Speed", 90);
            listView.Columns.Add("Form Factor", 100);
            listView.Columns.Add("Manufacturer", 140);
            listView.Columns.Add("Part Number", 180);

            foreach (var slot in currentInfo.RamSlots)
            {
                ListViewItem item = new ListViewItem(slot.SlotLabel);
                item.SubItems.Add(slot.Capacity);
                item.SubItems.Add(slot.Speed);
                item.SubItems.Add(slot.FormFactor);
                item.SubItems.Add(slot.Manufacturer);
                item.SubItems.Add(slot.PartNumber);
                listView.Items.Add(item);
            }

            page.Controls.Add(listView);
            page.Controls.Add(topSummary);
            return page;
        }

        private TabPage BuildGpuTabPage()
        {
            TabPage page = CreateBaseTabPage("Graphics (GPU)");
            TableLayoutPanel grid = CreatePropertyGrid();

            int gpuIndex = 1;
            foreach (var gpu in currentInfo.Gpus)
            {
                AddGridRow(grid, "GPU #" + gpuIndex + " Name", gpu.Name);
                AddGridRow(grid, "Dedicated VRAM", gpu.Vram);
                AddGridRow(grid, "Driver Version", gpu.DriverVersion);
                AddGridRow(grid, "Driver Date", gpu.DriverDate);
                AddGridRow(grid, "Video Processor", gpu.VideoProcessor);
                AddGridRow(grid, "Resolution & Refresh", gpu.Resolution);
                AddGridRow(grid, "---", "---");
                gpuIndex++;
            }

            page.Controls.Add(grid);
            return page;
        }

        private TabPage BuildStorageTabPage()
        {
            TabPage page = CreateBaseTabPage("Storage");
            TableLayoutPanel grid = CreatePropertyGrid();

            int diskIndex = 1;
            foreach (var drive in currentInfo.Drives)
            {
                AddGridRow(grid, "Disk Drive #" + diskIndex, drive.Model);
                AddGridRow(grid, "Interface / Type", drive.InterfaceType + " / " + drive.MediaType);
                AddGridRow(grid, "Total Disk Size", drive.Size);
                
                if (drive.Partitions.Count > 0)
                {
                    string vols = string.Join("\n", drive.Partitions.ToArray());
                    AddGridRow(grid, "Mounted Volumes", vols);
                }
                AddGridRow(grid, "------------------", "----------------------------------");
                diskIndex++;
            }

            page.Controls.Add(grid);
            return page;
        }

        private TabPage BuildBatteryTabPage()
        {
            TabPage page = CreateBaseTabPage("Battery & Power");
            TableLayoutPanel grid = CreatePropertyGrid();

            AddGridRow(grid, "Battery Detected", currentInfo.HasBattery ? "Yes (Laptop)" : "No (Desktop PC / AC Only)");
            AddGridRow(grid, "Battery Status", currentInfo.BatteryStatus);
            if (currentInfo.HasBattery)
            {
                AddGridRow(grid, "Charge Level", currentInfo.BatteryPercentage);
                AddGridRow(grid, "Est. Time Remaining", currentInfo.EstimatedTimeRemaining);
            }

            page.Controls.Add(grid);
            return page;
        }

        private TabPage BuildOsTabPage()
        {
            TabPage page = CreateBaseTabPage("OS & Network");
            TableLayoutPanel grid = CreatePropertyGrid();

            AddGridRow(grid, "Operating System", currentInfo.OsName);
            AddGridRow(grid, "OS Version", currentInfo.OsVersion + " (Build " + currentInfo.OsBuild + ")");
            AddGridRow(grid, "Architecture", currentInfo.OsArchitecture);
            AddGridRow(grid, "Computer Name", currentInfo.ComputerName);
            AddGridRow(grid, "System Uptime", currentInfo.SystemUptime);

            if (currentInfo.NetworkAdapters.Count > 0)
            {
                string nets = string.Join("\n", currentInfo.NetworkAdapters.ToArray());
                AddGridRow(grid, "Active Adapters", nets);
            }

            page.Controls.Add(grid);
            return page;
        }

        private TabPage CreateBaseTabPage(string title)
        {
            TabPage page = new TabPage(title);
            page.BackColor = panelColor;
            page.Padding = new Padding(10);
            return page;
        }

        private TableLayoutPanel CreatePropertyGrid()
        {
            TableLayoutPanel panel = new TableLayoutPanel();
            panel.Dock = DockStyle.Fill;
            panel.AutoScroll = true;
            panel.ColumnCount = 2;
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180f));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            panel.Padding = new Padding(10);
            return panel;
        }

        private void AddGridRow(TableLayoutPanel grid, string key, string value)
        {
            Label lblKey = new Label();
            lblKey.Text = key;
            lblKey.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            lblKey.ForeColor = labelHeaderColor;
            lblKey.AutoSize = true;
            lblKey.Margin = new Padding(3, 6, 3, 6);

            Label lblVal = new Label();
            lblVal.Text = value;
            lblVal.Font = new Font("Consolas", 9.5f, FontStyle.Regular);
            lblVal.ForeColor = textColor;
            lblVal.AutoSize = true;
            lblVal.Margin = new Padding(3, 6, 3, 6);

            grid.Controls.Add(lblKey);
            grid.Controls.Add(lblVal);
        }

        private void LiveTimer_Tick(object sender, EventArgs e)
        {
            float cpuLoad = HardwareInspector.GetCpuLoad();
            if (cpuProgressBar != null && !cpuProgressBar.IsDisposed)
            {
                cpuProgressBar.Value = Math.Min(100, Math.Max(0, (int)cpuLoad));
                cpuLoadLabel.Text = string.Format("{0:F1}%", cpuLoad);
            }
        }

        private void BtnSaveExcelCsv_Click(object sender, EventArgs e)
        {
            if (txtMachineName != null)
                currentInfo.CustomMachineName = txtMachineName.Text.Trim();

            string defaultPath = "Inventario_Equipos_Excel.csv";

            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Filter = "Archivos de Excel (*.csv)|*.csv|Todos los archivos (*.*)|*.*";
                dialog.FileName = defaultPath;
                dialog.Title = "Guardar / Anexar a Hoja de Excel";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    bool fileExists = File.Exists(dialog.FileName);
                    StringBuilder sb = new StringBuilder();

                    if (!fileExists)
                    {
                        sb.Append(ReportGenerator.GetCsvHeader());
                    }

                    sb.Append(ReportGenerator.GenerateCsvRow(currentInfo));
                    File.AppendAllText(dialog.FileName, sb.ToString(), Encoding.UTF8);

                    string machineName = string.IsNullOrEmpty(currentInfo.CustomMachineName) ? currentInfo.ComputerName : currentInfo.CustomMachineName;
                    MessageBox.Show("Especificaciones agregadas exitosamente al archivo de Excel.\n\nArchivo: " + dialog.FileName + "\nEquipo registrado: " + machineName + "\n\nPuedes hacer doble clic sobre el archivo .csv para abrirlo directamente en Microsoft Excel.", "Guardado en Excel", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void BtnExport_Click(object sender, EventArgs e)

        {
            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*";
                dialog.FileName = "Reporte_Hardware_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".txt";
                dialog.Title = "Save Hardware Spec Report";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    if (txtMachineName != null)
                        currentInfo.CustomMachineName = txtMachineName.Text.Trim();
                    string report = ReportGenerator.GenerateTextReport(currentInfo);
                    File.WriteAllText(dialog.FileName, report);
                    MessageBox.Show("Reporte de hardware guardado correctamente en:\n" + dialog.FileName, "Reporte Exportado", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void BtnSendSheets_Click(object sender, EventArgs e)
        {
            if (txtMachineName != null)
                currentInfo.CustomMachineName = txtMachineName.Text.Trim();

            string machineName = string.IsNullOrEmpty(currentInfo.CustomMachineName) ? currentInfo.ComputerName : currentInfo.CustomMachineName;

            // Check if URL is saved in google_script_url.txt
            if (File.Exists(googleScriptUrlFile))
            {
                try { defaultGoogleScriptUrl = File.ReadAllText(googleScriptUrlFile).Trim(); } catch { }
            }

            if (string.IsNullOrEmpty(defaultGoogleScriptUrl))
            {
                string input = PromptInput("Vincular Google Sheet", "Pega aqui la URL de tu Web App de Google Script (formato: https://script.google.com/macros/s/.../exec):\n\nEsta URL se guardara para siempre y no te la volvera a pedir.", defaultGoogleScriptUrl);
                if (!string.IsNullOrEmpty(input))
                {
                    defaultGoogleScriptUrl = input.Trim();
                    try { File.WriteAllText(googleScriptUrlFile, defaultGoogleScriptUrl); } catch { }
                }
                else
                {
                    return;
                }
            }

            try
            {
                statusLabel.Text = "Guardando en Google Sheets...";
                this.Cursor = Cursors.WaitCursor;

                string jsonPayload = ReportGenerator.GenerateJsonPayload(currentInfo);
                
                using (WebClient client = new WebClient())
                {
                    client.Headers[HttpRequestHeader.ContentType] = "application/json; charset=utf-8";
                    client.Encoding = Encoding.UTF8;
                    string response = client.UploadString(defaultGoogleScriptUrl, "POST", jsonPayload);
                }

                this.Cursor = Cursors.Default;
                statusLabel.Text = "Guardado en Google Sheets";
                MessageBox.Show("Los datos de la máquina '" + machineName + "' se han guardado exitosamente en tu Google Sheet.", "Guardado Exitoso", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                this.Cursor = Cursors.Default;
                statusLabel.Text = "Error al conectar";
                
                DialogResult dr = MessageBox.Show("No se pudo conectar con la URL de Google Script (" + ex.Message + ").\n\n¿Deseas cambiar la URL de tu Google Script?", "Error de Conexión", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (dr == DialogResult.Yes)
                {
                    string input = PromptInput("Cambiar URL de Google Script", "Ingresa la nueva URL del Web App (https://script.google.com/macros/s/.../exec):", defaultGoogleScriptUrl);
                    if (!string.IsNullOrEmpty(input))
                    {
                        defaultGoogleScriptUrl = input.Trim();
                        try { File.WriteAllText(googleScriptUrlFile, defaultGoogleScriptUrl); } catch { }
                    }
                }
            }
        }


        private void BtnCopyExcel_Click(object sender, EventArgs e)
        {
            if (txtMachineName != null)
                currentInfo.CustomMachineName = txtMachineName.Text.Trim();

            string rowData = ReportGenerator.GenerateTabSeparatedRow(currentInfo);
            Clipboard.SetText(rowData);
            
            string machineName = string.IsNullOrEmpty(currentInfo.CustomMachineName) ? currentInfo.ComputerName : currentInfo.CustomMachineName;
            MessageBox.Show("Fila de especificaciones copiada al portapapeles para '" + machineName + "'.\n\nPuedes ir a tu Google Sheet (o Excel), seleccionar una celda vacía y presionar Ctrl + V para pegarla en columnas.", "Copiado para Google Sheets / Excel", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ShowGoogleScriptInstructions()
        {
            string scriptCode = 
@"// --- CÓDIGO ULTRA RÁPIDO PARA TU HOJA DE GOOGLE SHEETS ---
// Documento: https://docs.google.com/spreadsheets/d/1LMYkSPb72mEWYweISyUMWt0lCS5HTHZt_g8wId28ov4
// 1. En tu hoja de Google Sheets, ve al menú superior: Extensiones > Apps Script
// 2. Borra todo el código y pega este bloque:

function doPost(e) {
  try {
    var ss = SpreadsheetApp.openById('1LMYkSPb72mEWYweISyUMWt0lCS5HTHZt_g8wId28ov4');
    var sheet = ss.getActiveSheet();
    var data = JSON.parse(e.postData.contents);
    sheet.appendRow([
      data.machineName,
      data.timestamp,
      data.cpu,
      data.ram,
      data.gpu,
      data.motherboard,
      data.storage,
      data.os,
      data.network
    ]);
    return ContentService.createTextOutput('OK');
  } catch (err) {
    return ContentService.createTextOutput('Error: ' + err.toString());
  }
}

// 3. Haz clic en 'Implementar' (botón azul arriba a la derecha) > 'Nueva implementación'
// 4. En tipo selecciona 'Aplicación web'
// 5. En 'Quién tiene acceso' selecciona 'Cualquier persona' (Anyone)
// 6. Haz clic en Implementar, copia la URL del Web App (https://script.google.com/macros/s/.../exec) y pégala en el programa.";

            Clipboard.SetText(scriptCode);
            MessageBox.Show("Las instrucciones y el código optimizado se han copiado al portapapeles.\n\nPuedes pegarlo en Extensiones > Apps Script en tu hoja.", "Instrucciones de Vinculación", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }



        private string PromptInput(string title, string promptText, string defaultValue)
        {
            Form form = new Form();
            Label label = new Label();
            TextBox textBox = new TextBox();
            Button buttonOk = new Button();
            Button buttonCancel = new Button();

            form.Text = title;
            label.Text = promptText;
            textBox.Text = defaultValue;

            buttonOk.Text = "OK";
            buttonCancel.Text = "Cancelar";
            buttonOk.DialogResult = DialogResult.OK;
            buttonCancel.DialogResult = DialogResult.Cancel;

            label.SetBounds(12, 12, 540, 60);
            textBox.SetBounds(12, 75, 540, 24);
            buttonOk.SetBounds(360, 110, 90, 30);
            buttonCancel.SetBounds(460, 110, 90, 30);

            label.AutoSize = false;
            form.ClientSize = new Size(570, 150);
            form.Controls.AddRange(new Control[] { label, textBox, buttonOk, buttonCancel });
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.StartPosition = FormStartPosition.CenterParent;
            form.MaximizeBox = false;
            form.MinimizeBox = false;
            form.AcceptButton = buttonOk;
            form.CancelButton = buttonCancel;

            DialogResult dialogResult = form.ShowDialog();
            return dialogResult == DialogResult.OK ? textBox.Text : null;
        }

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}

