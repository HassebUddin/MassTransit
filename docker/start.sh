#!/bin/bash
set -e

redis-server --daemonize yes --bind 0.0.0.0 --protected-mode no

exec /usr/local/bin/docker-entrypoint.sh rabbitmq-server
