#!/usr/bin/env bash

set -e

API="http://localhost:5000/api"
TOKEN=""

post() {
  local label="$1"
  local url="$2"
  local data="$3"
  local auth_header=""
  if [ -n "$TOKEN" ]; then
    auth_header="Authorization: Bearer $TOKEN"
  fi
  local status
  status=$(curl -s -o /dev/null -w "%{http_code}" -X POST "$url" \
    -H "Content-Type: application/json" ${auth_header:+-H "$auth_header"} -d "$data")
  echo "$label — $status"
}

auth_post() {
  local label="$1"
  local url="$2"
  local data="$3"
  local response
  response=$(curl -s -X POST "$url" \
    -H "Content-Type: application/json" -d "$data")
  TOKEN=$(echo "$response" | grep -o '"token":"[^"]*"' | cut -d'"' -f4)
  if [ -n "$TOKEN" ]; then
    echo "$label — logged in"
  else
    echo "$label — FAILED"
    exit 1
  fi
}

echo "=== Users ==="
post "Virginia Augustus (1001)" "$API/users" \
  '{"username":"1001","password":"password","firstName":"Virginia","lastName":"Augustus"}'
post "Andrew Wiggin (1002)" "$API/users" \
  '{"username":"1002","password":"password","firstName":"Andrew","lastName":"Wiggin"}'

echo ""
echo "=== Authenticating ==="
auth_post "Login as Virginia (1001)" "$API/sessions" '{"username":"1001","password":"password"}'

echo ""
echo "=== Menu Items ==="
post "Hamburger" "$API/menuitems" '{"name":"Hamburger","price":8.99}'
post "Cheeseburger" "$API/menuitems" '{"name":"Cheeseburger","price":9.99}'
post "Medium Fries" "$API/menuitems" '{"name":"Medium Fries","price":3.49}'
post "Large Fries" "$API/menuitems" '{"name":"Large Fries","price":4.49}'
post "Soda" "$API/menuitems" '{"name":"Soda","price":1.99}'
post "Milkshake" "$API/menuitems" '{"name":"Milkshake","price":5.49}'

echo ""
echo "=== Discounts ==="
post "10% Off" "$API/discounts" '{"name":"10% Off","type":"percent","amount":10}'
post "\$2 Off" "$API/discounts" '{"name":"$2 Off","type":"fixed","amount":2.00}'
post "Half Off" "$API/discounts" '{"name":"Half Off","type":"percent","amount":50}'

echo ""
echo "=== Taxes ==="
post "City Tax (4%)" "$API/taxes" '{"name":"City Tax","percentage":4.00}'
post "State Tax (6.25%)" "$API/taxes" '{"name":"State Tax","percentage":6.25}'

echo ""
echo "=== Orders (as Virginia) ==="

post "Virginia order 1" "$API/orders" '{
  "items": [
    {"menuItemId": 1, "quantity": 1},
    {"menuItemId": 3, "quantity": 2}
  ],
  "discountIds": [1],
  "taxIds": [1, 2]
}'

post "Virginia order 2" "$API/orders" '{
  "items": [
    {"menuItemId": 2, "quantity": 1},
    {"menuItemId": 4, "quantity": 1},
    {"menuItemId": 5, "quantity": 1}
  ],
  "discountIds": [],
  "taxIds": [1, 2]
}'

echo ""
echo "=== Switching to Andrew ==="
auth_post "Login as Andrew (1002)" "$API/sessions" '{"username":"1002","password":"password"}'

echo ""
echo "=== Orders (as Andrew) ==="

post "Andrew order 1" "$API/orders" '{
  "items": [
    {"menuItemId": 1, "quantity": 2},
    {"menuItemId": 6, "quantity": 1}
  ],
  "discountIds": [2],
  "taxIds": [1, 2]
}'

echo ""
echo "Done!"
