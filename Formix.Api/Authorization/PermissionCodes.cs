namespace Formix.Api.Authorization;

/// <summary>
/// Códigos alineados con la tabla Sys_Permisos en base de datos.
/// </summary>
public static class PermissionCodes
{
    public const string VerRoles = "VER_ROLES";
    public const string CrearRoles = "CREAR_ROLES";
    public const string EditarRoles = "EDITAR_ROLES";
    public const string EliminarRoles = "ELIMINAR_ROLES";

    public const string VerPermisos = "VER_PERMISOS";
    public const string CrearPermisos = "CREAR_PERMISOS";
    public const string EditarPermisos = "EDITAR_PERMISOS";
    public const string EliminarPermisos = "ELIMINAR_PERMISOS";

    public const string VerUsuarios = "VER_USUARIOS";
    public const string CrearUsuarios = "CREAR_USUARIOS";
    public const string EditarUsuarios = "EDITAR_USUARIOS";
    public const string EliminarUsuarios = "ELIMINAR_USUARIOS";
    public const string AsignarRoles = "ASIGNAR_ROLES";
    public const string ResetearClaveUsuario = "RESETEAR_CLAVE_USUARIO";

    // Proyectos
    public const string VerProyectos = "VER_PROYECTOS";
    public const string CrearProyectos = "CREAR_PROYECTOS";
    public const string EditarProyectos = "EDITAR_PROYECTOS";
    public const string EliminarProyectos = "ELIMINAR_PROYECTOS";

    // Inmuebles
    public const string VerInmuebles = "VER_INMUEBLES";
    public const string CrearInmuebles = "CREAR_INMUEBLES";
    public const string EditarInmuebles = "EDITAR_INMUEBLES";
    public const string EliminarInmuebles = "ELIMINAR_INMUEBLES";
    public const string ImportarInmuebles = "IMPORTAR_INMUEBLES";

    // Radicados
    public const string VerRadicados = "VER_RADICADOS";
    public const string CrearRadicados = "CREAR_RADICADOS";
    public const string EditarRadicados = "EDITAR_RADICADOS";
    public const string EliminarRadicados = "ELIMINAR_RADICADOS";
    public const string CargarPdfRadicado = "CARGAR_PDF_RADICADO";
    public const string CrearPago = "CREAR_PAGO";
    public const string VerEscrituracion = "VER_ESCRITURACION";

    // Terceros
    public const string VerTerceros = "VER_TERCEROS";
    public const string CrearTerceros = "CREAR_TERCEROS";
    public const string EditarTerceros = "EDITAR_TERCEROS";
    public const string EliminarTerceros = "ELIMINAR_TERCEROS";

    // Plantillas
    public const string VerPlantillas = "VER_PLANTILLAS";
    public const string GestionarPlantillas = "GESTIONAR_PLANTILLAS";

    // Catálogos y tenants
    public const string VerCatalogos = "VER_CATALOGOS";
    public const string GestionarCatalogos = "GESTIONAR_CATALOGOS";
    public const string VerTenants = "VER_TENANTS";
    public const string GestionarTenants = "GESTIONAR_TENANTS";
}
