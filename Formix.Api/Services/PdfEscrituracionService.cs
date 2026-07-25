using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Formix.Infrastructure.Data.Entities;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using static System.Collections.Specialized.BitVector32;
using static System.Runtime.InteropServices.JavaScript.JSType;

public class Otorgante
{
    public string Nombre { get; set; }
    public string Documento { get; set; }
    public string TipoDocumento { get; set; }
    public string EstadoCivil { get; set; }
    public string Direccion { get; set; }
    public string Email { get; set; }
    public string Telefono { get; set; }
    public List<OtorganteCalidad> Calidades { get; set; } = new();
    //public string Porcentaje { get; set; }
    //public string AnioAdquisicion { get; set; }
    //public string CasaHabitacion { get; set; }
}

public class OtorganteCalidad
{
    public string Calidad { get; set; }          // Comprador, Vendedor, Constituyente
    public string ActoCodigo { get; set; }       // VIS, CONP, VENT
    public string ActoDescripcion { get; set; }

    public string Porcentaje { get; set; }       // 100%, 50%
    public string AnioAdquisicion { get; set; }  // 2021, N/A
    public string CasaHabitacion { get; set; }   // SI / NO / N/A
}


public class ActoCuantia
{
    public string Descripcion { get; set; }
    public string Cuantia { get; set; }
    public string Avaluo { get; set; }
    public string AnioAdquisicion { get; set; }
}

public class InmuebleSabana
{
    public string MatriculaInmobiliaria { get; set; }
    public string Municipio { get; set; }
    public string Ubicacion { get; set; }
    public string CedulaCatastral { get; set; }
    public decimal ValorBien { get; set; }
}

public class PdfEscrituracionService
{
    public async Task<(List<Otorgante>, List<ActoCuantia>, List<InmuebleSabana>)> ExtraerAsync(string pdfPath, Radicado radicado)
    {
        return await Task.Run(() =>
        {
            var otorgantes = new List<Otorgante>();
            var actos = new List<ActoCuantia>();
            var inmuebles = new List<InmuebleSabana>();

            using (var reader = new PdfReader(pdfPath))
            using (var pdfDoc = new PdfDocument(reader))
            {
                for (int page = 1; page <= pdfDoc.GetNumberOfPages(); page++)
                {
                    var strategy = new SimpleTextExtractionStrategy();
                    var text = PdfTextExtractor.GetTextFromPage(pdfDoc.GetPage(page), strategy);

                    if (page == 1)
                    {
                        string numeroRadicado = ProcesarSeccionRadicado(text);
                        if (string.IsNullOrEmpty(numeroRadicado) || radicado.Consecutivo.ToString() != numeroRadicado)
                        {
                            throw new Exception("El número del radicado no corresponde.");
                        }
                    }

                    if (text.Contains("ACTOS Y CUANTIAS"))
                    {
                        var actosExtraidos = ProcesarSeccionActosCuantias(text);
                        actos.AddRange(actosExtraidos);
                    }

                    if (text.Contains("OTORGANTES VINCULADOS"))
                    {
                        var otorgantesExtraidos = ProcesarSeccionOtorgantesCompleta(text);
                        otorgantes.AddRange(otorgantesExtraidos);
                    }

                    if (text.Contains("DATOS BIENES INMUEBLES"))
                    {
                        var inmueblesExtraidos = ProcesarSeccionInmuebles(text);
                        inmuebles.AddRange(inmueblesExtraidos);
                    }
                }
            }
            return (otorgantes, actos, inmuebles);
        });
    }

    public string ProcesarSeccionRadicado(string text)
    {
        // 1. Encontrar la sección "Indice Radicado"
        int inicio = text.IndexOf("Indice Radicado", StringComparison.OrdinalIgnoreCase);
        if (inicio == -1)
            return "";

        // 2. Buscar el siguiente título (para cortar el bloque)
        int fin = text.IndexOf("Fecha programada para Firma", inicio, StringComparison.OrdinalIgnoreCase);
        if (fin == -1)
            fin = text.Length;

        // 3. Extraer solo la sección
        string seccion = text.Substring(inicio, fin - inicio);

        // 4. Dividir en líneas
        var lineas = seccion.Split('\n', '\r', StringSplitOptions.RemoveEmptyEntries);

        // 5. Buscar la primera línea que tenga solo dígitos
        foreach (var linea in lineas)
        {
            var l = linea.Trim();
            if (Regex.IsMatch(l, @"^\d{6,}$")) // 6+ dígitos para evitar falsos positivos
                return l;
        }

        return "";
    }


    //    private List<ActoCuantia> ProcesarSeccionActosCuantias(string text)
    //    {
    //        var actos = new List<ActoCuantia>();

    //        int inicio = text.IndexOf("ACTOS Y CUANTIAS", StringComparison.OrdinalIgnoreCase);
    //        if (inicio == -1) return actos;

    //        int fin = text.IndexOf("OTORGANTES VINCULADOS", inicio, StringComparison.OrdinalIgnoreCase);
    //        if (fin == -1) fin = text.Length;

    //        string seccion = text.Substring(inicio, fin - inicio);

    //        // Quitamos encabezados
    //        var lineas = seccion.Split('\n', StringSplitOptions.RemoveEmptyEntries);

    //        foreach (var linea in lineas)
    //        {
    //            var l = linea.Trim();

    //            // Saltar encabezados
    //            if (l.StartsWith("Descripcion") || l.StartsWith("ACTOS"))
    //                continue;

    //            // Regex flexible por fila
    //            //var match = Regex.Match(l,
    //            //    @"^(?<desc>[A-Z\s]+(?:\([A-Z]+\))?)\s+" +
    //            //    @"(?<cuantia>\$[\d\.,]+|Sin Cuantia)\s+" +
    //            //    @"(?<avaluo>\$[\d\.,]+)\s*" +
    //            //    @"(?<fecha>\d{2}/\w+/\d{4})?");

    //            //        var match = Regex.Match(l,
    //            //@"^(?<desc>[A-Z\s]+(?:\([A-Z]+\))?)\s+" +
    //            //@"(?:(?:Exento\s*\()?(?<cuantia>\$[\d\.,]+)\)?|(?<cuantia>Sin\s+Cuantia))\s+" +
    //            //@"(?<avaluo>\$[\d\.,]+)\s*" +
    //            //@"(?<fecha>\d{2}/[A-Za-z]+/\d{4})?");

    //            var match = Regex.Match(l,
    //@"^(?<desc>.+?)\s+" +
    //@"(?:(?:Exento\s*\(\s*)?(?<cuantia>\$[\d\.,]+)\s*\)?|(?<cuantia>Sin\s+Cuantia))\s+" +
    //@"(?<avaluo>\$[\d\.,]+)\s*" +
    //@"(?<fecha>\d{2}/[A-Za-z]+/\d{4})?");

    //            if (match.Success)
    //            {
    //                actos.Add(new ActoCuantia
    //                {
    //                    Descripcion = match.Groups["desc"].Value.Trim(),
    //                    Cuantia = match.Groups["cuantia"].Value.Trim(),
    //                    Avaluo = match.Groups["avaluo"].Value.Trim(),
    //                    AnioAdquisicion = match.Groups["fecha"]?.Value.Trim()
    //                });
    //            }
    //        }

    //        return actos;
    //    }

    private List<ActoCuantia> ProcesarSeccionActosCuantias(string text)
    {
        var actos = new List<ActoCuantia>();

        int inicio = text.IndexOf("ACTOS Y CUANTIAS", StringComparison.OrdinalIgnoreCase);
        if (inicio == -1) return actos;

        int fin = text.IndexOf("OTORGANTES VINCULADOS", inicio, StringComparison.OrdinalIgnoreCase);
        if (fin == -1) fin = text.Length;

        string seccion = text.Substring(inicio, fin - inicio);

        var lineas = seccion.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        // 🔹 1. Reconstruir filas (clave para HIPOTECA partida)
        var filas = new List<string>();
        var buffer = new StringBuilder();

        foreach (var linea in lineas)
        {
            var l = linea.Trim();

            if (string.IsNullOrWhiteSpace(l)) continue;

            // Saltar encabezados
            if (l.StartsWith("Descripcion", StringComparison.OrdinalIgnoreCase) ||
                l.StartsWith("ACTOS", StringComparison.OrdinalIgnoreCase))
                continue;

            // 🔹 Caso: línea que empieza con $ => final de registro partido
            if (l.StartsWith("$"))
            {
                buffer.Append(" " + l);
                filas.Add(buffer.ToString().Trim());
                buffer.Clear();
                continue;
            }

            // 🔹 Si ya había texto acumulado, seguimos concatenando
            if (buffer.Length > 0)
                buffer.Append(" " + l);
            else
                buffer.Append(l);

            // 🔹 Si en esta misma línea ya hay valores ($ o Sin Cuantia), cerramos
            if (Regex.IsMatch(l, @"\$\d") || l.Contains("Sin Cuantia", StringComparison.OrdinalIgnoreCase))
            {
                filas.Add(buffer.ToString().Trim());
                buffer.Clear();
            }
        }

        // 🔹 2. Regex final (ya sobre filas completas)
        var regex = new Regex(
            @"^(?<desc>.+?)\s+" +
            @"(?:(?:Exento\s*\(\s*)?(?<cuantia>\$[\d\.,]+)\s*\)?|(?<cuantia>Sin\s+Cuantia))\s+" +
            @"(?<avaluo>\$[\d\.,]+)\s*" +
            @"(?<fecha>\d{2}/[A-Za-z]+/\d{4})?",
            RegexOptions.IgnoreCase);

        foreach (var l in filas)
        {
            var match = regex.Match(l);

            if (match.Success)
            {
                actos.Add(new ActoCuantia
                {
                    Descripcion = match.Groups["desc"].Value.Trim(),
                    Cuantia = match.Groups["cuantia"].Value.Trim(),
                    Avaluo = match.Groups["avaluo"].Value.Trim(),
                    AnioAdquisicion = match.Groups["fecha"]?.Value.Trim()
                });
            }
        }

        return actos;
    }


    private List<Otorgante> ProcesarSeccionOtorgantesCompleta(string text)
    {
        var otorgantes = new List<Otorgante>();
        var otorgante = new Otorgante();
        //var documento = "";
        // Encontrar la sección completa de OTORGANTES VINCULADOS
        int inicioSeccion = text.IndexOf("OTORGANTES VINCULADOS");
        if (inicioSeccion == -1) return otorgantes;

        // Buscar el fin de la sección
        int finSeccion = text.IndexOf("DATOS BIENES INMUEBLES", inicioSeccion);
        if (finSeccion == -1) finSeccion = text.Length;

        string seccionOtorgantesCompleta = text.Substring(inicioSeccion, finSeccion - inicioSeccion);

        //        var bloques = Regex.Matches(
        //    seccionOtorgantes,
        //    @"(?m)(^[A-ZÁÉÍÓÚÑ][A-ZÁÉÍÓÚÑ\s\.\-]{4,}(?:\n[A-ZÁÉÍÓÚÑ][A-ZÁÉÍÓÚÑ\s\.\-]{2,})*)([\s\S]*?)(?=\n[A-ZÁÉÍÓÚÑ]{5,}|\Z)"
        //);

        // Primero, reemplazar los \n literales por saltos de línea reales
        string seccionOtorgantes = seccionOtorgantesCompleta.Replace("\\n", "\n");

        // Dividir en líneas y procesar
        var lineasSeccionOtorgantes = seccionOtorgantes.Split('\n');





        List<string> nombresOtorgantes = new List<string>();

        // Patrón que busca nombres completos hasta encontrar marcadores
        //string patron = @"(?<=\n)(?!OTORGANTES|Otorgante|Vendedor|Interesado|Constituyente|Comprador|DEUDOR|ACREEDOR|N/A)([A-Z][^\n]+(?:\n(?!\b(?:NIT|C\.C\.|CL|KR|CALLE|recepcion)[^\n]*)[^\n]+)*)";
        string patron = @"
(?<=(?:\n|^))

(?!

    (?:
        OTORGANTES|
        Otorgantes|
        OTORGANTE\s*\d*|
        Otorgante\s*\d*|
        VENDEDOR|
        Vendedor|
        COMPRADOR|
        Comprador|
        INTERESADO|
        Interesado|
        ACREEDOR(?:\s*CH)?|
        Acreedor(?:\s*CH)?|
        DEUDOR(?:\s*CH)?|
        Deudor(?:\s*CH)?|
        CONSTITUYENTE|
        Constituyente|
        APODERADO|
        Apoderado|
        PODERDANTE|
        Poderdante|
        FIDEICOMITENTE|
        Fideicomitente|
        FIDEICOMISARIO|
        Fideicomisario|
        N/A
    )

    \s*(?:\n|$)

)

(
    [A-Z][A-Z\s\.ÑÁÉÍÓÚÜ\-\""]+
    (?:
        \s+
        (?:
            S\.A\.|
            S\.A\.S\.|
            COMO|
            DEL|
            AUTONOMO|
            \-|
            Y|
            [A-Z]+
        )
    )*
    (?:
        \n[A-Z][A-Z\s\.ÑÁÉÍÓÚÜ\-]+
    )*
)

(?=
    \s*\n\s*
    (?:
        NIT|
        NIIT|
        CC|
        N\.I\.T\.|
        C\.C\.|
        CL|
        KR|
        CALLE|
        recepcion|
        \d|
        \(
    )
)";
        var varmatch = Regex.Matches(
    seccionOtorgantes,
    patron,
    RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace
);
        foreach (Match match in varmatch)
        {
            if (match.Success)
            {
                string nombre = match.Value;

                // Dividir en líneas y procesar
                string[] lineas = nombre.Split('\n');
                List<string> partesNombre = new List<string>();

                foreach (string linea in lineas)
                {
                    string lineaTrim = linea.Trim();

                    // Si la línea contiene NIT, C.C., etc., detenerse
                    if (Regex.IsMatch(lineaTrim, @"^(NIT |NIT|NIIT|N\.I\.T\.|C\.C\.|CL |KR |CALLE |recepcion)"))
                    {
                        //// Extraer documento (C.C.)
                        //var docMatch = Regex.Match(seccionOtorgantes, @"C\.C\.\s*(\d[\d\.]*)");
                        //if (docMatch.Success)
                        //{
                        //    documento = $"C.C. {docMatch.Groups[1].Value}";
                        //}
                        break;
                    }


                    // Si es una línea válida del nombre
                    if (!string.IsNullOrWhiteSpace(lineaTrim) &&
                        !Regex.IsMatch(lineaTrim, @"^\(.*\)$") && // No paréntesis solos
                        !Regex.IsMatch(lineaTrim, @"^\d+$"))      // No solo números
                    {
                        partesNombre.Add(lineaTrim);
                    }
                }

                if (partesNombre.Count > 0)
                {
                    string nombreCompleto = string.Join(" ", partesNombre);
                    nombreCompleto = Regex.Replace(nombreCompleto, @"\s+", " ").Trim();

                    // Filtrar por longitud mínima y excluir ciertos patrones
                    if (nombreCompleto.Length >= 5 &&
                        !nombreCompleto.Contains("VINCULADOS") &&
                        !nombreCompleto.Contains("Datos de contacto"))
                    {
                        // Extraer documento (C.C.)
                        //var docMatch = Regex.Match(seccionOtorgantes, @"C\.C\.\s*(\d[\d\.]*)");
                        //if (docMatch.Success)
                        //{
                        //    otorgante.Documento = $"C.C. {docMatch.Groups[1].Value}";
                        //}
                        nombresOtorgantes.Add(nombreCompleto);
                        //otorgante.Nombre = nombreCompleto;

                        //var bloques = new List<string>();

                        //for (int i = 0; i < nombresOtorgantes.Count; i++)
                        //{
                        //    int inicio = nombresOtorgantes[i].Index;
                        //    int fin = (i + 1 < nombresOtorgantes.Count)
                        //        ? nombresOtorgantes[i + 1].Index
                        //        : seccionOtorgantes.Length;

                        //    bloques.Add(
                        //        seccionOtorgantes.Substring(inicio, fin - inicio).Trim()
                        //    );
                        //}

                        ////string seccion = "";
                        //int inicio = seccionOtorgantes.Replace().IndexOf(nombreCompleto);
                        ////if (inicio == -1) return string.Empty;

                        //int fin = seccionOtorgantes.Length;

                        //foreach (var nom in nombresOtorgantes)
                        //{
                        //    if (nom == nombreCompleto) continue;

                        //    int idx = seccionOtorgantes.IndexOf(nom, inicio + nombreCompleto.Length);
                        //    if (idx != -1 && idx < fin)
                        //        fin = idx;
                        //}

                        //var resultadoseccion = seccionOtorgantes.Substring(inicio, fin - inicio).Trim();
                        //otorgantes.Add(otorgante);
                    }
                }
            }
        }

        //        string nombrePlano =
        //    "FIDUCIARIA BOGOTA S.A. COMO";

        //        string patronNombre = Regex.Replace(
        //            nombrePlano.Trim(),
        //            @"\s+",
        //            @"\s*\n\s*"
        //        );

        //        var matchInicio = Regex.Match(
        //    seccionOtorgantesCompleta,
        //    patronNombre,
        //    RegexOptions.Multiline
        //);

        //        var matchSiguiente = Regex.Match(
        //    seccionOtorgantesCompleta.Substring(matchInicio.Index + matchInicio.Length),
        //    @"(?m)^[A-ZÁÉÍÓÚÑ][A-ZÁÉÍÓÚÑ\s\.]{4,}\.\s*$"
        //);

        //        int inicio = matchInicio.Index;
        //        int fin = matchSiguiente.Success
        //            ? inicio + matchInicio.Length + matchSiguiente.Index
        //            : seccionOtorgantesCompleta.Length;

        //        string bloqueOtorgante = seccionOtorgantesCompleta.Substring(inicio, fin - inicio).Trim();

        //        if (!matchInicio.Success)
        //            throw new Exception("No se encontró el otorgante");


        List<string> bloquesOtorgantes = new();

        for (int i = 0; i < nombresOtorgantes.Count; i++)
        {
            var nombreActual = ConstruirPatronNombreFlexible(nombresOtorgantes[i]);

            string patron1;

            if (i < nombresOtorgantes.Count - 1)
            {
                var siguienteNombre = ConstruirPatronNombreFlexible(nombresOtorgantes[i + 1]);
                patron1 = $"{nombreActual}[\\s\\S]*?(?={siguienteNombre})";
            }
            else
            {
                patron1 = $"{nombreActual}[\\s\\S]*$";
            }

            var match = Regex.Match(
                seccionOtorgantes,
                patron1,
                RegexOptions.Multiline
            );

            if (match.Success)
            {
                bloquesOtorgantes.Add(match.Value.Trim());
            }
        }



        //return otorgantes;
        // Patrón para capturar bloques de nombres
        //string patron = @"(?<=(?:\n|^))(?!(?:OTORGANTES VINCULADOS|Otorgante))([A-Z][A-Z\s\.ÑÁÉÍÓÚÜ]+(?:\s+(?:S\.A\.|S\.A\.S\.|COMO|DEL|AUTONOMO|\-|[A-Z]+))*(?:\n[A-Z][A-Z\s\.ÑÁÉÍÓÚÜ\-]+)*)";

        //string patron = @"(?<=\n)(?!OTORGANTES|Otorgante|Vendedor|Interesado|Constituyente|Comprador|DEUDOR|ACREEDOR)([A-Z][^\n]+(?:\n(?!\b(?:NIT|C\.C\.|CL|KR|CALLE|@)[^\n]*)[^\n]+)*)";

        //var matches = Regex.Matches(seccionOtorgantes, patron, RegexOptions.Multiline);

        //List<string> nombresExtraidos = new List<string>();

        //foreach (Match match in matches)
        //{
        //    if (match.Success)
        //    {
        //        string nombre = match.Value.Trim();

        //        // Solo tomar los que tienen contenido significativo (no solo una palabra)
        //        if (nombre.Length > 5 && !nombre.Contains("VINCULADOS") && !nombre.Contains("Otorgante"))
        //        {
        //            // Unir líneas y limpiar espacios
        //            nombre = Regex.Replace(nombre, @"\s*\n\s*", " ");
        //            nombre = Regex.Replace(nombre, @"\s+", " ");
        //            nombresExtraidos.Add(nombre);
        //        }
        //    }
        //}

        //// Mostrar resultados
        //foreach (var nombre in nombresExtraidos.Distinct())
        //{
        //    Console.WriteLine($"- {nombre}");
        //}







        //var bloquesOtorgantes = ExtraerBloquesOtorgantes(lineasSeccionOtorgantes, nombresOtorgantes);

        for (int i = 0; i < bloquesOtorgantes.Count; i++)
        {
            var otorgante1 = ProcesarBloqueOtorgante(bloquesOtorgantes[i].Split('\n', '\r', StringSplitOptions.RemoveEmptyEntries).ToList(), nombresOtorgantes[i]);
            if (otorgante1 != null)
            {
                otorgantes.Add(otorgante1);
            }
        }

        return otorgantes;
    }

    private string ConstruirPatronNombreFlexible(string nombrePlano)
    {
        var palabras = nombrePlano
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(Regex.Escape);

        // Permite espacios, tabs y saltos de línea entre palabras
        return string.Join(@"[\s\r\n]+", palabras);
    }


    private List<List<string>> ExtraerBloquesOtorgantes(string[] lineas, List<string> nombresOtorgantes)
    {
        var bloques = new List<List<string>>();
        var bloqueActual = new List<string>();
        bool enTablaOtorgantes = false;
        int contadorLineasVacias = 0;
        bool esNombre = false;

        foreach (var linea in lineas)
        {
            var lineaTrim = linea.Trim();

            // Iniciar cuando encontramos los encabezados de la tabla
            if (lineaTrim.Contains("Otorgante") && lineaTrim.Contains("Datos de contacto") &&
                lineaTrim.Contains("Calidad") && lineaTrim.Contains("% Part"))
            {
                enTablaOtorgantes = true;
                continue;
            }

            if (!enTablaOtorgantes) continue;

            // Detectar fin de sección
            if (lineaTrim.Contains("DATOS BIENES INMUEBLES") ||
                lineaTrim.Contains("Documentos Entregados"))
            {
                break;
            }

            // Si encontramos una línea vacía, incrementar contador
            if (string.IsNullOrEmpty(lineaTrim))
            {
                contadorLineasVacias++;
                if (contadorLineasVacias >= 2 && bloqueActual.Count > 0)
                {
                    bloques.Add(new List<string>(bloqueActual));
                    bloqueActual.Clear();
                    esNombre = false;
                    contadorLineasVacias = 0;
                }
                continue;
            }

            contadorLineasVacias = 0;

            // Detectar inicio de nuevo otorgante (nombre en mayúsculas)
            if (Regex.IsMatch(lineaTrim, @"^[A-ZÁÉÍÓÚÑ][A-ZÁÉÍÓÚÑ\s]+$") && !lineaTrim.Contains("OTORGANTES"))
            {
                var valido = EsNombreValido(lineaTrim);
                if (valido && bloqueActual.Count > 0)
                {
                    bloques.Add(new List<string>(bloqueActual));
                    bloqueActual.Clear();
                }
                if (valido)
                {
                    bloqueActual.Add(lineaTrim);
                }
            }
            else
            {
                bloqueActual.Add(lineaTrim);
            }
        }

        // Agregar el último bloque
        if (bloqueActual.Count > 0)
        {
            bloques.Add(bloqueActual);
        }

        return bloques;
    }

    private bool EsNombreValido(string linea)
    {
        // Condiciones básicas
        bool condicionesBasicas = Regex.IsMatch(linea, @"^[A-ZÁÉÍÓÚÑ\s]+$") &&
                                  !linea.Contains("OTORGANTES") &&
                                  !linea.Contains("C.C.") &&
                                  !Regex.IsMatch(linea, @"(Solter[oa]|Casad[oa])") &&
                                  !linea.Contains("@") &&
                                  !Regex.IsMatch(linea, @"\d");

        if (!condicionesBasicas) return false;

        // Contar palabras en la línea
        var palabras = linea.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        // Si tiene 1-4 palabras, probablemente es un nombre
        // Si tiene 0 o más de 4 palabras, probablemente no es un nombre
        return palabras.Length > 1;
    }

    private string ExtraerNombreOtorgante(List<string> bloque)
    {
        var nombreLineas = new List<string>();

        foreach (var linea in bloque)
        {
            var l = linea.Trim();

            if (string.IsNullOrWhiteSpace(l))
                continue;

            // 🔴 CORTE REAL: documento
            if (Regex.IsMatch(
                l,
                @"\b(C\.?\s*C\.?|C\s*C|CEDULA|CÉDULA|NIT)\b",
                RegexOptions.IgnoreCase))
            {
                break;
            }

            // ❌ Nunca parte del nombre
            if (
                l.Contains("@") ||                         // correo
                Regex.IsMatch(l, @"\d{7,}") ||              // teléfonos / números largos
                l.StartsWith("CL ") ||
                l.StartsWith("KR ") ||
                l.StartsWith("CALLE") ||
                l.StartsWith("CARRERA")
            )
            {
                continue;
            }

            // ✅ TODO lo demás es nombre
            nombreLineas.Add(l);
        }

        return Regex.Replace(
            string.Join(" ", nombreLineas),
            @"\s+",
            " "
        ).Trim();
    }


    private Otorgante ProcesarBloqueOtorgante(List<string> bloque, string nombreOtorgante)
    {
        //if (bloque.Count < 3) return null;

        var otorgante = new Otorgante();
        var textoCompleto = string.Join(" ", bloque);

        // Extraer nombre (primera línea del bloque)
        //otorgante.Nombre = bloque[0].Trim();
        otorgante.Nombre = nombreOtorgante;//ExtraerNombreOtorgante(bloque);


        // Extraer documento (C.C.)
        var docMatch = Regex.Match(
    textoCompleto,
    @"\b(C\.C\.|N\.I\.T\.|CC|NIT|NIIT)\s*(\d[\d\.]*)",
    RegexOptions.IgnoreCase
);

        if (docMatch.Success)
        {
            var tipo = docMatch.Groups[1].Value.ToUpper() == "NIIT" ? "NIT" : docMatch.Groups[1].Value;
            otorgante.Documento = $"{docMatch.Groups[2].Value.Replace(".", "")}";
            otorgante.TipoDocumento = $"{tipo.Replace(".", "")}";
        }

        // Extraer estado civil
        var estadoCivilMatch = Regex.Match(textoCompleto, @"(Solter[oa]|Casad[oa])\s*\([^)]*\)");
        if (estadoCivilMatch.Success)
        {
            otorgante.EstadoCivil = estadoCivilMatch.Value.Trim();
        }

        // Extraer dirección (primera línea después del nombre que no sea documento ni estado civil)
        foreach (var linea in bloque)
        {
            var l = linea.Trim();

            if (string.IsNullOrWhiteSpace(l))
                continue;

            // descartar documentos, estado civil, correos, roles, teléfonos
            if (Regex.IsMatch(l, @"\b(C\.C\.|NIT|NIIT)\b", RegexOptions.IgnoreCase)) continue;
            if (Regex.IsMatch(l, @"(Solter[oa]|Casad[oa])", RegexOptions.IgnoreCase)) continue;
            if (Regex.IsMatch(l, @"@")) continue;
            if (Regex.IsMatch(l, @"^\(?\s*(Otorgantes|
Otorgante\s*\d*|
Vendedor|
Comprador|
Interesado|
Acreedor(?:\s*CH)?|
Deudor(?:\s*CH)?|
Constituyente|
Apoderado|
Poderdante|
Fideicomitente|
Fideicomisario)\b", RegexOptions.IgnoreCase)) continue;
            if (Regex.IsMatch(l, @"^\(?\s*\w+\s+\(\s*[A-Z]+\s+\d+\s*\)", RegexOptions.IgnoreCase)) continue;
            if (Regex.IsMatch(l, @"^\d{7,}([-\s]\d+)*$")) continue;

            //        // 👉 ACEPTAR SOLO SI PARECE DIRECCIÓN
            //        if (Regex.IsMatch(l,
            //@"^(Cll|CL|CALLE|KR|CRA|CARRERA|AV|AVENIDA|DG|DIAGONAL|TRANSVERSAL|#)\b",
            //RegexOptions.IgnoreCase))
            //        {
            //            otorgante.Direccion = l;
            //            break;
            //        }

            // 👉 ACEPTAR SI PARECE DIRECCIÓN (FORMIX REAL)
            bool tienePrefijo = Regex.IsMatch(l,
                @"\b(CRA|KR|CARRERA|Cll|CL|CALLE|AV|AVENIDA|DG|DIAGONAL|TRANSVERSAL)\b",
                RegexOptions.IgnoreCase);

            bool tieneNumeros = Regex.IsMatch(l, @"\d");

            bool tieneFormatoHash = Regex.IsMatch(l,
                @"#\s*\d{1,4}\s*[-–]?\s*\d*",
                RegexOptions.IgnoreCase);

            bool formatoSoloNumerico = Regex.IsMatch(l,
                @"^\d{1,4}\s+\d{1,4}\s+\d{1,4}$"); // ej: 63 73 11

            if ((tienePrefijo && tieneNumeros) || tieneFormatoHash || formatoSoloNumerico)
            {
                otorgante.Direccion = l;
                break;
            }

        }


        // Extraer email
        var emailMatch = Regex.Match(textoCompleto, @"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}");
        if (emailMatch.Success)
        {
            otorgante.Email = emailMatch.Value;
        }

        var telefonos = Regex.Matches(
    textoCompleto,
    @"\b(3\d{2}\s?\d{7}|\d{7})\b"
);

        if (telefonos.Count > 0)
        {
            otorgante.Telefono = string.Join("-", telefonos.Select(t => t.Value));
        }

        //// Extraer calidad (Vendedor/Comprador)
        //if (textoCompleto.Contains("Vendedor"))
        //{
        //    otorgante.Calidad = "Vendedor";
        //}
        //else if (textoCompleto.Contains("Comprador"))
        //{
        //    otorgante.Calidad = "Comprador";
        //}

        var calidadMatches = Regex.Matches(
    textoCompleto,
    @"(?<calidad>
        Otorgante\s*\d*|
        Vendedor|
        Comprador|
        Interesado|
        Acreedor(?:\s*CH)?|
        Deudor(?:\s*CH)?|
        Constituyente|
        Apoderado|
        Poderdante|
        Fideicomitente|
        Fideicomisario
    )
    \s*
    (?<acto>\(\s*[A-Z0-9]{3,10}\s+\d+\s*\))",
    RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace
);


        string patronDatos = @"(?<porcentaje>\d+(?:\.\d+)?|N\/A)
                       \s*
                       (?<anio>\d{4}|N\/A)
                       \s*
                       (?<casa>SI|NO|N\/A)";

        foreach (Match m in calidadMatches)
        {
            var calidad = CultureInfo.CurrentCulture.TextInfo
                .ToTitleCase(m.Groups["calidad"].Value.ToLower());

            var actoCodigo = m.Groups["acto"].Value;

            // Buscar los datos DESPUÉS de esta calidad
            var datosMatch = Regex.Match(
                textoCompleto.Substring(m.Index + m.Length),
                patronDatos,
                RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace
            );

            string porcentaje = null;
            string anio = null;
            string casa = null;

            if (datosMatch.Success)
            {
                porcentaje = datosMatch.Groups["porcentaje"].Value != "N/A"
                    ? datosMatch.Groups["porcentaje"].Value + "%"
                    : "N/A";

                anio = datosMatch.Groups["anio"].Value;
                casa = datosMatch.Groups["casa"].Value;
            }

            otorgante.Calidades.Add(new OtorganteCalidad
            {
                Calidad = calidad,
                ActoCodigo = actoCodigo,
                ActoDescripcion = null,
                Porcentaje = porcentaje,
                AnioAdquisicion = anio,
                CasaHabitacion = casa
            });
        }


        //// Extraer los 3 campos de la tabla en una sola operación
        //var tablaMatch = Regex.Match(textoCompleto, @"(\d{1,3}(?:\.\d{1,2})?)\s+(\d{4}|N/A)\s+(SI|NO|N0|N/A)");
        //if (tablaMatch.Success)
        //{
        //    otorgante.Porcentaje = tablaMatch.Groups[1].Value + "%";
        //    otorgante.AnioAdquisicion = tablaMatch.Groups[2].Value;
        //    otorgante.CasaHabitacion = tablaMatch.Groups[3].Value;
        //}
        //else
        //{
        //    // Fallback si el patrón completo falla
        //    otorgante.Porcentaje = "";
        //    otorgante.AnioAdquisicion = "";
        //    otorgante.CasaHabitacion = "";
        //}

        return otorgante;
    }

    // Método para convertir a JSON
    public string ConvertirOtorgantesAJson(List<Otorgante> otorgantes)
    {
        var jsonBuilder = new StringBuilder();
        jsonBuilder.Append("[");

        for (int i = 0; i < otorgantes.Count; i++)
        {
            var o = otorgantes[i];
            jsonBuilder.Append("{");
            jsonBuilder.Append($"\"nombre\": \"{EscapeJson(o.Nombre)}\", ");
            jsonBuilder.Append($"\"documento\": \"{EscapeJson(o.Documento)}\", ");
            jsonBuilder.Append($"\"estadoCivil\": \"{EscapeJson(o.EstadoCivil)}\", ");
            jsonBuilder.Append($"\"direccion\": \"{EscapeJson(o.Direccion)}\", ");
            jsonBuilder.Append($"\"email\": \"{EscapeJson(o.Email)}\", ");
            jsonBuilder.Append($"\"telefono\": \"{EscapeJson(o.Telefono)}\", ");
            //jsonBuilder.Append($"\"calidad\": \"{EscapeJson(o.Calidades)}\", ");
            //jsonBuilder.Append($"\"porcentaje\": \"{EscapeJson(o.Porcentaje)}\", ");
            //jsonBuilder.Append($"\"anioAdquisicion\": \"{EscapeJson(o.AnioAdquisicion)}\", ");
            //jsonBuilder.Append($"\"casaHabitacion\": \"{EscapeJson(o.CasaHabitacion)}\"");
            jsonBuilder.Append("}");

            if (i < otorgantes.Count - 1)
            {
                jsonBuilder.Append(", ");
            }
        }

        jsonBuilder.Append("]");
        return jsonBuilder.ToString();
    }

    private List<InmuebleSabana> ProcesarSeccionInmuebles(string text)
    {
        var inmuebles = new List<InmuebleSabana>();

        // Encontrar la sección completa de DATOS BIENES INMUEBLES
        int inicioSeccion = text.IndexOf("DATOS BIENES INMUEBLES");
        if (inicioSeccion == -1) return inmuebles;

        // Buscar el fin de la sección
        int finSeccion = text.IndexOf("Documentos Entregados", inicioSeccion);
        int finSeccion2 = text.IndexOf("Documentos Pendientes", inicioSeccion);
        int finSeccion3 = text.IndexOf("Observaciones", inicioSeccion);
        int finSeccion4 = text.IndexOf("Firma del funcionario", inicioSeccion);
        int finSeccion5 = text.IndexOf(" Firma del cliente", inicioSeccion);



        if (finSeccion == -1 && finSeccion2 > -1)
        {
            finSeccion = finSeccion2;
        }
        else if (finSeccion == -1 && finSeccion2 == -1 && finSeccion3 > -1)
        {
            finSeccion = finSeccion3;
        }
        else if (finSeccion == -1 && finSeccion2 == -1 && finSeccion3 == -1 && finSeccion4 > -1)
        {
            finSeccion = finSeccion4;
        }
        else if (finSeccion == -1 && finSeccion2 == -1 && finSeccion3 == -1 && finSeccion4 == -1 && finSeccion5 > -1)
        {
            finSeccion = finSeccion5;
        }
        else if (finSeccion == -1)
        {
            finSeccion = text.Length;
        }

        string seccionInmuebles = text.Substring(inicioSeccion, finSeccion - inicioSeccion);

        // Dividir en líneas y procesar
        var lineas = seccionInmuebles.Split('\n');
        var bloquesInmuebles = ExtraerBloquesInmuebles(lineas);

        foreach (var bloque in bloquesInmuebles)
        {
            var inmueble = ProcesarBloqueInmueble(bloque);
            if (inmueble != null)
            {
                inmuebles.Add(inmueble);
            }
        }

        return inmuebles;
    }

    private List<List<string>> ExtraerBloquesInmuebles(string[] lineas)
    {
        var bloques = new List<List<string>>();
        var bloqueActual = new List<string>();
        bool enTablaInmuebles = false;
        int contadorLineasVacias = 0;

        for (int i = 2; i < lineas.Length; i++)
        {
            var lineaTrim = lineas[i].Trim();

            // Iniciar cuando encontramos los encabezados de la tabla
            if (!lineaTrim.Contains("DATOS BIENES INMUEBLES") && !lineaTrim.Contains("Matricula Inmobiliaria")
                && !lineaTrim.Contains("Municipio") && !lineaTrim.Contains("Ubicación")
                && !lineaTrim.Contains("Cédula Catastral")
                 && !lineaTrim.Contains("Valor Bien"))
            {
                enTablaInmuebles = true;
            }

            if (!enTablaInmuebles) continue;

            // Detectar fin de sección
            if (string.IsNullOrEmpty(lineaTrim)
                || lineaTrim.Contains("Documentos Entregados")
                || lineaTrim.Contains("Documentos Pendientes")
                || lineaTrim.Contains("Observaciones")
                || lineaTrim.Contains("Firma del funcionario")
                || lineaTrim.Contains("Firma del cliente"))
            {
                if (bloqueActual.Count > 0)
                {
                    bloques.Add(new List<string>(bloqueActual));
                    bloqueActual.Clear();
                }
                continue;
            }

            // Detectar nueva fila de inmueble (empieza con formato de matrícula)
            //if (Regex.IsMatch(lineaTrim, @"^\d{2,3}[A-Z]-\d{5,}") && bloqueActual.Count > 0)
            //if (Regex.IsMatch(lineaTrim, @"^\d{2,3}[A-Z]-\d+") && bloqueActual.Count > 0)
            if (Regex.IsMatch(lineaTrim, @"^(?:\d{2,3}[A-Z]-\d+|\d{2,3}-\d+)") && bloqueActual.Count > 0)
            {
                bloques.Add(new List<string>(bloqueActual));
                bloqueActual.Clear();
            }

            bloqueActual.Add(lineaTrim);
        }

        // Agregar el último bloque
        if (bloqueActual.Count > 0)
        {
            bloques.Add(bloqueActual);
        }

        return bloques;
    }

    private InmuebleSabana ProcesarBloqueInmueble(List<string> bloque)
    {
        if (bloque.Count == 0) return null;

        var inmueble = new InmuebleSabana();
        var textoCompleto = string.Join(" ", bloque);

        Console.WriteLine($"=== PROCESANDO BLOQUE: '{textoCompleto}' ===");

        // 1. EXTRAER MATRÍCULA (primero)
        var matriculaMatch = Regex.Match(textoCompleto, @"^([\dA-Z]+-[\dA-Z]+)");
        if (matriculaMatch.Success)
        {
            inmueble.MatriculaInmobiliaria = matriculaMatch.Groups[1].Value;
            Console.WriteLine($"Matrícula: {inmueble.MatriculaInmobiliaria}");

            // Remover matrícula del texto
            textoCompleto = textoCompleto.Substring(matriculaMatch.Length).Trim();
        }

        // 2. EXTRAER MUNICIPIO (entre paréntesis)
        var municipioMatch = Regex.Match(textoCompleto, @"(\([^)]+\))");
        if (municipioMatch.Success)
        {
            inmueble.Municipio = municipioMatch.Groups[1].Value;
            Console.WriteLine($"Municipio: {inmueble.Municipio}");

            // Remover municipio del texto
            textoCompleto = textoCompleto.Replace(municipioMatch.Groups[1].Value, "").Trim();
        }

        // 3. EXTRAER CÉDULA CATASTRAL
        var cedulaMatch = Regex.Match(textoCompleto, @"(\d{3}-\d{4}-\d{4}-\d{5}-\d)");
        if (cedulaMatch.Success)
        {
            inmueble.CedulaCatastral = cedulaMatch.Groups[1].Value;
            Console.WriteLine($"Cédula Catastral: {inmueble.CedulaCatastral}");

            // Remover cédula del texto
            textoCompleto = textoCompleto.Replace(cedulaMatch.Groups[1].Value, "").Trim();
        }

        // 4. EXTRAER VALOR BIEN (último número)
        var valorMatch = Regex.Matches(textoCompleto, @"\d{1,3}(?:\.\d{3})*(?:,\d{2})?");
        if (valorMatch.Count <= 0)
        {
            // Buscar solo "0" al final
            valorMatch = Regex.Matches(textoCompleto, @"(\d+)$");
        }

        if (valorMatch.Count > 0)
        {
            //inmueble.ValorBien = valorMatch.Groups[1].Value;
            var valorTexto = valorMatch[valorMatch.Count - 1].Value;//valorMatch.Groups[0].Value;
            if (decimal.TryParse(valorTexto, NumberStyles.Any, new CultureInfo("es-CO"), out decimal valorDecimal))
            {
                inmueble.ValorBien = valorDecimal;
            }
            Console.WriteLine($"Valor Bien: {inmueble.ValorBien}");

            // Remover valor del texto
            textoCompleto = textoCompleto.Replace(valorTexto, "").Trim();
        }

        // 5. LO QUE QUEDA ES LA UBICACIÓN
        textoCompleto = Regex.Replace(textoCompleto, @"\s+", " ").Trim();
        inmueble.Ubicacion = textoCompleto;

        Console.WriteLine($"Ubicación final: '{inmueble.Ubicacion}'");
        Console.WriteLine("=== FIN PROCESAMIENTO ===\n");

        return inmueble;
    }

    //private InmuebleSabana ProcesarBloqueInmueble(List<string> bloque)
    //{
    //    if (bloque.Count == 0) return null;

    //    var inmueble = new InmuebleSabana();
    //    var textoCompleto = string.Join(" ", bloque);

    //    //Extraer matrícula inmobiliaria(primer elemento, formato: 505 - 40798982)
    //    var matriculaMatch = Regex.Match(textoCompleto, @"([\dA-Z]+-[\dA-Z]+)");
    //    if (matriculaMatch.Success)
    //    {
    //        inmueble.MatriculaInmobiliaria = matriculaMatch.Value;
    //    }

    //    // Extraer municipio (generalmente entre paréntesis)
    //    var municipioMatch = Regex.Match(textoCompleto, @"\(([^)]+)\)");
    //    if (municipioMatch.Success)
    //    {
    //        inmueble.Municipio = municipioMatch.Value;
    //    }

    //    // Extraer ubicación (buscar patrones de direcciones)
    //    inmueble.Ubicacion = ExtraerUbicacion(textoCompleto);

    //    // Extraer cédula catastral (formato: 000-0000-0000-00055-0)
    //    var cedulaMatch = Regex.Match(textoCompleto, @"(\d{3}-\d{4}-\d{4}-\d{5}-\d)");
    //    if (cedulaMatch.Success)
    //    {
    //        inmueble.CedulaCatastral = cedulaMatch.Groups[1].Value;
    //    }

    //    // Extraer valor del bien (formato: 150.000.000)
    //    var valorMatch = Regex.Match(textoCompleto, @"(\d{1,3}(?:\.\d{3})*(?:\.\d+)?)");
    //    if (valorMatch.Success)
    //    {
    //        // Tomar el último número grande encontrado (asumiendo que es el valor)
    //        var todosValores = Regex.Matches(textoCompleto, @"\b\d{1,3}(?:\.\d{3})+\b");
    //        if (todosValores.Count > 0)
    //        {
    //            inmueble.ValorBien = todosValores[todosValores.Count - 1].Value;
    //        }
    //    }

    //    return inmueble;
    //}

    private string ExtraerUbicacion(string linea)
    {
        // Para líneas como: "TRANSVERSAL 96 NO. 93-07 SUR 000-0000-0000-00055-0 150.000.000"

        // Primero, remover la cédula catastral si existe
        var cedulaMatch = Regex.Match(linea, @"\d{3}-\d{4}-\d{4}-\d{5}-\d");
        if (cedulaMatch.Success)
        {
            linea = linea.Replace(cedulaMatch.Value, "").Trim();
        }

        // Luego, remover el valor del bien si existe
        var valorMatch = Regex.Match(linea, @"\b\d{1,3}(?:\.\d{3})+\b");
        if (valorMatch.Success)
        {
            linea = linea.Replace(valorMatch.Value, "").Trim();
        }

        // Limpiar espacios múltiples
        linea = Regex.Replace(linea, @"\s+", " ").Trim();

        // Verificar que lo que queda es una dirección válida
        if ((linea.Contains("TRANSVERSAL") || linea.Contains("KR") || linea.Contains("CL") ||
             linea.Contains("CARRERA") || linea.Contains("CALLE") || linea.Contains("SUR") ||
             linea.Contains("NO.") || linea.Contains("NUMERO")) &&
            !Regex.IsMatch(linea, @"^\d+-\d+") &&
            !linea.Contains("(DISTRITO CAPITAL"))
        {
            return linea;
        }

        return "";
    }

    private string EscapeJson(string value)
    {
        if (string.IsNullOrEmpty(value)) return "";
        return value.Replace("\"", "\\\"").Replace("\n", " ").Replace("\r", " ").Trim();
    }
}