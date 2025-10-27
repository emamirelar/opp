#!/usr/bin/env python3
"""
Test endpoint selection for the query: "Get me details of partner The Sunrise Project Australia Limited and its related engagements"
"""

import sys
import os
import json

# Add the project root to Python path
sys.path.append(os.path.join(os.path.dirname(__file__), 'UNOPS.PAO.AIService'))

def test_endpoint_selection():
    """Test which endpoint gets selected for partner search query"""
    
    print("🧪 Testing Endpoint Selection")
    print("=" * 60)
    print("Query: 'Get me details of partner The Sunrise Project Australia Limited and its related engagements'")
    print("=" * 60)
    
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
        
        # Score all endpoints
        scored_endpoints = []
        
        for endpoint in endpoints:
            try:
                score = score_endpoint_for_intent_standalone(
                    endpoint=endpoint,
                    entity_name=entity_name,
                    intent=intent,
                    extracted_params=params
                )
                
                scored_endpoints.append({
                    'name': endpoint.get('name', 'Unknown'),
                    'url': endpoint.get('url', ''),
                    'method': endpoint.get('method', 'GET'),
                    'description': endpoint.get('description', '')[:100] + '...',
                    'score': score
                })
                
            except Exception as e:
                print(f"❌ Error scoring endpoint {endpoint.get('name', 'Unknown')}: {e}")
        
        # Sort by score (highest first)
        scored_endpoints.sort(key=lambda x: x['score'], reverse=True)
        
        print("🏆 TOP 5 SCORING ENDPOINTS:")
        print("-" * 80)
        for i, endpoint in enumerate(scored_endpoints[:5], 1):
            print(f"{i}. {endpoint['name']} (Score: {endpoint['score']})")
            print(f"   URL: {endpoint['url']}")
            print(f"   Method: {endpoint['method']}")
            print(f"   Description: {endpoint['description']}")
            print()
        
        # Show the winner
        if scored_endpoints:
            winner = scored_endpoints[0]
            print("🎯 SELECTED ENDPOINT:")
            print("=" * 40)
            print(f"Name: {winner['name']}")
            print(f"Score: {winner['score']}")
            print(f"URL: {winner['url']}")
            print(f"Method: {winner['method']}")
            print()
            
        # Test the full find_entity_endpoint function
        print("🔍 Testing find_entity_endpoint function:")
        print("-" * 40)
        
        from ai_assistant.sub_agents.task_executor_agent.utils import find_entity_endpoint
        
        result = find_entity_endpoint(
            entity_name=entity_name,
            intent=intent,
            extracted_params=json.dumps(params)
        )
        
        print("Result:")
        print(json.dumps(json.loads(result), indent=2))
        
        # Test the new combined find_and_invoke_api_tool
        print("\n🚀 Testing find_and_invoke_api_tool (combined function):")
        print("-" * 60)
        
        from ai_assistant.sub_agents.task_executor_agent.utils import find_and_invoke_api_tool
        
        combined_result = find_and_invoke_api_tool(
            entity_name=entity_name,
            intent=intent,
            params=json.dumps(params)
        )
        
        print("Combined Result:")
        print(json.dumps(json.loads(combined_result), indent=2))
            
    else:
        print("❌ No endpoints found!")
            
    except Exception as e:
        print(f"❌ Error during test: {e}")
        import traceback
        traceback.print_exc()

if __name__ == "__main__":
    test_endpoint_selection()
