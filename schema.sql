-- Script de creación del esquema para TechStore Database-First
-- Ejecutar en MySQL Workbench o XAMPP antes de hacer scaffolding.
-- Este archivo documenta la fuente de verdad de la base de datos.

CREATE DATABASE IF NOT EXISTS techstore_db_df
  CHARACTER SET utf8mb4
  COLLATE utf8mb4_unicode_ci;

USE techstore_db_df;

-- ============================================================
-- TPH: Usuarios (Cliente / Empleado / Administrador en una sola tabla)
-- La columna UserType es el discriminador que reemplaza la jerarquía
-- de herencia C# que existía en la etapa code-first.
-- ============================================================
CREATE TABLE IF NOT EXISTS Usuarios (
  Id                  INT          NOT NULL,
  Nombre              VARCHAR(150) NOT NULL,
  Correo              VARCHAR(200) NOT NULL,
  UserType            VARCHAR(15)  NOT NULL,   -- discriminador: 'Cliente','Empleado','Administrador'
  -- Columnas exclusivas de Cliente
  Direccion           VARCHAR(300) NULL,
  -- Columnas exclusivas de Empleado
  Cargo               VARCHAR(100) NULL,
  -- Composición (table-splitting): información de contacto del Empleado
  Contacto_Telefono   VARCHAR(20)  NULL,
  Contacto_Direccion  VARCHAR(300) NULL,
  -- Columnas exclusivas de Administrador
  NivelAcceso         INT          NULL,
  PRIMARY KEY (Id),
  UNIQUE KEY UX_Usuarios_Correo (Correo)
) ENGINE=InnoDB;

-- ============================================================
-- Categorias (entidad independiente para la relación muchos-a-muchos)
-- ============================================================
CREATE TABLE IF NOT EXISTS Categorias (
  Codigo  VARCHAR(50)  NOT NULL,
  Nombre  VARCHAR(200) NOT NULL,
  PRIMARY KEY (Codigo)
) ENGINE=InnoDB;

-- ============================================================
-- Productos
-- ============================================================
CREATE TABLE IF NOT EXISTS Productos (
  Codigo      INT           NOT NULL,
  Nombre      VARCHAR(200)  NOT NULL,
  Descripcion VARCHAR(500)  NULL,
  Precio      DECIMAL(12,2) NOT NULL,
  Stock       INT           NOT NULL DEFAULT 0,
  PRIMARY KEY (Codigo)
) ENGINE=InnoDB;

-- ============================================================
-- AGREGACIÓN: Productos ↔ Categorias (muchos-a-muchos)
-- Tabla intermedia — ninguno de los dos lados "posee" al otro.
-- ============================================================
CREATE TABLE IF NOT EXISTS ProductoCategorias (
  ProductoCodigo  INT         NOT NULL,
  CategoriaCodigo VARCHAR(50) NOT NULL,
  PRIMARY KEY (ProductoCodigo, CategoriaCodigo),
  CONSTRAINT FK_PC_Producto  FOREIGN KEY (ProductoCodigo)  REFERENCES Productos(Codigo)  ON DELETE CASCADE,
  CONSTRAINT FK_PC_Categoria FOREIGN KEY (CategoriaCodigo) REFERENCES Categorias(Codigo) ON DELETE CASCADE
) ENGINE=InnoDB;

-- ============================================================
-- Ventas — FK opcionales ON DELETE SET NULL (AGREGACIÓN)
-- Si se elimina un cliente o empleado, la venta queda huérfana
-- pero no se borra (el historial comercial se preserva).
-- ============================================================
CREATE TABLE IF NOT EXISTS Ventas (
  Codigo     INT      NOT NULL,
  Fecha      DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  ClienteId  INT      NULL,
  EmpleadoId INT      NULL,
  PRIMARY KEY (Codigo),
  CONSTRAINT FK_Ventas_Cliente  FOREIGN KEY (ClienteId)  REFERENCES Usuarios(Id) ON DELETE SET NULL,
  CONSTRAINT FK_Ventas_Empleado FOREIGN KEY (EmpleadoId) REFERENCES Usuarios(Id) ON DELETE SET NULL
) ENGINE=InnoDB;

-- ============================================================
-- DetallesVenta — COMPOSICIÓN: ON DELETE CASCADE
-- Los detalles no tienen sentido sin su venta padre.
-- ============================================================
CREATE TABLE IF NOT EXISTS DetallesVenta (
  Id              INT           NOT NULL AUTO_INCREMENT,
  VentaCodigo     INT           NOT NULL,
  ProductoCodigo  INT           NOT NULL,
  Cantidad        INT           NOT NULL,
  PrecioUnitario  DECIMAL(12,2) NOT NULL,
  PRIMARY KEY (Id),
  CONSTRAINT FK_Detalle_Venta    FOREIGN KEY (VentaCodigo)   REFERENCES Ventas(Codigo)    ON DELETE CASCADE,
  CONSTRAINT FK_Detalle_Producto FOREIGN KEY (ProductoCodigo) REFERENCES Productos(Codigo) ON DELETE RESTRICT
) ENGINE=InnoDB;
