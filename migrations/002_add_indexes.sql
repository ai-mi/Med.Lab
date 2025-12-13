CREATE INDEX IF NOT EXISTS idx_event_store_aggregate ON event_store(aggregate_id);
CREATE INDEX IF NOT EXISTS idx_read_model_patient ON laboratory_read_model(patient_id);
CREATE INDEX IF NOT EXISTS idx_read_model_testtype ON laboratory_read_model(test_type);
CREATE INDEX IF NOT EXISTS idx_outbox_published ON outbox(published);
