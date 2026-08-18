/**
 * CPU Beyond - Google Apps Script Integration
 * Target Google Sheet: https://docs.google.com/spreadsheets/d/1LMYkSPb72mEWYweISyUMWt0lCS5HTHZt_g8wId28ov4
 *
 * Instructions:
 * 1. Open your Google Sheet in the browser
 * 2. Go to Extensions > Apps Script
 * 3. Replace all existing code with this script
 * 4. Click Deploy > New deployment > Web App > Access: Anyone
 * 5. Copy the generated Web App URL and paste it into cpuBeyond.exe (or save in google_script_url.txt).
 */

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
