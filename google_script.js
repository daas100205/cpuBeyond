/**
 * CPU Beyond - Integración con Google Apps Script
 * Hoja de destino: https://docs.google.com/spreadsheets/d/1LMYkSPb72mEWYweISyUMWt0lCS5HTHZt_g8wId28ov4
 *
 * Instrucciones:
 * 1. Abre tu hoja de Google Sheets en el navegador.
 * 2. Ve al menú superior: Extensiones > Apps Script.
 * 3. Reemplaza todo el código por este script.
 * 4. Haz clic en Implementar > Nueva implementación > Aplicación web > Acceso: Cualquier persona.
 * 5. Copia la URL del Web App generada y pégala en cpuBeyond.exe (o guárdala en google_script_url.txt).
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
