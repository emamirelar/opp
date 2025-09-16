#!/usr/bin/env python3
"""
Standalone Endpoint Scoring Test
===============================

This test focuses purely on the scoring logic without framework dependencies.
Perfect for testing endpoint selection locally.

Usage:
1. Modify the TEST_QUERIES list below
2. Run: python test_scoring_standalone.py
3. See which endpoints get the highest scores
"""

import json
from typing import Optional, Dict, Any

# =============================================================================
# COPY OF THE SCORING FUNCTION (from utils.py)
# =============================================================================

def score_endpoint_for_intent_standalone(endpoint: dict, intent: str, entity_name: str, extracted_params: Optional[dict] = None) -> int:
    """
    PERFECT SCORING ALGORITHM - Standalone version
    
    Intelligently scores endpoints based on:
    1. HTTP method compatibility (MANDATORY)
    2. Search capability analysis (for search intents)
    3. Intent-endpoint alignment
    4. Parameter compatibility
    5. Fallback ranking
    """
    
    # Extract endpoint metadata
    endpoint_name = endpoint.get('name', '')
    description = endpoint.get('description', '')
    url = endpoint.get('url', '')
    method = endpoint.get('method', 'GET').upper()
    when_to_use = endpoint.get('when_to_use', '')
    example_uses = endpoint.get('example_uses', [])
    parameters = endpoint.get('parameters', {})
    
    # Get new JSON structure fields if available
    endpoint_intents = endpoint.get('intent', [])
    search_capability = endpoint.get('searchCapability', {})
    scoring_hints = endpoint.get('scoring', {})
    
    # Normalize for analysis
    name_lower = endpoint_name.lower()
    desc_lower = description.lower()
    url_lower = url.lower()
    when_lower = when_to_use.lower()
    
    print(f"🎯 [SCORING] Evaluating {endpoint_name} for {intent} on {entity_name}")
    
    # STEP 1: HTTP METHOD COMPATIBILITY (MANDATORY)
    intent_method_map = {
        'search': 'GET', 'find': 'GET', 'get': 'GET', 'list': 'GET', 'retrieve': 'GET', 'show': 'GET', 'browse': 'GET',
        'create': 'POST', 'add': 'POST', 'new': 'POST', 'insert': 'POST',
        'update': 'PUT', 'modify': 'PUT', 'edit': 'PUT', 'change': 'PUT', 'patch': 'PATCH',
        'delete': 'DELETE', 'remove': 'DELETE', 'destroy': 'DELETE'
    }
    
    required_method = intent_method_map.get(intent.lower(), 'GET')
    if method != required_method:
        print(f"❌ [SCORING] {endpoint_name}: Wrong method {method} (need {required_method}) = 0")
        return 0
    
    # STEP 2: BASE SCORE from new JSON structure
    score = 0
    
    # Use scoring hints if available
    if scoring_hints:
        if intent.lower() in ['search', 'find'] and 'searchBonus' in scoring_hints:
            score += scoring_hints.get('searchBonus', 0)
        if 'intentMatch' in scoring_hints:
            # Check if intent matches endpoint intents
            if intent.lower() in [i.lower() for i in endpoint_intents]:
                score += scoring_hints.get('intentMatch', 0)
    
    # STEP 3: INTENT MATCHING (Enhanced for new structure)
    intent_lower = intent.lower()
    
    # Check against endpoint's intent array
    if endpoint_intents:
        intent_scores = []
        for ep_intent in endpoint_intents:
            if intent_lower == ep_intent.lower():
                intent_scores.append(100)  # Perfect match
            elif intent_lower in ep_intent.lower() or ep_intent.lower() in intent_lower:
                intent_scores.append(80)   # Partial match
        
        if intent_scores:
            score += max(intent_scores)
            print(f"✅ [SCORING] {endpoint_name}: Intent array match (+{max(intent_scores)})")
    else:
        # Fallback to name-based matching
        if intent_lower == name_lower:
            score += 60
        elif intent_lower in name_lower:
            score += 40
        elif name_lower.startswith(intent_lower):
            score += 45
    
    # STEP 4: SEARCH CAPABILITY ANALYSIS (Enhanced)
    has_search_params = extracted_params and any(key in extracted_params for key in ['query', 'name', 'searchText', 'search'])
    
    if intent_lower in ['search', 'find'] and has_search_params:
        search_value = extracted_params.get('query') or extracted_params.get('name') or extracted_params.get('searchText') or extracted_params.get('search')
        print(f"🔍 [SCORING] Search query detected: '{search_value}'")
        
        # Use searchCapability from new JSON structure
        if search_capability:
            capability_type = search_capability.get('type', 'none')
            search_fields = search_capability.get('searchFields', [])
            supports_pagination = search_capability.get('supportsPagination', False)
            
            if capability_type == 'simple':
                score += 100  # Perfect for simple searches
                print(f"✅ [SCORING] {endpoint_name}: Simple search capability (+100)")
            elif capability_type == 'advanced':
                score += 80   # Good for complex searches
                print(f"✅ [SCORING] {endpoint_name}: Advanced search capability (+80)")
            elif capability_type == 'none':
                score -= 50   # Not ideal for searches
                print(f"⚠️ [SCORING] {endpoint_name}: No search capability (-50)")
            
            if search_fields:
                score += len(search_fields) * 5  # Bonus for more searchable fields
                print(f"✅ [SCORING] {endpoint_name}: {len(search_fields)} search fields (+{len(search_fields) * 5})")
        
        # Fallback URL analysis
        if '{searchtext}' in url_lower or '{query}' in url_lower:
            score += 80
            print(f"✅ [SCORING] {endpoint_name}: Search parameter in URL (+80)")
        elif 'search' in url_lower:
            score += 60
            print(f"✅ [SCORING] {endpoint_name}: Search in URL path (+60)")
    
    # STEP 5: ID PARAMETER MATCHING (Enhanced)
    if extracted_params and 'id' in extracted_params:
        if '{id}' in url and url.count('/') <= 3:  # Simple ID endpoint like /api/partner/{id}
            score += 150  # Much higher bonus for direct ID access
            print(f"🎯 [SCORING] {endpoint_name}: Direct ID endpoint (+150)")
        elif '{id}' in url:
            score += 50   # Lower bonus for complex ID endpoints
            print(f"🎯 [SCORING] {endpoint_name}: Complex ID endpoint (+50)")
    
    # STEP 6: SEMANTIC SEARCH DETECTION
    if extracted_params and (extracted_params.get('semantic', False) or 
                           any(term in str(extracted_params).lower() for term in ['similar', 'semantic', 'ai'])):
        if any(term in endpoint_name.lower() for term in ['deep', 'semantic', 'ai']):
            score += 100  # Big bonus for semantic search requests
            print(f"🧠 [SCORING] {endpoint_name}: Semantic search match (+100)")
    
    # STEP 7: LIST-ALL PENALTY (for search queries)
    if has_search_params and any(indicator in name_lower for indicator in ['listall', 'getall', 'all']):
        if 'search' not in name_lower and 'find' not in name_lower:
            score -= 100  # Heavy penalty for list-all when searching
            print(f"❌ [SCORING] {endpoint_name}: List-all penalty for search (-100)")
    
    # STEP 8: ENTITY MATCHING
    entity_lower = entity_name.lower()
    entity_bonus = 0
    
    if entity_lower in name_lower:
        entity_bonus += 20
    if entity_lower in url_lower:
        entity_bonus += 10
    if entity_lower in desc_lower:
        entity_bonus += 5
        
    score += entity_bonus
    if entity_bonus > 0:
        print(f"🏢 [SCORING] {endpoint_name}: Entity matching (+{entity_bonus})")
    
    final_score = max(0, score)  # Ensure non-negative
    print(f"🏆 [SCORING] {endpoint_name}: FINAL SCORE = {final_score}")
    
    return final_score

# =============================================================================
# SAMPLE PARTNER ENDPOINTS (from partner-tools.json)
# =============================================================================

SAMPLE_ENDPOINTS = [
    {
        "name": "SearchPartners",
        "url": "/api/partner/search?searchText={searchText}&request.pageIndex={request.pageIndex}&request.pageSize={request.pageSize}&request.orderBy={request.orderBy}&request.ascending={request.ascending}",
        "method": "GET",
        "intent": ["search", "find", "lookup", "query"],
        "description": "Performs simple text search across multiple partner fields (name, description, etc.).",
        "searchCapability": {
            "type": "simple",
            "searchFields": ["name", "partnerShortDescription", "partnerLongDescription"],
            "supportsPagination": True,
            "supportsSort": True
        },
        "parameters": {},
        "when_to_use": "Use this for simple name, description, or basic field searches. NOT for complex criteria or relationship searches.",
        "scoring": {
            "searchBonus": 100,
            "intentMatch": 95,
            "complexity": "simple"
        }
    },
    {
        "name": "AdvancedSearchPartners", 
        "url": "/api/partner/advanced-search?searchCriteria={searchCriteria}&searchText={searchText}&request.pageIndex={request.pageIndex}&request.pageSize={request.pageSize}&request.orderBy={request.orderBy}&request.ascending={request.ascending}",
        "method": "GET",
        "intent": ["search", "find", "lookup", "query"],
        "description": "Performs advanced search with structured criteria including status, dates, relationships, and complex filters.",
        "searchCapability": {
            "type": "advanced",
            "searchFields": ["id", "name", "status", "partnerShortDescription", "partnerLongDescription"],
            "supportsPagination": True,
            "supportsSort": True
        },
        "parameters": {},
        "when_to_use": "Use this for searches involving partner status, dates, types, complex criteria, or multiple field combinations.",
        "scoring": {
            "searchBonus": 80,
            "intentMatch": 95,
            "complexity": "medium"
        }
    },
    {
        "name": "Get",
        "url": "/api/partner/{id}",
        "method": "GET",
        "intent": ["get", "retrieve", "show", "details"],
        "description": "Retrieves a specific partner by ID with complete details including documents, contacts, and office information.",
        "searchCapability": {
            "type": "none",
            "searchFields": [],
            "supportsPagination": False,
            "supportsSort": False
        },
        "parameters": {},
        "when_to_use": "Use this when the user asks for specific partner details by ID or when you need complete partner information.",
        "scoring": {
            "searchBonus": 10,
            "intentMatch": 90,
            "complexity": "simple"
        }
    },
    {
        "name": "ListAllPartners",
        "url": "/api/partner?pageIndex={pageIndex}&pageSize={pageSize}&orderBy={orderBy}&ascending={ascending}",
        "method": "GET",
        "intent": ["list", "get", "browse", "show"],
        "description": "Retrieves all partners with basic pagination and ordering (no search criteria).",
        "searchCapability": {
            "type": "none",
            "searchFields": [],
            "supportsPagination": True,
            "supportsSort": True
        },
        "parameters": {},
        "when_to_use": "Use this when the user wants to see ALL partners without any search criteria or when asking for a general partner list.",
        "scoring": {
            "searchBonus": 20,
            "intentMatch": 90,
            "complexity": "simple"
        }
    },
    {
        "name": "DeepSearch",
        "url": "/api/partner/deepSearch?query={query}&threshold={threshold}&limit={limit}",
        "method": "GET",
        "intent": ["search", "find", "similar", "semantic"],
        "description": "Performs semantic search on partners using AI embeddings to find similar partners based on natural language queries.",
        "searchCapability": {
            "type": "simple",
            "searchFields": ["name", "partnerShortDescription", "partnerLongDescription"],
            "supportsPagination": False,
            "supportsSort": False
        },
        "parameters": {},
        "when_to_use": "Use this when the user wants to find partners using natural language queries or semantic similarity.",
        "scoring": {
            "searchBonus": 100,
            "intentMatch": 95,
            "complexity": "medium"
        }
    }
]

# =============================================================================
# TEST QUERIES - MODIFY THESE TO TEST YOUR SCENARIOS
# =============================================================================

TEST_QUERIES = [
    {
        "description": "Search for UNICEF",
        "entity": "Partner",
        "intent": "search",
        "params": {"query": "UNICEF"}
    },
    {
        "description": "Get partner by ID 123",
        "entity": "Partner",
        "intent": "get", 
        "params": {"id": "123"}
    },
    {
        "description": "Find similar partners using AI",
        "entity": "Partner",
        "intent": "search",
        "params": {"query": "organizations similar to Red Cross", "semantic": True}
    },
    {
        "description": "List all partners",
        "entity": "Partner",
        "intent": "list",
        "params": {}
    },
    {
        "description": "Show partner details",
        "entity": "Partner",
        "intent": "show",
        "params": {"id": "456"}
    }
]

# =============================================================================
# TEST RUNNER
# =============================================================================

def test_endpoint_scoring():
    """Test endpoint scoring for various queries"""
    print("🔍 STANDALONE ENDPOINT SCORING TEST")
    print("=" * 80)
    print("This test shows which endpoints get the highest scores for different queries.")
    print("No framework dependencies - pure scoring logic testing.\n")
    
    for i, test_case in enumerate(TEST_QUERIES, 1):
        print(f"🧪 TEST {i}: {test_case['description']}")
        print("=" * 60)
        
        entity = test_case['entity']
        intent = test_case['intent']
        params = test_case['params']
        
        print(f"📋 Entity: {entity}")
        print(f"📋 Intent: {intent}")
        print(f"📋 Params: {json.dumps(params)}")
        print()
        
        # Score all endpoints for this query
        scored_endpoints = []
        
        for endpoint in SAMPLE_ENDPOINTS:
            score = score_endpoint_for_intent_standalone(endpoint, intent, entity, params)
            scored_endpoints.append({
                'name': endpoint['name'],
                'score': score,
                'method': endpoint['method'],
                'url': endpoint['url']
            })
        
        # Sort by score (highest first)
        scored_endpoints.sort(key=lambda x: x['score'], reverse=True)
        
        print(f"\n🏆 RANKING:")
        print("-" * 40)
        
        for rank, endpoint in enumerate(scored_endpoints, 1):
            emoji = "🥇" if rank == 1 else "🥈" if rank == 2 else "🥉" if rank == 3 else "📍"
            print(f"{emoji} {rank}. {endpoint['name']}: {endpoint['score']} points")
            print(f"     Method: {endpoint['method']}")
            print(f"     URL: {endpoint['url'][:80]}{'...' if len(endpoint['url']) > 80 else ''}")
        
        winner = scored_endpoints[0]
        print(f"\n✅ WINNER: {winner['name']} with {winner['score']} points")
        print("\n" + "="*80 + "\n")

def quick_test(entity, intent, params_dict):
    """Quick test for a single query"""
    print(f"🔍 QUICK TEST: {entity} - {intent} - {json.dumps(params_dict)}")
    print("-" * 50)
    
    scored_endpoints = []
    for endpoint in SAMPLE_ENDPOINTS:
        score = score_endpoint_for_intent_standalone(endpoint, intent, entity, params_dict)
        if score > 0:  # Only show endpoints with positive scores
            scored_endpoints.append({'name': endpoint['name'], 'score': score})
    
    scored_endpoints.sort(key=lambda x: x['score'], reverse=True)
    
    print("\n🏆 TOP 3:")
    for i, ep in enumerate(scored_endpoints[:3], 1):
        print(f"{i}. {ep['name']}: {ep['score']} points")
    print()

if __name__ == "__main__":
    print("🧪 Starting Standalone Endpoint Scoring Tests...\n")
    
    # Run all tests
    test_endpoint_scoring()
    
    print("📝 QUICK TESTS:")
    print("="*50)
    quick_test("Partner", "search", {"query": "Red Cross"})
    quick_test("Partner", "get", {"id": "789"})
    quick_test("Partner", "search", {"query": "AI organizations", "semantic": True})
    
    print("✅ All tests completed!")
    print("\n🎯 How to use this test:")
    print("1. Modify TEST_QUERIES to test your scenarios")
    print("2. Add more endpoints to SAMPLE_ENDPOINTS if needed")
    print("3. Run again to see which endpoints score highest")
    print("4. Use quick_test() for rapid testing of individual queries")
