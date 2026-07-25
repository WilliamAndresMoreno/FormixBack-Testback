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
                new SysPermiso { Codigo = "VER_ROLES", Nombre = "Ver Roles", Descripcion = "Permite ver la lista de roles" },
                new SysPermiso { Codigo = "CREAR_ROLES", Nombre = "Crear Roles", Descripcion = "Permite crear nuevos roles" },
                new SysPermiso { Codigo = "EDITAR_ROLES", Nombre = "Editar Roles", Descripcion = "Permite editar roles existentes" },
                new SysPermiso { Codigo = "ELIMINAR_ROLES", Nombre = "Eliminar Roles", Descripcion = "Permite eliminar roles" },
                
                new SysPermiso { Codigo = "VER_PERMISOS", Nombre = "Ver Permisos", Descripcion = "Permite ver la lista de permisos" },
                new SysPermiso { Codigo = "CREAR_PERMISOS", Nombre = "Crear Permisos", Descripcion = "Permite crear nuevos permisos" },
                new SysPermiso { Codigo = "EDITAR_PERMISOS", Nombre = "Editar Permisos", Descripcion = "Permite editar permisos existentes" },
                new SysPermiso { Codigo = "ELIMINAR_PERMISOS", Nombre = "Eliminar Permisos", Descripcion = "Permite eliminar permisos" },
                
                new SysPermiso { Codigo = "VER_USUARIOS", Nombre = "Ver Usuarios", Descripcion = "Permite ver la lista de usuarios" },
                new SysPermiso { Codigo = "ASIGNAR_ROLES", Nombre = "Asignar Roles", Descripcion = "Permite asignar roles a los usuarios" },
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
        }
    }
}
