//using System.Text;
//using System.Text.RegularExpressions;
//using UglyToad.PdfPig;
//using Formix.Infrastructure.Data.Entities;
//using Microsoft.EntityFrameworkCore;
//using AutoMapper;
//using Formix.Infrastructure.Data.Configurations;
//using Formix.Domain.Dtos;
//using System.Globalization;

//public class PdfEscrituracionService
//{
//    private readonly AppDbContext _context;
//    private readonly IMapper _mapper;

//    public PdfEscrituracionService(AppDbContext context, IMapper mapper)
//    {
//        _context = context;
//        _mapper = mapper;
//    }

//    public async Task<int> ProcesarPdfAsync(string pdfPath)
//    {
//        // Leer PDF completo
//        var texto = ExtraerTexto(pdfPath);

//        // ----------------------------
//        // 1. DATOS BÁSICOS ESCRITURACIÓN
//        // ----------------------------
//        var escr = new EscrituracionDto
//        {
//            FechaSolicitud = ParseFechaSpanish(ExtractField(texto, "Fecha Solicitud", "Plazo elaboración")),
//            PlazoElaboracion = ParseFechaSpanish(ExtractField(texto, "Plazo elaboración", "Modelo Escritura", "Revisor")),
//            ModeloEscritura = ExtractField(texto, "Modelo Escritura", "Revisor"),
//            Revisor = ExtractField(texto, "Revisor", "Digitador"),
//            Digitador = ExtractField(texto, "Digitador", "Flujo de Trabajo"),
//            FlujoTrabajo = ExtractField(texto, "Flujo de Trabajo", "Indice Radicado"),
//            IndiceRadicado = ExtractField(texto, "Indice Radicado", "Fecha programada"),
//            FechaFirma = ParseFechaSpanish(ExtractField(texto, "Fecha programada para Firma", "ACTOS Y CUANTIAS")),
//            Observaciones = ExtractField(texto, "Observaciones", "Firma del Funcionario")
//        };

//        //await _context.Escrituraciones.AddAsync(escr);
//        //await _context.SaveChangesAsync();

//        // ----------------------------
//        // 2. OTORGANTES
//        // ----------------------------
//        var otorgantesText = GetSection(texto, "OTORGANTES VINCULADOS", "DATOS BIENES INMUEBLES");
//        //var otorgantes = ExtractOtorgantes(texto);

//        var list = ParseOtorgantes(otorgantesText);

//        //await _context.Otorgantes.AddRangeAsync(otorgantes);

//        // ----------------------------
//        // 3. BIENES INMUEBLES
//        // ----------------------------
//        //var bienes = ExtraerBienes(texto, escr.Id);

//        //await _context.BienesInmuebles.AddRangeAsync(bienes);

//        // ----------------------------
//        // 4. DOCUMENTOS ADJUNTOS
//        // ----------------------------
//        //var documentos = ExtraerDocumentos(texto, escr.Id);

//        //await _context.DocumentosAdjuntos.AddRangeAsync(documentos);

//        // Guardar todo
//        //await _context.SaveChangesAsync();

//        return 1;
//    }

//    string GetSection(string text, string start, string end)
//    {
//        int i1 = text.IndexOf(start);
//        if (i1 == -1) return "";

//        int i2 = text.IndexOf(end, i1 + start.Length);
//        if (i2 == -1) return text.Substring(i1 + start.Length);

//        return text.Substring(i1 + start.Length, i2 - (i1 + start.Length));
//    }

//    public List<OtorganteDto> ParseOtorgantes(string otorgantesText)
//    {
//        var list = new List<OtorganteDto>();

//        var pattern = @"
//(?<nombre>[A-ZÁÉÍÓÚÑ ]+)\s*
//C\.C\. (?<cc>\d+)\s*
//(?<estado>[A-Za-z() ]+)\s*
//(?<direccion>.+?)\s*
//(?<correo>\S+@\S+)\s*
//(?<telefono>\d{7,}-\d{7,})\s*
//(?<calidad>Comprador|Vendedor)\s*
//\( CVAPTO 121\)\s*
//(?<part>[\d/NA]+)\s*
//(?<anio>[\d/NA]+)\s*
//(?<casa>[\dA-Za-z/NA]+)
//";

//        var norm = NormalizeOtorgantes(otorgantesText);

//        var matches = Regex.Matches(norm, pattern,
//            RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);

//        foreach (Match m in matches)
//        {
//            list.Add(new OtorganteDto
//            {
//                Nombre = m.Groups["nombre"].Value.Trim(),
//                Cedula = m.Groups["cc"].Value.Trim(),
//                EstadoCivil = m.Groups["estado"].Value.Trim(),
//                Direccion = m.Groups["direccion"].Value.Trim(),
//                Correo = m.Groups["correo"].Value.Trim(),
//                Telefonos = m.Groups["telefono"].Value.Trim(),
//                Calidad = m.Groups["calidad"].Value.Trim(),
//                Porcentaje = m.Groups["part"].Value.Trim(),
//                AnioAdquisicion = m.Groups["anio"].Value.Trim()
//                //CasaHab = m.Groups["casa"].Value.Trim()
//            });
//        }

//        return list;
//    }

//    string NormalizeOtorgantes(string text)
//    {
//        // Separar nombres (están todos en MAYÚSCULAS)
//        text = Regex.Replace(text, @"([A-ZÁÉÍÓÚÑ ]{3,})(?=C\.C\.)", "\n$1\n");

//        // Separar C.C.
//        text = Regex.Replace(text, @"(C\.C\. \d+)", "\n$1\n");

//        // Separar estado civil (termina en ')')
//        text = Regex.Replace(text, @"(\(.*?\))", "$1\n");

//        // Separar dirección (cuando aparece @ o números de teléfono)
//        text = Regex.Replace(text, @"(\S+@\S+)", "\n$1\n");

//        // Teléfonos
//        text = Regex.Replace(text, @"(\d{7,}-\d{7,})", "$1\n");

//        // Calidad (Comprador, Vendedor)
//        text = Regex.Replace(text, @"(Comprador|Vendedor)", "\n$1\n");

//        // Acto CVAPTO
//        text = Regex.Replace(text, @"(\( CVAPTO 121\))", "\n$1");

//        // Separar números después del acto
//        text = Regex.Replace(text, @"\)(\d+)", ")\n$1\n");

//        return text;
//    }


//    private string ExtraerTexto(string rutaPdf)
//    {
//        var sb = new StringBuilder();

//        using (var pdf = PdfDocument.Open(rutaPdf))
//        {
//            foreach (var page in pdf.GetPages())
//            {
//                sb.AppendLine(page.Text);
//            }
//        }

//        return sb.ToString();
//    }

//    private string ExtractField(string text, string fieldName, params string[] stopBefore)
//    {
//        // Construye el lookahead para detener la captura antes de los siguientes campos
//        var lookahead = stopBefore != null && stopBefore.Length > 0
//            ? "(?=(?:" + string.Join("|", stopBefore.Select(Regex.Escape)) + ")|$)"
//            : "(?=$)";

//        // Busca "Campo: valor" usando captura NO codiciosa
//        var pattern = $@"{Regex.Escape(fieldName)}\s*:?\s*(.*?){lookahead}";

//        var m = Regex.Match(text, pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
//        return m.Success ? m.Groups[1].Value.Trim() : string.Empty;
//    }

//    private DateTime ParseFecha(string input)
//    {
//        if (DateTime.TryParse(input.Replace(".", ""), out var fecha))
//            return fecha;

//        return DateTime.MinValue;
//    }

//    private DateTime ParseFechaSpanish(string input)
//    {
//        if (string.IsNullOrWhiteSpace(input))
//            return DateTime.MinValue;

//        string[] formatos = new[]
//        {
//        "dd/MMMM/yyyy hh:mm:ss tt",
//        "d/MMMM/yyyy hh:mm:ss tt",
//        "dd/MMMM/yyyy h:mm:ss tt",
//        "d/MMMM/yyyy h:mm:ss tt",
//        "dd/MMMM/yyyy",
//        "d/MMMM/yyyy"
//    };

//        DateTime fecha;

//        if (DateTime.TryParseExact(
//            input,
//            formatos,
//            new CultureInfo("es-CO"),
//            DateTimeStyles.AllowWhiteSpaces,
//            out fecha))
//        {
//            return fecha;
//        }

//        return DateTime.MinValue;
//    }


//    //// ==================================================
//    //// OTORGANTES
//    //// ==================================================
//    public List<OtorganteDto> ExtractOtorgantes(string text)
//    {
//        var otorgantes = new List<OtorganteDto>();

//        var blockPattern = @"Casa Hab(.*?)DATOS BIENES";
//        var blockMatch = Regex.Match(text, blockPattern, RegexOptions.Singleline | RegexOptions.IgnoreCase);
//        if (!blockMatch.Success)
//            return otorgantes;

//        var raw = blockMatch.Groups[1].Value;

//        var normalized = NormalizeOtorgantesBlock(raw);
//        var items = SplitOtorgantes(normalized);

//        foreach (var item in items)
//            otorgantes.Add(ParseOtorgante(item));

//        return otorgantes;
//    }

//    private List<string> SplitOtorgantes(string normalized)
//    {
//        var pattern = @"(?=^[A-ZÁÉÍÓÚÑ ]+\nC\.C\.)";
//        var list = Regex.Split(normalized, pattern, RegexOptions.Multiline)
//            .Where(x => x.Contains("C.C.") && x.Length > 20)
//            .ToList();

//        return list;
//    }


//    private OtorganteDto ParseOtorgante(string o)
//    {
//        return new OtorganteDto
//        {
//            Nombre = Regex.Match(o, @"^[A-ZÁÉÍÓÚÑ ]+").Value.Trim(),
//            Cedula = Regex.Match(o, @"C\.C\.\s*(\d+)").Groups[1].Value,
//            EstadoCivil = Regex.Match(o, @"(Soltero|Soltera|Casado|Casada)[^\n]+").Value.Trim(),
//            Direccion = Regex.Match(o, @"\n([A-Za-zÁÉÍÓÚÑ0-9#\-\.\s]+)\n[A-Za-z0-9._%+-]+@").Groups[1].Value.Trim(),
//            Correo = Regex.Match(o, @"([A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,})").Groups[1].Value,
//            Telefonos = Regex.Match(o, @"\n(\d{7,}[-]?\d*)\n").Groups[1].Value.Trim(),
//            Calidad = Regex.Match(o, @"(Comprador|Vendedor)").Value.Trim(),
//            Acto = Regex.Match(o, @"\(\s*([^)]+)\)").Groups[1].Value.Trim(),
//            Porcentaje = Regex.Match(o, @"\n(\d{1,3})\n").Groups[1].Value,
//            AnioAdquisicion = Regex.Match(o, @"\n\d{1,3}\n([A-Za-z0-9/]+)").Groups[1].Value
//        };
//    }

//    private string NormalizeOtorgantesBlock(string text)
//    {
//        var t = text;

//        // Correos partidos en 2 líneas
//        t = t.Replace("@\n", "@");

//        // Separar datos pegados
//        t = Regex.Replace(t, @"([a-zA-Z])(\d)", "$1\n$2"); // texto + número
//        t = Regex.Replace(t, @"(\d)([A-Za-z])", "$1\n$2"); // número + texto

//        // Cortes típicos del PDF
//        t = t.Replace("C.C.", "\nC.C.");
//        t = t.Replace("Comprador", "\nComprador");
//        t = t.Replace("Vendedor", "\nVendedor");

//        t = t.Replace("Soltero", "\nSoltero");
//        t = t.Replace("Soltera", "\nSoltera");
//        t = t.Replace("Casado", "\nCasado");
//        t = t.Replace("Casada", "\nCasada");

//        // Acto final
//        t = t.Replace(")", ")\n");

//        // Quitar dobles saltos
//        t = Regex.Replace(t, @"\n{2,}", "\n");

//        return t.Trim();
//    }

//    private string NormalizePdfText(string text)
//    {
//        var t = text;

//        // Unir correos separados
//        t = t.Replace("@\n", "@");

//        // Insertar salto antes de nombres de personas
//        t = Regex.Replace(t, @"(?<!\n)([A-ZÁÉÍÓÚÑ]{3,}\s+[A-ZÁÉÍÓÚÑ]{3,})", "\n$1");

//        // Separar números pegados a letras
//        t = Regex.Replace(t, @"([a-zA-Z])(\d)", "$1\n$2");
//        t = Regex.Replace(t, @"(\d)([A-Za-zÁÉÍÓÚÑ])", "$1\n$2");

//        // Normalizar etiquetas del PDF
//        t = t.Replace("C.C.", "\nC.C.");
//        t = t.Replace("Comprador", "\nComprador");
//        t = t.Replace("Vendedor", "\nVendedor");

//        t = t.Replace("Soltero", "\nSoltero");
//        t = t.Replace("Soltera", "\nSoltera");
//        t = t.Replace("Casado", "\nCasado");
//        t = t.Replace("Casada", "\nCasada");

//        // Acto
//        t = t.Replace(")", ")\n");

//        // % participación pegado a acto
//        t = Regex.Replace(t, @"\)(\d{1,3})", ")\n$1");

//        // Año adquisición pegado a porcentaje
//        t = Regex.Replace(t, @"(\d{1,3})(N/A|N0|\d{4})", "$1\n$2");

//        // Quitar saltos duplicados
//        t = Regex.Replace(t, @"\n{2,}", "\n");

//        return t.Trim();
//    }



//    //// ==================================================
//    //// BIENES
//    //// ==================================================
//    //private List<BienInmueble> ExtraerBienes(string texto, int escrId)
//    //{
//    //    var lista = new List<BienInmueble>();

//    //    var bloque = Regex.Split(texto, @"DATOS BIENES INMUEBLES")[1];
//    //    var bloqueDocs = Regex.Split(bloque, @"Documentos Entregados")[0];

//    //    var lineas = bloqueDocs.Split("\n")
//    //                           .Select(l => l.Trim())
//    //                           .Where(l => l.Contains("50"))
//    //                           .ToList();

//    //    foreach (var linea in lineas)
//    //    {
//    //        var partes = linea.Split(" ");

//    //        var bien = new BienInmueble
//    //        {
//    //            EscrituracionId = escrId,
//    //            Matricula = partes[0],
//    //            Municipio = partes[1] + " " + partes[2] + " " + partes[3],
//    //            Ubicacion = string.Join(" ", partes.Skip(4).Take(6)),
//    //            CedulaCatastral = partes[10],
//    //            ValorBien = decimal.Parse(partes.Last().Replace(".", "").Replace(",", ""))
//    //        };

//    //        lista.Add(bien);
//    //    }

//    //    return lista;
//    //}

//    //// ==================================================
//    //// DOCUMENTOS
//    //// ==================================================
//    //private List<DocumentoAdjunto> ExtraerDocumentos(string texto, int escrId)
//    //{
//    //    var lista = new List<DocumentoAdjunto>();

//    //    var entregados = RegexValue(texto, @"Documentos Entregados\s*(.*)")
//    //                        .Split(",")
//    //                        .Select(d => d.Trim());

//    //    foreach (var item in entregados)
//    //    {
//    //        lista.Add(new DocumentoAdjunto
//    //        {
//    //            EscrituracionId = escrId,
//    //            Tipo = "Entregado",
//    //            Descripcion = item
//    //        });
//    //    }

//    //    var pendientes = RegexValue(texto, @"Documentos Pendientes\s*(.*)")
//    //                        .Split(",")
//    //                        .Select(d => d.Trim());

//    //    foreach (var item in pendientes)
//    //    {
//    //        lista.Add(new DocumentoAdjunto
//    //        {
//    //            EscrituracionId = escrId,
//    //            Tipo = "Pendiente",
//    //            Descripcion = item
//    //        });
//    //    }

//    //    return lista;
//    //}
//}
