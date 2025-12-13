-- Event Store
CREATE TABLE IF NOT EXISTS event_store (
    id UUID PRIMARY KEY,
    aggregate_id UUID NOT NULL,
    event_type VARCHAR(255) NOT NULL,
    event_data JSONB NOT NULL,
    version BIGINT NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT NOW()
);

-- Snapshot Store
CREATE TABLE IF NOT EXISTS snapshot_store (
    patient_id UUID PRIMARY KEY,
    snapshot_data JSONB NOT NULL,
    version INT NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT NOW()
);

-- Read Model
CREATE TABLE IF NOT EXISTS laboratory_read_model (
    result_id UUID PRIMARY KEY,
    patient_id UUID NOT NULL,
    test_type VARCHAR(100) NOT NULL,
    result DOUBLE PRECISION NOT NULL,
    normal_min DOUBLE PRECISION NOT NULL,
    normal_max DOUBLE PRECISION NOT NULL,
    comment TEXT,
    created_at TIMESTAMP NOT NULL DEFAULT NOW()
);

-- Outbox
CREATE TABLE IF NOT EXISTS outbox (
    id UUID PRIMARY KEY,
    event_type VARCHAR(255) NOT NULL,
    payload JSONB NOT NULL,
    published BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMP NOT NULL DEFAULT NOW()
);
