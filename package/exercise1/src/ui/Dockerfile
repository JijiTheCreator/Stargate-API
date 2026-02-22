# Stage 1: Build
FROM node:20-alpine AS build
WORKDIR /app

# Copy package files and install dependencies (layer caching)
COPY package*.json ./
RUN npm ci

# Copy source and build
COPY . .
RUN npm run build -- --configuration production

# Stage 2: Serve with Nginx
FROM nginx:alpine AS runtime

# Create non-root user
RUN adduser -D -g '' appuser

# Copy custom nginx config
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Copy built Angular app from build stage
COPY --from=build /app/dist/stargate-ui/browser /usr/share/nginx/html

# Switch to non-root user
USER appuser

EXPOSE 80

HEALTHCHECK --interval=10s --timeout=5s --retries=3 \
    CMD wget -q --spider http://localhost:80/ || exit 1
