#!/bin/sh

if [ -z "$REDIS_PASSWORD" ]; then
  echo "Redis password is not set. Exiting..."
  exit 1
fi

exec redis-server --requirepass "$REDIS_PASSWORD"