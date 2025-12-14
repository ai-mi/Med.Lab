-- Migration: create outbox table supporting transactional outbox pattern
-- Adds columns for locking, retry/backoff, error tracking and auditing.
BEGIN;

CREATE TABLE IF NOT EXISTS outbox (
  id uuid PRIMARY KEY,
  payload jsonb NOT NULL,
  event_type text NOT NULL,
  published boolean NOT NULL DEFAULT false,
  attempts integer NOT NULL DEFAULT 0,
  last_error text,
  next_attempt_at timestamptz,
  locked_by uuid,
  locked_at timestamptz,
  created_at timestamptz NOT NULL DEFAULT NOW(),
  published_at timestamptz
);

CREATE INDEX IF NOT EXISTS idx_outbox_published_next_attempt
  ON outbox (published, next_attempt_at);

CREATE INDEX IF NOT EXISTS idx_outbox_locked_by
  ON outbox (locked_by);

CREATE INDEX IF NOT EXISTS idx_outbox_created_at
  ON outbox (created_at);

COMMIT;