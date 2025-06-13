#!/bin/bash

# Harmoni360 Standalone Server Deployment Script
# This script automates the deployment process for standalone server

set -e  # Exit on any error

# Configuration
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(dirname "$SCRIPT_DIR")"
LOG_FILE="/tmp/harmoni360-deploy-$(date +%Y%m%d_%H%M%S).log"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Logging function
log() {
    echo -e "${BLUE}[$(date '+%Y-%m-%d %H:%M:%S')]${NC} $1" | tee -a "$LOG_FILE"
}

error() {
    echo -e "${RED}[ERROR]${NC} $1" | tee -a "$LOG_FILE"
    exit 1
}

warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1" | tee -a "$LOG_FILE"
}

success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1" | tee -a "$LOG_FILE"
}

# Check if running as root
check_root() {
    if [[ $EUID -eq 0 ]]; then
        error "This script should not be run as root. Please run as a regular user with sudo privileges."
    fi
}

# Check prerequisites
check_prerequisites() {
    log "Checking prerequisites..."
    
    # Check if Docker is installed
    if ! command -v docker &> /dev/null; then
        error "Docker is not installed. Please install Docker first."
    fi
    
    # Check if Docker Compose is installed
    if ! command -v docker-compose &> /dev/null; then
        error "Docker Compose is not installed. Please install Docker Compose first."
    fi
    
    # Check if user is in docker group
    if ! groups $USER | grep -q docker; then
        error "User $USER is not in the docker group. Please add user to docker group and re-login."
    fi
    
    # Check if Git is installed
    if ! command -v git &> /dev/null; then
        error "Git is not installed. Please install Git first."
    fi
    
    # Check available disk space (minimum 10GB)
    AVAILABLE_SPACE=$(df / | awk 'NR==2 {print $4}')
    if [ $AVAILABLE_SPACE -lt 10485760 ]; then  # 10GB in KB
        error "Insufficient disk space. At least 10GB of free space is required."
    fi
    
    success "Prerequisites check passed"
}

# Create directory structure
create_directories() {
    log "Creating directory structure..."
    
    sudo mkdir -p /opt/harmoni360/{data,logs,backups,ssl,scripts}
    sudo mkdir -p /opt/harmoni360/data/{postgres,redis,uploads,seq,prometheus,grafana}
    sudo mkdir -p /opt/harmoni360/logs/{app,nginx}
    sudo mkdir -p /opt/harmoni360/backups/{postgres,uploads}
    
    # Set ownership
    sudo chown -R $USER:$USER /opt/harmoni360
    
    # Set permissions
    chmod -R 755 /opt/harmoni360
    chmod -R 700 /opt/harmoni360/backups
    chmod -R 700 /opt/harmoni360/ssl
    
    success "Directory structure created"
}

# Check environment configuration
check_environment() {
    log "Checking environment configuration..."
    
    if [ ! -f "$PROJECT_ROOT/.env.prod" ]; then
        warning "Production environment file not found. Creating from template..."
        cp "$PROJECT_ROOT/.env.prod.example" "$PROJECT_ROOT/.env.prod"
        error "Please edit .env.prod with your configuration values and run the script again."
    fi
    
    # Source environment file
    source "$PROJECT_ROOT/.env.prod"
    
    # Check required variables
    REQUIRED_VARS=("POSTGRES_PASSWORD" "REDIS_PASSWORD" "JWT_KEY" "DOMAIN_NAME")
    for var in "${REQUIRED_VARS[@]}"; do
        if [ -z "${!var}" ]; then
            error "Required environment variable $var is not set in .env.prod"
        fi
    done
    
    success "Environment configuration validated"
}

# Generate SSL certificates
setup_ssl() {
    log "Setting up SSL certificates..."
    
    if [ ! -f "/opt/harmoni360/ssl/cert.pem" ] || [ ! -f "/opt/harmoni360/ssl/key.pem" ]; then
        log "SSL certificates not found. Generating self-signed certificates for testing..."
        
        sudo openssl req -x509 -nodes -days 365 -newkey rsa:2048 \
            -keyout /opt/harmoni360/ssl/key.pem \
            -out /opt/harmoni360/ssl/cert.pem \
            -subj "/C=US/ST=State/L=City/O=Harmoni360/CN=${DOMAIN_NAME:-localhost}" \
            2>/dev/null
        
        sudo chown $USER:$USER /opt/harmoni360/ssl/*.pem
        chmod 600 /opt/harmoni360/ssl/key.pem
        chmod 644 /opt/harmoni360/ssl/cert.pem
        
        warning "Self-signed certificates generated. For production, please use Let's Encrypt or proper CA certificates."
    else
        log "SSL certificates already exist"
    fi
    
    success "SSL certificates configured"
}

# Build and deploy application
deploy_application() {
    log "Building and deploying application..."
    
    cd "$PROJECT_ROOT"
    
    # Pull latest changes (if this is an update)
    if [ -d ".git" ]; then
        log "Pulling latest changes..."
        git pull origin main || warning "Failed to pull latest changes. Continuing with current code."
    fi
    
    # Build application
    log "Building Docker images..."
    docker-compose -f docker-compose.prod.yml build --no-cache
    
    # Start services
    log "Starting services..."
    docker-compose -f docker-compose.prod.yml up -d
    
    # Wait for services to be ready
    log "Waiting for services to start..."
    sleep 30
    
    # Check service health
    check_service_health
    
    success "Application deployed successfully"
}

# Check service health
check_service_health() {
    log "Checking service health..."
    
    # Check if all containers are running
    FAILED_SERVICES=()
    SERVICES=("postgres" "redis" "app" "nginx" "seq" "prometheus" "grafana")
    
    for service in "${SERVICES[@]}"; do
        if ! docker-compose -f docker-compose.prod.yml ps "$service" | grep -q "Up"; then
            FAILED_SERVICES+=("$service")
        fi
    done
    
    if [ ${#FAILED_SERVICES[@]} -ne 0 ]; then
        error "The following services failed to start: ${FAILED_SERVICES[*]}"
    fi
    
    # Test application health endpoint
    log "Testing application health endpoint..."
    for i in {1..10}; do
        if curl -f -s -k "https://localhost/health" > /dev/null 2>&1; then
            success "Application health check passed"
            return 0
        fi
        log "Health check attempt $i/10 failed, retrying in 10 seconds..."
        sleep 10
    done
    
    error "Application health check failed after 10 attempts"
}

# Run database migrations
run_migrations() {
    log "Running database migrations..."
    
    # Wait for database to be ready
    log "Waiting for database to be ready..."
    for i in {1..30}; do
        if docker-compose -f docker-compose.prod.yml exec -T postgres pg_isready -U harmoni360 > /dev/null 2>&1; then
            break
        fi
        if [ $i -eq 30 ]; then
            error "Database failed to become ready after 5 minutes"
        fi
        sleep 10
    done
    
    # Run migrations
    log "Applying database migrations..."
    docker-compose -f docker-compose.prod.yml exec -T app dotnet ef database update || error "Database migration failed"
    
    success "Database migrations completed"
}

# Install maintenance scripts
install_maintenance_scripts() {
    log "Installing maintenance scripts..."
    
    # Copy maintenance scripts
    cp "$PROJECT_ROOT/docs/Deployment/Standalone_Server/scripts/"* /opt/harmoni360/scripts/ 2>/dev/null || true
    
    # Make scripts executable
    chmod +x /opt/harmoni360/scripts/*.sh 2>/dev/null || true
    
    # Install cron jobs
    log "Installing cron jobs..."
    
    # Backup script (daily at 2 AM)
    (crontab -l 2>/dev/null | grep -v "harmoni360-backup"; echo "0 2 * * * /opt/harmoni360/scripts/backup.sh") | crontab -
    
    # Health check script (every 15 minutes)
    (crontab -l 2>/dev/null | grep -v "harmoni360-health"; echo "*/15 * * * * /opt/harmoni360/scripts/health-check.sh") | crontab -
    
    # Weekly maintenance (Sunday at 3 AM)
    (crontab -l 2>/dev/null | grep -v "harmoni360-weekly"; echo "0 3 * * 0 /opt/harmoni360/scripts/weekly-maintenance.sh") | crontab -
    
    success "Maintenance scripts installed"
}

# Configure firewall
configure_firewall() {
    log "Configuring firewall..."
    
    # Check if ufw is installed
    if command -v ufw &> /dev/null; then
        # Allow SSH, HTTP, and HTTPS
        sudo ufw allow ssh
        sudo ufw allow 80/tcp
        sudo ufw allow 443/tcp
        
        # Enable firewall if not already enabled
        sudo ufw --force enable
        
        success "Firewall configured"
    else
        warning "UFW firewall not found. Please configure firewall manually."
    fi
}

# Display deployment summary
display_summary() {
    log "Deployment completed successfully!"
    echo
    echo "=== Harmoni360 Deployment Summary ==="
    echo "Application URL: https://${DOMAIN_NAME:-localhost}"
    echo "Grafana Dashboard: https://${DOMAIN_NAME:-localhost}:3000"
    echo "Seq Logs: http://$(hostname -I | awk '{print $1}'):5341"
    echo
    echo "Default Credentials:"
    echo "- Grafana: admin / ${GRAFANA_ADMIN_PASSWORD:-admin}"
    echo
    echo "Important Files:"
    echo "- Configuration: $PROJECT_ROOT/.env.prod"
    echo "- Data Directory: /opt/harmoni360/data"
    echo "- Logs Directory: /opt/harmoni360/logs"
    echo "- Backups Directory: /opt/harmoni360/backups"
    echo
    echo "Maintenance:"
    echo "- Automated backups: Daily at 2 AM"
    echo "- Health checks: Every 15 minutes"
    echo "- Weekly maintenance: Sunday at 3 AM"
    echo
    echo "Next Steps:"
    echo "1. Configure proper SSL certificates for production"
    echo "2. Set up monitoring alerts in Grafana"
    echo "3. Test backup and recovery procedures"
    echo "4. Review security settings"
    echo
    echo "For troubleshooting, check: $LOG_FILE"
    echo "Documentation: docs/Deployment/Standalone_Server/"
}

# Main deployment function
main() {
    log "Starting Harmoni360 standalone server deployment..."
    
    check_root
    check_prerequisites
    create_directories
    check_environment
    setup_ssl
    configure_firewall
    deploy_application
    run_migrations
    install_maintenance_scripts
    display_summary
    
    success "Deployment completed successfully!"
}

# Handle script interruption
trap 'error "Deployment interrupted. Check log file: $LOG_FILE"' INT TERM

# Run main function
main "$@"
