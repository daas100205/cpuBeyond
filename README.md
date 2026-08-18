# CPU Beyond v1.0 - Inspector de Hardware

**CPU Beyond** es una aplicación ejecutable independiente para Windows desarrollada en C# por **Diego A**. Permite inspeccionar, mostrar y exportar especificaciones detalladas de hardware y sistema (Procesador CPU, Memoria RAM, Tarjeta de Video GPU, Discos de Almacenamiento, Tarjeta Madre, Batería, Sistema Operativo y Red) con monitoreo en tiempo real e integración directa a Google Sheets y Excel.

---

## Características Principales

- **Inspección Exhaustiva de Hardware**:
  - **Procesador (CPU)**: Modelo, núcleos físicos, hilos lógicos, velocidad en MHz, cachés L2/L3, virtualización y uso de CPU % en tiempo real.
  - **Memoria (RAM)**: Capacidad total, usada, libre y detalle por ranura (Capacidad, MHz, Fabricante, Número de Parte).
  - **Tarjeta Gráfica (GPU)**: GPUs dedicadas e integradas, VRAM, versión de controlador y resolución de pantalla.
  - **Almacenamiento (Discos)**: Unidades físicas y volúmenes, con la unidad principal de sistema (C:\) priorizada al inicio.
  - **Tarjeta Madre / BIOS**: Fabricante, modelo, versión, número de serie y fecha de BIOS.
  - **Batería (Laptops)**: Estado de carga, porcentaje y tiempo estimado restante.
  - **Sistema Operativo y Red**: Edición de Windows, versión de compilación, tiempo de actividad, IP y direcciones MAC activas.

- **Nombre de Equipo Personalizable**: Asigna un identificador personalizado a cualquier equipo (ej. Laptop-Diego, PC-Recepcion) antes de exportar.
- **Integración Directa a Google Sheets**: Envía los datos directamente a tu hoja de Google Sheets con un solo clic.
- **Exportación Directa a Excel**: Guarda o anexa especificaciones en un archivo maestro de inventario .csv compatible con Microsoft Excel.
- **Ejecutable Portátil**: Binario independiente (cpuBeyond.exe) con icono multirresolución incrustado.

---

## Código de Vinculación para Google Sheets (google_script.js)

Para recibir los datos desde cpuBeyond.exe directamente en tu hoja de Google Sheets:

1. Abre tu hoja de Google Sheets en el navegador: https://docs.google.com/spreadsheets/d/1LMYkSPb72mEWYweISyUMWt0lCS5HTHZt_g8wId28ov4
2. Ve al menú superior: **Extensiones > Apps Script**
3. Copia y pega el contenido del archivo google_script.js:

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

4. Haz clic en **Implementar > Nueva implementación > Aplicación web > Acceso: Cualquier persona**.
5. Copia la URL del Web App generada y pégala en cpuBeyond.exe cuando la solicite (o guárdala en google_script_url.txt).

---

## Compilar el Código Fuente

Para compilar el ejecutable desde el código fuente C#:
1. Abre CMD o PowerShell en la carpeta del proyecto.
2. Ejecuta el script de compilación:
`cmd
.\build.bat
`

---

## Desarrollado Por
Desarrollado por **Diego A**.
