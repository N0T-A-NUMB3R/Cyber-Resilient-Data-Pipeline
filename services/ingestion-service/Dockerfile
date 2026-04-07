FROM maven:3.9-eclipse-temurin-17-alpine AS build
WORKDIR /src

COPY pom.xml .
RUN mvn dependency:go-offline -q

COPY src/ ./src/
RUN mvn package -DskipTests -q

FROM eclipse-temurin:17-jre-alpine AS runtime

RUN addgroup -S ingestion && adduser -S ingestion -G ingestion
RUN mkdir -p /data && chown ingestion:ingestion /data

USER ingestion
WORKDIR /app

COPY --from=build /src/target/*.jar app.jar

VOLUME ["/data"]
EXPOSE 8081

ENTRYPOINT ["java", \
  "-Djava.security.egd=file:/dev/./urandom", \
  "-XX:MaxRAMPercentage=75.0", \
  "-jar", "app.jar"]
