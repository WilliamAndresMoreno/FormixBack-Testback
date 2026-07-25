using System;
using System.Linq;
using System.Threading.Tasks;
using Formix.Infrastructure.Data.Configurations;
using Formix.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Formix.Infrastructure.Data
{
    public static class DbSeeder
    {
        public static async Task SeedDataAsync(AppDbContext context)
        {
            // 1. Asegurar la creación de Roles Base
            // Validamos por nombre para no duplicar si los IDs difieren por alguna razón, pero respetamos la data mostrada
            var roles = new[]
            {
                new SysRole { Nombre = "SuperAdmin", Descripcion = "Administrador global del sistema", EsAdministrador = true, Activo = true },
                new SysRole { Nombre = "AdminNotaria", Descripcion = "Administrador de una notaria", EsAdministrador = false, Activo = true },
                new SysRole { Nombre = "UsuarioNotaria", Descripcion = "Usuario estándar de una notaria", EsAdministrador = false, Activo = true }
            };

            foreach (var roleDef in roles)
            {
                var roleExists = await context.SysRoles.AnyAsync(r => r.Nombre == roleDef.Nombre);
                if (!roleExists)
                {
                    context.SysRoles.Add(roleDef);
                }
            }

            // Guardamos los roles si hubo cambios para tener los IDs generados disponibles (o los existentes)
            if (context.ChangeTracker.HasChanges())
            {
                await context.SaveChangesAsync();
            }

            // 2. Asegurar la creación de Permisos Base
            var permisos = new[]
            {
                new SysPermiso { PermisoId = 1, Codigo = "VER_ROLES", Nombre = "Ver Roles", Descripcion = "Permite ver la lista de roles" },
                new SysPermiso { PermisoId = 2, Codigo = "CREAR_ROLES", Nombre = "Crear Roles", Descripcion = "Permite crear nuevos roles" },
                new SysPermiso { PermisoId = 3, Codigo = "EDITAR_ROLES", Nombre = "Editar Roles", Descripcion = "Permite editar roles existentes" },
                new SysPermiso { PermisoId = 4, Codigo = "ELIMINAR_ROLES", Nombre = "Eliminar Roles", Descripcion = "Permite eliminar roles" },
                new SysPermiso { PermisoId = 5, Codigo = "VER_PERMISOS", Nombre = "Ver Permisos", Descripcion = "Permite ver la lista de permisos" },
                new SysPermiso { PermisoId = 6, Codigo = "CREAR_PERMISOS", Nombre = "Crear Permisos", Descripcion = "Permite crear nuevos permisos" },
                new SysPermiso { PermisoId = 7, Codigo = "EDITAR_PERMISOS", Nombre = "Editar Permisos", Descripcion = "Permite editar permisos existentes" },
                new SysPermiso { PermisoId = 8, Codigo = "ELIMINAR_PERMISOS", Nombre = "Eliminar Permisos", Descripcion = "Permite eliminar permisos" },
                new SysPermiso { PermisoId = 9, Codigo = "VER_USUARIOS", Nombre = "Ver Usuarios", Descripcion = "Permite ver la lista de usuarios" },
                new SysPermiso { PermisoId = 10, Codigo = "ASIGNAR_ROLES", Nombre = "Asignar Roles", Descripcion = "Permite asignar roles a los usuarios" },
                new SysPermiso { PermisoId = 11, Codigo = "CREAR_USUARIOS", Nombre = "Crear usuarios", Descripcion = "Permite registrar y crear usuarios en el tenant" },
                new SysPermiso { PermisoId = 12, Codigo = "EDITAR_USUARIOS", Nombre = "Editar usuarios", Descripcion = "Permite actualizar datos de usuarios" },
                new SysPermiso { PermisoId = 13, Codigo = "ELIMINAR_USUARIOS", Nombre = "Eliminar usuarios", Descripcion = "Permite eliminar usuarios del tenant" },
                new SysPermiso { PermisoId = 14, Codigo = "RESETEAR_CLAVE_USUARIO", Nombre = "Resetear clave de usuario", Descripcion = "Permite cambiar la contraseña de otro usuario" },
                new SysPermiso { PermisoId = 15, Codigo = "VER_PROYECTOS", Nombre = "Ver proyectos", Descripcion = "Listar y consultar proyectos inmobiliarios" },
                new SysPermiso { PermisoId = 16, Codigo = "CREAR_PROYECTOS", Nombre = "Crear proyectos", Descripcion = "Registrar nuevos proyectos" },
                new SysPermiso { PermisoId = 17, Codigo = "EDITAR_PROYECTOS", Nombre = "Editar proyectos", Descripcion = "Modificar proyectos existentes" },
                new SysPermiso { PermisoId = 18, Codigo = "ELIMINAR_PROYECTOS", Nombre = "Eliminar proyectos", Descripcion = "Eliminar proyectos" },
                new SysPermiso { PermisoId = 19, Codigo = "VER_INMUEBLES", Nombre = "Ver inmuebles", Descripcion = "Consultar unidades/inmuebles del proyecto" },
                new SysPermiso { PermisoId = 20, Codigo = "CREAR_INMUEBLES", Nombre = "Crear inmuebles", Descripcion = "Crear inmuebles manualmente" },
                new SysPermiso { PermisoId = 21, Codigo = "EDITAR_INMUEBLES", Nombre = "Editar inmuebles", Descripcion = "Modificar inmuebles y vínculos con terceros" },
                new SysPermiso { PermisoId = 22, Codigo = "ELIMINAR_INMUEBLES", Nombre = "Eliminar inmuebles", Descripcion = "Eliminar inmuebles" },
                new SysPermiso { PermisoId = 23, Codigo = "IMPORTAR_INMUEBLES", Nombre = "Importar inmuebles", Descripcion = "Carga masiva de inmuebles desde Excel" },
                new SysPermiso { PermisoId = 24, Codigo = "VER_RADICADOS", Nombre = "Ver radicados", Descripcion = "Listar y consultar radicados" },
                new SysPermiso { PermisoId = 25, Codigo = "CREAR_RADICADOS", Nombre = "Crear radicados", Descripcion = "Crear nuevos radicados" },
                new SysPermiso { PermisoId = 26, Codigo = "EDITAR_RADICADOS", Nombre = "Editar radicados", Descripcion = "Editar radicado, actos, otorgantes e inmuebles asociados" },
                new SysPermiso { PermisoId = 27, Codigo = "ELIMINAR_RADICADOS", Nombre = "Eliminar radicados", Descripcion = "Eliminar radicados" },
                new SysPermiso { PermisoId = 28, Codigo = "CARGAR_PDF_RADICADO", Nombre = "Cargar PDF sábana", Descripcion = "Importar datos desde PDF de sábana notarial al radicado" },
                new SysPermiso { PermisoId = 29, Codigo = "VER_TERCEROS", Nombre = "Ver terceros", Descripcion = "Consultar personas (compradores, vendedores, etc.)" },
                new SysPermiso { PermisoId = 30, Codigo = "CREAR_TERCEROS", Nombre = "Crear terceros", Descripcion = "Registrar terceros" },
                new SysPermiso { PermisoId = 31, Codigo = "EDITAR_TERCEROS", Nombre = "Editar terceros", Descripcion = "Modificar terceros" },
                new SysPermiso { PermisoId = 32, Codigo = "ELIMINAR_TERCEROS", Nombre = "Eliminar terceros", Descripcion = "Eliminar terceros" },
                new SysPermiso { PermisoId = 33, Codigo = "VER_PLANTILLAS", Nombre = "Ver plantillas", Descripcion = "Consultar y descargar plantillas de proyecto" },
                new SysPermiso { PermisoId = 34, Codigo = "GESTIONAR_PLANTILLAS", Nombre = "Gestionar plantillas", Descripcion = "Asignar, subir y administrar plantillas por proyecto" },
                new SysPermiso { PermisoId = 35, Codigo = "VER_CATALOGOS", Nombre = "Ver catálogos", Descripcion = "Departamentos, municipios, tipos documento, estado civil, etc." },
                new SysPermiso { PermisoId = 36, Codigo = "GESTIONAR_CATALOGOS", Nombre = "Gestionar catálogos", Descripcion = "CRUD de actos, tipos inmueble, tipos otorgante" },
                new SysPermiso { PermisoId = 37, Codigo = "VER_TENANTS", Nombre = "Ver tenants", Descripcion = "Consultar notarías registradas" },
                new SysPermiso { PermisoId = 38, Codigo = "GESTIONAR_TENANTS", Nombre = "Gestionar tenants", Descripcion = "Crear, editar y eliminar tenants" },
                new SysPermiso { PermisoId = 39, Codigo = "CREAR_PAGO", Nombre = "Crear pago", Descripcion = "Crear y gestionar pagos" },
                new SysPermiso { PermisoId = 40, Codigo = "VER_ESCRITURACION", Nombre = "Ver escrituración", Descripcion = "Permite ver la vista de gestión de escrituración" }
            };

            foreach (var permDef in permisos)
            {
                var permExists = await context.SysPermisos.AnyAsync(p => p.Codigo == permDef.Codigo);
                if (!permExists)
                {
                    context.SysPermisos.Add(permDef);
                }
            }

            if (context.ChangeTracker.HasChanges())
            {
                await context.SaveChangesAsync();
            }

            // 3. Asignar Permisos a SuperAdmin
            var superAdmin = await context.SysRoles.FirstOrDefaultAsync(r => r.Nombre == "SuperAdmin");
            
            if (superAdmin != null)
            {
                // Obtenemos todos los permisos actuales en la base de datos
                var allPermisos = await context.SysPermisos.ToListAsync();

                foreach (var permiso in allPermisos)
                {
                    var relacionExiste = await context.SysRolPermisos
                        .AnyAsync(rp => rp.RolId == superAdmin.RolId && rp.PermisoId == permiso.PermisoId);

                    if (!relacionExiste)
                    {
                        context.SysRolPermisos.Add(new SysRolPermiso
                        {
                            RolId = superAdmin.RolId,
                            PermisoId = permiso.PermisoId
                        });
                    }
                }

                if (context.ChangeTracker.HasChanges())
                {
                    await context.SaveChangesAsync();
                }
            }

            // 4. Asignar rol SuperAdmin al usuario "admin" (si existe)
            var adminUser = await context.SysUsuarios.FirstOrDefaultAsync(u => u.NombreUsuario == "admin");
            if (adminUser != null && superAdmin != null)
            {
                var relacionUsuarioRolExiste = await context.SysUsuarioRoles
                    .AnyAsync(ur => ur.UsuarioId == adminUser.UsuarioId && ur.RolId == superAdmin.RolId);

                if (!relacionUsuarioRolExiste)
                {
                    context.SysUsuarioRoles.Add(new SysUsuarioRole
                    {
                        UsuarioId = adminUser.UsuarioId,
                        RolId = superAdmin.RolId,
                        FechaAsignacion = DateTime.UtcNow
                    });
                }

                if (context.ChangeTracker.HasChanges())
                {
                    await context.SaveChangesAsync();
                }
            }

            // 5. Asegurar Cajas de Compensación
            var cajasNombres = new[] { "Compensar", "Colsubsidio", "Cafam", "Comfenalco" };

            foreach (var nombre in cajasNombres)
            {
                var exists = await context.CajasCompensacions.AnyAsync(c => c.Nombre == nombre);
                if (!exists)
                {
                    context.CajasCompensacions.Add(new CajasCompensacion { Nombre = nombre });
                }
            }

            if (context.ChangeTracker.HasChanges())
            {
                await context.SaveChangesAsync();
            }
        }
    }
}
