#!/usr/bin/env python3
"""
Script to test all endpoints are accessible with correct prefixes
"""

import requests
import json

def test_endpoints():
    """Test that all endpoints are accessible"""
    base_url = "http://localhost:8000"
    
    endpoints_to_test = [
        {
            "name": "HTML Test Page",
            "url": f"{base_url}/api/ai-assistant/test-streaming-ui",
            "method": "GET",
            "expected_content_type": "text/html"
        },
        {
            "name": "Simple Streaming Test",
            "url": f"{base_url}/api/ai-assistant/test-stream", 
            "method": "GET",
            "expected_content_type": "text/event-stream"
        },
        {
            "name": "Framework Info",
            "url": f"{base_url}/framework/info",
            "method": "GET", 
            "expected_content_type": "application/json"
        }
    ]
    
    print("🧪 Testing Endpoint Accessibility")
    print("=" * 50)
    
    for endpoint in endpoints_to_test:
        try:
            print(f"\n📡 Testing: {endpoint['name']}")
            print(f"   URL: {endpoint['url']}")
            
            response = requests.get(endpoint['url'], timeout=5, stream=True)
            
            print(f"   ✅ Status: {response.status_code}")
            print(f"   📋 Content-Type: {response.headers.get('content-type', 'unknown')}")
            
            if response.status_code == 200:
                print(f"   🎉 SUCCESS: {endpoint['name']} is accessible!")
            else:
                print(f"   ❌ FAILED: Unexpected status code")
                
        except requests.exceptions.ConnectionError:
            print(f"   ❌ FAILED: Cannot connect to server")
            print(f"   💡 Make sure server is running with: python main.py")
        except requests.exceptions.Timeout:
            print(f"   ⏰ TIMEOUT: Request took too long")
        except Exception as e:
            print(f"   ❌ ERROR: {str(e)}")
    
    print("\n" + "=" * 50)
    print("💡 If HTML Test Page works, try it in your browser:")
    print(f"   {base_url}/api/ai-assistant/test-streaming-ui")

if __name__ == "__main__":
    test_endpoints()
