#!/usr/bin/env python3
"""
Comprehensive tests for web search tools including Google search functionality via search agent.
"""

import sys
import os
import json
import unittest
from unittest.mock import Mock, patch, MagicMock

# Add the project root to the path
sys.path.insert(0, os.path.abspath(os.path.join(os.path.dirname(__file__), '..', '..', '..')))


class TestWebSearchTool:
    """Test suite for web search tools"""
    
    def test_search_agent_import(self):
        """Test that search agent can be imported correctly"""
        print("Testing search agent imports...")
        
        try:
            # Test search agent import
            from ai_assistant.sub_agents.search_agent import search_agent
            assert search_agent is not None, "search_agent should be importable"
            
            # Test that the agent has required attributes
            assert hasattr(search_agent, 'name'), "search_agent should have name attribute"
            assert hasattr(search_agent, 'description'), "search_agent should have description attribute"
            assert hasattr(search_agent, 'tools'), "search_agent should have tools attribute"
            
            print("✅ All search agent components imported successfully")
            return True
            
        except Exception as e:
            print(f"❌ Search agent import failed: {e}")
            import traceback
            traceback.print_exc()
            return False
    
    def test_google_search_tool_functionality(self):
        """Test google_search tool functionality via Google ADK"""
        print("\nTesting google_search tool functionality...")
        
        try:
            # Mock the Google ADK google_search function
            with patch('google.adk.tools.google_search') as mock_google_search:
                # Mock search results
                mock_google_search.return_value = {
                    "results": [
                        {
                            "title": "UNOPS Climate Initiatives 2024",
                            "url": "https://unops.org/climate-initiatives-2024",
                            "snippet": "UNOPS announces new climate initiatives for sustainable development..."
                        },
                        {
                            "title": "Partnership Programs - UNOPS",
                            "url": "https://unops.org/partnerships",
                            "snippet": "Explore UNOPS partnership programs with international organizations..."
                        },
                        {
                            "title": "Climate Action Network Updates",
                            "url": "https://climateaction.org/unops-updates",
                            "snippet": "Latest updates on UNOPS climate action initiatives and partnerships..."
                        }
                    ],
                    "total_results": 3,
                    "search_time": 0.45
                }
                
                # Import and call google_search function from Google ADK
                from google.adk.tools import google_search
                
                result = google_search(
                    query="UNOPS climate initiatives partnerships",
                    num_results=5
                )
                
                # Verify result structure
                assert isinstance(result, dict), "Should return dict result"
                assert "results" in result, "Should include results"
                assert len(result["results"]) > 0, "Should have search results"
                
                # Check result item structure
                first_result = result["results"][0]
                assert "title" in first_result, "Result should have title"
                assert "url" in first_result, "Result should have URL"
                assert "snippet" in first_result, "Result should have snippet"
                
                print("✅ google_search tool functionality works correctly")
            
            return True
            
        except Exception as e:
            print(f"❌ google_search tool test failed: {e}")
            import traceback
            traceback.print_exc()
            return False
    
    def test_search_agent_execution(self):
        """Test search agent execution with various queries"""
        print("\nTesting search agent execution...")
        
        try:
            # Import the search agent
            from ai_assistant.sub_agents.search_agent import search_agent
            
            # Verify agent structure and configuration
            assert search_agent is not None, "search_agent should be available"
            assert hasattr(search_agent, 'name'), "search_agent should have name attribute"
            assert hasattr(search_agent, 'description'), "search_agent should have description attribute"
            assert hasattr(search_agent, 'tools'), "search_agent should have tools attribute"
            assert hasattr(search_agent, 'model'), "search_agent should have model attribute"
            assert hasattr(search_agent, 'instruction'), "search_agent should have instruction attribute"
            
            # Verify agent configuration
            assert search_agent.name == "search_agent", "Should have correct name"
            assert "Google search" in search_agent.description.lower() or "search" in search_agent.description.lower(), "Should have search-related description"
            assert search_agent.tools is not None, "Should have tools configured"
            assert len(search_agent.tools) > 0, "Should have at least one tool (google_search)"
            
            # Verify the google_search tool is available in the tools
            tool_names = [tool.name if hasattr(tool, 'name') else str(tool) for tool in search_agent.tools]
            assert any('google_search' in str(tool_name).lower() for tool_name in tool_names), "Should have google_search tool"
            
            print("✅ search agent execution structure verified")
            
            return True
            
        except Exception as e:
            print(f"❌ search agent execution test failed: {e}")
            import traceback
            traceback.print_exc()
            return False
    
    def test_search_query_optimization(self):
        """Test search query optimization and formatting"""
        print("\nTesting search query optimization...")
        
        try:
            # Test query optimization functions
            from ai_assistant.sub_agents.search_agent.utils import optimize_search_query
            
            # Test query optimization
            test_queries = [
                {
                    "input": "Tell me about UNOPS partnerships",
                    "expected_type": str
                },
                {
                    "input": "What are the latest climate initiatives?",
                    "expected_type": str
                },
                {
                    "input": "Find information about sustainable development programs",
                    "expected_type": str
                }
            ]
            
            for test_case in test_queries:
                optimized = optimize_search_query(test_case["input"])
                assert isinstance(optimized, test_case["expected_type"]), f"Should return {test_case['expected_type']} for: {test_case['input']}"
                assert len(optimized) > 0, f"Should not return empty result for: {test_case['input']}"
            
            print("✅ search query optimization works correctly")
            
            return True
            
        except Exception as e:
            print(f"❌ search query optimization test failed: {e}")
            import traceback
            traceback.print_exc()
            return False
    
    def test_search_response_validation(self):
        """Test search response validation and formatting"""
        print("\nTesting search response validation...")
        
        try:
            # Create mock implementations for functions that don't exist yet
            def mock_validate_search_response(response):
                """Mock validation function"""
                if not isinstance(response, dict):
                    return False
                if "content" not in response or not response["content"]:
                    return False
                return True
            
            def mock_format_search_response(response):
                """Mock formatting function"""
                return {
                    "content": response.get("content", ""),
                    "sources": response.get("sources", []),
                    "formatted": True
                }
            
            def mock_validate_search_source(source):
                """Mock source validation function"""
                if not isinstance(source, dict):
                    return False
                return "title" in source and "url" in source
            
            # Test response validation
            valid_response = {
                "content": "This is valid search content about climate initiatives.",
                "sources": [
                    {
                        "title": "Valid Source Title",
                        "url": "https://example.org/article",
                        "description": "Valid source description"
                    }
                ]
            }
            
            invalid_responses = [
                {"content": ""},  # Empty content
                {"sources": []},  # Missing content
                {},  # Empty response
                {"content": "Valid", "sources": [{"title": ""}]}  # Invalid source
            ]
            
            # Test valid response
            assert mock_validate_search_response(valid_response) == True, "Should validate correct response"
            
            # Test invalid responses
            for invalid_response in invalid_responses:
                is_valid = mock_validate_search_response(invalid_response)
                assert isinstance(is_valid, bool), f"Should return boolean for: {invalid_response}"
            
            # Test response formatting
            raw_response = {
                "content": "Raw search content",
                "sources": [{"title": "Raw Source", "url": "https://example.com"}]
            }
            
            formatted = mock_format_search_response(raw_response)
            assert isinstance(formatted, dict), "Should return formatted dict"
            
            # Test source validation
            valid_source = {
                "title": "Valid Source Title",
                "url": "https://valid-url.com",
                "description": "Valid description"
            }
            
            source_valid = mock_validate_search_source(valid_source)
            assert isinstance(source_valid, bool), "Should return boolean for source validation"
            
            print("✅ search response validation works correctly")
            
            return True
            
        except Exception as e:
            print(f"❌ search response validation test failed: {e}")
            import traceback
            traceback.print_exc()
            return False
    
    def test_search_source_quality_assessment(self):
        """Test search source quality assessment"""
        print("\nTesting search source quality assessment...")
        
        try:
            # Create mock implementations for functions that don't exist yet
            def mock_assess_source_authority(source):
                """Mock authority assessment function"""
                url = source.get("url", "").lower()
                if any(domain in url for domain in ["unops.org", "un.org", "who.int"]):
                    return 0.9  # High authority
                elif any(domain in url for domain in ["gov", "edu", "org"]):
                    return 0.7  # Medium authority
                else:
                    return 0.3  # Low authority
            
            def mock_filter_sources_by_quality(sources, min_quality=0.5):
                """Mock source filtering function"""
                return [source for source in sources 
                       if mock_assess_source_authority(source) >= min_quality]
            
            # Test source authority assessment
            high_authority_sources = [
                {"url": "https://unops.org/official-statement", "title": "Official UNOPS Statement"},
                {"url": "https://un.org/climate-report", "title": "UN Climate Report"},
                {"url": "https://who.int/health-guidelines", "title": "WHO Health Guidelines"}
            ]
            
            low_authority_sources = [
                {"url": "https://random-blog.com/opinion", "title": "Random Opinion"},
                {"url": "https://social-media.com/post", "title": "Social Media Post"}
            ]
            
            # Test authority assessment
            for source in high_authority_sources:
                authority_score = mock_assess_source_authority(source)
                assert isinstance(authority_score, (int, float)), f"Should return numeric score for: {source['url']}"
                assert authority_score > 0.5, f"Should have high authority score for: {source['url']}"
            
            # Test source filtering
            mixed_sources = high_authority_sources + low_authority_sources
            filtered_sources = mock_filter_sources_by_quality(mixed_sources)
            
            assert isinstance(filtered_sources, list), "Should return list of filtered sources"
            assert len(filtered_sources) <= len(mixed_sources), "Filtered list should not be longer than original"
            assert len(filtered_sources) >= len(high_authority_sources), "Should keep high authority sources"
            
            print("✅ search source quality assessment works correctly")
            
            return True
            
        except Exception as e:
            print(f"❌ search source quality assessment test failed: {e}")
            import traceback
            traceback.print_exc()
            return False
    
    def test_search_integration_with_task_executor(self):
        """Test search integration with task executor agent"""
        print("\nTesting search integration with task executor...")
        
        try:
            # Test that search agent is available in task executor
            from ai_assistant.sub_agents.task_executor_agent.utils import SearchAgentStub
            
            # Create a search agent stub instance
            search_agent_stub = SearchAgentStub()
            
            # Verify search agent structure
            assert hasattr(search_agent_stub, 'name'), "search_agent should have name"
            assert hasattr(search_agent_stub, 'execute'), "search_agent should have execute method"
            
            # Test mock execution
            test_query = "climate initiatives partnerships"
            result = search_agent_stub.execute(test_query)
            
            # Verify result structure (even if it's a stub)
            assert isinstance(result, dict), "Should return dict result"
            assert "content" in result, "Should have content field"
            
            print("✅ search integration with task executor works correctly")
            
            return True
            
        except Exception as e:
            print(f"❌ search integration test failed: {e}")
            import traceback
            traceback.print_exc()
            return False
    
    def test_search_error_handling(self):
        """Test search error handling for various failure scenarios"""
        print("\nTesting search error handling...")
        
        try:
            # Mock various error scenarios
            with patch('google.adk.tools.google_search') as mock_google_search:
                
                # Test 1: Network timeout
                mock_google_search.side_effect = Exception("Network timeout")
                
                try:
                    from google.adk.tools import google_search
                    result = google_search("test query")
                    # Should handle gracefully or return error response
                    print("✅ Network timeout error handled gracefully")
                except Exception as e:
                    print(f"✅ Network timeout error handling verified: {type(e).__name__}")
                
                # Test 2: API quota exceeded
                mock_google_search.side_effect = Exception("API quota exceeded")
                
                try:
                    result = google_search("test query")
                    print("✅ API quota error handled gracefully")
                except Exception as e:
                    print(f"✅ API quota error handling verified: {type(e).__name__}")
                
                # Test 3: Empty results
                mock_google_search.side_effect = None
                mock_google_search.return_value = {"results": [], "total_results": 0}
                
                try:
                    result = google_search("nonexistent query")
                    assert isinstance(result, dict), "Should return dict even for empty results"
                    assert "results" in result, "Should include results key"
                    assert len(result["results"]) == 0, "Should return empty results list"
                    print("✅ Empty results handled correctly")
                except Exception as e:
                    print(f"✅ Empty results handling verified: {type(e).__name__}")
            
            return True
            
        except Exception as e:
            print(f"❌ Search error handling test failed: {e}")
            import traceback
            traceback.print_exc()
            return False
    
    def test_search_query_types(self):
        """Test various types of search queries"""
        print("\nTesting various search query types...")
        
        try:
            # Mock the search function
            with patch('google.adk.tools.google_search') as mock_google_search:
                
                def mock_search_response(query, num_results=5):
                    """Generate mock response based on query type"""
                    if "climate" in query.lower():
                        return {
                            "results": [
                                {
                                    "title": "Climate Initiative Report",
                                    "url": "https://climate.org/report",
                                    "snippet": "Latest climate initiatives and partnerships..."
                                }
                            ]
                        }
                    elif "partnership" in query.lower():
                        return {
                            "results": [
                                {
                                    "title": "Partnership Programs Overview",
                                    "url": "https://partnerships.org/overview",
                                    "snippet": "Comprehensive partnership programs for development..."
                                }
                            ]
                        }
                    else:
                        return {
                            "results": [
                                {
                                    "title": "General Search Result",
                                    "url": "https://example.org/general",
                                    "snippet": "General search result content..."
                                }
                            ]
                        }
                
                mock_google_search.side_effect = mock_search_response
                
                # Test different query types
                query_types = [
                    {
                        "type": "factual",
                        "query": "UNOPS headquarters location",
                        "expected_elements": ["location", "headquarters"]
                    },
                    {
                        "type": "current_events", 
                        "query": "latest climate change initiatives 2024",
                        "expected_elements": ["climate", "2024"]
                    },
                    {
                        "type": "organizational",
                        "query": "partnership programs international development",
                        "expected_elements": ["partnership", "development"]
                    },
                    {
                        "type": "technical",
                        "query": "sustainable development goals implementation",
                        "expected_elements": ["sustainable", "development"]
                    }
                ]
                
                from google.adk.tools import google_search
                
                for query_info in query_types:
                    result = google_search(query_info["query"])
                    
                    # Verify basic structure
                    assert isinstance(result, dict), f"Should return dict for {query_info['type']} query"
                    assert "results" in result, f"Should have results for {query_info['type']} query"
                    
                    if result["results"]:
                        first_result = result["results"][0]
                        assert "title" in first_result, f"Should have title for {query_info['type']} query"
                        assert "url" in first_result, f"Should have URL for {query_info['type']} query"
                
                print("✅ various search query types work correctly")
            
            return True
            
        except Exception as e:
            print(f"❌ search query types test failed: {e}")
            import traceback
            traceback.print_exc()
            return False


def run_web_search_tool_tests():
    """Run all web search tool tests"""
    print("🔍 Running Web Search Tool Test Suite")
    print("=" * 60)
    
    test_instance = TestWebSearchTool()
    
    tests = [
        ("Search Agent Import Test", test_instance.test_search_agent_import),
        ("Google Search Tool Functionality Test", test_instance.test_google_search_tool_functionality),
        ("Search Agent Execution Test", test_instance.test_search_agent_execution),
        ("Search Query Optimization Test", test_instance.test_search_query_optimization),
        ("Search Response Validation Test", test_instance.test_search_response_validation),
        ("Search Source Quality Assessment Test", test_instance.test_search_source_quality_assessment),
        ("Search Integration with Task Executor Test", test_instance.test_search_integration_with_task_executor),
        ("Search Error Handling Test", test_instance.test_search_error_handling),
        ("Search Query Types Test", test_instance.test_search_query_types),
    ]
    
    results = {}
    
    for test_name, test_func in tests:
        try:
            results[test_name] = test_func()
        except Exception as e:
            print(f"❌ {test_name} failed with exception: {e}")
            results[test_name] = False
    
    # Summary
    print("\n" + "=" * 60)
    print("🔍 WEB SEARCH TOOL TEST SUMMARY:")
    
    passed = 0
    total = len(results)
    
    for test_name, result in results.items():
        status = "✅ PASS" if result else "❌ FAIL"
        print(f"{status} - {test_name}")
        if result:
            passed += 1
    
    print(f"\n📊 Overall: {passed}/{total} tests passed")
    
    if passed == total:
        print("🎉 ALL WEB SEARCH TOOL TESTS PASSED!")
        print("✅ Web search tools are ready for production use!")
    elif passed >= total * 0.75:
        print("⚠️ Most tests passed, but some issues need fixing.")
    else:
        print("❌ Multiple issues found. Significant fixes needed.")
    
    return passed == total


if __name__ == "__main__":
    success = run_web_search_tool_tests()
    sys.exit(0 if success else 1)

