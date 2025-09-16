#!/usr/bin/env python3
"""
Comprehensive test cases for the PERFECT SCORING ALGORITHM
Tests various scenarios and validates endpoint selection logic
"""

import sys
import os
import json

# Add the project root to Python path
sys.path.append(os.path.join(os.path.dirname(__file__), 'UNOPS.PAO.AIService'))

def test_scoring_scenarios():
    """Test comprehensive scoring scenarios"""
    
    print("🧪 PERFECT SCORING ALGORITHM - COMPREHENSIVE TESTS")
    print("=" * 80)
    
    try:
        # Import the scoring function
        from ai_assistant.sub_agents.task_executor_agent.utils import score_endpoint_for_intent_standalone
        
        # Load partner endpoints configuration
        config_path = "UNOPS.PAO.AIService/config/tools/endpoints/partner-tools.json"
        with open(config_path, 'r') as f:
            config = json.load(f)
        
        endpoints = config.get('endpoints', [])
        
        # TEST SCENARIOS
        test_cases = [
            {
                "name": "Simple Partner Name Search",
                "entity": "Partner",
                "intent": "search", 
                "params": {"query": "The Sunrise Project Australia Limited"},
                "expected_winner": "SearchForPartner",
                "description": "Should select simple text search for partner name"
            },
            {
                "name": "Complex Partner Criteria Search",
                "entity": "Partner", 
                "intent": "search",
                "params": {"status": "Active", "category": "NGO", "region": "Asia"},
                "expected_winner": "AdvancedSearchPartners",
                "description": "Should select advanced search for complex criteria"
            },
            {
                "name": "Partner List Request",
                "entity": "Partner",
                "intent": "list",
                "params": {},
                "expected_winner": "ListAllPartners", 
                "description": "Should select list-all for general listing"
            },
            {
                "name": "Partner Creation Request",
                "entity": "Partner",
                "intent": "create",
                "params": {"name": "New Partner Org", "type": "NGO"},
                "expected_winner": "Create",
                "description": "Should select create endpoint for new partners"
            }
        ]
        
        for i, test_case in enumerate(test_cases, 1):
            print(f"\n📋 TEST CASE {i}: {test_case['name']}")
            print("-" * 60)
            print(f"Description: {test_case['description']}")
            print(f"Entity: {test_case['entity']}")
            print(f"Intent: {test_case['intent']}")
            print(f"Params: {test_case['params']}")
            print(f"Expected Winner: {test_case['expected_winner']}")
            print()
            
            # Score all endpoints for this test case
            scored_endpoints = []
            
            for endpoint in endpoints:
                try:
                    score = score_endpoint_for_intent_standalone(
                        endpoint=endpoint,
                        entity_name=test_case['entity'],
                        intent=test_case['intent'],
                        extracted_params=test_case['params']
                    )
                    
                    if score > 0:  # Only include valid endpoints
                        scored_endpoints.append({
                            'name': endpoint.get('name', 'Unknown'),
                            'url': endpoint.get('url', ''),
                            'method': endpoint.get('method', 'GET'),
                            'score': score
                        })
                        
                except Exception as e:
                    print(f"❌ Error scoring {endpoint.get('name', 'Unknown')}: {e}")
            
            # Sort by score (highest first)
            scored_endpoints.sort(key=lambda x: x['score'], reverse=True)
            
            print(f"\n🏆 TOP 5 RESULTS FOR TEST CASE {i}:")
            print("-" * 40)
            
            for j, endpoint in enumerate(scored_endpoints[:5], 1):
                winner_indicator = "🥇" if j == 1 else "🥈" if j == 2 else "🥉" if j == 3 else f"{j}."
                print(f"{winner_indicator} {endpoint['name']} (Score: {endpoint['score']})")
                print(f"   URL: {endpoint['url']}")
                print(f"   Method: {endpoint['method']}")
                
                # Check if this matches expected winner
                if j == 1:
                    actual_winner = endpoint['name']
                    expected_winner = test_case['expected_winner']
                    
                    if actual_winner == expected_winner:
                        print(f"   ✅ CORRECT: Selected {actual_winner} as expected!")
                    else:
                        print(f"   ❌ INCORRECT: Selected {actual_winner}, expected {expected_winner}")
                print()
            
            # Show scoring breakdown for winner
            if scored_endpoints:
                winner = scored_endpoints[0]
                print(f"🔍 WINNER ANALYSIS: {winner['name']}")
                print(f"Final Score: {winner['score']}")
                print()
        
        print("\n" + "=" * 80)
        print("🎯 PERFECT SCORING ALGORITHM TEST COMPLETED")
        print("=" * 80)
        
    except Exception as e:
        print(f"❌ Error during comprehensive testing: {e}")
        import traceback
        traceback.print_exc()

def test_fallback_ranking():
    """Test fallback endpoint ranking"""
    print("\n🔄 FALLBACK RANKING TEST")
    print("-" * 40)
    
    try:
        from ai_assistant.sub_agents.task_executor_agent.utils import find_entity_endpoint
        
        # Test a search query that should show fallback options
        result = find_entity_endpoint(
            entity_name="Partner",
            intent="search",
            extracted_params='{"query": "Complex Organization Name"}'
        )
        
        data = json.loads(result)
        
        print("Primary Endpoint:")
        primary = data.get('endpoint', {})
        print(f"  Name: {primary.get('name', 'Unknown')}")
        print(f"  Score: {data.get('score', 'N/A')}")
        print(f"  URL: {data.get('full_url', 'N/A')}")
        
        print("\nFallback Options:")
        fallbacks = data.get('retry_info', {}).get('fallback_endpoints', [])
        for i, fallback in enumerate(fallbacks[:3], 1):
            print(f"  {i}. {fallback.get('name', 'Unknown')} (Score: {fallback.get('score', 'N/A')})")
            print(f"     URL: {fallback.get('full_url', 'N/A')}")
        
        print(f"\nTotal Fallbacks Available: {len(fallbacks)}")
        
    except Exception as e:
        print(f"❌ Fallback test error: {e}")

if __name__ == "__main__":
    test_scoring_scenarios()
    test_fallback_ranking()
