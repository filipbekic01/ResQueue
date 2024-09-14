FROM mongo:latest

LABEL container_name="webone-mongo"

EXPOSE 27017

RUN echo "rs.initiate();" > /docker-entrypoint-initdb.d/init-rs.js

CMD ["mongod", "--replSet", "rs0"]
