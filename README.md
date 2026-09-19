# TechStore ORM Database-First

Este proyecto fue convertido de un diseño WinForms en memoria hacia una arquitectura con persistencia en MySQL utilizando Entity Framework Core bajo el enfoque **Database-First**.

## Configuración y Variables de Entorno

El proyecto lee sus credenciales desde un archivo `.env` en la raíz (ignorado en Git). Renombre el archivo `.env.example` a `.env` e ingrese los datos de su base de datos MySQL local antes de ejecutar.

## Comando de Scaffolding Utilizado

Para cumplir con la rúbrica y generar este código a partir de la base de datos `techstore_db_df`, se ejecutó el siguiente comando utilizando la herramienta local `dotnet-ef`:

```powershell
dotnet ef dbcontext scaffold "Server=localhost;Port=3306;Database=techstore_db_df;User=root;Password=;" Pomelo.EntityFrameworkCore.MySql --output-dir Models --context-dir Data --context TechStoreDbContext --namespace TiendaLinea.Models --context-namespace TiendaLinea.Data --no-onconfiguring --force
```

Vea el archivo `DOCUMENTACION_DOMINIO.md` para entender los compromisos arquitectónicos al pasar de Code-First a Database-First.
