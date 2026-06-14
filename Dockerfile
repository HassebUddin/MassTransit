FROM rabbitmq:3-management

RUN apt-get update \
    && apt-get install -y --no-install-recommends wget redis-server \
    && wget -q https://github.com/rabbitmq/rabbitmq-delayed-message-exchange/releases/download/v3.13.0/rabbitmq_delayed_message_exchange-3.13.0.ez -O /plugins/rabbitmq_delayed_message_exchange-3.13.0.ez \
    && rabbitmq-plugins enable --offline rabbitmq_delayed_message_exchange \
    && rm -rf /var/lib/apt/lists/*

COPY docker/start.sh /start.sh
RUN sed -i 's/\r$//' /start.sh && chmod +x /start.sh

EXPOSE 5672 15672 6379

ENTRYPOINT ["/start.sh"]
