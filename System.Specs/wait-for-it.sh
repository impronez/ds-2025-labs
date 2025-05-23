#!/usr/bin/env bash
# wait-for-it.sh

set -e

HOST_PORT="$1"
shift

CMD="$@"

HOST=$(echo "$HOST_PORT" | cut -d : -f 1)
PORT=$(echo "$HOST_PORT" | cut -d : -f 2)

echo "Waiting for $HOST:$PORT to become available..."

while ! nc -z "$HOST" "$PORT"; do
  sleep 1
done

echo "$HOST:$PORT is available. Running command: $CMD"
exec $CMD
