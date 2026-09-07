using Formix.Domain.Dtos.Mayasoft;
using Formix.Infrastructure.Data.Configurations;
using Formix.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Formix.Infrastructure.ExternalServices;

// Convierte un trámite de MayasoftAPI en el grafo normalizado de Formix:
// Radicados, RadicadosActos, RadicadosInmuebles, RadicadosOtorgantes, RadicadosOtorgantesTipos,
// RadicadosPagos, Inmuebles, InmublesTerceros y Terceros.
public class MayasoftRadicadoBuilder
{
    private readonly AppDbContext _db;
    private readonly MayasoftApiOptions _options;
    private readonly ILogger<MayasoftRadicadoBuilder> _logger;

    public MayasoftRadicadoBuilder(AppDbContext db, IOptions<MayasoftApiOptions> options, ILogger<MayasoftRadicadoBuilder> logger)
    {
        _db = db;
        _options = options.Value;
        _logger = logger;
    }

    // Crea el radicado completo para un trámite que aún no existe en Formix. Devuelve el IdRadicado generado.
    public async Task<int> CrearRadicadoAsync(TramiteMayasoftResponseDto dto, CancellationToken ct)
    {
        var proyecto = await ObtenerOCrearProyectoAsync(dto, ct);
        var ahora = DateTime.Now;

        // 1. Radicado principal
        var radicado = new Radicado
        {
            TenantId = _options.TenantId,
            ProyectoId = proyecto.ProyectoId,
            PlantillaId = null,
            UsuarioId = _options.UsuarioSistemaId,
            Consecutivo = Trunc(dto.CasoTramite?.NumeroDeCaso ?? $"MSF-{dto.Id}", 20),
            FechaRadicado = ahora,
            FechaOe = dto.CasoTramite?.FechaProyectadaProgramadaFinal,
            IdEstado = _options.IdEstadoRadicadoInicial
        };
        _db.Radicados.Add(radicado);
        await _db.SaveChangesAsync(ct); // Se guarda primero para obtener el IdRadicado que usan las tablas hijas

        // 2. Terceros (comprador titular + alternos) y su vínculo como otorgantes del radicado
        var terceros = await ObtenerOCrearTercerosAsync(dto, ct);
        var porcentaje = terceros.Count > 0 ? Math.Round(100m / terceros.Count, 2) : 100m;

        foreach (var (tercero, esTitular) in terceros)
        {
            var otorgante = new RadicadosOtorgante { IdRadicado = radicado.IdRadicado, IdTercero = tercero.IdTercero };
            _db.RadicadosOtorgantes.Add(otorgante);
            await _db.SaveChangesAsync(ct); // Necesario para obtener IdRadicadoOtorgante

            _db.RadicadosOtorgantesTipos.Add(new RadicadosOtorgantesTipo
            {
                IdRadicadoOtorgante = otorgante.IdRadicadoOtorgante,
                IdTipoOtorgante = esTitular ? _options.IdTipoOtorganteComprador : _options.IdTipoOtorganteCompradorAlterno, // Titular = Comprador Principal, alternos = Comprador Alterno
                ActoCodigo = _options.ActoCodigoCompraventa,
                Porcentaje = porcentaje,
                AnioAdquisicion = ahora.Year,
                CasaHabitacion = "SI",
                FechaCreacion = ahora
            });
        }

        // 3. Inmuebles (unidad principal + garajes + depósitos + depósitos útiles) y su vínculo con el radicado y los terceros
        var inmuebles = await ObtenerOCrearInmueblesAsync(dto, proyecto.ProyectoId, ct);
        var orden = 1;
        foreach (var inmueble in inmuebles)
        {
            _db.RadicadosInmuebles.Add(new RadicadosInmueble
            {
                IdRadicado = radicado.IdRadicado,
                IdInmueble = inmueble.InmuebleId,
                Orden = orden++
            });

            foreach (var (tercero, esTitular) in terceros)
            {
                var yaExiste = await _db.InmublesTerceros
                    .AnyAsync(x => x.IdTercero == tercero.IdTercero && x.IdInmueble == inmueble.InmuebleId, ct);
                if (!yaExiste)
                {
                    _db.InmublesTerceros.Add(new InmublesTercero
                    {
                        IdTercero = tercero.IdTercero,
                        IdInmueble = inmueble.InmuebleId,
                        IdTipoOtorgante = esTitular ? _options.IdTipoOtorganteComprador : _options.IdTipoOtorganteCompradorAlterno
                    });
                }
            }
        }

        // 4. Acto de compraventa
        var valorTotal = CalcularValorTotal(dto);
        _db.RadicadosActos.Add(new RadicadosActo
        {
            IdRadicado = radicado.IdRadicado,
            IdActo = _options.IdActoCompraventa,
            Cuantia = valorTotal,
            Avaluo = valorTotal,
            FechaAdquisicion = DateOnly.FromDateTime(ahora)
        });

        // 5. Pagos / cierre financiero
        var pago = new RadicadosPago { IdRadicado = radicado.IdRadicado };
        await AplicarPagosAsync(pago, dto, ct);
        _db.RadicadosPagos.Add(pago);

        await _db.SaveChangesAsync(ct);
        return radicado.IdRadicado;
    }

    // Actualiza un radicado ya creado: refresca el cierre financiero y los datos de contacto de los compradores
    public async Task ActualizarRadicadoAsync(int idRadicado, TramiteMayasoftResponseDto dto, CancellationToken ct)
    {
        var pago = await _db.RadicadosPagos.FirstOrDefaultAsync(p => p.IdRadicado == idRadicado, ct);
        if (pago is null)
        {
            pago = new RadicadosPago { IdRadicado = idRadicado };
            _db.RadicadosPagos.Add(pago);
        }
        await AplicarPagosAsync(pago, dto, ct);

        var radicado = await _db.Radicados.FirstOrDefaultAsync(r => r.IdRadicado == idRadicado, ct);
        if (radicado is not null)
            radicado.FechaOe = dto.CasoTramite?.FechaProyectadaProgramadaFinal ?? radicado.FechaOe;

        await ObtenerOCrearTercerosAsync(dto, ct); // Refresca correo/celular/estado civil de los compradores existentes

        await _db.SaveChangesAsync(ct);
    }

    // ---------- Proyecto ----------

    private async Task<Proyecto> ObtenerOCrearProyectoAsync(TramiteMayasoftResponseDto dto, CancellationToken ct)
    {
        var nombre = dto.Proyecto?.NombreDelProyecto?.Trim();
        if (string.IsNullOrWhiteSpace(nombre))
            throw new InvalidOperationException($"El trámite {dto.Id} no trae nombre de proyecto.");

        var proyecto = await _db.Proyectos
            .FirstOrDefaultAsync(p => p.TenantId == _options.TenantId && p.Nombre.ToLower() == nombre.ToLower(), ct);

        if (proyecto is not null) return proyecto;

        if (!_options.CrearProyectoSiNoExiste)
            throw new InvalidOperationException($"El proyecto '{nombre}' del trámite {dto.Id} no existe en Formix.");

        _logger.LogWarning("Proyecto '{Nombre}' no existe en Formix; se crea con datos mínimos (trámite {Id}).", nombre, dto.Id);
        proyecto = new Proyecto
        {
            TenantId = _options.TenantId,
            Nombre = nombre,
            Descripcion = dto.Macroproyecto?.NombreDelMacroproyecto ?? nombre,
            MunicipioCodigoDane = _options.MunicipioCodigoDaneDefault,
            Direccion = dto.Ciudad ?? string.Empty,
            MatriculaInmobiliaria = string.Empty
        };
        _db.Proyectos.Add(proyecto);
        await _db.SaveChangesAsync(ct);
        return proyecto;
    }

    // ---------- Terceros ----------

    // Devuelve cada tercero junto con un indicador de si es el comprador titular (true) o alterno (false)
    private async Task<List<(Tercero Tercero, bool EsTitular)>> ObtenerOCrearTercerosAsync(TramiteMayasoftResponseDto dto, CancellationToken ct)
    {
        var compradores = new List<(CompradorDto Comprador, bool EsTitular)>();
        if (dto.CompradorTitular is not null) compradores.Add((dto.CompradorTitular, true));
        if (dto.CompradoresAlternos is not null) compradores.AddRange(dto.CompradoresAlternos.Select(a => ((CompradorDto)a, false)));

        var resultado = new List<(Tercero Tercero, bool EsTitular)>();
        foreach (var (c, esTitular) in compradores)
        {
            if (string.IsNullOrWhiteSpace(c.Nombre) && string.IsNullOrWhiteSpace(c.NumeroDeIdentificacion)) continue;

            var documento = c.NumeroDeIdentificacion?.Trim();
            Tercero? tercero = null;

            if (!string.IsNullOrWhiteSpace(documento))
            {
                tercero = await _db.Terceros
                    .FirstOrDefaultAsync(t => t.TenantId == _options.TenantId && t.Documento == documento, ct);
            }

            var (nombres, apellidos) = SepararNombre(c.Nombre);

            if (tercero is null)
            {
                tercero = new Tercero
                {
                    TenantId = _options.TenantId,
                    NombreCompleto = Trunc(c.Nombre ?? documento ?? "SIN NOMBRE", 200)!,
                    Nombre = Trunc(nombres, 200),
                    Apellido = Trunc(apellidos, 200),
                    Documento = Trunc(documento, 20),
                    IdTipoDocumento = await BuscarTipoDocumentoAsync(c.TipoDeIdentificacion, ct),
                    Correo = Trunc(c.CorreoElectronico, 100),
                    Celular = Trunc(c.Celular, 100),
                    IdEstadoCivil = await BuscarEstadoCivilAsync(c.EstadoCivil, ct)
                };
                _db.Terceros.Add(tercero);
                await _db.SaveChangesAsync(ct);
            }
            else
            {
                // Ya existe: solo se refrescan los datos de contacto que pueden cambiar
                tercero.Correo = Trunc(c.CorreoElectronico, 100) ?? tercero.Correo;
                tercero.Celular = Trunc(c.Celular, 100) ?? tercero.Celular;
                tercero.IdEstadoCivil = await BuscarEstadoCivilAsync(c.EstadoCivil, ct) ?? tercero.IdEstadoCivil;
            }

            if (!resultado.Any(r => r.Tercero.IdTercero == tercero.IdTercero))
                resultado.Add((tercero, esTitular));
        }

        return resultado;
    }

    private async Task<int?> BuscarTipoDocumentoAsync(string? texto, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(texto)) return null;
        var t = texto.Trim().ToLower();
        var tipo = await _db.TiposDocumentos.FirstOrDefaultAsync(x =>
            x.Nombre.ToLower() == t ||
            (x.Sigla != null && x.Sigla.ToLower() == t) ||
            (x.Codigo != null && x.Codigo.ToLower() == t), ct);
        return tipo?.IdTipoDocumento;
    }

    private async Task<int?> BuscarEstadoCivilAsync(string? texto, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(texto)) return null;
        var t = texto.Trim().ToLower();
        var ec = await _db.TiposEstadoCivils.FirstOrDefaultAsync(x =>
            x.EstadoCivil.ToLower() == t ||
            (x.Codigo != null && x.Codigo.ToLower() == t), ct);
        return ec?.IdEstadoCivil;
    }

    // ---------- Inmuebles ----------

    private async Task<List<Inmueble>> ObtenerOCrearInmueblesAsync(TramiteMayasoftResponseDto dto, int proyectoId, CancellationToken ct)
    {
        // El orden define RadicadosInmuebles.Orden: 1 = unidad principal, luego garajes, depósitos y depósitos útiles
        var productos = new List<(ProductoDto Producto, string Tipo)>();
        if (dto.UnidadPrincipal is not null) productos.Add((dto.UnidadPrincipal, "Apartamento")); // Se refina por el nombre del producto (Casa, Local...)
        productos.AddRange((dto.Garajes ?? new()).Select(p => (p, "Garaje")));
        productos.AddRange((dto.Depositos ?? new()).Select(p => (p, "Deposito")));
        productos.AddRange((dto.DepositosUtiles ?? new()).Select(p => (p, "Deposito")));

        var resultado = new List<Inmueble>();
        foreach (var (p, tipoNombre) in productos)
        {
            var matricula = Trunc(p.Matricula, 30);
            Inmueble? inmueble = null;

            if (!string.IsNullOrWhiteSpace(matricula))
            {
                inmueble = await _db.Inmuebles
                    .FirstOrDefaultAsync(i => i.ProyectoId == proyectoId && i.MatriculaInmobiliaria == matricula, ct);
            }

            if (inmueble is null)
            {
                inmueble = new Inmueble
                {
                    TenantId = _options.TenantId,
                    ProyectoId = proyectoId,
                    Nombre = Trunc(p.Nombre ?? matricula ?? tipoNombre, 200)!,
                    Numero = Trunc(ExtraerNumero(p.Nombre), 20),
                    MatriculaInmobiliaria = matricula,
                    TipoInmuebleId = await BuscarTipoInmuebleAsync(p.Nombre, tipoNombre, ct),
                    ValorInmueble = p.Precio,
                    FechaCreacion = DateTime.Now
                };
                _db.Inmuebles.Add(inmueble);
                await _db.SaveChangesAsync(ct);
            }
            else
            {
                inmueble.ValorInmueble = p.Precio ?? inmueble.ValorInmueble;
                inmueble.FechaActualizacion = DateTime.Now;
            }

            if (!resultado.Any(r => r.InmuebleId == inmueble.InmuebleId))
                resultado.Add(inmueble);
        }

        return resultado;
    }

    // Primero intenta con la primera palabra del nombre del producto ("Casa 12" -> Casa, "Local 3" -> Local);
    // si no coincide con ningún TipoInmueble, usa el tipo por defecto de la categoría (Apartamento, Garaje, Deposito)
    private async Task<int?> BuscarTipoInmuebleAsync(string? nombreProducto, string tipoDefecto, CancellationToken ct)
    {
        var primeraPalabra = nombreProducto?.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(primeraPalabra))
        {
            var pp = QuitarTildes(primeraPalabra).ToLower();
            var porNombre = await _db.TipoInmuebles.FirstOrDefaultAsync(x => x.Nombre.ToLower() == pp, ct);
            if (porNombre is not null) return porNombre.TipoInmuebleId;
        }

        var td = tipoDefecto.ToLower();
        var tipo = await _db.TipoInmuebles.FirstOrDefaultAsync(x => x.Nombre.ToLower() == td, ct);
        return tipo?.TipoInmuebleId;
    }

    // "Depósito" -> "Deposito", para comparar contra los nombres sin tilde de TipoInmuebles
    private static string QuitarTildes(string texto)
    {
        var normalizado = texto.Normalize(System.Text.NormalizationForm.FormD);
        var sinMarcas = new string(normalizado.Where(c => System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) != System.Globalization.UnicodeCategory.NonSpacingMark).ToArray());
        return sinMarcas.Normalize(System.Text.NormalizationForm.FormC);
    }

    // ---------- Pagos ----------

    private async Task AplicarPagosAsync(RadicadosPago pago, TramiteMayasoftResponseDto dto, CancellationToken ct)
    {
        var valorTotal = CalcularValorTotal(dto);
        var credito = dto.CreditoTerceros ?? dto.LeasingHabitacional; // Son mutuamente excluyentes según la documentación
        var subsidioCaja = dto.Subsidios?.SubsidioCajaCompensacion;
        var subsidioHabitat = dto.Subsidios?.SubsidioHabitat;

        pago.ValorInmueble = valorTotal;
        pago.ValorVenta = valorTotal;
        pago.ValorEscritura = valorTotal;
        pago.CuotaInicial = dto.PlanDePago?.FirstOrDefault()?.CuotaInicial;

        pago.CreditoEntidad = Trunc(credito?.Entidad ?? dto.CasoTramite?.EntidadFinanciera?.Nombre, 100);
        pago.CreditoValor = credito?.Monto ?? dto.MontoDeCredito;
        pago.ValorCredito = pago.CreditoValor;

        if (dto.Cesantias is { Count: > 0 })
        {
            pago.CesantiasEntidad = Trunc(string.Join(", ", dto.Cesantias.Select(c => c.Entidad).Where(e => !string.IsNullOrWhiteSpace(e))), 100);
            pago.CesantiasValor = dto.Cesantias.Sum(c => c.Monto ?? 0);
        }

        pago.SubsidioEntidad = Trunc(subsidioCaja?.Entidad, 100);
        pago.SubsidioValor = subsidioCaja?.Monto;
        pago.SubsidioFchAsigna = subsidioCaja?.FechaAprobacionDeSubsidio is DateTime f ? DateOnly.FromDateTime(f) : null;
        pago.ValorSubsidioCc = subsidioCaja?.Monto ?? 0;
        pago.ValorSubsidioSh = subsidioHabitat?.Monto ?? 0;
        pago.IdCajaCompensacion = await BuscarCajaCompensacionAsync(subsidioCaja?.Entidad, ct);

        pago.AhorroEntidad = dto.RecursosPropios?.ValorPactado is not null ? "Recursos propios" : null;
        pago.AhorroValor = dto.RecursosPropios?.ValorPactado;
    }

    private async Task<int?> BuscarCajaCompensacionAsync(string? nombre, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(nombre)) return null;
        var n = nombre.Trim().ToLower();
        var caja = await _db.CajasCompensacions.FirstOrDefaultAsync(c =>
            c.Nombre.ToLower() == n || c.Nombre.ToLower().Contains(n) || n.Contains(c.Nombre.ToLower()), ct);
        return caja?.IdCajaCompensacion;
    }

    // ---------- Utilidades ----------

    // Valor total del negocio = suma del precio de todos los productos (unidad + garajes + depósitos)
    private static decimal CalcularValorTotal(TramiteMayasoftResponseDto dto)
    {
        var total = dto.UnidadPrincipal?.Precio ?? 0;
        total += (dto.Garajes ?? new()).Sum(p => p.Precio ?? 0);
        total += (dto.Depositos ?? new()).Sum(p => p.Precio ?? 0);
        total += (dto.DepositosUtiles ?? new()).Sum(p => p.Precio ?? 0);
        return total;
    }

    // Separa "Juan Carlos Pérez Gómez" en nombres y apellidos: 1 palabra = nombre; 2 = 1+1; 3 = 1+2; 4 o más = 2+resto
    private static (string? Nombres, string? Apellidos) SepararNombre(string? completo)
    {
        if (string.IsNullOrWhiteSpace(completo)) return (null, null);
        var partes = completo.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return partes.Length switch
        {
            1 => (partes[0], null),
            2 => (partes[0], partes[1]),
            3 => (partes[0], $"{partes[1]} {partes[2]}"),
            _ => (string.Join(' ', partes.Take(2)), string.Join(' ', partes.Skip(2)))
        };
    }

    // Extrae el primer bloque numérico del nombre del producto ("Apartamento 301" -> "301")
    private static string? ExtraerNumero(string? nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre)) return null;
        var digitos = new string(nombre.SkipWhile(c => !char.IsDigit(c)).TakeWhile(c => char.IsDigit(c)).ToArray());
        return string.IsNullOrEmpty(digitos) ? null : digitos;
    }

    // Recorta el texto al máximo permitido por la columna para evitar errores de truncamiento en SQL Server
    private static string? Trunc(string? valor, int max)
    {
        if (valor is null) return null;
        var v = valor.Trim();
        return v.Length <= max ? v : v[..max];
    }
}
