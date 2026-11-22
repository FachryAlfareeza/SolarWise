-- Additions on top of the ERD tables (users, admins, locations, solar_panels, weather_data,
-- calculations, recomendation). Only adds columns/tables or relaxes a NOT NULL; nothing is dropped.
-- Safe to run more than once. Run in the Supabase SQL editor (already applied by the developer).

ALTER TABLE users ADD COLUMN IF NOT EXISTS full_name varchar(100) NOT NULL DEFAULT '';

ALTER TABLE weather_data ADD COLUMN IF NOT EXISTS is_sample boolean NOT NULL DEFAULT false;

-- A calculation can be made without a weather record (sample irradiance), so weather_data_id is optional.
ALTER TABLE calculations ALTER COLUMN weather_data_id DROP NOT NULL;
ALTER TABLE calculations ADD COLUMN IF NOT EXISTS project_name varchar(150) NOT NULL DEFAULT '';
ALTER TABLE calculations ADD COLUMN IF NOT EXISTS organization_id bigint;
ALTER TABLE calculations ADD COLUMN IF NOT EXISTS panel_count integer NOT NULL DEFAULT 0;
ALTER TABLE calculations ADD COLUMN IF NOT EXISTS system_capacity_kwp numeric(12,3) NOT NULL DEFAULT 0;
ALTER TABLE calculations ADD COLUMN IF NOT EXISTS irradiance numeric(8,3) NOT NULL DEFAULT 0;
ALTER TABLE calculations ADD COLUMN IF NOT EXISTS is_sample boolean NOT NULL DEFAULT false;

ALTER TABLE recomendation ADD COLUMN IF NOT EXISTS summary text NOT NULL DEFAULT '';

CREATE TABLE IF NOT EXISTS organizations (
    organization_id bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    name        varchar(100) NOT NULL UNIQUE,
    description text NOT NULL DEFAULT '',
    join_code   varchar(30)  NOT NULL UNIQUE,
    created_at  timestamptz DEFAULT now()
);

CREATE TABLE IF NOT EXISTS memberships (
    user_id         bigint NOT NULL REFERENCES users(user_id) ON DELETE CASCADE,
    organization_id bigint NOT NULL REFERENCES organizations(organization_id) ON DELETE CASCADE,
    role            varchar(20) NOT NULL DEFAULT 'Member',
    PRIMARY KEY (user_id, organization_id)
);

-- Deleting an organization keeps its calculations (they just lose the organization link).
DO $$ BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'calculations_organization_id_fkey') THEN
        ALTER TABLE calculations ADD CONSTRAINT calculations_organization_id_fkey
            FOREIGN KEY (organization_id) REFERENCES organizations(organization_id) ON DELETE SET NULL;
    END IF;
END $$;
