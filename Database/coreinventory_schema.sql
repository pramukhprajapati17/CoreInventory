-- CoreInventory schema (PostgreSQL)

-- Users
create table if not exists t_user (
    c_user_id bigserial primary key,
    c_full_name varchar(150) not null,
    c_email varchar(200) not null unique,
    c_password varchar(255) not null,
    c_phone varchar(20),
    c_is_active boolean not null default true,
    c_created_at timestamptz not null default now(),
    c_updated_at timestamptz not null default now()
);

-- Master data
create table if not exists t_uom (
    c_uom_id bigserial primary key,
    c_uom_name varchar(50) not null unique,
    c_uom_code varchar(20) not null unique
);

create table if not exists t_product_category (
    c_category_id bigserial primary key,
    c_category_name varchar(120) not null unique,
    c_category_code varchar(30) not null unique
);

create table if not exists t_product (
    c_product_id bigserial primary key,
    c_product_name varchar(200) not null,
    c_sku varchar(80) not null unique,
    c_category_id bigint references t_product_category(c_category_id),
    c_uom_id bigint references t_uom(c_uom_id),
    c_is_active boolean not null default true,
    c_created_at timestamptz not null default now(),
    c_updated_at timestamptz not null default now()
);

-- Warehousing
create table if not exists t_warehouse (
    c_warehouse_id bigserial primary key,
    c_warehouse_name varchar(150) not null unique,
    c_code varchar(30) not null unique,
    c_is_active boolean not null default true
);

create table if not exists t_location (
    c_location_id bigserial primary key,
    c_warehouse_id bigint not null references t_warehouse(c_warehouse_id),
    c_location_name varchar(150) not null,
    c_location_code varchar(30) not null,
    c_is_active boolean not null default true,
    unique (c_warehouse_id, c_location_code)
);

-- Partners
create table if not exists t_supplier (
    c_supplier_id bigserial primary key,
    c_supplier_name varchar(200) not null,
    c_email varchar(200),
    c_phone varchar(20)
);

create table if not exists t_customer (
    c_customer_id bigserial primary key,
    c_customer_name varchar(200) not null,
    c_email varchar(200),
    c_phone varchar(20)
);

-- Stock summary per location
create table if not exists t_stock (
    c_stock_id bigserial primary key,
    c_product_id bigint not null references t_product(c_product_id),
    c_location_id bigint not null references t_location(c_location_id),
    c_qty numeric(18, 3) not null default 0,
    unique (c_product_id, c_location_id)
);

-- Receipts (Incoming)
create table if not exists t_receipt (
    c_receipt_id bigserial primary key,
    c_receipt_no varchar(50) not null unique,
    c_supplier_id bigint references t_supplier(c_supplier_id),
    c_status varchar(20) not null default 'Draft',
    c_expected_date date,
    c_created_by bigint references t_user(c_user_id),
    c_created_at timestamptz not null default now()
);

create table if not exists t_receipt_line (
    c_receipt_line_id bigserial primary key,
    c_receipt_id bigint not null references t_receipt(c_receipt_id) on delete cascade,
    c_product_id bigint not null references t_product(c_product_id),
    c_location_id bigint references t_location(c_location_id),
    c_qty numeric(18, 3) not null
);

-- Delivery Orders (Outgoing)
create table if not exists t_delivery (
    c_delivery_id bigserial primary key,
    c_delivery_no varchar(50) not null unique,
    c_customer_id bigint references t_customer(c_customer_id),
    c_status varchar(20) not null default 'Draft',
    c_expected_date date,
    c_created_by bigint references t_user(c_user_id),
    c_created_at timestamptz not null default now()
);

create table if not exists t_delivery_line (
    c_delivery_line_id bigserial primary key,
    c_delivery_id bigint not null references t_delivery(c_delivery_id) on delete cascade,
    c_product_id bigint not null references t_product(c_product_id),
    c_location_id bigint references t_location(c_location_id),
    c_qty numeric(18, 3) not null
);

-- Internal Transfers
create table if not exists t_transfer (
    c_transfer_id bigserial primary key,
    c_transfer_no varchar(50) not null unique,
    c_from_location_id bigint not null references t_location(c_location_id),
    c_to_location_id bigint not null references t_location(c_location_id),
    c_status varchar(20) not null default 'Draft',
    c_created_by bigint references t_user(c_user_id),
    c_created_at timestamptz not null default now()
);

create table if not exists t_transfer_line (
    c_transfer_line_id bigserial primary key,
    c_transfer_id bigint not null references t_transfer(c_transfer_id) on delete cascade,
    c_product_id bigint not null references t_product(c_product_id),
    c_qty numeric(18, 3) not null
);

-- Stock Adjustments
create table if not exists t_adjustment (
    c_adjustment_id bigserial primary key,
    c_adjustment_no varchar(50) not null unique,
    c_location_id bigint not null references t_location(c_location_id),
    c_status varchar(20) not null default 'Draft',
    c_reason varchar(200),
    c_created_by bigint references t_user(c_user_id),
    c_created_at timestamptz not null default now()
);

create table if not exists t_adjustment_line (
    c_adjustment_line_id bigserial primary key,
    c_adjustment_id bigint not null references t_adjustment(c_adjustment_id) on delete cascade,
    c_product_id bigint not null references t_product(c_product_id),
    c_counted_qty numeric(18, 3) not null,
    c_system_qty numeric(18, 3) not null
);

-- Reordering Rules
create table if not exists t_reorder_rule (
    c_reorder_rule_id bigserial primary key,
    c_product_id bigint not null references t_product(c_product_id),
    c_location_id bigint references t_location(c_location_id),
    c_min_qty numeric(18, 3) not null,
    c_max_qty numeric(18, 3),
    c_is_active boolean not null default true
);

-- Stock Ledger (history)
create table if not exists t_stock_ledger (
    c_ledger_id bigserial primary key,
    c_product_id bigint not null references t_product(c_product_id),
    c_location_id bigint references t_location(c_location_id),
    c_doc_type varchar(30) not null,
    c_doc_id bigint not null,
    c_qty_change numeric(18, 3) not null,
    c_created_at timestamptz not null default now()
);
