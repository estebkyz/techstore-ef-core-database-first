# Cambios en el Modelo de Dominio (Code-First vs Database-First)

Este documento explica las diferencias fundamentales entre el diseño original orientado a objetos (code-first) y el diseño actual basado en datos (database-first), cumpliendo con el Criterio E de la rúbrica.

## 1. La pérdida de la Herencia (TPH)
En el proyecto code-first teníamos tres clases reales: `Cliente`, `Empleado` y `Administrador`, que heredaban de una clase base abstracta `Usuario`. Esto permitía usar polimorfismo y métodos específicos.

Al pasar a **database-first**, EF Core leyó la base de datos (donde el patrón TPH guarda todo en una sola tabla `Usuarios` con una columna discriminadora `UserType`). El scaffolding no puede "adivinar" que había herencia de clases ahí, así que generó **una única clase plana `Usuario`**.
*   **Consecuencia:** Ya no hay subclases en C#. La lógica para saber si un usuario es Cliente o Empleado se movió a la capa de aplicación (UI/AppState.cs), que ahora filtra la misma lista de Usuarios usando el discriminador (`u.UserType == UserTypes.Cliente`).

## 2. Conservación de la Agregación (Muchos a Muchos)
Teníamos una relación de agregación muchos a muchos entre `Producto` y `Categoria`. En la base de datos, esto es la tabla intermedia `ProductoCategorias`.
*   **Consecuencia:** Esto **sí sobrevivió perfectamente**. EF Core scaffolea esto detectando las dos llaves foráneas y genera las colecciones `Producto.CategoriaCodigos` y `Categoria.ProductoCodigos`. No hubo pérdida porque la agregación relacional se mapea directamente a colecciones en C#.

## 3. Conservación parcial de la Composición
En el modelo original, un `DetalleVenta` no podía existir sin su `Venta` padre. Toda la creación pasaba por el método `Venta.AgregarDetalle()`.
*   **Consecuencia:** A nivel de C#, **se perdió el encapsulamiento**. Scaffolding generó `DetallesVentum` como una entidad totalmente independiente con su propio DbSet. Ahora cualquier parte del código puede insertar un detalle suelto (ver `AppState.AddVentaAsync`). 
*   Sin embargo, a nivel de base de datos **la composición sobrevive** gracias a la restricción `ON DELETE CASCADE`. Si borras una venta, MySQL borrará sus detalles automáticamente.

## 4. Agregación débil (Claves foráneas opcionales)
La `Venta` está relacionada con un `Cliente` y un `Empleado`.
*   **Consecuencia:** Antes, esto se representaba adjuntando el objeto completo. Ahora, gracias a que el scaffolding generó propiedades escalares (`ClienteId`, `EmpleadoId`), es mucho más fácil asociarlos. Solo asignamos el ID (int) al guardar, sin necesidad de tener el objeto rastreado por EF Core. Esto simplificó el código en el `AppState`.
