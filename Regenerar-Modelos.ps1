$solutionRoot = (Get-Location).Path  # Carpeta actual donde ejecutas el script

# Rutas
$entitiesPath = Join-Path $solutionRoot "Formix.Infrastructure\Data\Entities"
$configurationsPath = Join-Path $solutionRoot "Formix.Infrastructure\Data\Configurations"
$dbContextPath = Join-Path $solutionRoot "Formix.Infrastructure\Data\AppDbContext.cs"
$projectPath = Join-Path $solutionRoot "Formix.Infrastructure\Formix.Infrastructure.csproj"
$dtosPath = Join-Path $solutionRoot "Formix.Domain\Dtos"

# Crear carpeta de DTOs si no existe
if (-not (Test-Path $dtosPath)) {
    New-Item -ItemType Directory -Path $dtosPath | Out-Null
}

# Limpiar DTOs previos
Write-Host "🧹 Eliminando DTOs anteriores en: $dtosPath"
Get-ChildItem -Path $dtosPath -Filter *.cs | Remove-Item -Force

Write-Host "Usando rutas:"
Write-Host "Entities: $entitiesPath"
Write-Host "Configurations: $configurationsPath"
Write-Host "DbContext: $dbContextPath"
Write-Host "Project: $projectPath"
Write-Host "DTOs: $dtosPath"

# 1️⃣ Scaffold normal de entidades en Infrastructure
dotnet ef dbcontext scaffold `
    "Server=Qa-formix.novatechh.com.co,15831;Database=FormixDB_DEV;User Id=usr_Formixdb;Password=1qa2ws3eDDD*;TrustServerCertificate=True;" `
    Microsoft.EntityFrameworkCore.SqlServer `
    --output-dir $entitiesPath `
    --context-dir $configurationsPath `
    --context AppDbContext `
    --project $projectPath `
    --force

# 2️⃣ Generar DTOs a partir de las entidades
Get-ChildItem -Path $entitiesPath -Filter *.cs | ForEach-Object {
    $entityFile = $_.FullName
    $entityName = [System.IO.Path]::GetFileNameWithoutExtension($_.Name)

    $dtoFile = Join-Path $dtosPath "$entityName`Dto.cs"

    $lines = Get-Content $entityFile
    $props = @()

    foreach ($line in $lines) {
        if ($line -match "public (?!virtual)([^\s]+) ([^\s]+) { get; set; }") {
            $props += "        public $($matches[1]) $($matches[2]) { get; set; }"
        }
    }

    $dtoClass = @"
using System;

namespace Formix.Domain.Dtos
{
    public class ${entityName}Dto
    {
$([string]::Join("`n", $props))
    }
}
"@

    Set-Content -Path $dtoFile -Value $dtoClass -Encoding UTF8
    Write-Host "✅ DTO generado: $dtoFile"
}
