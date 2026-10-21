-- Local bootstrap schema only. Replace with reviewed EF migrations before production.
CREATE TABLE IF NOT EXISTS organizations (
    id uuid PRIMARY KEY,
    name varchar(180) NOT NULL
);

CREATE TABLE IF NOT EXISTS tenant_memberships (
    tenant_id uuid NOT NULL REFERENCES organizations(id) ON DELETE CASCADE,
    subject_id varchar(200) NOT NULL,
    PRIMARY KEY (tenant_id, subject_id)
);

CREATE TABLE IF NOT EXISTS contacts (
    id uuid PRIMARY KEY,
    tenant_id uuid NOT NULL REFERENCES organizations(id) ON DELETE CASCADE,
    name varchar(160) NOT NULL,
    email varchar(320),
    created_at timestamptz NOT NULL
);
CREATE INDEX IF NOT EXISTS ix_contacts_tenant_created_at
    ON contacts (tenant_id, created_at);
