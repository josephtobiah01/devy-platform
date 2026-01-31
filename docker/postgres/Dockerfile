FROM postgres:16-alpine

# Install extensions
RUN apk add --no-cache postgresql-contrib

# Set timezone
ENV TZ=UTC
RUN ln -snf /usr/share/zoneinfo/$TZ /etc/localtime && echo $TZ > /etc/timezone

# Health check
HEALTHCHECK --interval=10s --timeout=5s --retries=5 \
  CMD pg_isready -U ${POSTGRES_USER} -d ${POSTGRES_DB} || exit 1

# Copy initialization scripts
COPY init-scripts/ /docker-entrypoint-initdb.d/