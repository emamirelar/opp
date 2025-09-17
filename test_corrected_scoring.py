#!/usr/bin/env python3
"""
Test the corrected scoring mechanism with proper endpoint names and URL templates
"""

import sys
import os
import json

# Add the project root to Python path
sys.path.append(os.path.join(os.path.dirname(__file__), 'UNOPS.PAO.AIService'))

def test_corrected_endpoint_selection():
    """Test endpoint selection with corrected names and URL templates"""
    
    print("🧪 TESTING CORRECTED ENDPOINT SELECTION")
    print("=" * 80)
    print("Query: 'Get me details of partner The Sunrise Project Australia Limited'")
    print("=" * 80)
    
    try:
        # Import the scoring function
        from ai_assistant.sub_agents.task_executor_agent.utils import score_endpoint_for_intent_standalone
        
        # Test parameters
        entity_name = "Partner"
        intent = "search"
        params = {
            "query": "The Sunrise Project Australia Limited",
            "name": "The Sunrise Project Australia Limited"
        }
        
        # Load partner endpoints configuration
        config_path = "UNOPS.PAO.AIService/config/tools/endpoints/partner-tools.json"
        with open(config_path, 'r') as f:
            config = json.load(f)
        
        endpoints = config.get('endpoints', [])
        print(f"📊 Found {len(endpoints)} partner endpoints to evaluate")
        print()
        
        # Score all endpoints for this test case
        scored_endpoints = []
        
        for endpoint in endpoints:
            try:
                score = score_endpoint_for_intent_standalone(
                    endpoint=endpoint,
                    entity_name=entity_name,
                    intent=intent,
                    extracted_params=params
                )
                
                if score > 0:  # Only include valid endpoints
                    scored_endpoints.append({
                        'name': endpoint.get('name', 'Unknown'),
                        'url': endpoint.get('url', ''),
                        'method': endpoint.get('method', 'GET'),
                        'score': score,
                        'description': endpoint.get('description', '')[:100] + '...'
                    })
                    
            except Exception as e:
                print(f"❌ Error scoring {endpoint.get('name', 'Unknown')}: {e}")
        
        # Sort by score (highest first)
        scored_endpoints.sort(key=lambda x: x['score'], reverse=True)
        
        print("🏆 TOP 5 SCORING ENDPOINTS:")
        print("-" * 80)
        for i, endpoint in enumerate(scored_endpoints[:5], 1):
            winner_indicator = "🥇" if i == 1 else "🥈" if i == 2 else "🥉" if i == 3 else f"{i}."
            print(f"{winner_indicator} {endpoint['name']} (Score: {endpoint['score']})")
            print(f"   URL: {endpoint['url']}")
            print(f"   Method: {endpoint['method']}")
            print(f"   Description: {endpoint['description']}")
            print()
        
        # Test the find_entity_endpoint function
        print("🔍 Testing find_entity_endpoint with corrected data:")
        print("-" * 50)
        
        from ai_assistant.sub_agents.task_executor_agent.utils import find_entity_endpoint
        
        result = find_entity_endpoint(
            entity_name=entity_name,
            intent=intent,
            extracted_params=json.dumps(params)
        )
        
        data = json.loads(result)
        print("Selected Endpoint:")
        print(f"  Name: {data.get('endpoint', {}).get('name', 'Unknown')}")
        print(f"  Score: {data.get('score', 'N/A')}")
        print(f"  URL: {data.get('full_url', 'N/A')}")
        print(f"  Method: {data.get('method', 'N/A')}")
        
        # Test the combined find_and_invoke_api_tool
        print("\n🚀 Testing combined find_and_invoke_api_tool:")
        print("-" * 50)
        
        from ai_assistant.sub_agents.task_executor_agent.utils import find_and_invoke_api_tool
        
        try:
            combined_result = find_and_invoke_api_tool(
                entity_name=entity_name,
                intent=intent,
                params=json.dumps(params)
            )
            
            combined_data = json.loads(combined_result)
            print("Combined Tool Result:")
            print(f"  Selected: {combined_data.get('endpoint_details', {}).get('selected_endpoint', 'Unknown')}")
            print(f"  Score: {combined_data.get('endpoint_details', {}).get('score', 'N/A')}")
            print(f"  URL: {combined_data.get('endpoint_details', {}).get('endpoint_url', 'N/A')}")
            print(f"  Fallbacks: {combined_data.get('endpoint_details', {}).get('fallback_available', False)}")
            
        except Exception as e:
            print(f"❌ Combined tool error: {e}")
        
        print("\n" + "=" * 80)
        print("🎯 CORRECTED ENDPOINT SELECTION TEST COMPLETED")
        print("=" * 80)
        
        # Expected Results Summary
        expected_winner = "SearchPartners"
        actual_winner = scored_endpoints[0]['name'] if scored_endpoints else "None"
        
        if actual_winner == expected_winner:
            print(f"✅ SUCCESS: Correctly selected {actual_winner}")
        else:
            print(f"❌ ISSUE: Expected {expected_winner}, got {actual_winner}")
            
        return scored_endpoints
        
    except Exception as e:
        print(f"❌ Error during test: {e}")
        import traceback
        traceback.print_exc()
        return []

if __name__ == "__main__":
    test_corrected_endpoint_selection()
