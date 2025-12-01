#!/bin/bash

# Base URL
API_URL="http://localhost:5046/api/Orders"

# ==========================================
# 🎨 COLOR & FORMATTING DEFINITIONS
# ==========================================
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
CYAN='\033[0;36m'
BOLD='\033[1m'
NC='\033[0m' # No Color

# ==========================================
# 🛠 HELPER FUNCTIONS
# ==========================================

print_header() {
    echo ""
    echo -e "${BLUE}==========================================${NC}"
    echo -e "${BOLD}$1${NC}"
    echo -e "${BLUE}==========================================${NC}"
}

print_step() {
    echo -e "${CYAN}➜ $1${NC}"
}

print_success() {
    echo -e "${GREEN}✅ $1${NC}"
}

print_error() {
    echo -e "${RED}❌ $1${NC}"
}

print_info() {
    echo -e "${YELLOW}ℹ️  $1${NC}"
}

# Function to handle curl requests and pretty print JSON if jq is installed
make_request() {
    local payload=$1
    
    # Send request via curl ( SILENT )
    response=$(curl -s -X POST "$API_URL" \
         -H "Content-Type: application/json" \
         -d "$payload")

    echo -e "${BOLD}Response:${NC}"
    
    # Check if 'jq' is installed for pretty printing
    if command -v jq &> /dev/null; then
        echo "$response" | jq .
    else
        echo "$response"
    fi
}

# ==========================================
# 🚀 MAIN SCRIPT EXECUTION
# ==========================================

clear
echo -e "${BOLD}Microservices Test Suite${NC}"
echo -e "Target URL: ${CYAN}$API_URL${NC}"

print_info "Ensure RabbitMQ is running and these services are up:"
echo -e "   1. ${GREEN}OrderApi${NC}"
echo -e "   2. ${GREEN}PaymentService${NC}"
echo -e "   3. ${GREEN}InventoryService${NC}"

# ------------------------------------------
# Scenario 1: Successful Order
# ------------------------------------------
print_header "Scenario 1: Successful Order (Amount < 1000)"

print_step "Sending order with Amount = 500..."

JSON_PAYLOAD_1='{
  "customerName": "John Doe",
  "totalAmount": 500,
  "cardNumber": "1234-5678-9012-3456"
}'

make_request "$JSON_PAYLOAD_1"

echo ""
print_info "Expected Outcome:"
echo -e "   📦 ${BOLD}OrderApi:${NC}         Order Received -> Confirmed"
echo -e "   💳 ${BOLD}PaymentService:${NC}   Payment ${GREEN}Success${NC}"
echo -e "   📝 ${BOLD}InventoryService:${NC} Allocate -> Commit"

# Optional: Pause between tests
echo ""
read -p "Press [Enter] to run the next scenario..."

# ------------------------------------------
# Scenario 2: Failed Order
# ------------------------------------------
print_header "Scenario 2: Failed Order (Amount >= 1000)"

print_step "Sending order with Amount = 1500..."

JSON_PAYLOAD_2='{
  "customerName": "Jane Doe",
  "totalAmount": 1500,
  "cardNumber": "9876-5432-1098-7654"
}'

make_request "$JSON_PAYLOAD_2"

echo ""
print_info "Expected Outcome:"
echo -e "   📦 ${BOLD}OrderApi:${NC}         Order Received -> Cancelled"
echo -e "   💳 ${BOLD}PaymentService:${NC}   Payment ${RED}Failed${NC}"
echo -e "   📝 ${BOLD}InventoryService:${NC} Allocate -> Release"

# ==========================================
# END
# ==========================================
echo ""
echo -e "${BLUE}==========================================${NC}"
print_success "Test Suite Completed."
print_info "Check the console logs of each service to verify the flow."
echo -e "${BLUE}==========================================${NC}"
echo ""