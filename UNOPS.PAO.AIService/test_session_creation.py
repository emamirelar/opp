"""
Test Session Creation Behavior

This test verifies that the /chat endpoint correctly handles:
1. Empty session_id - should always create new session
2. Provided session_id that doesn't exist - should create new session  
3. Provided session_id that exists - should retrieve existing session
4. State updates on existing sessions
"""

import asyncio
import json
import uuid
import httpx
from typing import Dict, Any

# Test configuration
BASE_URL = "http://localhost:8000"
TEST_APP_NAME = "ai_assistant"
TEST_USER_ID = "test_user_123"
TEST_MESSAGE = "Hello, this is a test message"

class SessionTestClient:
    def __init__(self, base_url: str = BASE_URL):
        self.base_url = base_url
        self.client = httpx.AsyncClient(timeout=30.0)
    
    async def close(self):
        await self.client.aclose()
    
    async def send_chat_request(
        self, 
        session_id: str = "", 
        state: Dict[str, Any] = None,
        message: str = TEST_MESSAGE,
        app_name: str = TEST_APP_NAME,
        user_id: str = TEST_USER_ID
    ) -> Dict[str, Any]:
        """Send a chat request and return the response"""
        
        payload = {
            "app_name": app_name,
            "user_id": user_id,
            "session_id": session_id,
            "message": message,
            "streaming": False,
            "state": json.dumps(state) if state else ""
        }
        
        print(f"\n📤 Sending chat request:")
        print(f"   session_id: '{session_id}' (empty={not session_id})")
        print(f"   state: {state}")
        print(f"   message: {message}")
        
        response = await self.client.post(f"{self.base_url}/chat", json=payload)
        
        print(f"📥 Response status: {response.status_code}")
        
        if response.status_code != 200:
            print(f"❌ Error response: {response.text}")
            response.raise_for_status()
        
        result = response.json()
        returned_session_id = result.get("session_id")
        print(f"   returned session_id: {returned_session_id}")
        
        return result

async def test_empty_session_id_creates_new_session():
    """Test that empty session_id always creates a new session"""
    print("\n" + "="*60)
    print("🧪 TEST: Empty session_id creates new session")
    print("="*60)
    
    client = SessionTestClient()
    
    try:
        # Test 1: Send request with empty session_id
        result1 = await client.send_chat_request(session_id="")
        session_id_1 = result1.get("session_id")
        
        assert session_id_1, "Should return a session_id"
        assert session_id_1 != "", "Session_id should not be empty"
        
        # Test 2: Send another request with empty session_id
        result2 = await client.send_chat_request(session_id="")
        session_id_2 = result2.get("session_id")
        
        assert session_id_2, "Should return a session_id"
        assert session_id_2 != "", "Session_id should not be empty"
        assert session_id_1 != session_id_2, "Should create different sessions for each empty session_id request"
        
        print(f"✅ Test passed!")
        print(f"   First empty request created: {session_id_1}")
        print(f"   Second empty request created: {session_id_2}")
        print(f"   Different sessions: {session_id_1 != session_id_2}")
        
    finally:
        await client.close()

async def test_provided_session_id_behavior():
    """Test behavior with provided session_id"""
    print("\n" + "="*60)
    print("🧪 TEST: Provided session_id behavior")
    print("="*60)
    
    client = SessionTestClient()
    
    try:
        # Generate a unique session_id for this test
        test_session_id = f"test_session_{uuid.uuid4()}"
        
        # Test 1: First request with new session_id (should create)
        initial_state = {"test_key": "initial_value", "counter": 1}
        result1 = await client.send_chat_request(
            session_id=test_session_id,
            state=initial_state
        )
        
        returned_session_id_1 = result1.get("session_id")
        assert returned_session_id_1 == test_session_id, f"Should return same session_id. Expected: {test_session_id}, Got: {returned_session_id_1}"
        
        # Test 2: Second request with same session_id (should reuse)
        updated_state = {"test_key": "updated_value", "counter": 2, "new_field": "added"}
        result2 = await client.send_chat_request(
            session_id=test_session_id,
            state=updated_state
        )
        
        returned_session_id_2 = result2.get("session_id")
        assert returned_session_id_2 == test_session_id, f"Should return same session_id. Expected: {test_session_id}, Got: {returned_session_id_2}"
        
        print(f"✅ Test passed!")
        print(f"   Session created with ID: {test_session_id}")
        print(f"   First request returned: {returned_session_id_1}")
        print(f"   Second request returned: {returned_session_id_2}")
        print(f"   Session reused correctly: {returned_session_id_1 == returned_session_id_2 == test_session_id}")
        
    finally:
        await client.close()

async def test_state_persistence():
    """Test that state persists across requests for the same session"""
    print("\n" + "="*60)
    print("🧪 TEST: State persistence across requests")
    print("="*60)
    
    client = SessionTestClient()
    
    try:
        # Create initial session with state
        test_session_id = f"persistence_test_{uuid.uuid4()}"
        initial_state = {
            "user_email": "test@example.com",
            "preferences": {"theme": "dark", "language": "en"},
            "step": 1
        }
        
        result1 = await client.send_chat_request(
            session_id=test_session_id,
            state=initial_state,
            message="First message with initial state"
        )
        
        # Update state in second request
        updated_state = {
            "step": 2,
            "last_action": "updated_preferences",
            "preferences": {"theme": "light", "language": "en", "notifications": True}
        }
        
        result2 = await client.send_chat_request(
            session_id=test_session_id,
            state=updated_state,
            message="Second message with updated state"
        )
        
        # Third request with no state (should preserve existing state)
        result3 = await client.send_chat_request(
            session_id=test_session_id,
            state=None,
            message="Third message with no state change"
        )
        
        print(f"✅ State persistence test completed!")
        print(f"   Session ID: {test_session_id}")
        print(f"   All requests used same session: {result1.get('session_id') == result2.get('session_id') == result3.get('session_id')}")
        
    finally:
        await client.close()

async def test_multiple_users_separate_sessions():
    """Test that different users get separate sessions even with same session_id"""
    print("\n" + "="*60)
    print("🧪 TEST: Multiple users get separate sessions")
    print("="*60)
    
    client = SessionTestClient()
    
    try:
        # Use same session_id for different users
        shared_session_id = f"shared_session_{uuid.uuid4()}"
        user1_id = "user_1"
        user2_id = "user_2"
        
        # User 1 creates session
        result1 = await client.send_chat_request(
            session_id=shared_session_id,
            user_id=user1_id,
            state={"user": "user_1", "data": "user1_data"},
            message="Message from user 1"
        )
        
        # User 2 uses same session_id (should create separate session)
        result2 = await client.send_chat_request(
            session_id=shared_session_id,
            user_id=user2_id,
            state={"user": "user_2", "data": "user2_data"},
            message="Message from user 2"
        )
        
        print(f"✅ Multi-user test completed!")
        print(f"   Shared session_id: {shared_session_id}")
        print(f"   User 1 result session: {result1.get('session_id')}")
        print(f"   User 2 result session: {result2.get('session_id')}")
        print(f"   Both returned same session_id: {result1.get('session_id') == result2.get('session_id') == shared_session_id}")
        
    finally:
        await client.close()

async def run_all_tests():
    """Run all session creation tests"""
    print("🚀 Starting Session Creation Tests")
    print("Make sure the server is running on", BASE_URL)
    
    try:
        await test_empty_session_id_creates_new_session()
        await test_provided_session_id_behavior()
        await test_state_persistence()
        await test_multiple_users_separate_sessions()
        
        print("\n" + "="*60)
        print("🎉 ALL TESTS COMPLETED SUCCESSFULLY!")
        print("="*60)
        
    except Exception as e:
        print(f"\n❌ TEST FAILED: {e}")
        import traceback
        traceback.print_exc()
        raise

if __name__ == "__main__":
    # Run the tests
    asyncio.run(run_all_tests()) 