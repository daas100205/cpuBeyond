# CPU Beyond v1.0 - Hardware Inspector

**CPU Beyond** is a standalone C# Windows desktop application developed by **Diego A**. It inspects, displays, and exports hardware and system specifications (CPU, RAM, GPU, Disks, Motherboard, Battery, OS, Network) with real-time monitoring and direct Google Sheets integration.

---

## Key Features

- **Exhaustive Hardware Inspection**: CPU, RAM, GPU, Disks (C:\ System Prioritized), Motherboard, BIOS, Battery, OS & Network.
- **Custom Machine Naming**: Tag any computer before exporting.
- **Google Sheets Integration**: Direct POST request integration using Google Apps Script.
- **Direct Excel Export**: Save or append specs to a local Excel .csv master inventory file.
- **Standalone Executable**: Single binary with multi-resolution embedded Windows icon.

---

## Google Sheets Integration Code (google_script.js)

To receive data from cpuBeyond.exe directly in your Google Sheet:

1. Open your Google Sheet in the browser: https://docs.google.com/spreadsheets/d/1LMYkSPb72mEWYweISyUMWt0lCS5HTHZt_g8wId28ov4
2. Go to **Extensions > Apps Script**
3. Copy and paste the contents of [google_script.js](./google_script.js):

`javascript
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
    
    return ContentService.createTextOutput("OK");
  } catch (err) {
    return ContentService.createTextOutput("Error: " + err.toString());
  }
}
`

4. Click **Deploy > New deployment > Web App > Access: Anyone**.
5. Copy the generated Web App URL and paste it into cpuBeyond.exe when prompted (or save it in google_script_url.txt).

---

## Developed By
Developed by **Diego A**.
