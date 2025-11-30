#!/bin/bash

BASE_URL="${1:-http://localhost:5146}"

echo "🎉 Testing WebSample endpoints..."
echo "Base URL: $BASE_URL"
echo ""

echo "1. 📅 Scheduling party (3 days from now)..."
if curl -sf "$BASE_URL/schedule-party" | jq . ; then
    echo "✅ Success"
else
    echo "❌ Failed"
fi
echo ""

echo "2. 📧 Sending birthday invite (will fail, 8h TTL)..."
if curl -sf "$BASE_URL/send-birthday-invite" | jq . ; then
    echo "✅ Success"
else
    echo "❌ Failed"
fi
echo ""

echo "3. 🍹 Ordering drinks (under 18, will go to dead-letter)..."
if curl -sf "$BASE_URL/order-drinks" | jq . ; then
    echo "✅ Success"
else
    echo "❌ Failed"
fi
echo ""

echo "4. 🌤️ Starting weather check job (every 3 minutes)..."
if curl -sf "$BASE_URL/start-weather-check" | jq . ; then
    echo "✅ Success"
else
    echo "❌ Failed"
fi
echo ""

echo "✅ All endpoints triggered!"
