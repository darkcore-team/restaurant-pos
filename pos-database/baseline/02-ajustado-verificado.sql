CREATE EXTENSION IF NOT EXISTS btree_gist;   -- para EXCLUDE de reservas (fase 2)

CREATE SCHEMA shared;      -- solo tipos ENUM compartidos (sin datos)
CREATE SCHEMA identity;    -- restaurante, sucursales, personas, usuarios, roles
CREATE SCHEMA catalog;     -- menú, modificadores, estaciones, impuestos, monedas
CREATE SCHEMA tables;      -- zonas, mesas, asignación mesero↔mesa
CREATE SCHEMA orders;      -- comandas, líneas, modificadores, historial
CREATE SCHEMA payments;    -- cajas, sesiones, pagos, tickets de control
CREATE SCHEMA reporting;   -- agregados deventa (alimentado por eventos)

SET search_path TO shared, identity, catalog, tables, orders, payments, reporting, public;

-- AGREGAR INVENTARIO


-- ═══ 1 · SHARED — TIPOS ENUM ══════════════════════════════════════════════
--  Son tipos, no datos: no rompen el aislamiento entre servicios.

CREATE TYPE shared.order_status AS ENUM
  ('OPEN','IN_PREPARATION','READY','SERVED','BILLING','CLOSED','CANCELLED');

--  Estado por línea: es lo que permite que el KDS marque "preparando/listo"
--  de forma independiente por estación.
CREATE TYPE shared.order_item_status AS ENUM
  ('QUEUED','SENT','PREPARING','READY','SERVED','VOIDED','CANCELLED');

CREATE TYPE shared.table_status AS ENUM
  ('FREE','OCCUPIED','BILLING','RESERVED','DIRTY','OUT_OF_SERVICE');

CREATE TYPE shared.order_type AS ENUM
  ('DINE_IN','TAKEAWAY','DELIVERY','DRIVE_THRU');

CREATE TYPE shared.payment_status AS ENUM
  ('PENDING','COMPLETED','VOIDED','REFUNDED');

CREATE TYPE shared.cash_session_status AS ENUM ('OPEN','CLOSED');


-- ═══ 2 · CATALOG — DATOS GLOBALES DE REFERENCIA ══════════════════════════
--  Sin restaurant_id y sin RLS: los comparte el sistema entero.
--  Se definen antes que identity porque identity los referencia por FK.

CREATE TABLE catalog.languages (
  id          BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  public_id   UUID NOT NULL DEFAULT gen_random_uuid() UNIQUE,
  code        CHAR(2) NOT NULL UNIQUE,
  name        VARCHAR(80) NOT NULL,
  is_active   BOOLEAN NOT NULL DEFAULT TRUE,
  created_at TIMESTAMPTZ NOT NULL DEFAULT now(), created_by BIGINT,
  updated_at TIMESTAMPTZ NOT NULL DEFAULT now(), updated_by BIGINT,
  deleted_at TIMESTAMPTZ,
  CONSTRAINT ck_lang_code CHECK (code ~ '^[a-z]{2}$')
);

CREATE TABLE catalog.currencies (
  id             BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  public_id      UUID NOT NULL DEFAULT gen_random_uuid() UNIQUE,
  code           CHAR(3) NOT NULL UNIQUE,
  name           VARCHAR(80) NOT NULL,
  symbol         VARCHAR(8) NOT NULL,
  decimal_places SMALLINT NOT NULL DEFAULT 2,
  is_active      BOOLEAN NOT NULL DEFAULT TRUE,
  created_at TIMESTAMPTZ NOT NULL DEFAULT now(), created_by BIGINT,
  updated_at TIMESTAMPTZ NOT NULL DEFAULT now(), updated_by BIGINT,
  deleted_at TIMESTAMPTZ,
  CONSTRAINT ck_curr_code CHECK (code ~ '^[A-Z]{3}$')
);

CREATE TABLE catalog.countries (
  id                  BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  public_id           UUID NOT NULL DEFAULT gen_random_uuid() UNIQUE,
  code                CHAR(2) NOT NULL UNIQUE,
  code_alpha3         CHAR(3) UNIQUE,
  name                VARCHAR(100) NOT NULL,
  phone_code          VARCHAR(8),
  default_currency_id BIGINT REFERENCES catalog.currencies (id),
  is_active           BOOLEAN NOT NULL DEFAULT TRUE,
  created_at TIMESTAMPTZ NOT NULL DEFAULT now(), created_by BIGINT,
  updated_at TIMESTAMPTZ NOT NULL DEFAULT now(), updated_by BIGINT,
  deleted_at TIMESTAMPTZ
);

CREATE TABLE catalog.regions (
  id          BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  public_id   UUID NOT NULL DEFAULT gen_random_uuid() UNIQUE,
  country_id  BIGINT NOT NULL REFERENCES catalog.countries (id),
  code        VARCHAR(12) NOT NULL,
  name        VARCHAR(120) NOT NULL,
  is_active   BOOLEAN NOT NULL DEFAULT TRUE,
  created_at TIMESTAMPTZ NOT NULL DEFAULT now(), created_by BIGINT,
  updated_at TIMESTAMPTZ NOT NULL DEFAULT now(), updated_by BIGINT,
  deleted_at TIMESTAMPTZ,
  CONSTRAINT uq_region_code UNIQUE (country_id, code)
);

CREATE TABLE catalog.cities (
  id         BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  public_id  UUID NOT NULL DEFAULT gen_random_uuid() UNIQUE,
  country_id BIGINT NOT NULL REFERENCES catalog.countries (id),
  region_id  BIGINT REFERENCES catalog.regions (id),
  code       VARCHAR(12),
  name       VARCHAR(120) NOT NULL,
  is_capital BOOLEAN NOT NULL DEFAULT FALSE,
  is_active  BOOLEAN NOT NULL DEFAULT TRUE,
  created_at TIMESTAMPTZ NOT NULL DEFAULT now(), created_by BIGINT,
  updated_at TIMESTAMPTZ NOT NULL DEFAULT now(), updated_by BIGINT,
  deleted_at TIMESTAMPTZ,
  CONSTRAINT uq_city_name UNIQUE (country_id, name)
);

--  Identificaciones Nicaragua: cédula, cédula de residencia, pasaporte.
--  El RUC del negocio vive en identity.restaurants.tax_id (no es de persona).
CREATE TABLE catalog.document_types (
  id                   BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  public_id            UUID NOT NULL DEFAULT gen_random_uuid() UNIQUE,
  country_id           BIGINT REFERENCES catalog.countries (id),
  code                 VARCHAR(20) NOT NULL,
  name                 VARCHAR(120) NOT NULL,
  requires_expiry_date BOOLEAN NOT NULL DEFAULT FALSE,
  is_active            BOOLEAN NOT NULL DEFAULT TRUE,
  created_at TIMESTAMPTZ NOT NULL DEFAULT now(), created_by BIGINT,
  updated_at TIMESTAMPTZ NOT NULL DEFAULT now(), updated_by BIGINT,
  deleted_at TIMESTAMPTZ,
  CONSTRAINT uq_doctype UNIQUE NULLS NOT DISTINCT (country_id, code)
);

--  IVA Nicaragua = 15% (Ley 822, art. 109 LCT). Catálogo global por país.
--  La exención NO es un impuesto con tasa 0: se marca con
--  catalog.items.is_tax_exempt (art. 111 Ley 822), por eso no hay fila aquí.
CREATE TABLE catalog.taxes (
  id           BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  public_id    UUID NOT NULL DEFAULT gen_random_uuid() UNIQUE,
  country_id   BIGINT NOT NULL REFERENCES catalog.countries (id),
  code         VARCHAR(20) NOT NULL,
  name         VARCHAR(80) NOT NULL,
  rate         NUMERIC(5,4) NOT NULL,
  is_inclusive BOOLEAN NOT NULL DEFAULT FALSE,
  is_active    BOOLEAN NOT NULL DEFAULT TRUE,
  created_at TIMESTAMPTZ NOT NULL DEFAULT now(), created_by BIGINT,
  updated_at TIMESTAMPTZ NOT NULL DEFAULT now(), updated_by BIGINT,
  deleted_at TIMESTAMPTZ,
  CONSTRAINT uq_tax UNIQUE (country_id, code),
  CONSTRAINT ck_tax_rate CHECK (rate >= 0 AND rate <= 1)
);

--  v1 SOLO registra pagos; no procesa tarjetas.
CREATE TABLE catalog.payment_methods (
  id                       BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  public_id                UUID NOT NULL DEFAULT gen_random_uuid() UNIQUE,
  code                     VARCHAR(30) NOT NULL UNIQUE,
  name                     VARCHAR(80) NOT NULL,
  requires_reference       BOOLEAN NOT NULL DEFAULT FALSE,
  requires_amount_tendered BOOLEAN NOT NULL DEFAULT FALSE,
  requires_cash_session    BOOLEAN NOT NULL DEFAULT FALSE,
  position                 SMALLINT NOT NULL DEFAULT 0,
  is_active                BOOLEAN NOT NULL DEFAULT TRUE,
  created_at TIMESTAMPTZ NOT NULL DEFAULT now(), created_by BIGINT,
  updated_at TIMESTAMPTZ NOT NULL DEFAULT now(), updated_by BIGINT,
  deleted_at TIMESTAMPTZ
);


-- ═══ 3 · IDENTITY ═════════════════════════════════════════════════════════
--  tenant (restaurants), sucursales, personas, usuarios y roles.

-- ── Tenant ────────────────────────────────────────────────────────────────
CREATE TABLE identity.restaurants (
  id                  BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  public_id           UUID NOT NULL DEFAULT gen_random_uuid() UNIQUE,
  name                VARCHAR(150) NOT NULL,
  legal_name          VARCHAR(180),
  tax_id              VARCHAR(40) UNIQUE,      -- RUC (Registro Único de Contribuyente)
  timezone            VARCHAR(64) NOT NULL DEFAULT 'America/Managua',
  locale              VARCHAR(10) NOT NULL DEFAULT 'es-NI',
  default_language_id BIGINT NOT NULL REFERENCES catalog.languages (id),
  default_currency_id BIGINT NOT NULL REFERENCES catalog.currencies (id),
  default_country_id  BIGINT REFERENCES catalog.countries (id),
  base_currency_id    BIGINT NOT NULL REFERENCES catalog.currencies (id),
  settings            JSONB NOT NULL DEFAULT '{}'::jsonb,
  is_active           BOOLEAN NOT NULL DEFAULT TRUE,
  created_at TIMESTAMPTZ NOT NULL DEFAULT now(), created_by BIGINT,
  updated_at TIMESTAMPTZ NOT NULL DEFAULT now(), updated_by BIGINT,
  deleted_at TIMESTAMPTZ
);

CREATE TABLE identity.branches (
  id            BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  public_id     UUID NOT NULL DEFAULT gen_random_uuid() UNIQUE,
  restaurant_id BIGINT NOT NULL REFERENCES identity.restaurants (id) ON DELETE CASCADE,
  city_id       BIGINT NOT NULL REFERENCES catalog.cities (id),
  code          VARCHAR(20) NOT NULL,
  name          VARCHAR(120) NOT NULL,
  address       TEXT,
  phone         VARCHAR(32),
  timezone      VARCHAR(64),
  is_default    BOOLEAN NOT NULL DEFAULT FALSE,
  opens_at      TIME,
  closes_at     TIME,
  is_active     BOOLEAN NOT NULL DEFAULT TRUE,
  created_at TIMESTAMPTZ NOT NULL DEFAULT now(), created_by BIGINT,
  updated_at TIMESTAMPTZ NOT NULL DEFAULT now(), updated_by BIGINT,
  deleted_at TIMESTAMPTZ,
  CONSTRAINT uq_branch_code            UNIQUE (restaurant_id, code),
  CONSTRAINT uq_branch_id_restaurant   UNIQUE (id, restaurant_id),
  CONSTRAINT ck_branch_hours CHECK (closes_at IS NULL OR opens_at IS NULL OR closes_at > opens_at)
);

--  Un único branch marcado como default por restaurante
CREATE UNIQUE INDEX uq_branch_one_default ON identity.branches (restaurant_id) WHERE is_default;

-- ── Personas ──────────────────────────────────────────────────────────────
--  DECISIÓN DE DISEÑO: personas y usuarios son GLOBALES (sin restaurant_id).
--  Un mismo login puede trabajar en varios restaurantes (user_branches).
--  CONSECUENCIA ASUMIDA: identity.users.username es UNIQUE global, por lo que
--  dos restaurantes distintos NO pueden tener un usuario con el mismo nombre.
--  Ver docs/ADR/0001-usuarios-globales.md
CREATE TABLE identity.people (
  id               BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  public_id        UUID NOT NULL DEFAULT gen_random_uuid() UNIQUE,
  first_name       VARCHAR(80) NOT NULL,
  last_name        VARCHAR(80) NOT NULL,
  display_name     VARCHAR(160),
  birth_date       DATE,
  gender           VARCHAR(20),
  photo_url        TEXT,
  document_type_id BIGINT REFERENCES catalog.document_types (id),
  document_number  VARCHAR(50),
  city_id          BIGINT REFERENCES catalog.cities (id),
  is_active        BOOLEAN NOT NULL DEFAULT TRUE,
  created_at TIMESTAMPTZ NOT NULL DEFAULT now(), created_by BIGINT,
  updated_at TIMESTAMPTZ NOT NULL DEFAULT now(), updated_by BIGINT,
  deleted_at TIMESTAMPTZ,
  CONSTRAINT ck_person_doc CHECK ((document_type_id IS NULL) = (document_number IS NULL))
);

CREATE INDEX ix_people_name ON identity.people (last_name, first_name);
CREATE UNIQUE INDEX uq_people_document ON identity.people (document_type_id, document_number)
  WHERE document_number IS NOT NULL;

CREATE TABLE identity.people_contacts (
  id            BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  public_id     UUID NOT NULL DEFAULT gen_random_uuid() UNIQUE,
  person_id     BIGINT NOT NULL REFERENCES identity.people (id) ON DELETE CASCADE,
  contact_type  VARCHAR(10) NOT NULL,
  contact_value VARCHAR(255) NOT NULL,
  is_primary    BOOLEAN NOT NULL DEFAULT FALSE,
  is_active     BOOLEAN NOT NULL DEFAULT TRUE,
  verified_at   TIMESTAMPTZ,
  created_at TIMESTAMPTZ NOT NULL DEFAULT now(), created_by BIGINT,
  updated_at TIMESTAMPTZ NOT NULL DEFAULT now(), updated_by BIGINT,
  deleted_at TIMESTAMPTZ,
  CONSTRAINT ck_contact_type CHECK (contact_type IN ('EMAIL','PHONE','ADDRESS'))
);

--  Permite varias líneas de cada tipo (teléfono de trabajo + personal)
CREATE UNIQUE INDEX uq_contact_value ON identity.people_contacts (person_id, contact_type, contact_value);

-- ── Autoridades (roles) ───────────────────────────────────────────────────
CREATE TABLE identity.authorities (
  id          BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  public_id   UUID NOT NULL DEFAULT gen_random_uuid() UNIQUE,
  code        VARCHAR(40) NOT NULL UNIQUE,
  name        VARCHAR(80) NOT NULL,
  description VARCHAR(255),
  position    SMALLINT NOT NULL DEFAULT 0,
  is_active   BOOLEAN NOT NULL DEFAULT TRUE,
  created_at TIMESTAMPTZ NOT NULL DEFAULT now(), created_by BIGINT,
  updated_at TIMESTAMPTZ NOT NULL DEFAULT now(), updated_by BIGINT,
  deleted_at TIMESTAMPTZ,
  CONSTRAINT ck_auth_code CHECK (code ~ '^[A-Z][A-Z0-9_]*$')
);

-- ── Usuarios ──────────────────────────────────────────────────────────────
CREATE TABLE identity.users (
  id                   BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  public_id            UUID NOT NULL DEFAULT gen_random_uuid() UNIQUE,
  person_id            BIGINT NOT NULL UNIQUE REFERENCES identity.people (id),
  username             VARCHAR(60) NOT NULL UNIQUE,        -- login · ÚNICO GLOBAL
  password_hash        TEXT NOT NULL,
  password_updated_at  TIMESTAMPTZ,
  must_change_password BOOLEAN NOT NULL DEFAULT TRUE,
  failed_count         SMALLINT NOT NULL DEFAULT 0,
  lockout_end          TIMESTAMPTZ,
  last_login_at        TIMESTAMPTZ,
  last_login_ip        INET,
  is_active            BOOLEAN NOT NULL DEFAULT TRUE,
  created_at TIMESTAMPTZ NOT NULL DEFAULT now(), created_by BIGINT,
  updated_at TIMESTAMPTZ NOT NULL DEFAULT now(), updated_by BIGINT,
  deleted_at TIMESTAMPTZ
);

CREATE TABLE identity.user_authorities (
  id           BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  user_id      BIGINT NOT NULL REFERENCES identity.users (id) ON DELETE CASCADE,
  authority_id BIGINT NOT NULL REFERENCES identity.authorities (id),
  granted_at   TIMESTAMPTZ NOT NULL DEFAULT now(),
  granted_by   BIGINT REFERENCES identity.users (id),
  expires_at   TIMESTAMPTZ,
  created_at TIMESTAMPTZ NOT NULL DEFAULT now(), created_by BIGINT,
  updated_at TIMESTAMPTZ NOT NULL DEFAULT now(), updated_by BIGINT
);

--  Un usuario NO puede tener el mismo rol dos veces
CREATE UNIQUE INDEX uq_user_authority ON identity.user_authorities (user_id, authority_id);

--  Vínculo usuario ↔ sucursal. Permite que un login sirva a varios
--  restaurantes y es por donde la API filtra el tenant.
CREATE TABLE identity.user_branches (
  id         BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  user_id    BIGINT NOT NULL REFERENCES identity.users (id) ON DELETE CASCADE,
  branch_id  BIGINT NOT NULL REFERENCES identity.branches (id) ON DELETE CASCADE,
  is_primary BOOLEAN NOT NULL DEFAULT FALSE,
  created_at TIMESTAMPTZ NOT NULL DEFAULT now(), created_by BIGINT,
  updated_at TIMESTAMPTZ NOT NULL DEFAULT now(), updated_by BIGINT
);

CREATE UNIQUE INDEX uq_user_branch ON identity.user_branches (user_id, branch_id);

CREATE TABLE identity.login_attempts (
  id             BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  public_id      UUID NOT NULL DEFAULT gen_random_uuid() UNIQUE,
  user_id        BIGINT REFERENCES identity.users (id),
  username_try   VARCHAR(60) NOT NULL,
  ip             INET,
  user_agent     VARCHAR(255),
  was_successful BOOLEAN NOT NULL,
  failure_reason VARCHAR(60),
  created_at TIMESTAMPTZ NOT NULL DEFAULT now(), created_by BIGINT,
  updated_at TIMESTAMPTZ NOT NULL DEFAULT now(), updated_by BIGINT
);

CREATE INDEX ix_login_attempts_user ON identity.login_attempts (user_id, created_at DESC);
CREATE INDEX ix_login_attempts_ip   ON identity.login_attempts (ip, created_at DESC);


-- ═══ 4 · CATALOG — MENÚ, MODIFICADORES, ESTACIONES ════════════════════════
--  Datos por tenant: llevan restaurant_id y están cubiertos por RLS.

-- ── Estaciones de preparación (KDS) ───────────────────────────────────────
--  UNA fila por estación. Es el atributo que enruta cada línea de la comanda
--  a la pantalla KDS correcta (cocina, bar, parrilla, postre).
--  branch_id NULL = estación compartida por todas las sucursales.
CREATE TABLE catalog.stations (
  id            BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  public_id     UUID NOT NULL DEFAULT gen_random_uuid() UNIQUE,
  restaurant_id BIGINT NOT NULL REFERENCES identity.restaurants (id) ON DELETE CASCADE,
  branch_id     BIGINT,
  code          VARCHAR(30) NOT NULL,
  name          VARCHAR(80) NOT NULL,
  color         VARCHAR(9),
  position      SMALLINT NOT NULL DEFAULT 0,
  is_active     BOOLEAN NOT NULL DEFAULT TRUE,
  created_at TIMESTAMPTZ NOT NULL DEFAULT now(), created_by BIGINT,
  updated_at TIMESTAMPTZ NOT NULL DEFAULT now(), updated_by BIGINT,
  deleted_at TIMESTAMPTZ,
  CONSTRAINT uq_station_code    UNIQUE (restaurant_id, code),
  CONSTRAINT uq_station_id_rest UNIQUE (id, restaurant_id)
);

-- ── Menú ──────────────────────────────────────────────────────────────────
--  branch_id OBLIGATORIO: el menú es por sucursal (decisión de diseño).
CREATE TABLE catalog.categories (
  id            BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  public_id     UUID NOT NULL DEFAULT gen_random_uuid() UNIQUE,
  restaurant_id BIGINT NOT NULL REFERENCES identity.restaurants (id) ON DELETE CASCADE,
  branch_id     BIGINT NOT NULL,
  name          VARCHAR(120) NOT NULL,
  name_en       VARCHAR(120),
  description   TEXT,
  description_en TEXT,
  image_url     TEXT,
  position      SMALLINT NOT NULL DEFAULT 0,
  is_active     BOOLEAN NOT NULL DEFAULT TRUE,
  created_at TIMESTAMPTZ NOT NULL DEFAULT now(), created_by BIGINT,
  updated_at TIMESTAMPTZ NOT NULL DEFAULT now(), updated_by BIGINT,
  deleted_at TIMESTAMPTZ,
  CONSTRAINT fk_cat_branch  FOREIGN KEY (branch_id, restaurant_id) REFERENCES identity.branches (id, restaurant_id),
  CONSTRAINT uq_cat_id_rest  UNIQUE (id, restaurant_id)
);

--  ÍNDICE NO ÚNICO a propósito: la restricción única del modelo original
--  (uq_cat_position) se eliminó porque obligaba a reindexar toda la tabla en
--  cada reordenamiento del menú.
CREATE INDEX ix_cat_branch_pos ON catalog.categories (branch_id, restaurant_id, position);

--  station_id es lo que define a qué KDS viaja la línea de la comanda.
CREATE TABLE catalog.items (
  id            BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  public_id     UUID NOT NULL DEFAULT gen_random_uuid() UNIQUE,
  restaurant_id BIGINT NOT NULL REFERENCES identity.restaurants (id) ON DELETE CASCADE,
  branch_id     BIGINT NOT NULL,
  category_id   BIGINT NOT NULL,
  station_id    BIGINT NOT NULL,
  name          VARCHAR(150) NOT NULL,
  name_en       VARCHAR(150),
  description   TEXT,
  description_en TEXT,
  base_price    NUMERIC(12,2) NOT NULL,
  image_url     TEXT,
  is_available  BOOLEAN NOT NULL DEFAULT TRUE,
  position      SMALLINT NOT NULL DEFAULT 0,
  tax_id        BIGINT REFERENCES catalog.taxes (id),
  is_tax_exempt BOOLEAN NOT NULL DEFAULT FALSE,   -- exento de IVA (art. 111 Ley 822)
  is_active     BOOLEAN NOT NULL DEFAULT TRUE,
  created_at TIMESTAMPTZ NOT NULL DEFAULT now(), created_by BIGINT,
  updated_at TIMESTAMPTZ NOT NULL DEFAULT now(), updated_by BIGINT,
  deleted_at TIMESTAMPTZ,
  CONSTRAINT fk_item_category FOREIGN KEY (category_id, restaurant_id) REFERENCES catalog.categories (id, restaurant_id),
  CONSTRAINT fk_item_branch   FOREIGN KEY (branch_id,   restaurant_id) REFERENCES identity.branches (id, restaurant_id),
  CONSTRAINT fk_item_station  FOREIGN KEY (station_id,  restaurant_id) REFERENCES catalog.stations (id, restaurant_id),
  CONSTRAINT uq_item_id_rest   UNIQUE (id, restaurant_id),
  CONSTRAINT ck_item_price    CHECK (base_price >= 0)
);

CREATE INDEX ix_items_branch ON catalog.items (branch_id, restaurant_id, is_active, category_id, position);

-- ── Modificadores (grupos + opciones) ──────────────────────────────────────
--  Reemplaza rs_menu_item_extras, que no podía expresar grupos.
--  Ejemplo: grupo "Tamaño" (min 1, max 1) con opciones Chica/Mediana/Grande;
--            grupo "Agregados" (min 0, max 3) con queso/cebolla/tocino.
CREATE TABLE catalog.modifier_groups (
  id            BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  public_id     UUID NOT NULL DEFAULT gen_random_uuid() UNIQUE,
  restaurant_id BIGINT NOT NULL REFERENCES identity.restaurants (id) ON DELETE CASCADE,
  branch_id     BIGINT,
  name          VARCHAR(120) NOT NULL,
  min_select    SMALLINT NOT NULL DEFAULT 1,
  max_select    SMALLINT NOT NULL DEFAULT 1,
  is_required   BOOLEAN NOT NULL DEFAULT FALSE,
  position      SMALLINT NOT NULL DEFAULT 0,
  is_active     BOOLEAN NOT NULL DEFAULT TRUE,
  created_at TIMESTAMPTZ NOT NULL DEFAULT now(), created_by BIGINT,
  updated_at TIMESTAMPTZ NOT NULL DEFAULT now(), updated_by BIGINT,
  deleted_at TIMESTAMPTZ,
  CONSTRAINT uq_mg_id_rest UNIQUE (id, restaurant_id),
  CONSTRAINT ck_mg_select CHECK (min_select >= 0 AND max_select >= 1 AND min_select <= max_select)
);

CREATE TABLE catalog.modifier_options (
  id                BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  public_id         UUID NOT NULL DEFAULT gen_random_uuid() UNIQUE,
  restaurant_id     BIGINT NOT NULL,
  modifier_group_id BIGINT NOT NULL,
  name              VARCHAR(120) NOT NULL,
  name_en           VARCHAR(120),
  price_delta       NUMERIC(12,2) NOT NULL DEFAULT 0,   -- delta de precio
  is_default        BOOLEAN NOT NULL DEFAULT FALSE,
  position          SMALLINT NOT NULL DEFAULT 0,
  is_active         BOOLEAN NOT NULL DEFAULT TRUE,
  created_at TIMESTAMPTZ NOT NULL DEFAULT now(), created_by BIGINT,
  updated_at TIMESTAMPTZ NOT NULL DEFAULT now(), updated_by BIGINT,
  deleted_at TIMESTAMPTZ,
  CONSTRAINT fk_mo_group FOREIGN KEY (modifier_group_id, restaurant_id) REFERENCES catalog.modifier_groups (id, restaurant_id),
  CONSTRAINT uq_mo_id_rest UNIQUE (id, restaurant_id),
  CONSTRAINT ck_mo_delta CHECK (price_delta >= 0)
);

CREATE INDEX ix_mo_group ON catalog.modifier_options (modifier_group_id, position);

--  Qué grupos aplican a qué producto. min/max nullable = hereda del grupo.
CREATE TABLE catalog.item_modifier_groups (
  id                BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  public_id         UUID NOT NULL DEFAULT gen_random_uuid() UNIQUE,
  restaurant_id     BIGINT NOT NULL,
  item_id           BIGINT NOT NULL,
  modifier_group_id BIGINT NOT NULL,
  min_select        SMALLINT,
  max_select        SMALLINT,
  position          SMALLINT NOT NULL DEFAULT 0,
  is_active         BOOLEAN NOT NULL DEFAULT TRUE,
  created_at TIMESTAMPTZ NOT NULL DEFAULT now(), created_by BIGINT,
  updated_at TIMESTAMPTZ NOT NULL DEFAULT now(), updated_by BIGINT,
  deleted_at TIMESTAMPTZ,
  CONSTRAINT fk_img_item  FOREIGN KEY (item_id,           restaurant_id) REFERENCES catalog.items (id, restaurant_id),
  CONSTRAINT fk_img_group FOREIGN KEY (modifier_group_id, restaurant_id) REFERENCES catalog.modifier_groups (id, restaurant_id),
  CONSTRAINT uq_img_group UNIQUE (item_id, modifier_group_id)
);


-- ═══ 5 · TABLES — ZONAS, MESAS, ASIGNACIÓN ════════════════════════════════

CREATE TABLE tables.zones (
  id            BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  public_id     UUID NOT NULL DEFAULT gen_random_uuid() UNIQUE,
  restaurant_id BIGINT NOT NULL REFERENCES identity.restaurants (id) ON DELETE CASCADE,
  branch_id     BIGINT NOT NULL,
  name          VARCHAR(100) NOT NULL,
  position      SMALLINT NOT NULL DEFAULT 0,
  is_active     BOOLEAN NOT NULL DEFAULT TRUE,
  created_at TIMESTAMPTZ NOT NULL DEFAULT now(), created_by BIGINT,
  updated_at TIMESTAMPTZ NOT NULL DEFAULT now(), updated_by BIGINT,
  deleted_at TIMESTAMPTZ,
  CONSTRAINT fk_zone_branch FOREIGN KEY (branch_id, restaurant_id) REFERENCES identity.branches (id, restaurant_id),
  CONSTRAINT uq_zone_id_rest UNIQUE (id, restaurant_id)
);

CREATE INDEX ix_zone_branch_pos ON tables.zones (branch_id, restaurant_id, position);

--  Sin pos_x/pos_y/pos_w/pos_h: el mapa visual del salón se resuelve en
--  frontend. 'position' solo ordena las mesas dentro de la zona.
CREATE TABLE tables.dining_tables (
  id            BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  public_id     UUID NOT NULL DEFAULT gen_random_uuid() UNIQUE,
  restaurant_id BIGINT NOT NULL REFERENCES identity.restaurants (id) ON DELETE CASCADE,
  branch_id     BIGINT NOT NULL,
  zone_id       BIGINT NOT NULL,
  code          VARCHAR(20) NOT NULL,
  name          VARCHAR(100) NOT NULL,
  min_capacity  SMALLINT NOT NULL DEFAULT 1,
  max_capacity  SMALLINT NOT NULL DEFAULT 4,
  is_combinable BOOLEAN NOT NULL DEFAULT FALSE,
  status        shared.table_status NOT NULL DEFAULT 'FREE',
  position      SMALLINT NOT NULL DEFAULT 0,
  notes         TEXT,
  is_active     BOOLEAN NOT NULL DEFAULT TRUE,
  created_at TIMESTAMPTZ NOT NULL DEFAULT now(), created_by BIGINT,
  updated_at TIMESTAMPTZ NOT NULL DEFAULT now(), updated_by BIGINT,
  deleted_at TIMESTAMPTZ,
  CONSTRAINT fk_table_zone   FOREIGN KEY (zone_id,   restaurant_id) REFERENCES tables.zones (id, restaurant_id),
  CONSTRAINT fk_table_branch FOREIGN KEY (branch_id, restaurant_id) REFERENCES identity.branches (id, restaurant_id),
  CONSTRAINT uq_table_code   UNIQUE (branch_id, code),
  CONSTRAINT uq_table_id_rest UNIQUE (id, restaurant_id),
  CONSTRAINT uq_table_id_branch UNIQUE (id, branch_id),
  CONSTRAINT ck_table_capacity CHECK (min_capacity >= 1 AND max_capacity >= min_capacity)
);

CREATE INDEX ix_table_branch ON tables.dining_tables (branch_id, zone_id, position);

--  Mesero ↔ mesa. Se conserva en v1: el KDS muestra pedidos por mesero y los
--  reportes atribuyen ventas y propinas.
CREATE TABLE tables.table_assignments (
  id            BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  public_id     UUID NOT NULL DEFAULT gen_random_uuid() UNIQUE,
  restaurant_id BIGINT NOT NULL,
  branch_id     BIGINT NOT NULL,
  table_id      BIGINT NOT NULL REFERENCES tables.dining_tables (id) ON DELETE CASCADE,
  waiter_id     BIGINT NOT NULL REFERENCES identity.users (id) ON DELETE CASCADE,
  assigned_at   TIMESTAMPTZ NOT NULL DEFAULT now(),
  assigned_by   BIGINT REFERENCES identity.users (id),
  released_at   TIMESTAMPTZ,
  released_by   BIGINT REFERENCES identity.users (id),
  created_at TIMESTAMPTZ NOT NULL DEFAULT now(), created_by BIGINT,
  updated_at TIMESTAMPTZ NOT NULL DEFAULT now(), updated_by BIGINT,
  CONSTRAINT uq_ta_id_rest UNIQUE (id, restaurant_id),
  CONSTRAINT ck_ta_release CHECK (released_at IS NULL OR released_at >= assigned_at)
);

--  Solo una asignación activa por mesa, y un mesero no puede tener dos mesas
--  activas a la vez.
CREATE UNIQUE INDEX uq_ta_active_waiter ON tables.table_assignments (table_id) WHERE released_at IS NULL;
CREATE UNIQUE INDEX uq_ta_active_table  ON tables.table_assignments (waiter_id) WHERE released_at IS NULL;


-- ═══ 6 · ORDERS ══════════════════════════════════════════════════════════

CREATE TABLE orders.orders (
  id            BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  public_id     UUID NOT NULL DEFAULT gen_random_uuid() UNIQUE,
  restaurant_id BIGINT NOT NULL REFERENCES identity.restaurants (id) ON DELETE CASCADE,
  branch_id     BIGINT NOT NULL,
  table_id      BIGINT,
  reservation_id BIGINT,                        -- se activa en fase 2
  order_number  VARCHAR(30) NOT NULL,
  order_type    shared.order_type NOT NULL DEFAULT 'DINE_IN',
  status        shared.order_status NOT NULL DEFAULT 'OPEN',
  currency_id   BIGINT NOT NULL,                 -- moneda en la que se cobró
  exchange_rate NUMERIC(14,6) NOT NULL DEFAULT 1,-- hacia base_currency del restaurante
  customer_name VARCHAR(160),
  customer_phone VARCHAR(32),
  covers        SMALLINT NOT NULL DEFAULT 1,
  -- Totales denormalizados (cache de líneas + pagos). Los recalcula la app.
  subtotal      NUMERIC(12,2) NOT NULL DEFAULT 0,
  discount_amount NUMERIC(12,2) NOT NULL DEFAULT 0,
  tax_amount    NUMERIC(12,2) NOT NULL DEFAULT 0,
  total         NUMERIC(12,2) NOT NULL DEFAULT 0,
  tip_amount    NUMERIC(12,2) NOT NULL DEFAULT 0,
  paid_amount   NUMERIC(12,2) NOT NULL DEFAULT 0,
  balance_due   NUMERIC(12,2) NOT NULL DEFAULT 0,
  opened_by     BIGINT,                         -- mesero que abrió
  opened_at     TIMESTAMPTZ NOT NULL DEFAULT now(),
  closed_by     BIGINT,
  closed_at     TIMESTAMPTZ,
  notes         TEXT,
  created_at TIMESTAMPTZ NOT NULL DEFAULT now(), created_by BIGINT,
  updated_at TIMESTAMPTZ NOT NULL DEFAULT now(), updated_by BIGINT,
  deleted_at TIMESTAMPTZ,
  CONSTRAINT fk_order_branch FOREIGN KEY (branch_id, restaurant_id) REFERENCES identity.branches (id, restaurant_id),
  CONSTRAINT fk_order_table  FOREIGN KEY (table_id,  branch_id)      REFERENCES tables.dining_tables (id, branch_id),
  CONSTRAINT uq_order_number UNIQUE (branch_id, order_number),
  CONSTRAINT uq_order_id_rest UNIQUE (id, restaurant_id),
  CONSTRAINT ck_order_table  CHECK (order_type <> 'DINE_IN' OR table_id IS NOT NULL),
  CONSTRAINT ck_order_closed CHECK ((closed_at IS NULL) = (closed_by IS NULL)),
  CONSTRAINT ck_order_amounts CHECK (subtotal >= 0 AND discount_amount >= 0
    AND tax_amount >= 0 AND total >= 0 AND tip_amount >= 0 AND paid_amount >= 0
    AND discount_amount <= subtotal)
);

--  Una sola orden abierta por mesa. Ahora legible gracias al ENUM: el modelo
--  original usaba status_id IN (1,2,3,4), acoplado a los ids del seed.
CREATE UNIQUE INDEX uq_order_open_per_table ON orders.orders (table_id)
  WHERE table_id IS NOT NULL
    AND status IN ('OPEN','IN_PREPARATION','READY','SERVED','BILLING');

CREATE INDEX ix_orders_branch_date ON orders.orders (branch_id, restaurant_id, opened_at DESC);
CREATE INDEX ix_orders_waiter      ON orders.orders (opened_by, opened_at DESC);

--  restaurant_id se denormaliza para que RLS aplique también a las líneas.
CREATE TABLE orders.order_items (
  id              BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  public_id       UUID NOT NULL DEFAULT gen_random_uuid() UNIQUE,
  restaurant_id   BIGINT NOT NULL,
  order_id        BIGINT NOT NULL,
  item_id         BIGINT,                        -- → catalog.items (otro servicio, sin FK)
  station_id      BIGINT,                        -- snapshot: a qué KDS va esta línea
  status          shared.order_item_status NOT NULL DEFAULT 'QUEUED',
  -- SNAPSHOT HISTÓRICO: si el producto cambia de nombre/precio, el ticket
  -- antiguo no se altera. Mismo criterio que el modelo original.
  name            VARCHAR(150) NOT NULL,
  description     TEXT,
  unit_price      NUMERIC(12,2) NOT NULL,
  quantity        NUMERIC(14,4) NOT NULL DEFAULT 1,
  modifiers_total NUMERIC(12,2) NOT NULL DEFAULT 0,
  discount_amount NUMERIC(12,2) NOT NULL DEFAULT 0,
  discount_reason VARCHAR(120),
  -- SNAPSHOT FISCAL: guardar tasa y monto evita que un cambio de alícuota
  -- altere los totales ya facturados.
  tax_id          BIGINT,                        -- → catalog.taxes
  tax_rate        NUMERIC(5,4),
  tax_amount      NUMERIC(12,2) NOT NULL DEFAULT 0,
  is_tax_exempt   BOOLEAN NOT NULL DEFAULT FALSE,
  tax_inclusive   BOOLEAN NOT NULL DEFAULT FALSE,
  line_total      NUMERIC(14,2) GENERATED ALWAYS AS (
                    ROUND(((unit_price + modifiers_total) * quantity - discount_amount)::numeric, 2)
                  ) STORED,
  notes           TEXT,
  voided_by       BIGINT,
  voided_at       TIMESTAMPTZ,
  void_reason     TEXT,
  created_at TIMESTAMPTZ NOT NULL DEFAULT now(), created_by BIGINT,
  updated_at TIMESTAMPTZ NOT NULL DEFAULT now(), updated_by BIGINT,
  CONSTRAINT fk_oi_order FOREIGN KEY (order_id, restaurant_id) REFERENCES orders.orders (id, restaurant_id),
  CONSTRAINT uq_oi_id_rest UNIQUE (id, restaurant_id),
  CONSTRAINT ck_oi_qty   CHECK (quantity > 0),
  CONSTRAINT ck_oi_price CHECK (unit_price >= 0 AND modifiers_total >= 0 AND discount_amount >= 0),
  CONSTRAINT ck_oi_tax   CHECK (tax_amount >= 0),
  -- Sustituye ck_oi_source del modelo original: aquí SÍ se permite que una
  -- línea tenga producto base Y modificadores (viven en order_item_modifiers).
  CONSTRAINT ck_oi_exempt CHECK (NOT is_tax_exempt OR tax_amount = 0)
);

CREATE INDEX ix_oi_order   ON orders.order_items (order_id);
--  Índice del KDS: líneas por estación que aún no están servidas
CREATE INDEX ix_oi_kds     ON orders.order_items (station_id, status, order_id);

--  Modificadores elegidos en cada línea, con snapshot de nombre y precio.
CREATE TABLE orders.order_item_modifiers (
  id                  BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  public_id           UUID NOT NULL DEFAULT gen_random_uuid() UNIQUE,
  restaurant_id       BIGINT NOT NULL,
  order_item_id       BIGINT NOT NULL,
  modifier_option_id  BIGINT,                    -- → catalog.modifier_options
  modifier_group_id   BIGINT,                    -- → catalog.modifier_groups
  name                VARCHAR(120) NOT NULL,     -- snapshot
  price_delta         NUMERIC(12,2) NOT NULL DEFAULT 0,
  notes               TEXT,
  created_at TIMESTAMPTZ NOT NULL DEFAULT now(), created_by BIGINT,
  updated_at TIMESTAMPTZ NOT NULL DEFAULT now(), updated_by BIGINT,
  CONSTRAINT fk_oim_item FOREIGN KEY (order_item_id, restaurant_id) REFERENCES orders.order_items (id, restaurant_id),
  CONSTRAINT ck_oim_delta CHECK (price_delta >= 0)
);

CREATE INDEX ix_oim_item ON orders.order_item_modifiers (order_item_id);

--  Auditoría de transiciones. Índice NORMAL (no UNIQUE): el modelo original
--  impedía volver a un estado ya visitado (p. ej. la cocina manda algo extra y
--  la orden regresa a IN_PREPARATION), lo que rompía flujos reales.
CREATE TABLE orders.order_status_history (
  id            BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  public_id     UUID NOT NULL DEFAULT gen_random_uuid() UNIQUE,
  restaurant_id BIGINT NOT NULL,
  order_id      BIGINT NOT NULL,
  from_status   shared.order_status,
  to_status     shared.order_status NOT NULL,
  changed_at    TIMESTAMPTZ NOT NULL DEFAULT now(),
  changed_by    BIGINT,
  reason        TEXT,
  created_at TIMESTAMPTZ NOT NULL DEFAULT now(), created_by BIGINT,
  updated_at TIMESTAMPTZ NOT NULL DEFAULT now(), updated_by BIGINT,
  CONSTRAINT fk_osh_order FOREIGN KEY (order_id, restaurant_id) REFERENCES orders.orders (id, restaurant_id)
);

CREATE INDEX ix_osh_order ON orders.order_status_history (order_id, changed_at DESC);


-- ═══ 7 · PAYMENTS — CAJA Y COBRO ══════════════════════════════════════════
--  Sustituye los campos de pago que el modelo original aplastaba en
--  rs_orders (payment_method_id, paid_amount, amount_received, change_given…).

CREATE TABLE payments.cash_registers (
  id            BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  public_id     UUID NOT NULL DEFAULT gen_random_uuid() UNIQUE,
  restaurant_id BIGINT NOT NULL REFERENCES identity.restaurants (id) ON DELETE CASCADE,
  branch_id     BIGINT NOT NULL,
  name          VARCHAR(80) NOT NULL,
  is_active     BOOLEAN NOT NULL DEFAULT TRUE,
  created_at TIMESTAMPTZ NOT NULL DEFAULT now(), created_by BIGINT,
  updated_at TIMESTAMPTZ NOT NULL DEFAULT now(), updated_by BIGINT,
  deleted_at TIMESTAMPTZ,
  CONSTRAINT fk_cr_branch  FOREIGN KEY (branch_id, restaurant_id) REFERENCES identity.branches (id, restaurant_id),
  CONSTRAINT uq_cr_name    UNIQUE (branch_id, name),
  CONSTRAINT uq_cr_id_rest UNIQUE (id, restaurant_id)
);

--  Turno de caja con arqueo de cierre.
CREATE TABLE payments.cash_sessions (
  id               BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  public_id        UUID NOT NULL DEFAULT gen_random_uuid() UNIQUE,
  restaurant_id    BIGINT NOT NULL,
  branch_id        BIGINT NOT NULL,
  cash_register_id BIGINT NOT NULL,
  status           shared.cash_session_status NOT NULL DEFAULT 'OPEN',
  opening_amount   NUMERIC(12,2) NOT NULL DEFAULT 0,   -- fondo con el que abre
  counted_amount   NUMERIC(12,2),                      -- cuánto contó al cerrar
  expected_amount  NUMERIC(12,2),                      -- efectivo esperado por el sistema
  difference       NUMERIC(12,2),                      -- descuadre
  opened_by        BIGINT NOT NULL,
  opened_at        TIMESTAMPTZ NOT NULL DEFAULT now(),
  closed_by        BIGINT,
  closed_at        TIMESTAMPTZ,
  notes            TEXT,
  created_at TIMESTAMPTZ NOT NULL DEFAULT now(), created_by BIGINT,
  updated_at TIMESTAMPTZ NOT NULL DEFAULT now(), updated_by BIGINT,
  CONSTRAINT fk_cs_register FOREIGN KEY (cash_register_id, restaurant_id) REFERENCES payments.cash_registers (id, restaurant_id),
  CONSTRAINT uq_cs_id_rest    UNIQUE (id, restaurant_id),
  CONSTRAINT ck_cs_closed     CHECK ((status = 'CLOSED') = (closed_at IS NOT NULL)),
  CONSTRAINT ck_cs_arqueo     CHECK (counted_amount IS NULL
    OR (expected_amount IS NOT NULL AND difference = counted_amount - expected_amount))
);

--  Solo un turno abierto por caja a la vez
CREATE UNIQUE INDEX uq_cs_open_register ON payments.cash_sessions (cash_register_id) WHERE status = 'OPEN';

--  UN pago por fila → una orden puede cobrarse en efectivo + tarjeta.
--  collected_by_user_id = QUIÉN trajo el dinero (mesero).
--  recorded_by_user_id  = quién lo registró en caja.
CREATE TABLE payments.payments (
  id                   BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  public_id            UUID NOT NULL DEFAULT gen_random_uuid() UNIQUE,
  restaurant_id        BIGINT NOT NULL,
  branch_id            BIGINT NOT NULL,
  order_id             BIGINT NOT NULL,              -- → orders.orders (sin FK: otro servicio)
  cash_session_id      BIGINT,                       -- obligatorio si el método usa caja
  payment_method_id    BIGINT NOT NULL REFERENCES catalog.payment_methods (id),
  status               shared.payment_status NOT NULL DEFAULT 'COMPLETED',
  amount               NUMERIC(12,2) NOT NULL,
  tip_amount           NUMERIC(12,2) NOT NULL DEFAULT 0,
  amount_tendered      NUMERIC(12,2),                -- solo efectivo
  change_given         NUMERIC(12,2),                -- solo efectivo
  reference            VARCHAR(120),                 -- id de la tarjeta / transferencia
  collected_by_user_id BIGINT,                       -- mesero que cobró en la mesa
  recorded_by_user_id  BIGINT NOT NULL,              -- cajero que registró
  paid_at              TIMESTAMPTZ NOT NULL DEFAULT now(),
  created_at TIMESTAMPTZ NOT NULL DEFAULT now(), created_by BIGINT,
  updated_at TIMESTAMPTZ NOT NULL DEFAULT now(), updated_by BIGINT,
  deleted_at TIMESTAMPTZ,
  CONSTRAINT fk_pay_session FOREIGN KEY (cash_session_id, restaurant_id) REFERENCES payments.cash_sessions (id, restaurant_id),
  CONSTRAINT uq_pay_id_rest   UNIQUE (id, restaurant_id),
  CONSTRAINT ck_pay_amount   CHECK (amount > 0 AND tip_amount >= 0),
  CONSTRAINT ck_pay_tendered CHECK (change_given IS NULL
    OR (amount_tendered IS NOT NULL AND change_given = amount_tendered - amount))
);

CREATE INDEX ix_pay_order   ON payments.payments (order_id, status);
CREATE INDEX ix_pay_session ON payments.payments (cash_session_id);
CREATE INDEX ix_pay_waiter  ON payments.payments (collected_by_user_id, paid_at DESC);

--  Numeración de tickets por sucursal (control interno, no fiscal).
CREATE TABLE payments.ticket_sequences (
  branch_id     BIGINT PRIMARY KEY,
  current_value BIGINT NOT NULL DEFAULT 0
);

CREATE TABLE payments.tickets (
  id            BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  public_id     UUID NOT NULL DEFAULT gen_random_uuid() UNIQUE,
  restaurant_id BIGINT NOT NULL,
  branch_id     BIGINT NOT NULL,
  order_id      BIGINT NOT NULL,          -- → orders.orders (sin FK)
  ticket_number VARCHAR(30) NOT NULL,
  ticket_type   VARCHAR(20) NOT NULL DEFAULT 'SALE',   -- SALE | CANCEL
  subtotal      NUMERIC(12,2) NOT NULL DEFAULT 0,
  discount_amount NUMERIC(12,2) NOT NULL DEFAULT 0,
  tax_amount    NUMERIC(12,2) NOT NULL DEFAULT 0,
  total_amount  NUMERIC(12,2) NOT NULL DEFAULT 0,
  tip_amount    NUMERIC(12,2) NOT NULL DEFAULT 0,
  issued_by     BIGINT NOT NULL,
  issued_at     TIMESTAMPTZ NOT NULL DEFAULT now(),
  created_at TIMESTAMPTZ NOT NULL DEFAULT now(), created_by BIGINT,
  updated_at TIMESTAMPTZ NOT NULL DEFAULT now(), updated_by BIGINT,
  CONSTRAINT fk_ticket_branch FOREIGN KEY (branch_id, restaurant_id) REFERENCES identity.branches (id, restaurant_id),
  CONSTRAINT uq_ticket_number UNIQUE (branch_id, ticket_number),
  CONSTRAINT ck_ticket_type  CHECK (ticket_type IN ('SALE','CANCEL'))
);

CREATE INDEX ix_ticket_order ON payments.tickets (order_id);


-- ═══ 8 · REPORTING ════════════════════════════════════════════════════════
--  Solo lectura, alimentado por eventos de orders/payments. Si esta tabla se
--  borra, el POS sigue operando.

CREATE TABLE reporting.daily_sales (
  id            BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  public_id     UUID NOT NULL DEFAULT gen_random_uuid() UNIQUE,
  restaurant_id BIGINT NOT NULL,
  branch_id     BIGINT NOT NULL,
  business_date DATE NOT NULL,
  order_count   BIGINT NOT NULL DEFAULT 0,
  subtotal      NUMERIC(12,2) NOT NULL DEFAULT 0,
  discount_amount NUMERIC(12,2) NOT NULL DEFAULT 0,
  tax_amount    NUMERIC(12,2) NOT NULL DEFAULT 0,
  total         NUMERIC(12,2) NOT NULL DEFAULT 0,
  tip_amount    NUMERIC(12,2) NOT NULL DEFAULT 0,
  by_payment_method JSONB NOT NULL DEFAULT '{}'::jsonb,
  updated_at TIMESTAMPTZ NOT NULL DEFAULT now(),
  CONSTRAINT fk_ds_branch FOREIGN KEY (branch_id, restaurant_id) REFERENCES identity.branches (id, restaurant_id),
  CONSTRAINT uq_daily_sales UNIQUE (branch_id, business_date)
);

CREATE INDEX ix_ds_restaurant ON reporting.daily_sales (restaurant_id, business_date DESC);


-- ═══ 9 · SEED — NICARAGUA ════════════════════════════════════════════════

INSERT INTO catalog.languages (id, code, name) OVERRIDING SYSTEM VALUE VALUES
  (1, 'es', 'Español'),
  (2, 'en', 'Inglés');

--  Moneda base: C$ (córdoba). USD para pagos en divisa.
INSERT INTO catalog.currencies (id, code, name, symbol, decimal_places) OVERRIDING SYSTEM VALUE VALUES
  (1, 'NIO', 'Córdoba nicaragüense', 'C$', 2),
  (2, 'USD', 'Dólar estadounidense',     'US$', 2);

INSERT INTO catalog.countries (id, code, code_alpha3, name, phone_code, default_currency_id) OVERRIDING SYSTEM VALUE VALUES
  (1, 'NI', 'NIC', 'Nicaragua', '+505', 1);

--  Departamentos de Nicaragua (ISO 3166-2:NI)
INSERT INTO catalog.regions (id, country_id, code, name) OVERRIDING SYSTEM VALUE VALUES
  (1, 1, 'NI-MN', 'Managua'),
  (2, 1, 'NI-LE', 'León'),
  (3, 1, 'NI-GR', 'Granada'),
  (4, 1, 'NI-MS', 'Masaya'),
  (5, 1, 'NI-CI', 'Chinandega'),
  (6, 1, 'NI-RI', 'Rivas'),
  (7, 1, 'NI-MT', 'Matagalpa'),
  (8, 1, 'NI-BO', 'Boaco');

INSERT INTO catalog.cities (id, country_id, region_id, code, name, is_capital) OVERRIDING SYSTEM VALUE VALUES
  (1, 1, 1, 'MGA', 'Managua',    TRUE),
  (2, 1, 2, 'LEO', 'León',       FALSE),
  (3, 1, 3, 'GRA', 'Granada',    FALSE),
  (4, 1, 4, 'MSY', 'Masaya',     FALSE),
  (5, 1, 5, 'CHI', 'Chinandega', FALSE),
  (6, 1, 6, 'RIV', 'Rivas',      FALSE),
  (7, 1, 7, 'MTG', 'Matagalpa',  FALSE),
  (8, 1, 8, 'BOA', 'Boaco',      FALSE);

--  Identificaciones de personas. El RUC del negocio va en restaurants.tax_id.
INSERT INTO catalog.document_types (id, country_id, code, name, requires_expiry_date) OVERRIDING SYSTEM VALUE VALUES
  (1, 1, 'CEDULA',     'Cédula de identidad',          FALSE),
  (2, 1, 'CEDULA_RES', 'Cédula de residencia',         TRUE),
  (3, 1, 'PASAPORTE',  'Pasaporte',                    TRUE),
  (4, NULL, 'DPI',     'Documento de identificación',  FALSE);

--  IVA Nicaragua 15% · Ley 822 art. 109 LCT (verificado con DGI).
--  La exención se marca con items.is_tax_exempt, no con una tasa de 0.
INSERT INTO catalog.taxes (id, country_id, code, name, rate, is_inclusive) OVERRIDING SYSTEM VALUE VALUES
  (1, 1, 'IVA', 'IVA', 0.1500, FALSE);

--  v1 SOLO registra pagos; no procesa tarjetas.
INSERT INTO catalog.payment_methods (id, code, name, requires_reference, requires_amount_tendered, requires_cash_session, position)
  OVERRIDING SYSTEM VALUE VALUES
  (1, 'CASH',        'Efectivo',              FALSE, TRUE,  TRUE,  1),
  (2, 'DEBIT_CARD',  'Tarjeta de débito',      TRUE,  FALSE, FALSE, 2),
  (3, 'CREDIT_CARD', 'Tarjeta de crédito',     TRUE,  FALSE, FALSE, 3),
  (4, 'TRANSFER',    'Transferencia bancaria', TRUE,  FALSE, FALSE, 4);

INSERT INTO identity.authorities (id, code, name, description, position) OVERRIDING SYSTEM VALUE VALUES
  (1, 'ADMIN',   'Administrador', 'Acceso total',                  1),
  (2, 'OWNER',   'Propietario',   'Dueño del restaurante',         2),
  (3, 'MANAGER', 'Gerente',       'Gestión de restaurante y menú', 3),
  (4, 'CASHIER', 'Cajero',        'Cobros, cierres y arqueo',       4),
  (5, 'WAITER',  'Mesero',        'Toma de pedidos y servicio',     5),
  (6, 'COOK',    'Cocina',        'Marca de platos listos',       6),
  (7, 'HOST',    'Host',          'Reservas y asignación de mesas', 7);

--  OVERRIDING SYSTEM VALUE deja las secuencias atrás: se alinean con MAX(id).
SELECT setval(pg_get_serial_sequence('catalog.languages',      'id'), (SELECT MAX(id) FROM catalog.languages));
SELECT setval(pg_get_serial_sequence('catalog.currencies',     'id'), (SELECT MAX(id) FROM catalog.currencies));
SELECT setval(pg_get_serial_sequence('catalog.countries',      'id'), (SELECT MAX(id) FROM catalog.countries));
SELECT setval(pg_get_serial_sequence('catalog.regions',        'id'), (SELECT MAX(id) FROM catalog.regions));
SELECT setval(pg_get_serial_sequence('catalog.cities',         'id'), (SELECT MAX(id) FROM catalog.cities));
SELECT setval(pg_get_serial_sequence('catalog.document_types', 'id'), (SELECT MAX(id) FROM catalog.document_types));
SELECT setval(pg_get_serial_sequence('catalog.taxes',          'id'), (SELECT MAX(id) FROM catalog.taxes));
SELECT setval(pg_get_serial_sequence('catalog.payment_methods','id'), (SELECT MAX(id) FROM catalog.payment_methods));
SELECT setval(pg_get_serial_sequence('identity.authorities',   'id'), (SELECT MAX(id) FROM identity.authorities));


-- ═══ 10 · ROW LEVEL SECURITY ═════════════════════════════════════════════
--  Cada conexión de la aplicación ejecuta, al abrir la transacción:
--      SET LOCAL app.restaurant_id = '<restaurant_id del token>';
--
--  RLS NO se aplica con FORCE: el propietario de las tablas (usado por las
--  migraciones) la bypasea, y el rol de aplicación pos_app sí queda sujeto.
--  Por eso la aplicación DEBE conectar con pos_app, nunca con el owner.

--  ── 10.1 Rol de aplicación ───────────────────────────────────────────────
DO $$
BEGIN
  IF NOT EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'pos_app') THEN
    CREATE ROLE pos_app NOLOGIN;
  END IF;
END $$;

GRANT USAGE ON SCHEMA identity, catalog, tables, orders, payments, reporting, shared TO pos_app;
GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES
  IN SCHEMA identity, catalog, tables, orders, payments, reporting TO pos_app;
GRANT USAGE, SELECT ON ALL SEQUENCES
  IN SCHEMA identity, catalog, tables, orders, payments, reporting TO pos_app;

--  ── 10.2 Políticas ───────────────────────────────────────────────────────
--  SIN RLS por diseño (datos globales de referencia):
--    catalog.languages, catalog.currencies, catalog.countries,
--    catalog.regions, catalog.cities, catalog.document_types,
--    catalog.taxes, catalog.payment_methods
--
--  SIN RLS por decisión de diseño (globales, se filtran por tenant en la API
--  a través de identity.user_branches; riesgo aceptado, ver ADR 0001):
--    identity.people, identity.people_contacts, identity.users,
--    identity.user_authorities, identity.user_branches,
--    identity.login_attempts, identity.authorities

--  El propio tenant se filtra por su propio id
ALTER TABLE identity.restaurants ENABLE ROW LEVEL SECURITY;
CREATE POLICY p_restaurants_tenant ON identity.restaurants
  USING      (id = current_setting('app.restaurant_id', TRUE)::BIGINT)
  WITH CHECK (id = current_setting('app.restaurant_id', TRUE)::BIGINT);

--  Resto de tablas con restaurant_id, misma regla
DO $$
DECLARE t TEXT;
BEGIN
  FOREACH t IN ARRAY ARRAY[
    'identity.branches',
    'catalog.stations','catalog.categories','catalog.items',
    'catalog.modifier_groups','catalog.modifier_options','catalog.item_modifier_groups',
    'tables.zones','tables.dining_tables','tables.table_assignments',
    'orders.orders','orders.order_items','orders.order_item_modifiers','orders.order_status_history',
    'payments.cash_registers','payments.cash_sessions','payments.payments','payments.tickets',
    'reporting.daily_sales'
  ] LOOP
    EXECUTE format('ALTER TABLE %s ENABLE ROW LEVEL SECURITY', t);
    EXECUTE format($pol$
      CREATE POLICY p_tenant_isolation ON %s
        USING      (restaurant_id = current_setting('app.restaurant_id', TRUE)::BIGINT)
        WITH CHECK (restaurant_id = current_setting('app.restaurant_id', TRUE)::BIGINT)
    $pol$, t);
  END LOOP;
END $$;

--  payments.ticket_sequences no lleva restaurant_id (PK = branch_id):
--  se protege con una política equivalente resuelta por sucursal.
ALTER TABLE payments.ticket_sequences ENABLE ROW LEVEL SECURITY;
CREATE POLICY p_ticket_seq_tenant ON payments.ticket_sequences
  USING      (branch_id IN (SELECT b.id FROM identity.branches b
                            WHERE b.restaurant_id = current_setting('app.restaurant_id', TRUE)::BIGINT))
  WITH CHECK (branch_id IN (SELECT b.id FROM identity.branches b
                            WHERE b.restaurant_id = current_setting('app.restaurant_id', TRUE)::BIGINT));


-- ═══ 11 · FASE 2 — PENDIENTE ═════════════════════════════════════════════
--  Reservas. Excluido del v1 por decisión de alcance. Al implementarlo:
--
--  CREATE TYPE shared.reservation_status AS ENUM
--    ('PENDING','CONFIRMED','SEATED','COMPLETED','NO_SHOW','CANCELLED');
--
--  CREATE TABLE tables.table_reservations (
--    id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
--    public_id UUID NOT NULL DEFAULT gen_random_uuid() UNIQUE,
--    restaurant_id BIGINT NOT NULL,
--    branch_id   BIGINT NOT NULL,
--    table_id    BIGINT,
--    customer_name  VARCHAR(160) NOT NULL,
--    customer_phone VARCHAR(32),
--    party_size     SMALLINT NOT NULL,
--    reserved_from  TIMESTAMPTZ NOT NULL,
--    reserved_to    TIMESTAMPTZ NOT NULL,
--    occasion       VARCHAR(40),
--    notes          TEXT,
--    seated_at      TIMESTAMPTZ,
--    status         shared.reservation_status NOT NULL DEFAULT 'PENDING',
--    created_at TIMESTAMPTZ NOT NULL DEFAULT now(), created_by BIGINT,
--    updated_at TIMESTAMPTZ NOT NULL DEFAULT now(), updated_by BIGINT,
--    CONSTRAINT fk_res_table FOREIGN KEY (table_id, restaurant_id) REFERENCES tables.dining_tables (id, restaurant_id),
--    CONSTRAINT ck_res_range CHECK (reserved_to > reserved_from AND party_size >= 1)
--  );
--
--  ALTER TABLE tables.table_reservations ADD CONSTRAINT ex_res_no_overlap
--    EXCLUDE USING gist (table_id WITH =, tstzrange(reserved_from, reserved_to) WITH &&)
--    WHERE (status IN ('PENDING','CONFIRMED'));
--  ↑ Al ser ENUM y no tabla, se puede usar IN dentro del índice parcial.
--    (Con el modelo original, status_id = 1 estaba acoplado al seed.)


-- ═══ 12 · PENDIENTE — TRANSACTIONAL OUTBOX ══════════════════════════════
--  Falta la tabla de outbox para publicar eventos a RabbitMQ de forma fiable:
--  el commit de la orden y la publicación del evento deben ser atómicos.
--  Se definirá en shared/BuildingBlocks al construir el primer servicio, ya
--  que su forma depende del transporte elegido.
--  Tablas previstas: shared.outbox_messages, shared.inbox_messages.


-- ═══ 13 · SEED DEMO (opcional) ═══════════════════════════════════════════
--  Datos mínimos para levantar el vertical slice: identity → catalog →
--  tables. Ejecutar DESPUÉS de crear el rol pos_app.
--  Reemplaza los :placeholders con los ids reales devueltos por cada INSERT.
--
--  INSERT INTO identity.restaurants
--    (name, tax_id, default_language_id, default_currency_id,
--     default_country_id, base_currency_id)
--  OVERRIDING SYSTEM VALUE VALUES
--    ('Restaurante Demo', 'RUC-0000000000', 1, 1, 1, 1)
--  RETURNING id;                                        -- → :restaurant_id
--
--  INSERT INTO identity.branches
--    (restaurant_id, city_id, code, name, is_default)
--  OVERRIDING SYSTEM VALUE VALUES
--    (:restaurant_id, 1, 'MAIN', 'Sucursal Principal', TRUE)
--  RETURNING id;                                        -- → :branch_id
--
--  -- Estaciones: cada producto se enruta a uno de estos KDS
--  INSERT INTO catalog.stations (restaurant_id, code, name, color, position)
--  OVERRIDING SYSTEM VALUE VALUES
--    (:restaurant_id, 'COCINA',   'Cocina',   '#ef4444', 1),
--    (:restaurant_id, 'BAR',      'Bar',      '#3b82f6', 2),
--    (:restaurant_id, 'PARRILLA', 'Parrilla', '#f59e0b', 3),
--    (:restaurant_id, 'POSTRE',   'Postres',  '#a855f7', 4)
--  RETURNING id;                                        -- → :cocina_id
--
--  INSERT INTO tables.zones (restaurant_id, branch_id, name)
--  OVERRIDING SYSTEM VALUE VALUES
--    (:restaurant_id, :branch_id, 'Salón Principal')
--  RETURNING id;                                        -- → :zone_id
--
--  INSERT INTO tables.dining_tables
--    (restaurant_id, branch_id, zone_id, code, name, max_capacity)
--  OVERRIDING SYSTEM VALUE VALUES
--    (:restaurant_id, :branch_id, :zone_id, 'T-01', 'Mesa 1', 4),
--    (:restaurant_id, :branch_id, :zone_id, 'T-02', 'Mesa 2', 4),
--    (:restaurant_id, :branch_id, :zone_id, 'T-03', 'Mesa 3', 6),
--    (:restaurant_id, :branch_id, :zone_id, 'T-04', 'Mesa 4', 2);
--
--  SELECT setval(pg_get_serial_sequence('identity.restaurants',  'id'), (SELECT MAX(id) FROM identity.restaurants));
--  SELECT setval(pg_get_serial_sequence('identity.branches',      'id'), (SELECT MAX(id) FROM identity.branches));
--  SELECT setval(pg_get_serial_sequence('catalog.stations',       'id'), (SELECT MAX(id) FROM catalog.stations));
--  SELECT setval(pg_get_serial_sequence('tables.zones',           'id'), (SELECT MAX(id) FROM tables.zones));
--  SELECT setval(pg_get_serial_sequence('tables.dining_tables',   'id'), (SELECT MAX(id) FROM tables.dining_tables));
