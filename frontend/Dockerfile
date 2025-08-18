# Build stage
FROM node:18-alpine AS build

WORKDIR /app

# Copy package files
COPY package*.json ./

# Install all dependencies (including devDependencies for build)
RUN npm install

# Copy source code
COPY . .

# Build the application
RUN npm run build

# Production stage
FROM node:18-alpine AS production

WORKDIR /app

# Install serve globally
RUN npm install -g serve

# Copy built application
COPY --from=build /app/dist ./dist
COPY --from=build /app/package.json ./

# Create non-root user
RUN addgroup -g 1001 -S nodejs
RUN adduser -S react -u 1001

# Change ownership
RUN chown -R react:nodejs /app
USER react

# Expose port
EXPOSE 3000

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
  CMD wget --no-verbose --tries=1 --spider http://localhost:3000/ || exit 1

# Start the application
CMD ["serve", "-s", "dist", "-l", "3000"]
