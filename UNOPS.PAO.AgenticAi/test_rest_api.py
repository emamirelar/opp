#!/usr/bin/env python3
"""
Test script for ADK REST API endpoints
Tests session creation with initial state and using /run_sse
"""

import requests
import json
import time
from dotenv import load_dotenv

load_dotenv()

def test_session_creation_with_state():
    """Test creating a session with initial state via REST API"""
    
    # ADK web server URL
    base_url = "http://localhost:8000"
    
    # Session creation endpoint
    session_url = f"{base_url}/apps/opportunity_ai_agent/users/user/sessions"
    
    # Initial state with user info
    initial_state = {
        "user_name": "Anusha Swaminathan",
        "user_email": "anushas@unops.org",
        "user_role": "Partnership Manager",
        "user_department": "UNOPS",
        "user_preferences": {
            "language": "en",
            "timezone": "UTC",
            "notification_enabled": True
        }
    }
    
    # Create session with initial state
    payload = {
        "state": initial_state
    }
    
    print("🔍 Testing Session Creation with Initial State")
    print("=" * 50)
    print(f"📍 Endpoint: POST {session_url}")
    print(f"📤 Payload: {json.dumps(payload, indent=2)}")
    
    try:
        response = requests.post(session_url, json=payload)
        
        if response.status_code == 200:
            session_data = response.json()
            session_id = session_data.get('id')
            
            print(f"✅ Session created successfully!")
            print(f"📋 Session ID: {session_id}")
            print(f"📊 Response: {json.dumps(session_data, indent=2)}")
            
            return session_id
        else:
            print(f"❌ Failed to create session: {response.status_code}")
            print(f"📄 Response: {response.text}")
            return None
            
    except Exception as e:
        print(f"❌ Error creating session: {e}")
        return None

def test_run_sse_with_state(session_id):
    """Test using /run_sse endpoint with a session that has initial state"""
    
    if not session_id:
        print("❌ No session ID provided")
        return
    
    base_url = "http://localhost:8000"
    
    # Run SSE endpoint
    run_sse_url = f"{base_url}/apps/opportunity_ai_agent/users/user/sessions/{session_id}/run_sse"
    
    # Query that should use the user info from initial state
    payload = {
        "message": "Hello! Can you tell me about my profile and help me with partnership opportunities?"
    }
    
    print("\n🔍 Testing /run_sse with Session State")
    print("=" * 50)
    print(f"📍 Endpoint: POST {run_sse_url}")
    print(f"📤 Payload: {json.dumps(payload, indent=2)}")
    
    try:
        response = requests.post(run_sse_url, json=payload, stream=True)
        
        if response.status_code == 200:
            print("✅ SSE stream started successfully!")
            print("📡 Streaming response:")
            print("-" * 30)
            
            for line in response.iter_lines():
                if line:
                    decoded_line = line.decode('utf-8')
                    if decoded_line.startswith('data: '):
                        data = decoded_line[6:]  # Remove 'data: ' prefix
                        if data.strip() and data != '[DONE]':
                            try:
                                event_data = json.loads(data)
                                print(f"📥 Event: {json.dumps(event_data, indent=2)}")
                            except json.JSONDecodeError:
                                print(f"📥 Raw data: {data}")
                    else:
                        print(f"📥 Line: {decoded_line}")
        else:
            print(f"❌ Failed to start SSE stream: {response.status_code}")
            print(f"📄 Response: {response.text}")
            
    except Exception as e:
        print(f"❌ Error with SSE stream: {e}")

def test_get_session_state(session_id):
    """Test retrieving session state to verify it persists"""
    
    if not session_id:
        print("❌ No session ID provided")
        return
    
    base_url = "http://localhost:8000"
    
    # Get session endpoint
    session_url = f"{base_url}/apps/opportunity_ai_agent/users/user/sessions/{session_id}"
    
    print("\n🔍 Testing Session State Retrieval")
    print("=" * 50)
    print(f"📍 Endpoint: GET {session_url}")
    
    try:
        response = requests.get(session_url)
        
        if response.status_code == 200:
            session_data = response.json()
            print("✅ Session retrieved successfully!")
            print(f"📊 Session State: {json.dumps(session_data.get('state', {}), indent=2)}")
            print(f"📋 Full Session: {json.dumps(session_data, indent=2)}")
        else:
            print(f"❌ Failed to retrieve session: {response.status_code}")
            print(f"📄 Response: {response.text}")
            
    except Exception as e:
        print(f"❌ Error retrieving session: {e}")

def main():
    """Main test function"""
    
    print("🚀 ADK REST API Session State Testing")
    print("=" * 60)
    print("ℹ️  Make sure your ADK web server is running on localhost:8000")
    print("ℹ️  Start it with: python run_adk_web.py")
    print()
    
    # Test 1: Create session with initial state
    session_id = test_session_creation_with_state()
    
    if session_id:
        # Test 2: Verify session state persists
        test_get_session_state(session_id)
        
        # Test 3: Use /run_sse with the session
        test_run_sse_with_state(session_id)
        
        # Test 4: Check state again after conversation
        test_get_session_state(session_id)
    
    print("\n✨ Testing completed!")

if __name__ == "__main__":
    main() 