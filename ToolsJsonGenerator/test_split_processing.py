#!/usr/bin/env python3
"""
Test script to verify the split processing functionality
"""
import os
import sys
import shutil
import tempfile
import json

def test_split_processing():
    """Test the split processing functionality"""
    print("[TEST] Testing split processing functionality...")
    
    # Create a temporary test directory
    test_dir = tempfile.mkdtemp(prefix="pao_test_")
    temp_endpoints_dir = os.path.join(test_dir, "temp-endpoints")
    os.makedirs(temp_endpoints_dir)
    
    try:
        # Create sample endpoint files
        sample_controllers = ["Partner", "Contact", "Document"]
        
        for controller in sample_controllers:
            endpoint_data = {
                "assemblyName": "UNOPS.PAO.Presentation",
                "assemblyVersion": "1.0.0.0",
                "extractedAt": "2024-01-15T10:30:00Z",
                "controllers": [{
                    "name": f"{controller}Controller",
                    "baseRoute": f"api/{controller.lower()}",
                    "methods": [
                        {
                            "name": f"Get{controller}",
                            "httpMethod": "GET",
                            "route": "",
                            "parameters": [],
                            "returnType": f"List<{controller}>",
                            "summary": f"Get all {controller.lower()}s",
                            "example_uses": [f"Retrieve {controller.lower()} data"]
                        },
                        {
                            "name": f"Create{controller}",
                            "httpMethod": "POST", 
                            "route": "",
                            "parameters": [{"name": f"{controller.lower()}", "type": f"{controller}CreateRequest"}],
                            "returnType": f"{controller}",
                            "summary": f"Create new {controller.lower()}",
                            "example_uses": [f"Add new {controller.lower()}"]
                        }
                    ]
                }]
            }
            
            # Save endpoint file
            endpoint_file = os.path.join(temp_endpoints_dir, f"{controller}-endpoints.json")
            with open(endpoint_file, 'w') as f:
                json.dump(endpoint_data, f, indent=2)
            
            print(f"   [CREATED] {controller}-endpoints.json")
        
        print(f"[INFO] Created test files in: {temp_endpoints_dir}")
        print(f"[INFO] You can manually test with:")
        print(f"   cd {os.path.dirname(__file__)}")
        print(f"   python llm_generator.py --input \"{temp_endpoints_dir}\" --output \"{test_dir}/tools.json\"")
        print()
        print("[NOTE] Test files will be cleaned up when you're done")
        print(f"[CLEANUP] To clean up: rm -rf \"{test_dir}\"")
        
        return temp_endpoints_dir, test_dir
        
    except Exception as e:
        # Cleanup on error
        shutil.rmtree(test_dir, ignore_errors=True)
        raise e

if __name__ == "__main__":
    try:
        endpoints_dir, test_dir = test_split_processing()
        print()
        print("=" * 80)
        print("[SUCCESS] Test setup complete!")
        print("=" * 80)
        print(f"Test directory: {test_dir}")
        print(f"Endpoints directory: {endpoints_dir}")
        print()
        print("To test manually:")
        print(f"  python llm_generator.py --input \"{endpoints_dir}\" --output \"{test_dir}/tools.json\"")
        print()
        print("Note: Test files left for manual testing. Clean up when done:")
        print(f"  rm -rf \"{test_dir}\"")
        
    except Exception as e:
        print(f"[ERROR] Test setup failed: {e}")
        sys.exit(1) 