$connectionString = "Server=localhost;Port=3306;Database=techstore_db_df;Uid=root;Pwd=;"

dotnet ef dbcontext scaffold $connectionString Pomelo.EntityFrameworkCore.MySql --output-dir Models --context-dir Data/Context --context AppDbContext --force --no-onconfiguring

$map = @{
    "Producto.cs"       = "Inventario"
    "Usuario.cs"        = "Usuarios"
    "Cliente.cs"        = "Usuarios"
    "Empleado.cs"       = "Usuarios"
    "Administrador.cs"  = "Usuarios"
    "Venta.cs"          = "Ventas"
    "DetalleVenta.cs" = "Ventas"
}


# --- CORRECCIÓN DEL NOMBRE LATINO (Detallesventum -> DetalleVenta) ---
if (Test-Path "Models\Detallesventum.cs") {
    $c = Get-Content "Models\Detallesventum.cs" -Raw
    $c = $c -replace "Detallesventum", "DetalleVenta"
    Set-Content "Models\Detallesventum.cs" -Value $c
    Rename-Item "Models\Detallesventum.cs" "DetalleVenta.cs"
}
foreach ($file in $map.Keys) {
    $source = "Models\$file"
    $targetFile = "Models\$($map[$file])\$file"

    if (Test-Path $source) {
        Move-Item -Path $source -Destination $targetFile -Force
        $content = Get-Content $targetFile -Raw
        $content = $content -replace "namespace TiendaLinea.Models;", "namespace TiendaLinea.Models.$($map[$file]);"
        Set-Content $targetFile -Value $content
    }
}


$ctxPath = "Data\Context\AppDbContext.cs"
$ctxContent = Get-Content $ctxPath -Raw
$usings = ""
$ctxContent = $usings + $ctxContent
$ctxContent = $ctxContent -replace "public virtual DbSet<Efmigrationshistory> Efmigrationshistories { get; set; }", ""
$ctxContent = $ctxContent -replace '(?s)modelBuilder.Entity<Efmigrationshistory>\(.*?\}\);', ""
Set-Content $ctxPath -Value $ctxContent

$usingsAll = ""
$dirs = @("Models\Inventario", "Models\Usuarios", "Models\Ventas")
foreach ($dir in $dirs) {
    if (Test-Path $dir) {
        foreach ($f in Get-ChildItem -Path $dir -Filter "*.cs") {
            $content = Get-Content $f.FullName -Raw
            if ($content -notmatch "using TiendaLinea.Models.Inventario;") {
                Set-Content $f.FullName -Value ($usingsAll + $content)
            }
        }
    }
}


