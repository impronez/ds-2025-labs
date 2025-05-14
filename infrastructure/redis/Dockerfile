FROM redis:alpine

COPY redis-entrypoint.sh /usr/local/bin/
RUN chmod +x /usr/local/bin/redis-entrypoint.sh

ENTRYPOINT ["/usr/local/bin/redis-entrypoint.sh"]
