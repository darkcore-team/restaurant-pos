-- Modelo de datos · Sistema de restaurantes · PostgreSQL 15+
-- 30 tablas. Solo DDL + seeds. Sin vistas, funciones ni triggers.

CREATE EXTENSION pgcrypto;
CREATE EXTENSION btree_gist;
CREATE SCHEMA rs;

SET search_path TO rs, public;

-- ═══ CATÁLOGOS ═══════════════════════════════════════════════════════════

CREATE TABLE rs_catalog_status (
  id           BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  public_id    UUID NOT NULL DEFAULT gen_random_uuid() UNIQUE,
  code         VARCHAR(30) NOT NULL UNIQUE,
  name         VARCHAR(80) NOT NULL,
  is_deletable BOOLEAN NOT NULL DEFAULT FALSE,
  position     SMALLINT NOT NULL DEFAULT 0,
  created_at TIMESTAMPTZ NOT NULL DEFAULT now(), created_by BIGINT,
  updated_at TIMESTAMPTZ NOT NULL DEFAULT now(), updated_by BIGINT,
  CONSTRAINT ck_status_code CHECK (code ~ '^[A-Z][A-Z0-9_]*$')
);

CREATE TABLE rs_catalog_languages (
  id          BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  public_id   UUID NOT NULL DEFAULT gen_random_uuid() UNIQUE,
  code        CHAR(2) NOT NULL UNIQUE,
  name        VARCHAR(80) NOT NULL,
  status_id   BIGINT NOT NULL,
  created_at TIMESTAMPTZ NOT NULL DEFAULT now(), created_by BIGINT,
  updated_at TIMESTAMPTZ NOT NULL DEFAULT now(), updated_by BIGINT,
  CONSTRAINT fk_lang_status FOREIGN KEY (status_id) REFERENCES rs_catalog_status (id),
  CONSTRAINT ck_lang_code CHECK (code ~ '^[a-z]{2}$')
);

CREATE TABLE rs_catalog_currencies (
  id             BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  public_id      UUID NOT NULL DEFAULT gen_random_uuid() UNIQUE,
  code           CHAR(3) NOT NULL UNIQUE,
  name           VARCHAR(80) NOT NULL,
  symbol         VARCHAR(8) NOT NULL,
  decimal_places SMALLINT NOT NULL DEFAULT 2,
  status_id      BIGINT NOT NULL,
  created_at TIMESTAMPTZ NOT NULL DEFAULT now(), created_by BIGINT,
  updated_at TIMESTAMPTZ NOT NULL DEFAULT now(), updated_by BIGINT,
  CONSTRAINT fk_curr_status FOREIGN KEY (status_id) REFERENCES rs_catalog_status (id),
  CONSTRAINT ck_curr_code CHECK (code ~ '^[A-Z]{3}$')
);

CREATE TABLE rs_catalog_countries (
  id                  BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  public_id           UUID NOT NULL DEFAULT gen_random_uuid() UNIQUE,
  code                CHAR(2) NOT NULL UNIQUE,
  code_alpha3         CHAR(3) UNIQUE,
  name                VARCHAR(100) NOT NULL,
  phone_code          VARCHAR(8),
  default_currency_id BIGINT,
  status_id           BIGINT NOT NULL,
  created_at TIMESTAMPTZ NOT NULL DEFAULT now(), created_by BIGINT,
  updated_at TIMESTAMPTZ NOT NULL DEFAULT now(), updated_by BIGINT,
  CONSTRAINT fk_country_status FOREIGN KEY (status_id) REFERENCES rs_catalog_status (id),
  CONSTRAINT fk_country_currency FOREIGN KEY (default_currency_id) REFERENCES rs_catalog_currencies (id)
);

CREATE TABLE rs_catalog_regions (
  id          BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  public_id   UUID NOT NULL DEFAULT gen_random_uuid() UNIQUE,
  country_id  BIGINT NOT NULL,
  code        VARCHAR(12) NOT NULL,
  name        VARCHAR(120) NOT NULL,
  status_id   BIGINT NOT NULL,
  created_at TIMESTAMPTZ NOT NULL DEFAULT now(), created_by BIGINT,
  updated_at TIMESTAMPTZ NOT NULL DEFAULT now(), updated_by BIGINT,
  CONSTRAINT fk_region_country FOREIGN KEY (country_id) REFERENCES rs_catalog_countries (id),
  CONSTRAINT fk_region_status FOREIGN KEY (status_id) REFERENCES rs_catalog_status (id),
  CONSTRAINT uq_region_code UNIQUE (country_id, code),
  CONSTRAINT uq_region_id_country UNIQUE (id, country_id)
);

CREATE TABLE rs_catalog_cities (
  id          BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  public_id   UUID NOT NULL DEFAULT gen_random_uuid() UNIQUE,
  country_id  BIGINT NOT NULL,
  region_id   BIGINT,
  code        VARCHAR(12),
  name        VARCHAR(120) NOT NULL,
  is_capital  BOOLEAN NOT NULL DEFAULT FALSE,
  status_id   BIGINT NOT NULL,
  created_at TIMESTAMPTZ NOT NULL DEFAULT now(), created_by BIGINT,
  updated_at TIMESTAMPTZ NOT NULL DEFAULT now(), updated_by BIGINT,
  CONSTRAINT fk_city_country FOREIGN KEY (country_id) REFERENCES rs_catalog_countries (id),
  CONSTRAINT fk_city_status FOREIGN KEY (status_id) REFERENCES rs_catalog_status (id),
  CONSTRAINT fk_city_region FOREIGN KEY (region_id, country_id) REFERENCES rs_catalog_regions (id, country_id),
  CONSTRAINT uq_city_name UNIQUE (country_id, name)
);

CREATE TABLE rs_catalog_document_types (
  id                   BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  public_id            UUID NOT NULL DEFAULT gen_random_uuid() UNIQUE,
  country_id           BIGINT,
  code                 VARCHAR(20) NOT NULL,
  name                 VARCHAR(120) NOT NULL,
  requires_expiry_date BOOLEAN NOT NULL DEFAULT FALSE,
  status_id            BIGINT NOT NULL,
  created_at TIMESTAMPTZ NOT NULL DEFAULT now(), created_by BIGINT,
  updated_at TIMESTAMPTZ NOT NULL DEFAULT now(), updated_by BIGINT,
  CONSTRAINT fk_doctype_country FOREIGN KEY (country_id) REFERENCES rs_catalog_countries (id),
  CONSTRAINT fk_doctype_status FOREIGN KEY (status_id) REFERENCES rs_catalog_status (id),
  CONSTRAINT uq_doctype UNIQUE NULLS NOT DISTINCT (country_id, code)
);

CREATE TABLE rs_catalog_payment_methods (
  id                       BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  public_id                UUID NOT NULL DEFAULT gen_random_uuid() UNIQUE,
  code                     VARCHAR(30) NOT NULL UNIQUE,
  name                     VARCHAR(80) NOT NULL,
  requires_reference       BOOLEAN NOT NULL DEFAULT FALSE,
  requires_amount_tendered BOOLEAN NOT NULL DEFAULT FALSE,
  position                 SMALLINT NOT NULL DEFAULT 0,
  status_id                BIGINT NOT NULL,
  created_at TIMESTAMPTZ NOT NULL DEFAULT now(), created_by BIGINT,
  updated_at TIMESTAMPTZ NOT NULL DEFAULT now(), updated_by BIGINT,
  CONSTRAINT fk_paymethod_status FOREIGN KEY (status_id) REFERENCES rs_catalog_status (id)
);

CREATE TABLE rs_catalog_authorities (
  id          BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  public_id   UUID NOT NULL DEFAULT gen_random_uuid() UNIQUE,
  code        VARCHAR(40) NOT NULL UNIQUE,
  name        VARCHAR(80) NOT NULL,
  description VARCHAR(255),
  position    SMALLINT NOT NULL DEFAULT 0,
  status_id   BIGINT NOT NULL,
  created_at TIMESTAMPTZ NOT NULL DEFAULT now(), created_by BIGINT,
  updated_at TIMESTAMPTZ NOT NULL DEFAULT now(), updated_by BIGINT,
  CONSTRAINT fk_auth_status FOREIGN KEY (status_id) REFERENCES rs_catalog_status (id),
  CONSTRAINT ck_auth_code CHECK (code ~ '^[A-Z][A-Z0-9_]*$')
);

CREATE TABLE rs_catalog_taxes (
  id           BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  public_id    UUID NOT NULL DEFAULT gen_random_uuid() UNIQUE,
  country_id   BIGINT NOT NULL,
  code         VARCHAR(20) NOT NULL,
  name         VARCHAR(80) NOT NULL,
  rate         NUMERIC(5,4) NOT NULL,
  is_inclusive BOOLEAN NOT NULL DEFAULT FALSE,
  status_id    BIGINT NOT NULL,
  created_at TIMESTAMPTZ NOT NULL DEFAULT now(), created_by BIGINT,
  updated_at TIMESTAMPTZ NOT NULL DEFAULT now(), updated_by BIGINT,
  CONSTRAINT fk_tax_country FOREIGN KEY (country_id) REFERENCES rs_catalog_countries (id),
  CONSTRAINT fk_tax_status FOREIGN KEY (status_id) REFERENCES rs_catalog_status (id),
  CONSTRAINT uq_tax UNIQUE (country_id, code),
  CONSTRAINT ck_tax_rate CHECK (rate >= 0 AND rate <= 1)
);

CREATE TABLE rs_catalog_order_status (
  id         BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  public_id  UUID NOT NULL DEFAULT gen_random_uuid() UNIQUE,
  code       VARCHAR(30) NOT NULL UNIQUE,
  name       VARCHAR(80) NOT NULL,
  is_final   BOOLEAN NOT NULL DEFAULT FALSE,
  is_open    BOOLEAN NOT NULL DEFAULT TRUE,
  position   SMALLINT NOT NULL DEFAULT 0,
  created_at TIMESTAMPTZ NOT NULL DEFAULT now(), created_by BIGINT,
  updated_at TIMESTAMPTZ NOT NULL DEFAULT now(), updated_by BIGINT
);

CREATE TABLE rs_catalog_table_status (
  id         BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  public_id  UUID NOT NULL DEFAULT gen_random_uuid() UNIQUE,
  code       VARCHAR(30) NOT NULL UNIQUE,
  name       VARCHAR(80) NOT NULL,
  position   SMALLINT NOT NULL DEFAULT 0,
  created_at TIMESTAMPTZ NOT NULL DEFAULT now(), created_by BIGINT,
  updated_at TIMESTAMPTZ NOT NULL DEFAULT now(), updated_by BIGINT
);

-- ═══ RESTAURANTE, PERSONAS, USUARIOS ══════════════════════════════════════

CREATE TABLE rs_restaurants (
  id                  BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  public_id           UUID NOT NULL DEFAULT gen_random_uuid() UNIQUE,
  name                VARCHAR(150) NOT NULL,
  legal_name          VARCHAR(180),
  tax_id              VARCHAR(40) UNIQUE,
  timezone            VARCHAR(64) NOT NULL DEFAULT 'UTC',
  locale              VARCHAR(10) NOT NULL DEFAULT 'es-MX',
  default_language_id BIGINT NOT NULL,
  default_currency_id BIGINT NOT NULL,
  default_country_id  BIGINT,
  settings            JSONB NOT NULL DEFAULT '{}'::jsonb,
  status_id           BIGINT NOT NULL,
  created_at TIMESTAMPTZ NOT NULL DEFAULT now(), created_by BIGINT,
  updated_at TIMESTAMPTZ NOT NULL DEFAULT now(), updated_by BIGINT,
  CONSTRAINT fk_rest_language FOREIGN KEY (default_language_id) REFERENCES rs_catalog_languages (id),
  CONSTRAINT fk_rest_currency FOREIGN KEY (default_currency_id) REFERENCES rs_catalog_currencies (id),
  CONSTRAINT fk_rest_country FOREIGN KEY (default_country_id) REFERENCES rs_catalog_countries (id),
  CONSTRAINT fk_rest_status FOREIGN KEY (status_id) REFERENCES rs_catalog_status (id)
);

CREATE TABLE rs_branches (
  id            BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  public_id     UUID NOT NULL DEFAULT gen_random_uuid() UNIQUE,
  restaurant_id BIGINT NOT NULL,
  city_id       BIGINT NOT NULL,
  code          VARCHAR(20) NOT NULL,
  name          VARCHAR(120) NOT NULL,
  address       TEXT,
  phone         VARCHAR(32),
  timezone      VARCHAR(64),
  latitude      NUMERIC(10,7),
  longitude     NUMERIC(10,7),
  is_default    BOOLEAN NOT NULL DEFAULT FALSE,
  opens_at      TIME,
  closes_at     TIME,
  status_id     BIGINT NOT NULL,
  created_at TIMESTAMPTZ NOT NULL DEFAULT now(), created_by BIGINT,
  updated_at TIMESTAMPTZ NOT NULL DEFAULT now(), updated_by BIGINT,
  CONSTRAINT fk_branch_restaurant FOREIGN KEY (restaurant_id) REFERENCES rs_restaurants (id) ON DELETE CASCADE,
  CONSTRAINT fk_branch_city FOREIGN KEY (city_id) REFERENCES rs_catalog_cities (id),
  CONSTRAINT fk_branch_status FOREIGN KEY (status_id) REFERENCES rs_catalog_status (id),
  CONSTRAINT uq_branch_code UNIQUE (restaurant_id, code),
  CONSTRAINT uq_branch_id_restaurant UNIQUE (id, restaurant_id),
  CONSTRAINT ck_branch_hours CHECK (closes_at IS NULL OR opens_at IS NULL OR closes_at > opens_at)
);

CREATE UNIQUE INDEX uq_branch_one_default ON rs_branches (restaurant_id) WHERE is_default;

CREATE TABLE rs_people (
  id               BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  public_id        UUID NOT NULL DEFAULT gen_random_uuid() UNIQUE,
  first_name       VARCHAR(80) NOT NULL,
  last_name        VARCHAR(80) NOT NULL,
  display_name     VARCHAR(160),
  birth_date       DATE,
  gender           VARCHAR(20),
  photo_url        TEXT,
  document_type_id BIGINT,
  document_number  VARCHAR(50),
  city_id          BIGINT,
  status_id        BIGINT NOT NULL,
  created_at TIMESTAMPTZ NOT NULL DEFAULT now(), created_by BIGINT,
  updated_at TIMESTAMPTZ NOT NULL DEFAULT now(), updated_by BIGINT,
  CONSTRAINT fk_person_doctype FOREIGN KEY (document_type_id) REFERENCES rs_catalog_document_types (id),
  CONSTRAINT fk_person_city FOREIGN KEY (city_id) REFERENCES rs_catalog_cities (id),
  CONSTRAINT fk_person_status FOREIGN KEY (status_id) REFERENCES rs_catalog_status (id),
  CONSTRAINT ck_person_doc CHECK ((document_type_id IS NULL) = (document_number IS NULL))
);

CREATE INDEX ix_people_name ON rs_people (last_name, first_name);
CREATE UNIQUE INDEX uq_people_document ON rs_people (document_type_id, document_number) WHERE document_number IS NOT NULL;

CREATE TABLE rs_people_contacts (
  id            BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  public_id     UUID NOT NULL DEFAULT gen_random_uuid() UNIQUE,
  person_id     BIGINT NOT NULL,
  contact_type  VARCHAR(10) NOT NULL,
  contact_value VARCHAR(255) NOT NULL,
  verified_at   TIMESTAMPTZ,
  created_at TIMESTAMPTZ NOT NULL DEFAULT now(), created_by BIGINT,
  updated_at TIMESTAMPTZ NOT NULL DEFAULT now(), updated_by BIGINT,
  CONSTRAINT fk_contact_person FOREIGN KEY (person_id) REFERENCES rs_people (id) ON DELETE CASCADE,
  CONSTRAINT ck_contact_type CHECK (contact_type IN ('EMAIL','PHONE','ADDRESS'))
);

CREATE UNIQUE INDEX uq_contact_type ON rs_people_contacts (person_id, contact_type);

CREATE TABLE rs_users (
  id                   BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  public_id            UUID NOT NULL DEFAULT gen_random_uuid() UNIQUE,
  person_id            BIGINT NOT NULL UNIQUE,
  username             VARCHAR(60) NOT NULL UNIQUE,
  password_hash        TEXT NOT NULL,
  password_updated_at  TIMESTAMPTZ,
  must_change_password BOOLEAN NOT NULL DEFAULT TRUE,
  failed_count         SMALLINT NOT NULL DEFAULT 0,
  lockout_end          TIMESTAMPTZ,
  last_login_at        TIMESTAMPTZ,
  last_login_ip        INET,
  status_id            BIGINT NOT NULL,
  created_at TIMESTAMPTZ NOT NULL DEFAULT now(), created_by BIGINT,
  updated_at TIMESTAMPTZ NOT NULL DEFAULT now(), updated_by BIGINT,
  CONSTRAINT fk_user_person FOREIGN KEY (person_id) REFERENCES rs_people (id),
  CONSTRAINT fk_user_status FOREIGN KEY (status_id) REFERENCES rs_catalog_status (id)
);

CREATE TABLE rs_user_authorities (
  id           BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  user_id      BIGINT NOT NULL,
  authority_id BIGINT NOT NULL,
  granted_at   TIMESTAMPTZ NOT NULL DEFAULT now(),
  granted_by   BIGINT,
  expires_at   TIMESTAMPTZ,
  created_at TIMESTAMPTZ NOT NULL DEFAULT now(), created_by BIGINT,
  updated_at TIMESTAMPTZ NOT NULL DEFAULT now(), updated_by BIGINT,
  CONSTRAINT fk_ua_user FOREIGN KEY (user_id) REFERENCES rs_users (id) ON DELETE CASCADE,
  CONSTRAINT fk_ua_authority FOREIGN KEY (authority_id) REFERENCES rs_catalog_authorities (id),
  CONSTRAINT fk_ua_granted_by FOREIGN KEY (granted_by) REFERENCES rs_users (id),
  CONSTRAINT uq_user_authority UNIQUE (user_id, authority_id)
);

CREATE TABLE rs_user_branches (
  id         BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  user_id    BIGINT NOT NULL,
  branch_id  BIGINT NOT NULL,
  is_primary BOOLEAN NOT NULL DEFAULT FALSE,
  created_at TIMESTAMPTZ NOT NULL DEFAULT now(), created_by BIGINT,
  updated_at TIMESTAMPTZ NOT NULL DEFAULT now(), updated_by BIGINT,
  CONSTRAINT fk_ub_user FOREIGN KEY (user_id) REFERENCES rs_users (id) ON DELETE CASCADE,
  CONSTRAINT fk_ub_branch FOREIGN KEY (branch_id) REFERENCES rs_branches (id) ON DELETE CASCADE,
  CONSTRAINT uq_user_branch UNIQUE (user_id, branch_id)
);

CREATE TABLE rs_login_attempts (
  id             BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  user_id        BIGINT,
  username_try   VARCHAR(60) NOT NULL,
  ip             INET,
  user_agent     VARCHAR(255),
  was_successful BOOLEAN NOT NULL,
  failure_reason VARCHAR(60),
  created_at TIMESTAMPTZ NOT NULL DEFAULT now(), created_by BIGINT,
  updated_at TIMESTAMPTZ NOT NULL DEFAULT now(), updated_by BIGINT,
  CONSTRAINT fk_la_user FOREIGN KEY (user_id) REFERENCES rs_users (id)
);

CREATE INDEX ix_login_attempts_user ON rs_login_attempts (user_id, created_at DESC);
CREATE INDEX ix_login_attempts_ip ON rs_login_attempts (ip, created_at DESC);

-- ═══ MENÚ ═════════════════════════════════════════════════════════════════

CREATE TABLE rs_menu_categories (
  id              BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  public_id       UUID NOT NULL DEFAULT gen_random_uuid() UNIQUE,
  restaurant_id   BIGINT NOT NULL,
  name            VARCHAR(120) NOT NULL,
  name_en         VARCHAR(120),
  description     TEXT,
  description_en  TEXT,
  position        SMALLINT NOT NULL DEFAULT 0,
  status_id       BIGINT NOT NULL,
  created_at TIMESTAMPTZ NOT NULL DEFAULT now(), created_by BIGINT,
  updated_at TIMESTAMPTZ NOT NULL DEFAULT now(), updated_by BIGINT,
  CONSTRAINT fk_cat_restaurant FOREIGN KEY (restaurant_id) REFERENCES rs_restaurants (id) ON DELETE CASCADE,
  CONSTRAINT fk_cat_status FOREIGN KEY (status_id) REFERENCES rs_catalog_status (id),
  CONSTRAINT uq_cat_position UNIQUE (restaurant_id, position),
  CONSTRAINT uq_cat_id_restaurant UNIQUE (id, restaurant_id)
);

CREATE TABLE rs_menu_items (
  id             BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  public_id      UUID NOT NULL DEFAULT gen_random_uuid() UNIQUE,
  restaurant_id  BIGINT NOT NULL,
  category_id    BIGINT NOT NULL,
  name           VARCHAR(150) NOT NULL,
  name_en        VARCHAR(150),
  description    TEXT,
  description_en TEXT,
  base_price     NUMERIC(12,2) NOT NULL,
  image_url      TEXT,
  is_available   BOOLEAN NOT NULL DEFAULT TRUE,
  position       SMALLINT NOT NULL DEFAULT 0,
  tax_id         BIGINT,
  status_id      BIGINT NOT NULL,
  created_at TIMESTAMPTZ NOT NULL DEFAULT now(), created_by BIGINT,
  updated_at TIMESTAMPTZ NOT NULL DEFAULT now(), updated_by BIGINT,
  CONSTRAINT fk_item_category FOREIGN KEY (category_id, restaurant_id) REFERENCES rs_menu_categories (id, restaurant_id),
  CONSTRAINT fk_item_tax FOREIGN KEY (tax_id) REFERENCES rs_catalog_taxes (id),
  CONSTRAINT fk_item_status FOREIGN KEY (status_id) REFERENCES rs_catalog_status (id),
  CONSTRAINT uq_item_position UNIQUE (category_id, position),
  CONSTRAINT ck_item_price CHECK (base_price >= 0)
);

CREATE INDEX ix_items_restaurant ON rs_menu_items (restaurant_id, status_id, position);

CREATE TABLE rs_menu_item_extras (
  id           BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  public_id    UUID NOT NULL DEFAULT gen_random_uuid() UNIQUE,
  menu_item_id BIGINT NOT NULL,
  name         VARCHAR(120) NOT NULL,
  name_en      VARCHAR(120),
  image_url    TEXT,
  price_delta  NUMERIC(12,2) NOT NULL DEFAULT 0,
  max_select   SMALLINT NOT NULL DEFAULT 1,
  is_required  BOOLEAN NOT NULL DEFAULT FALSE,
  position     SMALLINT NOT NULL DEFAULT 0,
  status_id    BIGINT NOT NULL,
  created_at TIMESTAMPTZ NOT NULL DEFAULT now(), created_by BIGINT,
  updated_at TIMESTAMPTZ NOT NULL DEFAULT now(), updated_by BIGINT,
  CONSTRAINT fk_extra_item FOREIGN KEY (menu_item_id) REFERENCES rs_menu_items (id) ON DELETE CASCADE,
  CONSTRAINT fk_extra_status FOREIGN KEY (status_id) REFERENCES rs_catalog_status (id),
  CONSTRAINT ck_extra_max CHECK (max_select >= 1)
);

CREATE INDEX ix_extras_item ON rs_menu_item_extras (menu_item_id, position);

-- ═══ ZONAS, MESAS, RESERVAS ═══════════════════════════════════════════════

CREATE TABLE rs_zones (
  id            BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  public_id     UUID NOT NULL DEFAULT gen_random_uuid() UNIQUE,
  restaurant_id BIGINT NOT NULL,
  branch_id     BIGINT NOT NULL,
  name          VARCHAR(100) NOT NULL,
  position      SMALLINT NOT NULL DEFAULT 0,
  status_id     BIGINT NOT NULL,
  created_at TIMESTAMPTZ NOT NULL DEFAULT now(), created_by BIGINT,
  updated_at TIMESTAMPTZ NOT NULL DEFAULT now(), updated_by BIGINT,
  CONSTRAINT fk_zone_restaurant FOREIGN KEY (restaurant_id) REFERENCES rs_restaurants (id) ON DELETE CASCADE,
  CONSTRAINT fk_zone_branch FOREIGN KEY (branch_id, restaurant_id) REFERENCES rs_branches (id, restaurant_id) ON DELETE CASCADE,
  CONSTRAINT fk_zone_status FOREIGN KEY (status_id) REFERENCES rs_catalog_status (id),
  CONSTRAINT uq_zone_position UNIQUE (branch_id, position),
  CONSTRAINT uq_zone_id_restaurant UNIQUE (id, restaurant_id)
);

CREATE TABLE rs_dining_tables (
  id            BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  public_id     UUID NOT NULL DEFAULT gen_random_uuid() UNIQUE,
  restaurant_id BIGINT NOT NULL,
  branch_id     BIGINT NOT NULL,
  zone_id       BIGINT NOT NULL,
  code          VARCHAR(20) NOT NULL,
  name          VARCHAR(100) NOT NULL,
  min_capacity  SMALLINT NOT NULL DEFAULT 1,
  max_capacity  SMALLINT NOT NULL DEFAULT 4,
  is_combinable BOOLEAN NOT NULL DEFAULT FALSE,
  notes         TEXT,
  status_id     BIGINT NOT NULL,
  created_at TIMESTAMPTZ NOT NULL DEFAULT now(), created_by BIGINT,
  updated_at TIMESTAMPTZ NOT NULL DEFAULT now(), updated_by BIGINT,
  CONSTRAINT fk_table_zone FOREIGN KEY (zone_id, restaurant_id) REFERENCES rs_zones (id, restaurant_id),
  CONSTRAINT fk_table_branch FOREIGN KEY (branch_id, restaurant_id) REFERENCES rs_branches (id, restaurant_id),
  CONSTRAINT fk_table_status FOREIGN KEY (status_id) REFERENCES rs_catalog_table_status (id),
  CONSTRAINT uq_table_code UNIQUE (branch_id, code),
  CONSTRAINT uq_table_id_branch UNIQUE (id, branch_id),
  CONSTRAINT ck_table_capacity CHECK (min_capacity >= 1 AND max_capacity >= min_capacity)
);

CREATE TABLE rs_table_reservations (
  id             BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  public_id      UUID NOT NULL DEFAULT gen_random_uuid() UNIQUE,
  restaurant_id  BIGINT NOT NULL,
  branch_id      BIGINT NOT NULL,
  table_id       BIGINT,
  customer_name  VARCHAR(160) NOT NULL,
  customer_phone VARCHAR(32),
  party_size     SMALLINT NOT NULL,
  reserved_from  TIMESTAMPTZ NOT NULL,
  reserved_to    TIMESTAMPTZ NOT NULL,
  occasion       VARCHAR(40),
  notes          TEXT,
  seated_at      TIMESTAMPTZ,
  status_id      BIGINT NOT NULL,
  created_at TIMESTAMPTZ NOT NULL DEFAULT now(), created_by BIGINT,
  updated_at TIMESTAMPTZ NOT NULL DEFAULT now(), updated_by BIGINT,
  CONSTRAINT fk_res_table FOREIGN KEY (table_id) REFERENCES rs_dining_tables (id),
  CONSTRAINT fk_res_branch FOREIGN KEY (branch_id) REFERENCES rs_branches (id) ON DELETE CASCADE,
  CONSTRAINT fk_res_restaurant FOREIGN KEY (restaurant_id) REFERENCES rs_restaurants (id) ON DELETE CASCADE,
  CONSTRAINT fk_res_status FOREIGN KEY (status_id) REFERENCES rs_catalog_status (id),
  CONSTRAINT ck_res_range CHECK (reserved_to > reserved_from),
  CONSTRAINT ck_res_party CHECK (party_size >= 1)
);

ALTER TABLE rs_table_reservations ADD CONSTRAINT ex_res_no_overlap
  EXCLUDE USING gist (table_id WITH =, tstzrange(reserved_from, reserved_to) WITH &&)
  WHERE (status_id = 1);

CREATE TABLE rs_table_assignments (
  id          BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  public_id   UUID NOT NULL DEFAULT gen_random_uuid() UNIQUE,
  table_id    BIGINT NOT NULL,
  waiter_id   BIGINT NOT NULL,
  assigned_at TIMESTAMPTZ NOT NULL DEFAULT now(),
  assigned_by BIGINT,
  released_at TIMESTAMPTZ,
  released_by BIGINT,
  created_at TIMESTAMPTZ NOT NULL DEFAULT now(), created_by BIGINT,
  updated_at TIMESTAMPTZ NOT NULL DEFAULT now(), updated_by BIGINT,
  CONSTRAINT fk_ta_table FOREIGN KEY (table_id) REFERENCES rs_dining_tables (id) ON DELETE CASCADE,
  CONSTRAINT fk_ta_waiter FOREIGN KEY (waiter_id) REFERENCES rs_users (id) ON DELETE CASCADE,
  CONSTRAINT fk_ta_assigned_by FOREIGN KEY (assigned_by) REFERENCES rs_users (id),
  CONSTRAINT fk_ta_released_by FOREIGN KEY (released_by) REFERENCES rs_users (id),
  CONSTRAINT ck_ta_release CHECK (released_at IS NULL OR released_at >= assigned_at)
);

CREATE UNIQUE INDEX uq_ta_active_waiter ON rs_table_assignments (table_id) WHERE released_at IS NULL;
CREATE UNIQUE INDEX uq_ta_active_table ON rs_table_assignments (waiter_id) WHERE released_at IS NULL;

-- ═══ PEDIDOS ══════════════════════════════════════════════════════════════

CREATE TABLE rs_orders (
  id                BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  public_id         UUID NOT NULL DEFAULT gen_random_uuid() UNIQUE,
  restaurant_id     BIGINT NOT NULL,
  branch_id         BIGINT NOT NULL,
  table_id          BIGINT,
  reservation_id    BIGINT,
  order_number      VARCHAR(30) NOT NULL,
  order_type        VARCHAR(20) NOT NULL DEFAULT 'DINE_IN',
  customer_name     VARCHAR(160),
  customer_phone    VARCHAR(32),
  covers            SMALLINT NOT NULL DEFAULT 1,
  status_id         BIGINT NOT NULL,
  discount_amount   NUMERIC(12,2) NOT NULL DEFAULT 0,
  tip_amount        NUMERIC(12,2) NOT NULL DEFAULT 0,
  payment_method_id BIGINT,
  paid_amount       NUMERIC(12,2),
  amount_received   NUMERIC(12,2),
  change_given      NUMERIC(12,2),
  payment_reference VARCHAR(120),
  paid_at           TIMESTAMPTZ,
  opened_by         BIGINT,
  opened_at         TIMESTAMPTZ NOT NULL DEFAULT now(),
  closed_by         BIGINT,
  closed_at         TIMESTAMPTZ,
  notes             TEXT,
  created_at TIMESTAMPTZ NOT NULL DEFAULT now(), created_by BIGINT,
  updated_at TIMESTAMPTZ NOT NULL DEFAULT now(), updated_by BIGINT,
  CONSTRAINT fk_order_table FOREIGN KEY (table_id, branch_id) REFERENCES rs_dining_tables (id, branch_id),
  CONSTRAINT fk_order_branch FOREIGN KEY (branch_id) REFERENCES rs_branches (id),
  CONSTRAINT fk_order_restaurant FOREIGN KEY (restaurant_id) REFERENCES rs_restaurants (id) ON DELETE CASCADE,
  CONSTRAINT fk_order_reservation FOREIGN KEY (reservation_id) REFERENCES rs_table_reservations (id),
  CONSTRAINT fk_order_status FOREIGN KEY (status_id) REFERENCES rs_catalog_order_status (id),
  CONSTRAINT fk_order_method FOREIGN KEY (payment_method_id) REFERENCES rs_catalog_payment_methods (id),
  CONSTRAINT fk_order_opened_by FOREIGN KEY (opened_by) REFERENCES rs_users (id),
  CONSTRAINT fk_order_closed_by FOREIGN KEY (closed_by) REFERENCES rs_users (id),
  CONSTRAINT uq_order_number UNIQUE (branch_id, order_number),
  CONSTRAINT ck_order_table CHECK (order_type <> 'DINE_IN' OR table_id IS NOT NULL),
  CONSTRAINT ck_order_closed CHECK ((closed_at IS NULL) = (closed_by IS NULL)),
  CONSTRAINT ck_order_discount CHECK (discount_amount >= 0),
  CONSTRAINT ck_order_tip CHECK (tip_amount >= 0),
  CONSTRAINT ck_order_payment CHECK ((payment_method_id IS NULL) = (paid_amount IS NULL)),
  CONSTRAINT ck_order_paid_at CHECK (paid_amount IS NULL OR paid_at IS NOT NULL),
  CONSTRAINT ck_order_paid CHECK (paid_amount IS NULL OR paid_amount >= 0),
  CONSTRAINT ck_order_change CHECK (change_given IS NULL
    OR (amount_received IS NOT NULL AND change_given = amount_received - paid_amount))
);

-- Depende de los ids del seed: estados con is_open = TRUE son 1,2,3,4
CREATE UNIQUE INDEX uq_order_open_per_table ON rs_orders (table_id)
  WHERE table_id IS NOT NULL AND status_id IN (1,2,3,4);

CREATE TABLE rs_order_items (
  id                 BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  public_id          UUID NOT NULL DEFAULT gen_random_uuid() UNIQUE,
  order_id           BIGINT NOT NULL,
  menu_item_id       BIGINT,
  menu_item_extra_id BIGINT,
  name               VARCHAR(150) NOT NULL,
  description        TEXT,
  unit_price         NUMERIC(12,2) NOT NULL,
  quantity           NUMERIC(14,4) NOT NULL DEFAULT 1,
  line_total         NUMERIC(14,2) GENERATED ALWAYS AS (ROUND((unit_price * quantity)::numeric, 2)) STORED,
  tax_id             BIGINT,
  is_voided          BOOLEAN NOT NULL DEFAULT FALSE,
  notes              TEXT,
  created_at TIMESTAMPTZ NOT NULL DEFAULT now(), created_by BIGINT,
  updated_at TIMESTAMPTZ NOT NULL DEFAULT now(), updated_by BIGINT,
  CONSTRAINT fk_oi_order FOREIGN KEY (order_id) REFERENCES rs_orders (id) ON DELETE CASCADE,
  CONSTRAINT fk_oi_item FOREIGN KEY (menu_item_id) REFERENCES rs_menu_items (id),
  CONSTRAINT fk_oi_extra FOREIGN KEY (menu_item_extra_id) REFERENCES rs_menu_item_extras (id),
  CONSTRAINT fk_oi_tax FOREIGN KEY (tax_id) REFERENCES rs_catalog_taxes (id),
  CONSTRAINT ck_oi_source CHECK ((menu_item_id IS NULL) <> (menu_item_extra_id IS NULL)),
  CONSTRAINT ck_oi_qty CHECK (quantity > 0),
  CONSTRAINT ck_oi_price CHECK (unit_price >= 0)
);

CREATE INDEX ix_oi_order ON rs_order_items (order_id);

CREATE TABLE rs_order_status_history (
  id             BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  public_id      UUID NOT NULL DEFAULT gen_random_uuid() UNIQUE,
  order_id       BIGINT NOT NULL,
  from_status_id BIGINT,
  to_status_id   BIGINT NOT NULL,
  changed_at     TIMESTAMPTZ NOT NULL DEFAULT now(),
  changed_by     BIGINT,
  reason         TEXT,
  created_at TIMESTAMPTZ NOT NULL DEFAULT now(), created_by BIGINT,
  updated_at TIMESTAMPTZ NOT NULL DEFAULT now(), updated_by BIGINT,
  CONSTRAINT fk_osh_order FOREIGN KEY (order_id) REFERENCES rs_orders (id) ON DELETE CASCADE,
  CONSTRAINT fk_osh_from FOREIGN KEY (from_status_id) REFERENCES rs_catalog_order_status (id),
  CONSTRAINT fk_osh_to FOREIGN KEY (to_status_id) REFERENCES rs_catalog_order_status (id),
  CONSTRAINT fk_osh_user FOREIGN KEY (changed_by) REFERENCES rs_users (id),
  CONSTRAINT uq_osh_transition UNIQUE (order_id, to_status_id)
);

-- ═══ SEED ═════════════════════════════════════════════════════════════════

INSERT INTO rs_catalog_status (id, code, name, is_deletable, position) OVERRIDING SYSTEM VALUE VALUES
  (1, 'ACTIVE', 'Activo', FALSE, 1),
  (2, 'INACTIVE', 'Inactivo', TRUE, 2),
  (3, 'DELETED', 'Eliminado', TRUE, 3);

INSERT INTO rs_catalog_languages (id, code, name, status_id) OVERRIDING SYSTEM VALUE VALUES
  (1, 'es', 'Español', 1),
  (2, 'en', 'Inglés', 1);

INSERT INTO rs_catalog_currencies (id, code, name, symbol, decimal_places, status_id) OVERRIDING SYSTEM VALUE VALUES
  (1, 'MXN', 'Peso mexicano', '$', 2, 1),
  (2, 'USD', 'Dólar', 'US$', 2, 1),
  (3, 'EUR', 'Euro', '€', 2, 1);

INSERT INTO rs_catalog_countries (id, code, code_alpha3, name, phone_code, default_currency_id, status_id) OVERRIDING SYSTEM VALUE VALUES
  (1, 'MX', 'MEX', 'México', '+52', 1, 1),
  (2, 'US', 'USA', 'Estados Unidos', '+1', 2, 1),
  (3, 'ES', 'ESP', 'España', '+34', 3, 1);

INSERT INTO rs_catalog_regions (id, country_id, code, name, status_id) OVERRIDING SYSTEM VALUE VALUES
  (1, 1, 'CMX', 'Ciudad de México', 1),
  (2, 1, 'JAL', 'Jalisco', 1);

INSERT INTO rs_catalog_cities (id, country_id, region_id, code, name, is_capital, status_id) OVERRIDING SYSTEM VALUE VALUES
  (1, 1, 1, 'MEX', 'Ciudad de México', TRUE, 1),
  (2, 1, 2, 'GDL', 'Guadalajara', FALSE, 1);

INSERT INTO rs_catalog_document_types (id, country_id, code, name, requires_expiry_date, status_id) OVERRIDING SYSTEM VALUE VALUES
  (1, NULL, 'PASSPORT', 'Pasaporte', TRUE, 1),
  (2, 1, 'RFC', 'RFC', FALSE, 1),
  (3, 1, 'CURP', 'CURP', FALSE, 1);

INSERT INTO rs_catalog_payment_methods (id, code, name, requires_reference, requires_amount_tendered, position, status_id) OVERRIDING SYSTEM VALUE VALUES
  (1, 'CASH', 'Efectivo', FALSE, TRUE, 1, 1),
  (2, 'CARD', 'Tarjeta', TRUE, FALSE, 2, 1),
  (3, 'TRANSFER', 'Transferencia', TRUE, FALSE, 3, 1);

INSERT INTO rs_catalog_authorities (id, code, name, description, position, status_id) OVERRIDING SYSTEM VALUE VALUES
  (1, 'ADMIN', 'Administrador', 'Acceso total', 1, 1),
  (2, 'OWNER', 'Propietario', 'Dueno del restaurante', 2, 1),
  (3, 'MANAGER', 'Gerente', 'Gestion de restaurante y menu', 3, 1),
  (4, 'CASHIER', 'Cajero', 'Cobros, cierres y pagos', 4, 1),
  (5, 'WAITER', 'Mesero', 'Toma de pedidos y servicio', 5, 1),
  (6, 'COOK', 'Cocina', 'Marca de platos listos', 6, 1),
  (7, 'HOST', 'Host', 'Reservas y asignacion de mesas', 7, 1);

INSERT INTO rs_catalog_taxes (id, country_id, code, name, rate, is_inclusive, status_id) OVERRIDING SYSTEM VALUE VALUES
  (1, 1, 'IVA', 'IVA', 0.1600, FALSE, 1);

INSERT INTO rs_catalog_order_status (id, code, name, is_final, is_open, position) OVERRIDING SYSTEM VALUE VALUES
  (1, 'OPEN', 'Abierta', FALSE, TRUE, 1),
  (2, 'IN_PREPARATION', 'En preparacion', FALSE, TRUE, 2),
  (3, 'SERVED', 'Servida', FALSE, TRUE, 3),
  (4, 'BILLING', 'Cobrando', FALSE, TRUE, 4),
  (5, 'CLOSED', 'Cerrada', TRUE, FALSE, 5),
  (6, 'CANCELLED', 'Cancelada', TRUE, FALSE, 6);

INSERT INTO rs_catalog_table_status (id, code, name, position) OVERRIDING SYSTEM VALUE VALUES
  (1, 'FREE', 'Libre', 1),
  (2, 'OCCUPIED', 'Ocupada', 2),
  (3, 'RESERVED', 'Reservada', 3),
  (4, 'DIRTY', 'Sucia', 4),
  (5, 'OUT_OF_SERVICE', 'Fuera de servicio', 5);
