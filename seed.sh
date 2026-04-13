#!/usr/bin/env bash

set -e

API="http://localhost:5000/api"

post() {
  local label="$1"
  local url="$2"
  local data="$3"
  local status
  status=$(curl -s -o /dev/null -w "%{http_code}" -X POST "$url" \
    -H "Content-Type: application/json" -d "$data")
  echo "$label — $status"
}

echo "=== Users ==="
post "Virginia Augustus (1001)" "$API/users" \
  '{"username":"1001","password":"password","firstName":"Virginia","lastName":"Augustus"}'
post "Andrew Wiggin (1002)" "$API/users" \
  '{"username":"1002","password":"password","firstName":"Andrew","lastName":"Wiggin"}'

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
echo "=== Orders ==="
# IDs assume fresh DB: Users 1-2, MenuItems 1-6, Discounts 1-3, Taxes 1-2

# Alice: 1 Hamburger, 2 Medium Fries + 10% Off + City Tax + State Tax
post "Alice order 1" "$API/orders" '{
  "userId": 1,
  "items": [
    {"menuItemId": 1, "quantity": 1},
    {"menuItemId": 3, "quantity": 2}
  ],
  "discountIds": [1],
  "taxIds": [1, 2]
}'

# Alice: 1 Cheeseburger, 1 Large Fries, 1 Soda + City Tax + State Tax
post "Alice order 2" "$API/orders" '{
  "userId": 1,
  "items": [
    {"menuItemId": 2, "quantity": 1},
    {"menuItemId": 4, "quantity": 1},
    {"menuItemId": 5, "quantity": 1}
  ],
  "discountIds": [],
  "taxIds": [1, 2]
}'

# Bob: 2 Hamburgers, 1 Milkshake + $2 Off + City Tax + State Tax
post "Bob order 1" "$API/orders" '{
  "userId": 2,
  "items": [
    {"menuItemId": 1, "quantity": 2},
    {"menuItemId": 6, "quantity": 1}
  ],
  "discountIds": [2],
  "taxIds": [1, 2]
}'

echo ""
echo "Done!"
