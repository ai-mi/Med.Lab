-- Example for adding versioning column for future upcasting
ALTER TABLE event_store ADD COLUMN IF NOT EXISTS event_version INT DEFAULT 1;
