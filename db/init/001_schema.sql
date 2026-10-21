-- Local development bootstrap only. Production needs versioned migrations.
CREATE TABLE IF NOT EXISTS organizations (
    id uuid PRIMARY KEY,
    name varchar(120) NOT NULL,
    created_at timestamptz NOT NULL
);

CREATE TABLE IF NOT EXISTS organization_members (
    organization_id uuid NOT NULL REFERENCES organizations(id) ON DELETE CASCADE,
    user_id varchar(255) NOT NULL,
    role varchar(16) NOT NULL CHECK (role IN ('Owner', 'Admin', 'Agent')),
    created_at timestamptz NOT NULL,
    PRIMARY KEY (organization_id, user_id)
);

CREATE TABLE IF NOT EXISTS contacts (
    id uuid PRIMARY KEY,
    organization_id uuid NOT NULL REFERENCES organizations(id) ON DELETE CASCADE,
    full_name varchar(160) NOT NULL,
    email varchar(254),
    phone varchar(32),
    notes varchar(2000),
    created_at timestamptz NOT NULL
);

CREATE INDEX IF NOT EXISTS idx_contacts_org_created
    ON contacts (organization_id ASC, created_at DESC);
