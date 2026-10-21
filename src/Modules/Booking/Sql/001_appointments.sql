-- P0 Booking: PostgreSQL concurrency invariant (standalone reference schema).
-- This script is not yet an EF migration and MUST be reconciled with the selected API foundation.
-- Run once against a dedicated/disposable database. Avoid applying to production until
-- Identity membership, tenant ownership, role-based authorization and migrations exist.
CREATE EXTENSION IF NOT EXISTS btree_gist;
CREATE SCHEMA IF NOT EXISTS booking;

CREATE TABLE booking.appointments (
    id uuid NOT NULL,
    tenant_id uuid NOT NULL,
    staff_id uuid NOT NULL,
    starts_at timestamptz NOT NULL,
    ends_at timestamptz NOT NULL,
    status text NOT NULL DEFAULT 'pending',
    CONSTRAINT booking_appointments_pk PRIMARY KEY (tenant_id, id),
    CONSTRAINT booking_appointments_nonempty_ids CHECK (
        id <> '00000000-0000-0000-0000-000000000000'::uuid AND
        tenant_id <> '00000000-0000-0000-0000-000000000000'::uuid AND
        staff_id <> '00000000-0000-0000-0000-000000000000'::uuid
    ),
    CONSTRAINT booking_appointments_positive_window CHECK (ends_at > starts_at),
    CONSTRAINT booking_appointments_valid_status CHECK (
        status IN ('pending', 'confirmed', 'cancelled')
    ),
    -- Half-open windows [start, end) allow adjacent bookings. The GiST
    -- exclusion constraint rejects conflicting INSERTs and UPDATEs even when
    -- two concurrent transactions both passed an in-memory availability check.
    CONSTRAINT booking_appointments_staff_no_overlap EXCLUDE USING gist (
        tenant_id WITH =,
        staff_id WITH =,
        tstzrange(starts_at, ends_at, '[)') WITH &&
    ) WHERE (status IN ('pending', 'confirmed'))
);

CREATE INDEX booking_appointments_tenant_start_idx
    ON booking.appointments (tenant_id, starts_at);
