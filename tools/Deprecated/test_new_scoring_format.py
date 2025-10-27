#!/usr/bin/env python3
"""
Test the find_entity_endpoint function with the new optimized JSON format
"""

import json
import sys
import os
from typing import Dict, Any, Optional

# Copy the scoring function locally to avoid import issues
def score_endpoint_for_intent_standalone(endpoint: dict, intent: str, entity_name: str, extracted_params: Optional[dict] = None) -> int:
    """
    PERFECT SCORING ALGORITHM
    
    Intelligently scores endpoints based on:
    1. HTTP method compatibility (MANDATORY)
    2. Search capability analysis (for search intents)
    3. Intent-endpoint alignment
    4. Parameter compatibility
    5. Fallback ranking
    """
    
    if not endpoint:
        return -1000
    
    # Extract endpoint metadata
    endpoint_name = endpoint.get('name', '')
    method = endpoint.get('method', 'GET').upper()
    url = endpoint.get('url', '')
    description = endpoint.get('description', '').lower()
    
    # NEW: Extract optimized JSON metadata
    endpoint_intents = endpoint.get('intent', [])
    search_capability = endpoint.get('searchCapability', {})
    scoring_hints = endpoint.get('scoring', {})
    
    search_type = search_capability.get('type', 'none')  # simple, advanced, none
    search_fields = search_capability.get('searchFields', [])
    search_bonus_hint = scoring_hints.get('searchBonus', 0)
    intent_match_hint = scoring_hints.get('intentMatch', 0)
    complexity = scoring_hints.get('complexity', 'medium')
    
    final_score = 0
    
    # 1. HTTP METHOD COMPATIBILITY (MANDATORY CHECK)
    method_compatibility = {
        'search': ['GET'],
        'find': ['GET'], 
        'lookup': ['GET'],
        'query': ['GET'],
        'list': ['GET'],
        'get': ['GET'],
        'retrieve': ['GET'],
        'show': ['GET'],
        'create': ['POST'],
        'add': ['POST'],
        'new': ['POST'],
        'register': ['POST'],
        'update': ['PUT', 'PATCH'],
        'modify': ['PUT', 'PATCH'],
        'edit': ['PUT', 'PATCH'],
        'change': ['PUT', 'PATCH'],
        'delete': ['DELETE'],
        'remove': ['DELETE'],
        'destroy': ['DELETE']
    }
    
    required_methods = method_compatibility.get(intent.lower(), ['GET'])
    if method not in required_methods:
        return -1000  # Incompatible method
    
    # 2. INTENT MATCHING (using new intent metadata)
    if intent.lower() in [i.lower() for i in endpoint_intents]:
        final_score += intent_match_hint or 100  # Use hint or default
    else:
        final_score += 10  # Small bonus for method compatibility
    
    # 3. SEARCH CAPABILITY ANALYSIS (for search intents)
    if intent.lower() in ['search', 'find', 'lookup', 'query']:
        if search_type == 'simple':
            final_score += search_bonus_hint or 100  # Use hint or default
        elif search_type == 'advanced':
            final_score += search_bonus_hint or 80   # Use hint or default
        elif search_type == 'none':
            # Check if it's a list endpoint that can serve as fallback
            if intent.lower() in [i.lower() for i in endpoint_intents] and any(i.lower() in ['list', 'get', 'show'] for i in endpoint_intents):
                final_score += search_bonus_hint or 20  # Use hint or default
        
        # Bonus for having relevant search fields
        if extracted_params and search_fields:
            param_keys = set(extracted_params.keys())
            search_field_set = set(f.lower() for f in search_fields)
            
            # Check for parameter alignment
            if 'query' in param_keys and any('name' in f.lower() or 'description' in f.lower() for f in search_fields):
                final_score += 20
            
            # Check for advanced search parameters
            if any(key in param_keys for key in ['status', 'type', 'category', 'criteria']):
                if search_type == 'advanced':
                    final_score += 30
    
    # 4. PARAMETER COMPATIBILITY
    if extracted_params:
        # ID parameter matching - MAJOR boost for exact ID endpoints
        if 'id' in extracted_params:
            if '{id}' in url and url.count('/') <= 3:  # Simple ID endpoint like /api/partner/{id}
                final_score += 150  # Much higher bonus for direct ID access
            elif '{id}' in url:
                final_score += 50  # Lower bonus for complex ID endpoints
        
        # Search text parameter matching  
        if 'query' in extracted_params or 'search' in extracted_params:
            if 'searchText' in url or 'query' in url:
                final_score += 30
        
        # Semantic search parameter detection
        if extracted_params.get('semantic', False) or any(term in str(extracted_params).lower() for term in ['similar', 'semantic', 'ai']):
            if 'semantic' in endpoint_intents or 'similar' in endpoint_intents or 'deep' in endpoint_name.lower():
                final_score += 100  # Big bonus for semantic search requests
    
    # 5. URL STRUCTURE ANALYSIS
    # Parameterized URLs typically indicate more specific functionality
    param_count = url.count('{')
    if param_count > 0:
        final_score += param_count * 5
    
    # Deeper URL paths often indicate more specific operations
    path_depth = url.count('/') - 2  # Subtract 2 for /api/entity
    if path_depth > 0:
        final_score += path_depth * 10
    
    # 6. ENDPOINT NAME ANALYSIS
    name_lower = endpoint_name.lower()
    
    # Exact intent matching in name
    if intent.lower() in name_lower:
        final_score += 25
    
    # Entity matching in name
    if entity_name.lower() in name_lower:
        final_score += 15
    
    # 7. COMPLEXITY PENALTIES/BONUSES
    complexity_modifiers = {
        'simple': 10,
        'medium': 0,
        'complex': -5
    }
    final_score += complexity_modifiers.get(complexity, 0)
    
    # 8. SPECIAL CASES AND PENALTIES
    
    # Heavy penalty for "list-all" endpoints when specific search is requested
    if (intent.lower() in ['search', 'find'] and 
        extracted_params and 
        len(extracted_params) > 0 and
        ('listall' in name_lower or 'getall' in name_lower)):
        final_score -= 100
    
    return max(final_score, -1000)  # Ensure minimum score

def load_partner_tools():
    """Load the new partner-tools.json with optimized format"""
    with open('UNOPS.PAO.AIService/config/tools/endpoints/partner-tools.json', 'r', encoding='utf-8') as f:
        return json.load(f)

def test_scoring_scenarios():
    """Test various scoring scenarios with the new JSON format"""
    
    partner_tools = load_partner_tools()
    endpoints = partner_tools['endpoints']
    
    print("🧪 TESTING NEW JSON FORMAT SCORING")
    print("=" * 70)
    print(f"📊 Loaded {len(endpoints)} endpoints from partner-tools.json")
    print()
    
    # Test scenarios
    test_cases = [
        {
            "query": "Find partners named UNICEF",
            "intent": "search", 
            "entity": "Partner",
            "params": {"query": "UNICEF"},
            "expected_winner": "SearchPartners"
        },
        {
            "query": "Search for government partners with advanced criteria",
            "intent": "search",
            "entity": "Partner", 
            "params": {"status": "Active", "type": "Government"},
            "expected_winner": "AdvancedSearchPartners"
        },
        {
            "query": "Show me all partners",
            "intent": "list",
            "entity": "Partner",
            "params": {},
            "expected_winner": "ListAllPartners"
        },
        {
            "query": "Get details of partner ID 123",
            "intent": "get",
            "entity": "Partner",
            "params": {"id": "123"},
            "expected_winner": "Get"
        },
        {
            "query": "Find partners similar to Red Cross using AI",
            "intent": "search",
            "entity": "Partner",
            "params": {"query": "Red Cross", "semantic": True},
            "expected_winner": "DeepSearch"
        }
    ]
    
    for i, test_case in enumerate(test_cases, 1):
        print(f"🎯 TEST CASE {i}: {test_case['query']}")
        print(f"   Intent: {test_case['intent']}")
        print(f"   Expected winner: {test_case['expected_winner']}")
        print()
        
        # Score all endpoints
        scored_endpoints = []
        for endpoint in endpoints:
            score = score_endpoint_for_intent_standalone(
                endpoint=endpoint,
                intent=test_case['intent'],
                entity_name=test_case['entity'],
                extracted_params=test_case['params']
            )
            scored_endpoints.append({
                'name': endpoint['name'],
                'score': score,
                'method': endpoint['method'],
                'intent': endpoint.get('intent', []),
                'searchCapability': endpoint.get('searchCapability', {}),
                'scoring': endpoint.get('scoring', {}),
                'url': endpoint['url']
            })
        
        # Sort by score (highest first)
        scored_endpoints.sort(key=lambda x: x['score'], reverse=True)
        
        # Show top 5 results
        print("   📈 TOP 5 SCORING ENDPOINTS:")
        for j, ep in enumerate(scored_endpoints[:5], 1):
            is_winner = ep['name'] == test_case['expected_winner']
            marker = "🏆" if is_winner else f"{j}. "
            
            print(f"   {marker} {ep['name']} (Score: {ep['score']})")
            print(f"      Method: {ep['method']}")
            print(f"      Intent: {ep['intent']}")
            print(f"      Search Type: {ep['searchCapability'].get('type', 'none')}")
            print(f"      Search Bonus: {ep['scoring'].get('searchBonus', 0)}")
            print(f"      Intent Match: {ep['scoring'].get('intentMatch', 0)}")
            print(f"      URL: {ep['url']}")
            print()
        
        # Check if expected winner is actually the winner
        actual_winner = scored_endpoints[0]['name']
        if actual_winner == test_case['expected_winner']:
            print(f"   ✅ SUCCESS: {actual_winner} correctly selected!")
        else:
            print(f"   ❌ ISSUE: Expected {test_case['expected_winner']}, got {actual_winner}")
        
        print("-" * 70)
        print()

def analyze_scoring_components():
    """Analyze how the new JSON scoring components work"""
    
    partner_tools = load_partner_tools()
    endpoints = partner_tools['endpoints']
    
    print("🔍 ANALYZING NEW SCORING COMPONENTS")
    print("=" * 70)
    
    # Group endpoints by search capability type
    search_types = {}
    for endpoint in endpoints:
        search_cap = endpoint.get('searchCapability', {})
        search_type = search_cap.get('type', 'none')
        
        if search_type not in search_types:
            search_types[search_type] = []
        search_types[search_type].append(endpoint)
    
    print("📊 ENDPOINTS BY SEARCH CAPABILITY TYPE:")
    for search_type, eps in search_types.items():
        print(f"   {search_type.upper()}: {len(eps)} endpoints")
        for ep in eps:
            scoring = ep.get('scoring', {})
            print(f"     - {ep['name']}: searchBonus={scoring.get('searchBonus', 0)}, intentMatch={scoring.get('intentMatch', 0)}")
    print()
    
    # Analyze intent distribution
    all_intents = set()
    intent_endpoints = {}
    
    for endpoint in endpoints:
        intents = endpoint.get('intent', [])
        for intent in intents:
            all_intents.add(intent)
            if intent not in intent_endpoints:
                intent_endpoints[intent] = []
            intent_endpoints[intent].append(endpoint['name'])
    
    print("🎯 ENDPOINTS BY INTENT:")
    for intent in sorted(all_intents):
        endpoints_with_intent = intent_endpoints[intent]
        print(f"   {intent.upper()}: {len(endpoints_with_intent)} endpoints")
        print(f"     {', '.join(endpoints_with_intent)}")
    print()

if __name__ == "__main__":
    try:
        print("🚀 TESTING NEW OPTIMIZED JSON FORMAT")
        print("=" * 70)
        print()
        
        # First analyze the structure
        analyze_scoring_components()
        
        # Then test scoring scenarios
        test_scoring_scenarios()
        
        print("✅ Testing completed!")
        
    except Exception as e:
        print(f"❌ Error during testing: {e}")
        import traceback
        traceback.print_exc()
