using System;
using System.Collections.Generic;
using Formix.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Formix.Infrastructure.Data.Configurations;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Acto> Actos { get; set; }

    public virtual DbSet<CajasCompensacion> CajasCompensacions { get; set; }

    public virtual DbSet<Departamento> Departamentos { get; set; }

    public virtual DbSet<InmublesTercero> InmublesTerceros { get; set; }

    public virtual DbSet<Inmueble> Inmuebles { get; set; }

    public virtual DbSet<ListaInmueblesByRadicado> ListaInmueblesByRadicados { get; set; }

    public virtual DbSet<ListaOrdenEscrituracion> ListaOrdenEscrituracions { get; set; }

    public virtual DbSet<ListaOtorgante> ListaOtorgantes { get; set; }

    public virtual DbSet<ListaRadicado> ListaRadicados { get; set; }

    public virtual DbSet<ListaRadicadosInmueble> ListaRadicadosInmuebles { get; set; }

    public virtual DbSet<ListaRadicadosOtorgante> ListaRadicadosOtorgantes { get; set; }

    public virtual DbSet<ListaTercero> ListaTerceros { get; set; }

    public virtual DbSet<MinCategoriaVariable> MinCategoriaVariables { get; set; }

    public virtual DbSet<MinVariable> MinVariables { get; set; }

    public virtual DbSet<Municipio> Municipios { get; set; }

    public virtual DbSet<Plane> Planes { get; set; }

    public virtual DbSet<PlantillasBak20260128> PlantillasBak20260128s { get; set; }

    public virtual DbSet<Proyecto> Proyectos { get; set; }

    public virtual DbSet<ProyectoPlantilla> ProyectoPlantillas { get; set; }

    public virtual DbSet<ProyectoPlantillaBak20260128> ProyectoPlantillaBak20260128s { get; set; }

    public virtual DbSet<Radicado> Radicados { get; set; }

    public virtual DbSet<RadicadosActo> RadicadosActos { get; set; }

    public virtual DbSet<RadicadosInmueble> RadicadosInmuebles { get; set; }

    public virtual DbSet<RadicadosOtorgante> RadicadosOtorgantes { get; set; }

    public virtual DbSet<RadicadosOtorgantesTipo> RadicadosOtorgantesTipos { get; set; }

    public virtual DbSet<RadicadosPago> RadicadosPagos { get; set; }

    public virtual DbSet<RegexPattern> RegexPatterns { get; set; }

    public virtual DbSet<Suscripcione> Suscripciones { get; set; }

    public virtual DbSet<SysAuditoriaLogin> SysAuditoriaLogins { get; set; }

    public virtual DbSet<SysConfiguracionSistema> SysConfiguracionSistemas { get; set; }

    public virtual DbSet<SysPermiso> SysPermisos { get; set; }

    public virtual DbSet<SysRolPermiso> SysRolPermisos { get; set; }

    public virtual DbSet<SysRole> SysRoles { get; set; }

    public virtual DbSet<SysUsuario> SysUsuarios { get; set; }

    public virtual DbSet<SysUsuarioRole> SysUsuarioRoles { get; set; }

    public virtual DbSet<TawkWebhookLog> TawkWebhookLogs { get; set; }

    public virtual DbSet<Tenant> Tenants { get; set; }

    public virtual DbSet<Tercero> Terceros { get; set; }

    public virtual DbSet<TipoInmueble> TipoInmuebles { get; set; }

    public virtual DbSet<TiposDocumento> TiposDocumentos { get; set; }

    public virtual DbSet<TiposEstadoCivil> TiposEstadoCivils { get; set; }

    public virtual DbSet<TiposEstadoRadicado> TiposEstadoRadicados { get; set; }

    public virtual DbSet<TiposInmuebleHomologación> TiposInmuebleHomologacións { get; set; }

    public virtual DbSet<TiposOtorgante> TiposOtorgantes { get; set; }

    public virtual DbSet<TramiteMayasoft> TramitesMayasoft { get; set; } // Tabla ancha de trámites extraídos de MayasoftAPI

    public virtual DbSet<VDatosMinutum> VDatosMinuta { get; set; }

    public virtual DbSet<VInmueblesMinutum> VInmueblesMinuta { get; set; }

    public virtual DbSet<VTercerosMinutum> VTercerosMinuta { get; set; }

//    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
//        => optionsBuilder.UseSqlServer("Server=Qa-formix.novatechh.com.co,15831;Database=FormixDB_DEV;User Id=usr_Formixdb;Password=1qa2ws3eDDD*;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Acto>(entity =>
        {
            entity.HasKey(e => e.IdActo).HasFillFactor(90);

            entity.Property(e => e.Abreviatura)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<CajasCompensacion>(entity =>
        {
            entity.HasKey(e => e.IdCajaCompensacion);

            entity.ToTable("CajasCompensacion");

            entity.Property(e => e.Celular)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Codigo)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Correo)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Direccion)
                .HasMaxLength(300)
                .IsUnicode(false);
            entity.Property(e => e.Documento)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Nombre)
                .HasMaxLength(200)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Departamento>(entity =>
        {
            entity.HasKey(e => e.CodigoDane)
                .HasName("PK__Departam__C2853731A86D0ECA")
                .HasFillFactor(90);

            entity.Property(e => e.CodigoDane)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("CodigoDANE");
            entity.Property(e => e.CodigoIso)
                .HasMaxLength(6)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("CodigoISO");
            entity.Property(e => e.NombreCorto)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.NombreDepartamento)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<InmublesTercero>(entity =>
        {
            entity.HasKey(e => new { e.IdTercero, e.IdInmueble }).HasFillFactor(90);

            entity.HasOne(d => d.IdInmuebleNavigation).WithMany(p => p.InmublesTerceros)
                .HasForeignKey(d => d.IdInmueble)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_InmublesTerceros_Inmuebles");

            entity.HasOne(d => d.IdTerceroNavigation).WithMany(p => p.InmublesTerceros)
                .HasForeignKey(d => d.IdTercero)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_InmublesTerceros_Terceros");

            entity.HasOne(d => d.IdTipoOtorganteNavigation).WithMany(p => p.InmublesTerceros)
                .HasForeignKey(d => d.IdTipoOtorgante)
                .HasConstraintName("FK_InmublesTerceros_TiposOtorgantes");
        });

        modelBuilder.Entity<Inmueble>(entity =>
        {
            entity.HasKey(e => e.InmuebleId).HasFillFactor(90);

            entity.Property(e => e.AhorroEntidad)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.AhorroValor).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.CedulaCatastral)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.CesantiasEntidad)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.CesantiasValor).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.ChipCatastral)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Coeficiente).HasColumnType("decimal(12, 6)");
            entity.Property(e => e.CreditoEntidad)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.CreditoValor).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.Direccion)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.FechaActualizacion).HasColumnType("datetime");
            entity.Property(e => e.FechaCreacion).HasColumnType("datetime");
            entity.Property(e => e.LinderoEspecial).IsUnicode(false);
            entity.Property(e => e.MatriculaInmobiliaria)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.NombreCtl)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("NombreCTL");
            entity.Property(e => e.NombreRph)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("NombreRPH");
            entity.Property(e => e.Numero)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.SubsidioEntidad)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.SubsidioValor).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.Unidad)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.ValorInmueble).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Proyecto).WithMany(p => p.Inmuebles)
                .HasForeignKey(d => d.ProyectoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Inmuebles_Proyectos");

            entity.HasOne(d => d.Tenant).WithMany(p => p.Inmuebles)
                .HasForeignKey(d => d.TenantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Inmuebles_Tenants");

            entity.HasOne(d => d.TipoInmueble).WithMany(p => p.Inmuebles)
                .HasForeignKey(d => d.TipoInmuebleId)
                .HasConstraintName("FK_Inmuebles_TipoInmuebles");
        });

        modelBuilder.Entity<ListaInmueblesByRadicado>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("ListaInmueblesByRadicado");

            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<ListaOrdenEscrituracion>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("ListaOrdenEscrituracion");

            entity.Property(e => e.FechaRadicado).HasColumnType("datetime");
            entity.Property(e => e.Proyecto)
                .HasMaxLength(200)
                .IsUnicode(false);
        });

        modelBuilder.Entity<ListaOtorgante>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("ListaOtorgantes");

            entity.Property(e => e.Apellido)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Celular)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Correo)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Documento)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.NombreCompleto)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.NumeroDocumento)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.TipoOtorgante)
                .HasMaxLength(200)
                .IsUnicode(false);
        });

        modelBuilder.Entity<ListaRadicado>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("ListaRadicados");

            entity.Property(e => e.FechaRadicado).HasColumnType("datetime");
            entity.Property(e => e.Proyecto)
                .HasMaxLength(200)
                .IsUnicode(false);
        });

        modelBuilder.Entity<ListaRadicadosInmueble>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("ListaRadicadosInmuebles");

            entity.Property(e => e.AhorroEntidad)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.AhorroValor).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.CedulaCatastral)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.CesantiasEntidad)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.CesantiasValor).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.ChipCatastral)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Coeficiente).HasColumnType("decimal(12, 6)");
            entity.Property(e => e.CreditoEntidad)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.CreditoValor).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.Direccion)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.FechaActualizacion).HasColumnType("datetime");
            entity.Property(e => e.FechaCreacion).HasColumnType("datetime");
            entity.Property(e => e.FechaRadicado).HasColumnType("datetime");
            entity.Property(e => e.LinderoEspecial).IsUnicode(false);
            entity.Property(e => e.MatriculaInmobiliaria)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.NombreCtl)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("NombreCTL");
            entity.Property(e => e.NombreRph)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("NombreRPH");
            entity.Property(e => e.Numero)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.SubsidioEntidad)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.SubsidioValor).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.TipoInmuebleNombre)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Unidad)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.ValorInmueble).HasColumnType("decimal(18, 2)");
        });

        modelBuilder.Entity<ListaRadicadosOtorgante>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("ListaRadicadosOtorgantes");

            entity.Property(e => e.Apellido)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Celular)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Correo)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Documento)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.NombreCompleto)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.NumeroDocumento)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.TipoOtorgante)
                .HasMaxLength(200)
                .IsUnicode(false);
        });

        modelBuilder.Entity<ListaTercero>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("ListaTerceros");

            entity.Property(e => e.Apellido)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.Celular)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Correo)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Documento)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Nombre)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.NombreCompleto)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.NumeroDocumento)
                .HasMaxLength(20)
                .IsUnicode(false);
        });

        modelBuilder.Entity<MinCategoriaVariable>(entity =>
        {
            entity.HasKey(e => e.CategoriaVariableId).HasFillFactor(90);

            entity.ToTable("minCategoriaVariable");

            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.Categoria)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<MinVariable>(entity =>
        {
            entity.HasKey(e => e.VariableId).HasFillFactor(90);

            entity.ToTable("minVariables", tb => tb.HasTrigger("trg_UpdateCodigoMinVariables"));

            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.Codigo)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Variable)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.CategoriaVariable).WithMany(p => p.MinVariables)
                .HasForeignKey(d => d.CategoriaVariableId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_minVariables_minCategoriaVariable");
        });

        modelBuilder.Entity<Municipio>(entity =>
        {
            entity.HasKey(e => e.CodigoDane)
                .HasName("PK__Municipi__C28537313865F7F0")
                .HasFillFactor(90);

            entity.HasIndex(e => e.CodigoDepartamento, "IX_Municipios_CodigoDepartamento").HasFillFactor(90);

            entity.HasIndex(e => e.NombreMunicipio, "IX_Municipios_Nombre").HasFillFactor(90);

            entity.Property(e => e.CodigoDane)
                .HasMaxLength(5)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("CodigoDANE");
            entity.Property(e => e.CodigoDepartamento)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.NombreMunicipio)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.TipoMunicipio)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.CodigoDepartamentoNavigation).WithMany(p => p.Municipios)
                .HasForeignKey(d => d.CodigoDepartamento)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Municipio_Departamento");
        });

        modelBuilder.Entity<Plane>(entity =>
        {
            entity.HasKey(e => e.PlanId)
                .HasName("PK__Planes__755C22B756ACD740")
                .HasFillFactor(90);

            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.Descripcion)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.NombrePlan)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PrecioMensual).HasColumnType("decimal(10, 2)");
        });

        modelBuilder.Entity<PlantillasBak20260128>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("Plantillas_BAK_20260128");

            entity.Property(e => e.Archivo)
                .HasMaxLength(400)
                .IsUnicode(false);
            entity.Property(e => e.IdPlantilla).ValueGeneratedOnAdd();
            entity.Property(e => e.Nombre)
                .HasMaxLength(200)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Proyecto>(entity =>
        {
            entity.HasKey(e => e.ProyectoId).HasFillFactor(90);

            entity.Property(e => e.Descripcion)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.Direccion)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.LinderoGeneral).IsUnicode(false);
            entity.Property(e => e.MatriculaInmobiliaria)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.MunicipioCodigoDane)
                .HasMaxLength(5)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("MunicipioCodigoDANE");
            entity.Property(e => e.Nombre)
                .HasMaxLength(200)
                .IsUnicode(false);

            entity.HasOne(d => d.MunicipioCodigoDaneNavigation).WithMany(p => p.Proyectos)
                .HasForeignKey(d => d.MunicipioCodigoDane)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Proyectos_Municipios");

            entity.HasOne(d => d.Tenant).WithMany(p => p.Proyectos)
                .HasForeignKey(d => d.TenantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Proyectos_Tenants");
        });

        modelBuilder.Entity<ProyectoPlantilla>(entity =>
        {
            entity.HasKey(e => e.PlantillaId)
                .HasName("PK_ProyectoPlantilla_1")
                .HasFillFactor(90);

            entity.ToTable("ProyectoPlantilla");

            entity.HasIndex(e => e.ProyectoId, "IX_ProyectoPlantilla_ProyectoId").HasFillFactor(90);

            entity.HasIndex(e => new { e.ProyectoId, e.Nombre }, "IX_ProyectoPlantilla_ProyectoId_Nombre").HasFillFactor(90);

            entity.Property(e => e.Archivo).HasMaxLength(500);
            entity.Property(e => e.Estado).HasDefaultValue(1);
            entity.Property(e => e.Nombre).HasMaxLength(200);

            entity.HasOne(d => d.Proyecto).WithMany(p => p.ProyectoPlantillas)
                .HasForeignKey(d => d.ProyectoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProyectoPlantilla_Proyecto");
        });

        modelBuilder.Entity<ProyectoPlantillaBak20260128>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("ProyectoPlantilla_BAK_20260128");

            entity.Property(e => e.Archivo)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.Nombre)
                .HasMaxLength(200)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Radicado>(entity =>
        {
            entity.HasKey(e => e.IdRadicado).HasFillFactor(90);

            entity.Property(e => e.FechaOe)
                .HasColumnType("datetime")
                .HasColumnName("FechaOE");
            entity.Property(e => e.FechaRadicado).HasColumnType("datetime");

            entity.HasOne(d => d.IdEstadoNavigation).WithMany(p => p.Radicados)
                .HasForeignKey(d => d.IdEstado)
                .HasConstraintName("FK_Radicados_TiposEstadoRadicado");

            entity.HasOne(d => d.Plantilla).WithMany(p => p.Radicados)
                .HasForeignKey(d => d.PlantillaId)
                .HasConstraintName("FK_Radicados_ProyectoPlantilla");

            entity.HasOne(d => d.Proyecto).WithMany(p => p.Radicados)
                .HasForeignKey(d => d.ProyectoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Radicados_Proyectos");
        });

        modelBuilder.Entity<RadicadosActo>(entity =>
        {
            entity.HasKey(e => new { e.IdRadicado, e.IdActo }).HasFillFactor(90);

            entity.Property(e => e.Avaluo).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Cuantia).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.IdActoNavigation).WithMany(p => p.RadicadosActos)
                .HasForeignKey(d => d.IdActo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RadicadosActos_Actos");

            entity.HasOne(d => d.IdRadicadoNavigation).WithMany(p => p.RadicadosActos)
                .HasForeignKey(d => d.IdRadicado)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RadicadosActos_Radicados");
        });

        modelBuilder.Entity<RadicadosInmueble>(entity =>
        {
            entity.HasKey(e => new { e.IdRadicado, e.IdInmueble }).HasFillFactor(90);

            entity.HasOne(d => d.IdInmuebleNavigation).WithMany(p => p.RadicadosInmuebles)
                .HasForeignKey(d => d.IdInmueble)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RadicadosInmuebles_Inmuebles");

            entity.HasOne(d => d.IdRadicadoNavigation).WithMany(p => p.RadicadosInmuebles)
                .HasForeignKey(d => d.IdRadicado)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RadicadosInmuebles_Radicados");
        });

        modelBuilder.Entity<RadicadosOtorgante>(entity =>
        {
            entity.HasKey(e => e.IdRadicadoOtorgante)
                .HasName("PK_TerceroTiposTercero")
                .HasFillFactor(90);

            entity.HasIndex(e => new { e.IdRadicado, e.IdTercero }, "UX_RadicadoOtorgante")
                .IsUnique()
                .HasFillFactor(90);

            entity.HasOne(d => d.IdRadicadoNavigation).WithMany(p => p.RadicadosOtorgantes)
                .HasForeignKey(d => d.IdRadicado)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RadicadosOtorgantes_Radicados");

            entity.HasOne(d => d.IdTerceroNavigation).WithMany(p => p.RadicadosOtorgantes)
                .HasForeignKey(d => d.IdTercero)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RadicadosOtorgantes_Terceros");
        });

        modelBuilder.Entity<RadicadosOtorgantesTipo>(entity =>
        {
            entity.HasKey(e => e.IdRadicadoOtorganteTipo).HasFillFactor(90);

            entity.HasIndex(e => new { e.IdRadicadoOtorgante, e.IdTipoOtorgante }, "UX_RadicadoOtorganteTipo")
                .IsUnique()
                .HasFillFactor(90);

            entity.Property(e => e.ActoCodigo)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.CasaHabitacion)
                .HasMaxLength(5)
                .IsUnicode(false);
            entity.Property(e => e.FechaCreacion)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Porcentaje).HasColumnType("decimal(5, 2)");

            entity.HasOne(d => d.IdRadicadoOtorganteNavigation).WithMany(p => p.RadicadosOtorgantesTipos)
                .HasForeignKey(d => d.IdRadicadoOtorgante)
                .HasConstraintName("FK_ROT_RadicadosOtorgantes");

            entity.HasOne(d => d.IdTipoOtorganteNavigation).WithMany(p => p.RadicadosOtorgantesTipos)
                .HasForeignKey(d => d.IdTipoOtorgante)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ROT_TiposOtorgantes");
        });

        modelBuilder.Entity<RadicadosPago>(entity =>
        {
            entity.HasKey(e => e.IdRadicadoPagos);

            entity.Property(e => e.AhorroEntidad)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.AhorroValor).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.CesantiasEntidad)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.CesantiasValor).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.CreditoEntidad)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.CreditoValor).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.CuotaInicial).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.SubsidioEntidad)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.SubsidioValor).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.ValorAnticipoSubsudio).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.ValorCredito).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.ValorEscritura).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.ValorInmueble).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.ValorSubsidioCc)
                .HasColumnType("decimal(18, 4)")
                .HasColumnName("ValorSubsidioCC");
            entity.Property(e => e.ValorSubsidioIndexacion).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.ValorSubsidioSh)
                .HasColumnType("decimal(18, 4)")
                .HasColumnName("ValorSubsidioSH");
            entity.Property(e => e.ValorVenta).HasColumnType("decimal(18, 4)");

            entity.HasOne(d => d.IdCajaCompensacionNavigation).WithMany(p => p.RadicadosPagos)
                .HasForeignKey(d => d.IdCajaCompensacion)
                .HasConstraintName("FK_RadicadosPagos_CajasCompensacion");

            entity.HasOne(d => d.IdRadicadoNavigation).WithMany(p => p.RadicadosPagos)
                .HasForeignKey(d => d.IdRadicado)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Radicados_RadicadosPagos");
        });

        modelBuilder.Entity<RegexPattern>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__RegexPat__3214EC0779A6ACEA");

            entity.Property(e => e.Tipo)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Suscripcione>(entity =>
        {
            entity.HasKey(e => e.SuscripcionId)
                .HasName("PK__Suscripc__814D76AB8085129A")
                .HasFillFactor(90);

            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Activa");
            entity.Property(e => e.FechaFin).HasColumnType("datetime");
            entity.Property(e => e.FechaInicio).HasColumnType("datetime");
            entity.Property(e => e.FechaProximoPago).HasColumnType("datetime");

            entity.HasOne(d => d.Plan).WithMany(p => p.Suscripciones)
                .HasForeignKey(d => d.PlanId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Suscripci__PlanI__403A8C7D");

            entity.HasOne(d => d.Tenant).WithMany(p => p.Suscripciones)
                .HasForeignKey(d => d.TenantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Suscripci__Tenan__3F466844");
        });

        modelBuilder.Entity<SysAuditoriaLogin>(entity =>
        {
            entity.HasKey(e => e.AuditoriaId)
                .HasName("PK__Sys_Audi__095694C31CF6B77C")
                .HasFillFactor(90);

            entity.ToTable("Sys_AuditoriaLogin");

            entity.HasIndex(e => e.FechaHora, "IX_AuditoriaLogin_FechaHora").HasFillFactor(90);

            entity.HasIndex(e => e.UsuarioId, "IX_AuditoriaLogin_UsuarioId").HasFillFactor(90);

            entity.Property(e => e.DireccionIp)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("DireccionIP");
            entity.Property(e => e.Dispositivo)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.FechaHora)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.MensajeError)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.Navegador)
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.HasOne(d => d.Tenant).WithMany(p => p.SysAuditoriaLogins)
                .HasForeignKey(d => d.TenantId)
                .HasConstraintName("FK__Sys_Audit__Tenan__5DCAEF64");

            entity.HasOne(d => d.Usuario).WithMany(p => p.SysAuditoriaLogins)
                .HasForeignKey(d => d.UsuarioId)
                .HasConstraintName("FK__Sys_Audit__Usuar__5CD6CB2B");
        });

        modelBuilder.Entity<SysConfiguracionSistema>(entity =>
        {
            entity.HasKey(e => e.ConfiguracionId)
                .HasName("PK__Sys_Conf__9B95E03679271229")
                .HasFillFactor(90);

            entity.ToTable("Sys_ConfiguracionSistema");

            entity.HasIndex(e => e.Clave, "UQ__Sys_Conf__E8181E1132486539")
                .IsUnique()
                .HasFillFactor(90);

            entity.Property(e => e.Clave)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Descripcion)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.EsSensible).HasDefaultValue(false);
            entity.Property(e => e.Valor).IsUnicode(false);
        });

        modelBuilder.Entity<SysPermiso>(entity =>
        {
            entity.HasKey(e => e.PermisoId)
                .HasName("PK__Sys_Perm__96E0C7234DD603EF")
                .HasFillFactor(90);

            entity.ToTable("Sys_Permisos");

            entity.HasIndex(e => e.Codigo, "UQ__Sys_Perm__06370DACA16554AA")
                .IsUnique()
                .HasFillFactor(90);

            entity.Property(e => e.Codigo)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Descripcion)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<SysRolPermiso>(entity =>
        {
            entity.HasKey(e => e.RolPermisoId)
                .HasName("PK__Sys_RolP__A80C547423E7E107")
                .HasFillFactor(90);

            entity.ToTable("Sys_RolPermisos");

            entity.HasIndex(e => new { e.RolId, e.PermisoId }, "UQ_RolPermiso")
                .IsUnique()
                .HasFillFactor(90);

            entity.HasOne(d => d.Permiso).WithMany(p => p.SysRolPermisos)
                .HasForeignKey(d => d.PermisoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Sys_RolPe__Permi__59FA5E80");

            entity.HasOne(d => d.Rol).WithMany(p => p.SysRolPermisos)
                .HasForeignKey(d => d.RolId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Sys_RolPe__RolId__59063A47");
        });

        modelBuilder.Entity<SysRole>(entity =>
        {
            entity.HasKey(e => e.RolId)
                .HasName("PK__Sys_Role__F92302F1DC6B185F")
                .HasFillFactor(90);

            entity.ToTable("Sys_Roles");

            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.Descripcion)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.EsAdministrador).HasDefaultValue(false);
            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.Tenant).WithMany(p => p.SysRoles)
                .HasForeignKey(d => d.TenantId)
                .HasConstraintName("FK__Sys_Roles__Tenan__44FF419A");
        });

        modelBuilder.Entity<SysUsuario>(entity =>
        {
            entity.HasKey(e => e.UsuarioId)
                .HasName("PK__Sys_Usua__2B3DE7B8096B8488")
                .HasFillFactor(90);

            entity.ToTable("Sys_Usuarios");

            entity.HasIndex(e => e.NombreUsuario, "IX_Usuarios_NombreUsuario").HasFillFactor(90);

            entity.HasIndex(e => e.TenantId, "IX_Usuarios_TenantId").HasFillFactor(90);

            entity.HasIndex(e => new { e.TenantId, e.NombreUsuario }, "UQ_UsuarioPorTenant")
                .IsUnique()
                .HasFillFactor(90);

            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.Bloqueado).HasDefaultValue(false);
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.FechaActualizacion).HasColumnType("datetime");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IntentosLogin).HasDefaultValue(0);
            entity.Property(e => e.NombreCompleto)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.NombreUsuario)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Telefono)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.UltimoLogin).HasColumnType("datetime");

            entity.HasOne(d => d.Tenant).WithMany(p => p.SysUsuarios)
                .HasForeignKey(d => d.TenantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Sys_Usuar__Tenan__49C3F6B7");
        });

        modelBuilder.Entity<SysUsuarioRole>(entity =>
        {
            entity.HasKey(e => e.UsuarioRolId)
                .HasName("PK__Sys_Usua__C869CDCAE24D9987")
                .HasFillFactor(90);

            entity.ToTable("Sys_UsuarioRoles");

            entity.Property(e => e.FechaAsignacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Rol).WithMany(p => p.SysUsuarioRoles)
                .HasForeignKey(d => d.RolId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Sys_Usuar__RolId__5165187F");

            entity.HasOne(d => d.Usuario).WithMany(p => p.SysUsuarioRoles)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Sys_Usuar__Usuar__5070F446");
        });

        modelBuilder.Entity<Tenant>(entity =>
        {
            entity.HasKey(e => e.TenantId)
                .HasName("PK__Tenants__2E9B47E1BC06D491")
                .HasFillFactor(90);

            entity.HasIndex(e => e.CodigoNotaria, "IX_Tenants_CodigoNotaria").HasFillFactor(90);

            entity.HasIndex(e => e.CodigoNotaria, "UQ__Tenants__8628CFE5E08DD969")
                .IsUnique()
                .HasFillFactor(90);

            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.Consecutivo).HasDefaultValue(false);
            entity.Property(e => e.CodigoNotaria)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.ConfiguracionJson).HasColumnName("ConfiguracionJSON");
            entity.Property(e => e.Direccion)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Nit)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("NIT");
            entity.Property(e => e.NombreComercial)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.NombreLegal)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Telefono)
                .HasMaxLength(20)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Tercero>(entity =>
        {
            entity.HasKey(e => e.IdTercero).HasFillFactor(90);

            entity.Property(e => e.Apellido)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.Celular)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Correo)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Direccion)
                .HasMaxLength(300)
                .IsUnicode(false);
            entity.Property(e => e.Documento)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.LugarExpedicionMunicipioCodigoDane)
                .HasMaxLength(5)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("LugarExpedicionMunicipioCodigoDANE");
            entity.Property(e => e.Nombre)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.NombreCompleto)
                .HasMaxLength(200)
                .IsUnicode(false);

            entity.HasOne(d => d.IdTipoDocumentoNavigation).WithMany(p => p.Terceros)
                .HasForeignKey(d => d.IdTipoDocumento)
                .HasConstraintName("FK_Terceros_TiposDocumento");

            entity.HasOne(d => d.Tenant).WithMany(p => p.Terceros)
                .HasForeignKey(d => d.TenantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Terceros_Tenants");
        });

        modelBuilder.Entity<TipoInmueble>(entity =>
        {
            entity.HasKey(e => e.TipoInmuebleId).HasFillFactor(90);

            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<TiposDocumento>(entity =>
        {
            entity.HasKey(e => e.IdTipoDocumento).HasFillFactor(90);

            entity.ToTable("TiposDocumento");

            entity.HasIndex(e => e.Sigla, "IX_TiposDocumento_Sigla")
                .IsUnique()
                .HasFillFactor(90);

            entity.Property(e => e.Codigo)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.Nombre)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Sigla)
                .HasMaxLength(10)
                .IsUnicode(false);
        });

        modelBuilder.Entity<TiposEstadoCivil>(entity =>
        {
            entity.HasKey(e => e.IdEstadoCivil).HasFillFactor(90);

            entity.ToTable("TiposEstadoCivil");

            entity.HasIndex(e => e.Codigo, "IX_TiposEstadoCivil_Codigo").IsUnique();

            entity.Property(e => e.Codigo)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Descripcion)
                .HasMaxLength(300)
                .IsUnicode(false);
            entity.Property(e => e.EstadoCivil)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<TiposEstadoRadicado>(entity =>
        {
            entity.HasKey(e => e.IdEstadoRadicado);

            entity.ToTable("TiposEstadoRadicado");

            entity.Property(e => e.IdEstadoRadicado).ValueGeneratedNever();
            entity.Property(e => e.Codigo)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Descripcion)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<TiposInmuebleHomologación>(entity =>
        {
            entity.HasKey(e => e.IdTipoInmuebleHomologado).HasFillFactor(90);

            entity.ToTable("TiposInmuebleHomologación", tb => tb.HasTrigger("TRG_ValidarConstructora"));

            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.HasOne(d => d.IdTerceroNavigation).WithMany(p => p.TiposInmuebleHomologacións)
                .HasForeignKey(d => d.IdTercero)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TiposInmuebleHomologación_Terceros");

            entity.HasOne(d => d.TipoInmueble).WithMany(p => p.TiposInmuebleHomologacións)
                .HasForeignKey(d => d.TipoInmuebleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TiposInmuebleHomologación_TipoInmuebles");
        });

        modelBuilder.Entity<TiposOtorgante>(entity =>
        {
            entity.HasKey(e => e.IdTipoOtorgante)
                .HasName("PK_TiposTercero")
                .HasFillFactor(90);

            entity.ToTable(tb => tb.HasComment("Tipo de natu"));

            entity.Property(e => e.Nombre)
                .HasMaxLength(200)
                .IsUnicode(false);
        });

        modelBuilder.Entity<VDatosMinutum>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vDatosMinuta");

            entity.Property(e => e.FechaActualizacion).HasColumnType("datetime");
            entity.Property(e => e.FechaCreacion).HasColumnType("datetime");
            entity.Property(e => e.InmuebleCedulaCatastral)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("Inmueble.CedulaCatastral");
            entity.Property(e => e.InmuebleChipCatastral)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("Inmueble.ChipCatastral");
            entity.Property(e => e.InmuebleDireccion)
                .HasMaxLength(250)
                .IsUnicode(false)
                .HasColumnName("Inmueble.Direccion");
            entity.Property(e => e.InmuebleMasivoMatriculaMasivo)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("InmuebleMasivo.MatriculaMasivo");
            entity.Property(e => e.InmuebleValorHipoteca)
                .HasMaxLength(4000)
                .HasColumnName("Inmueble.ValorHipoteca");
            entity.Property(e => e.InmuebleValorHipotecaLetras).HasColumnName("Inmueble.ValorHipotecaLetras");
            entity.Property(e => e.InmuebleValorVenta)
                .HasMaxLength(4000)
                .HasColumnName("Inmueble.ValorVenta");
            entity.Property(e => e.InmuebleValorVentaLetras).HasColumnName("Inmueble.ValorVentaLetras");
            entity.Property(e => e.NombreCtl)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("NombreCTL");
            entity.Property(e => e.NombreRph)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("NombreRPH");
            entity.Property(e => e.PagoAhorroEntidad)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("Pago.AhorroEntidad");
            entity.Property(e => e.PagoAhorroValor)
                .HasMaxLength(4000)
                .HasColumnName("Pago.AhorroValor");
            entity.Property(e => e.PagoAhorroValorLetras).HasColumnName("Pago.AhorroValorLetras");
            entity.Property(e => e.PagoCesantiasEntidad)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("Pago.CesantiasEntidad");
            entity.Property(e => e.PagoCesantiasValor)
                .HasMaxLength(4000)
                .HasColumnName("Pago.CesantiasValor");
            entity.Property(e => e.PagoCesantiasValorLetras).HasColumnName("Pago.CesantiasValorLetras");
            entity.Property(e => e.PagoCreditoEntidad)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("Pago.CreditoEntidad");
            entity.Property(e => e.PagoCreditoValor)
                .HasMaxLength(4000)
                .HasColumnName("Pago.CreditoValor");
            entity.Property(e => e.PagoCreditoValorLetras).HasColumnName("Pago.CreditoValorLetras");
            entity.Property(e => e.PagoSubsidioEntidad)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("Pago.SubsidioEntidad");
            entity.Property(e => e.PagoSubsidioFchAjusteAsigna).HasColumnName("Pago.SubsidioFchAjusteAsigna");
            entity.Property(e => e.PagoSubsidioFchAsigna).HasColumnName("Pago.SubsidioFchAsigna");
            entity.Property(e => e.PagoSubsidioFchAsignaLiteral)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("Pago.SubsidioFchAsignaLiteral");
            entity.Property(e => e.PagoSubsidioFchCartaProrroga).HasColumnName("Pago.SubsidioFchCartaProrroga");
            entity.Property(e => e.PagoSubsidioValor)
                .HasMaxLength(4000)
                .HasColumnName("Pago.SubsidioValor");
            entity.Property(e => e.PagoSubsidioValorLetras).HasColumnName("Pago.SubsidioValorLetras");
        });

        modelBuilder.Entity<VInmueblesMinutum>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vInmueblesMinuta");

            entity.Property(e => e.InmuebleBlCoeficiente)
                .HasMaxLength(4000)
                .HasColumnName("InmuebleBL.Coeficiente");
            entity.Property(e => e.InmuebleBlCoeficienteLetras)
                .IsUnicode(false)
                .HasColumnName("InmuebleBL.CoeficienteLetras");
            entity.Property(e => e.InmuebleBlLinderoEspecial)
                .IsUnicode(false)
                .HasColumnName("InmuebleBL.LinderoEspecial");
            entity.Property(e => e.InmuebleBlMatricula)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("InmuebleBL.Matricula");
            entity.Property(e => e.InmuebleBlNombre)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("InmuebleBL.Nombre");
            entity.Property(e => e.InmuebleBlNombreLetras).HasColumnName("InmuebleBL.NombreLetras");
            entity.Property(e => e.InmuebleBlNumero)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("InmuebleBL.Numero");
            entity.Property(e => e.InmuebleBlTipoInmueble)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("InmuebleBL.TipoInmueble");
            entity.Property(e => e.InmuebleBlUnidad)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("InmuebleBL.Unidad");
            entity.Property(e => e.InmuebleBlValor)
                .HasMaxLength(4000)
                .HasColumnName("InmuebleBL.Valor");
            entity.Property(e => e.InmuebleBlValorLetras).HasColumnName("InmuebleBL.ValorLetras");
        });

        modelBuilder.Entity<VTercerosMinutum>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vTercerosMinuta");

            entity.Property(e => e.AcreedorApellido)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("Acreedor.Apellido");
            entity.Property(e => e.AcreedorCelular)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("Acreedor.Celular");
            entity.Property(e => e.AcreedorCorreo)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("Acreedor.Correo");
            entity.Property(e => e.AcreedorDireccion)
                .HasMaxLength(300)
                .IsUnicode(false)
                .HasColumnName("Acreedor.Direccion");
            entity.Property(e => e.AcreedorEstadoCivil)
                .HasMaxLength(300)
                .IsUnicode(false)
                .HasColumnName("Acreedor.EstadoCivil");
            entity.Property(e => e.AcreedorLugarExpedicionDocumento)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("Acreedor.LugarExpedicionDocumento");
            entity.Property(e => e.AcreedorNombre)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("Acreedor.Nombre");
            entity.Property(e => e.AcreedorNombreCompleto)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("Acreedor.NombreCompleto");
            entity.Property(e => e.AcreedorNroDocumento)
                .HasMaxLength(4000)
                .HasColumnName("Acreedor.NroDocumento");
            entity.Property(e => e.AcreedorTipoDocumento)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("Acreedor.TipoDocumento");
            entity.Property(e => e.AcreedorTipoDocumentoAbrev)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("Acreedor.TipoDocumentoAbrev");
            entity.Property(e => e.CompradorApellido)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("Comprador.Apellido");
            entity.Property(e => e.CompradorCelular)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("Comprador.Celular");
            entity.Property(e => e.CompradorCorreo)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("Comprador.Correo");
            entity.Property(e => e.CompradorDireccion)
                .HasMaxLength(300)
                .IsUnicode(false)
                .HasColumnName("Comprador.Direccion");
            entity.Property(e => e.CompradorEstadoCivil)
                .HasMaxLength(300)
                .IsUnicode(false)
                .HasColumnName("Comprador.EstadoCivil");
            entity.Property(e => e.CompradorLugarExpedicionDocumento)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("Comprador.LugarExpedicionDocumento");
            entity.Property(e => e.CompradorNombre)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("Comprador.Nombre");
            entity.Property(e => e.CompradorNombreCompleto)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("Comprador.NombreCompleto");
            entity.Property(e => e.CompradorNroDocumento)
                .HasMaxLength(4000)
                .HasColumnName("Comprador.NroDocumento");
            entity.Property(e => e.CompradorTipoDocumento)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("Comprador.TipoDocumento");
            entity.Property(e => e.CompradorTipoDocumentoAbrev)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("Comprador.TipoDocumentoAbrev");
            entity.Property(e => e.TipoOtorgante)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.VendedorApellido)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("Vendedor.Apellido");
            entity.Property(e => e.VendedorCelular)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("Vendedor.Celular");
            entity.Property(e => e.VendedorCorreo)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("Vendedor.Correo");
            entity.Property(e => e.VendedorDireccion)
                .HasMaxLength(300)
                .IsUnicode(false)
                .HasColumnName("Vendedor.Direccion");
            entity.Property(e => e.VendedorEstadoCivil)
                .HasMaxLength(300)
                .IsUnicode(false)
                .HasColumnName("Vendedor.EstadoCivil");
            entity.Property(e => e.VendedorLugarExpedicionDocumento)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("Vendedor.LugarExpedicionDocumento");
            entity.Property(e => e.VendedorNombre)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("Vendedor.Nombre");
            entity.Property(e => e.VendedorNombreCompleto)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("Vendedor.NombreCompleto");
            entity.Property(e => e.VendedorNroDocumento)
                .HasMaxLength(4000)
                .HasColumnName("Vendedor.NroDocumento");
            entity.Property(e => e.VendedorTipoDocumento)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("Vendedor.TipoDocumento");
            entity.Property(e => e.VendedorTipoDocumentoAbrev)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("Vendedor.TipoDocumentoAbrev");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}