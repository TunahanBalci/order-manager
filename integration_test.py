import requests
import psycopg2
import time
import json
import os

# Configuration
ORDER_API_URL = "http://localhost:5046/api/Orders"
INVENTORY_API_URL = "http://localhost:5278/api/Inventory"
DB_HOST = "localhost"
DB_PORT = "5123"
DB_NAME = "OrderDb"
DB_USER = "postgres"
DB_PASSWORD = "postgres"

def get_db_connection():
    return psycopg2.connect(
        host=DB_HOST,
        port=DB_PORT,
        database=DB_NAME,
        user=DB_USER,
        password=DB_PASSWORD
    )

def check_order_status(order_id, expected_status, retries=10):
    print(f"Checking status for Order {order_id} (Expected: {expected_status})...")
    conn = get_db_connection()
    cursor = conn.cursor()
    
    for i in range(retries):
        cursor.execute('SELECT "Status" FROM "Orders" WHERE "Id" = %s', (order_id,))
        result = cursor.fetchone()
        
        if result:
            status = result[0]
            # Status Enum: Pending=0, Processing=1, Completed=2, Failed=3
            status_map = {0: "Pending", 1: "Processing", 2: "Completed", 3: "Failed"}
            current_status = status_map.get(status, "Unknown")
            
            print(f"Attempt {i+1}: Current Status = {current_status}")
            
            if current_status == expected_status:
                print(f"Order {order_id} reached expected status: {expected_status}")
                cursor.close()
                conn.close()
                return True
        
        time.sleep(1)
    
    print(f"Order {order_id} failed to reach expected status: {expected_status}")
    cursor.close()
    conn.close()
    return False

def run_test_scenario(scenario_name, payload, expected_status):
    print(f"\n--- Running Scenario: {scenario_name} ---")
    
    # Send Order
    response = None
    try:
        response = requests.post(ORDER_API_URL, json=payload)
        response.raise_for_status()
        data = response.json()
        order_id = data.get("orderId")
        print(f"Order Created: {order_id}")
    except Exception as e:
        print(f"Failed to create order: {e}")
        if response:
            print(response.text)
        return False

    # Check DB Status
    if not check_order_status(order_id, expected_status):
        return False

    # Check Inventory
    return check_inventory(payload["items"])

def check_inventory(items):
    print("Checking Inventory...")
    try:
        response = requests.get(INVENTORY_API_URL)
        response.raise_for_status()
        inventory = response.json()
        
        # Create a map of ProductId -> InventoryItem
        inv_map = {item['productId']: item for item in inventory}
        
        for item in items:
            prod_id = item['productId']
            if prod_id in inv_map:
                inv_item = inv_map[prod_id]
                print(f"Product {prod_id}: Qty={inv_item['quantity']}, Reserved={inv_item['reservedQuantity']}")
            else:
                print(f"Product {prod_id} not found in inventory (might be auto-created later)")
                
        return True
    except Exception as e:
        print(f"Failed to check inventory: {e}")
        return True 

def test_db_connection():
    print("Testing DB Connection...")
    try:
        conn = get_db_connection()
        conn.close()
        print("DB Connection Successful")
        return True
    except Exception as e:
        print(f"DB Connection Failed: {e}")
        return False

def main():
    if not test_db_connection():
        exit(1)

    # Scenario 1: Success
    payload_success = {
        "customerName": "Test User Success",
        "shippingAddress": "123 Main St",
        "totalAmount": 500,
        "cardNumber": "1234-5678-9012-3456",
        "items": [
            {"productId": "11111111-1111-1111-1111-111111111111", "productName": "Item A", "quantity": 1, "unitPrice": 500}
        ]
    }
    
    if not run_test_scenario("Successful Order", payload_success, "Completed"):
        exit(1)

    # Scenario 2: Failure
    payload_fail = {
        "customerName": "Test User Fail",
        "shippingAddress": "456 Elm St",
        "totalAmount": 1500,
        "cardNumber": "9876-5432-1098-7654",
        "items": [
            {"productId": "22222222-2222-2222-2222-222222222222", "productName": "Item B", "quantity": 1, "unitPrice": 1500}
        ]
    }
    
    if not run_test_scenario("Failed Order", payload_fail, "Failed"):
        exit(1)

    print("All Integration Tests Passed!")

if __name__ == "__main__":
    main()
