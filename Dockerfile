FROM rabbitmq:3-management

RUN apt-get update \
    && apt-get install -y --no-install-recommends redis-server \
    && rm -rf /var/lib/apt/lists/*

COPY docker/start.sh /start.sh
RUN sed -i 's/\r$//' /start.sh && chmod +x /start.sh

EXPOSE 5672 15672 6379

ENTRYPOINT ["/start.sh"]
