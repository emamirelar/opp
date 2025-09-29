#!/usr/bin/env python3
"""
Simple script to test if streaming is working properly
Run this to verify that chunks appear immediately in the browser
"""

import requests
import time
import sys

def test_streaming_endpoint():
    """Test the test-stream endpoint to verify real-time streaming"""
    print("🧪 Testing streaming endpoint...")
    print("📝 This should show chunks appearing every second, not all at once")
    print("=" * 60)
    
    try:
        # Replace with your actual server URL
        url = "http://localhost:8000/api/ai-assistant/test-stream"  # Update port if different
        
        print(f"📡 Connecting to: {url}")
        
        # Stream the response
        with requests.get(url, stream=True, timeout=30) as response:
            if response.status_code != 200:
                print(f"❌ Error: HTTP {response.status_code}")
                return False
                
            print("✅ Connected! Streaming chunks:")
            print("-" * 40)
            
            chunk_count = 0
            start_time = time.time()
            
            for line in response.iter_lines(decode_unicode=True):
                if line:
                    current_time = time.time()
                    elapsed = current_time - start_time
                    
                    if line.startswith('data: '):
                        chunk_count += 1
                        data = line[6:]  # Remove 'data: ' prefix
                        print(f"⏰ [{elapsed:.1f}s] Chunk {chunk_count}: {data[:80]}...")
                        
                        # If chunks come in real-time, there should be ~1 second between them
                        if chunk_count > 1:
                            expected_time = chunk_count - 1  # First chunk comes immediately
                            if abs(elapsed - expected_time) > 2:  # Allow 2 second tolerance
                                print(f"⚠️  WARNING: Chunk timing suggests buffering (expected ~{expected_time}s, got {elapsed:.1f}s)")
            
            total_time = time.time() - start_time
            print("-" * 40)
            print(f"📊 Test Results:")
            print(f"   - Total chunks: {chunk_count}")
            print(f"   - Total time: {total_time:.1f}s")
            
            if chunk_count >= 5 and total_time >= 4:  # 5 chunks with 1s delays = ~5s minimum
                print("✅ Streaming appears to be working correctly!")
                return True
            else:
                print("❌ Streaming may still have buffering issues")
                return False
                
    except requests.exceptions.RequestException as e:
        print(f"❌ Connection error: {e}")
        return False
    except KeyboardInterrupt:
        print("\n⏹️ Test interrupted by user")
        return False

if __name__ == "__main__":
    print("🚀 UNOPS AI Service - Streaming Test")
    print("=" * 60)
    
    success = test_streaming_endpoint()
    
    print("\n" + "=" * 60)
    if success:
        print("🎉 Streaming test PASSED!")
        print("💡 Your chat streaming should now work in the browser")
    else:
        print("⚠️ Streaming test FAILED!")
        print("💡 Check server configuration and try again")
        
    print("\n📋 Next steps:")
    print("1. Make sure your server is running with the updated uvicorn flags")
    print("2. Test this endpoint in your browser network tab")
    print("3. If still buffered, check if you're behind nginx/proxy")
    
    sys.exit(0 if success else 1)
