#!/usr/bin/env python3
"""
Comprehensive tests for Google Drive search tools using external APIs.
Tests the new external API-based Google Drive utilities.
"""

import sys
import os
import json
import unittest
from unittest.mock import Mock, patch, MagicMock

# Add the project root to the path
sys.path.insert(0, os.path.abspath(os.path.join(os.path.dirname(__file__), '..', '..', '..')))


class TestGoogleDriveExternalAPI:
    """Test suite for external API-based Google Drive search tools"""
    
    def test_external_api_imports(self):
        """Test that external API Google Drive tools can be imported correctly"""
        print("Testing external API Google Drive tool imports...")
        
        try:
            # Test imports of new external API tools
            from ai_assistant.tools.google_drive_utils import (
                search_unops_google_drive,
                search_external_drive_service,
                read_content_from_url,
                convert_markdown_to_google_doc
            )
            
            assert search_unops_google_drive is not None, "search_unops_google_drive should be importable"
            assert search_external_drive_service is not None, "search_external_drive_service should be importable"
            assert read_content_from_url is not None, "read_content_from_url should be importable"
            assert convert_markdown_to_google_doc is not None, "convert_markdown_to_google_doc should be importable"
            
            print("✅ All external API Google Drive tools imported successfully")
            return True
            
        except Exception as e:
            print(f"❌ External API Google Drive tool import failed: {e}")
            import traceback
            traceback.print_exc()
            return False
    
    def test_search_unops_google_drive(self):
        """Test UNOPS Google Drive search functionality"""
        print("\nTesting search_unops_google_drive...")
        
        try:
            from ai_assistant.tools.google_drive_utils import search_unops_google_drive
            
            # Mock the invoke_api_tool dependency
            with patch('ai_assistant.tools.google_drive_utils.search_external_drive_service') as mock_search:
                # Mock successful search response
                mock_response = {
                    "content": "📁 **External Search Results for 'climate change':**\n\n**Document 1: Climate Strategy 2024**\n*Content:* This document outlines UNOPS climate initiatives...",
                    "sources": [
                        {
                            "title": "Climate Strategy 2024",
                            "url": "https://docs.google.com/document/d/abc123/view",
                            "description": "UNOPS Climate Strategy Document",
                            "last_updated": "2024-01-15",
                            "file_type": "application/vnd.google-apps.document"
                        }
                    ],
                    "stats": {
                        "total_found": 1,
                        "content_read": 1,
                        "query": "climate change"
                    }
                }
                
                mock_search.return_value = json.dumps(mock_response)
                
                # Test the function
                result = search_unops_google_drive("climate change")
                
                # Verify result format
                assert isinstance(result, str), "Should return JSON string"
                parsed_result = json.loads(result)
                assert "content" in parsed_result, "Should contain content"
                assert "sources" in parsed_result, "Should contain sources"
                
                # Verify search was called with correct endpoint
                mock_search.assert_called_once_with(
                    "climate change", 
                    "https://api.ai.dev.unops.org/v1/tools/google-drive/search", 
                    None
                )
                
                print("✅ search_unops_google_drive works correctly")
            
            return True
            
        except Exception as e:
            print(f"❌ search_unops_google_drive test failed: {e}")
            import traceback
            traceback.print_exc()
            return False
    
    def test_search_external_drive_service(self):
        """Test generic external drive service search functionality"""
        print("\nTesting search_external_drive_service...")
        
        try:
            from ai_assistant.tools.google_drive_utils import search_external_drive_service
            
            # Mock the invoke_api_tool dependency
            with patch('ai_assistant.utils.common_callbacks.invoke_api_tool') as mock_invoke:
                with patch('ai_assistant.tools.google_drive_utils._read_external_files_from_urls') as mock_read:
                    # Mock external API response
                    mock_invoke.return_value = {
                        "status": "success",
                        "response": {
                            "documents": [
                                {
                                    "id": "doc_123",
                                    "name": "Partnership Agreement",
                                    "webViewLink": "https://docs.google.com/document/d/doc_123/view",
                                    "mimeType": "application/vnd.google-apps.document",
                                    "updatedAt": "2024-01-15T10:30:00Z"
                                }
                            ]
                        }
                    }
                    
                    # Mock URL content reading
                    mock_read.return_value = json.dumps({
                        "content": "📁 **External Search Results for 'partnership':**\n\n**Document 1: Partnership Agreement**\n*Content:* Partnership details...",
                        "sources": [
                            {
                                "title": "Partnership Agreement",
                                "url": "https://docs.google.com/document/d/doc_123/view",
                                "description": "Document: Partnership Agreement"
                            }
                        ]
                    })
                    
                    # Test the function
                    result = search_external_drive_service(
                        "partnership",
                        "https://api.example.com/search",
                        {"Authorization": "Bearer test_token"}
                    )
                    
                    # Verify result format
                    assert isinstance(result, str), "Should return JSON string"
                    parsed_result = json.loads(result)
                    assert "content" in parsed_result, "Should contain content"
                    assert "sources" in parsed_result, "Should contain sources"
                    
                    # Verify invoke_api_tool was called correctly
                    mock_invoke.assert_called_once_with(
                        url="https://api.example.com/search",
                        method="POST",
                        body={"query": "partnership", "maxResults": 20},
                        headers={"Content-Type": "application/json", "Authorization": "Bearer test_token"}
                    )
                    
                    print("✅ search_external_drive_service works correctly")
            
            return True
            
        except Exception as e:
            print(f"❌ search_external_drive_service test failed: {e}")
            import traceback
            traceback.print_exc()
            return False
    
    def test_read_content_from_url(self):
        """Test URL content reading functionality"""
        print("\nTesting read_content_from_url...")
        
        try:
            from ai_assistant.tools.google_drive_utils import read_content_from_url
            
            # Mock the invoke_api_tool dependency
            with patch('ai_assistant.utils.common_callbacks.invoke_api_tool') as mock_invoke:
                # Mock successful content reading response
                mock_invoke.return_value = {
                    "status": "success",
                    "response": {
                        "data": "# Document Title\n\nThis is the content of the document converted to markdown..."
                    }
                }
                
                # Test the function
                result = read_content_from_url(
                    "https://docs.google.com/document/d/abc123/view",
                    include_json=True,
                    output_format="markdown",
                    title="Test Document"
                )
                
                # Verify result format
                assert isinstance(result, str), "Should return JSON string"
                parsed_result = json.loads(result)
                assert "content" in parsed_result, "Should contain content"
                assert "url" in parsed_result, "Should contain URL"
                assert "success" in parsed_result, "Should contain success flag"
                assert parsed_result["success"] == True, "Should be successful"
                
                # Verify invoke_api_tool was called correctly
                mock_invoke.assert_called_once_with(
                    url="https://api.ai.dev.unops.org/v1/convert/url",
                    method="POST",
                    body={
                        "includeJson": True,
                        "outputFormat": "markdown",
                        "gcsOutput": "",
                        "chunkSize": 1,
                        "embeddingsModel": "",
                        "title": "Test Document",
                        "description": "",
                        "url": "https://docs.google.com/document/d/abc123/view"
                    },
                    headers={"Content-Type": "application/json"}
                )
                
                print("✅ read_content_from_url works correctly")
            
            return True
            
        except Exception as e:
            print(f"❌ read_content_from_url test failed: {e}")
            import traceback
            traceback.print_exc()
            return False
    
    def test_convert_markdown_to_google_doc(self):
        """Test markdown to Google Doc conversion functionality"""
        print("\nTesting convert_markdown_to_google_doc...")
        
        try:
            from ai_assistant.tools.google_drive_utils import convert_markdown_to_google_doc
            
            # Test that the function exists and is callable
            assert callable(convert_markdown_to_google_doc), "Should be callable"
            
            # Test the function with real call (will get 401 but should handle gracefully)
            markdown_content = "# Test Document\n\nThis is a test markdown document."
            result = convert_markdown_to_google_doc(
                markdown_content,
                "test_document.md",
                {"author": "Test User"}
            )
            
            # Verify result format
            assert isinstance(result, str), "Should return JSON string"
            parsed_result = json.loads(result)
            
            # Should handle authentication error gracefully
            if "error" in parsed_result:
                assert "credentials" in parsed_result["error"].lower() or "auth" in parsed_result["error"].lower(), "Should be authentication error"
                print("✅ convert_markdown_to_google_doc handles auth error correctly")
            else:
                # If it actually worked (with proper auth)
                assert "document_id" in parsed_result or "status" in parsed_result, "Should contain result fields"
                print("✅ convert_markdown_to_google_doc works correctly")
            
            return True
            
        except Exception as e:
            print(f"❌ convert_markdown_to_google_doc test failed: {e}")
            import traceback
            traceback.print_exc()
            return False
    
    def test_error_handling(self):
        """Test error handling for various failure scenarios"""
        print("\nTesting error handling...")
        
        try:
            from ai_assistant.tools.google_drive_utils import (
                search_unops_google_drive,
                read_content_from_url
            )
            
            # Test 1: External API error handling
            with patch('ai_assistant.tools.google_drive_utils.search_external_drive_service') as mock_search:
                mock_search.return_value = json.dumps({
                    "content": "Error calling external search service: API timeout",
                    "sources": []
                })
                
                result = search_unops_google_drive("test query")
                parsed_result = json.loads(result)
                assert "content" in parsed_result, "Should handle errors gracefully"
                print("✅ External API error handled gracefully")
            
            # Test 2: URL content reading error handling
            with patch('ai_assistant.utils.common_callbacks.invoke_api_tool') as mock_invoke:
                mock_invoke.return_value = {
                    "status": "error",
                    "error": "URL not accessible"
                }
                
                result = read_content_from_url("https://invalid.url")
                parsed_result = json.loads(result)
                assert "error" in parsed_result, "Should contain error information"
                print("✅ URL content reading error handled gracefully")
            
            # Test 3: Network exception handling
            with patch('ai_assistant.utils.common_callbacks.invoke_api_tool') as mock_invoke:
                mock_invoke.side_effect = Exception("Network error")
                
                result = read_content_from_url("https://docs.google.com/test")
                parsed_result = json.loads(result)
                assert "error" in parsed_result, "Should handle network errors"
                print("✅ Network exception handled gracefully")
            
            return True
            
        except Exception as e:
            print(f"❌ Error handling test failed: {e}")
            import traceback
            traceback.print_exc()
            return False
    
    def test_integration_with_task_executor(self):
        """Test integration with task executor agent utilities"""
        print("\nTesting integration with task executor...")
        
        try:
            # Test that the functions exist in task executor utils
            from ai_assistant.sub_agents.task_executor_agent.utils import (
                search_unops_google_drive,
                search_external_drive_service,
                read_content_from_url,
                convert_markdown_to_google_doc
            )
            
            # Verify functions are callable
            assert callable(search_unops_google_drive), "search_unops_google_drive should be callable"
            assert callable(search_external_drive_service), "search_external_drive_service should be callable"
            assert callable(read_content_from_url), "read_content_from_url should be callable"
            assert callable(convert_markdown_to_google_doc), "convert_markdown_to_google_doc should be callable"
            
            # Test stub implementations work (these return JSON strings)
            result1 = search_unops_google_drive("test query")
            assert isinstance(result1, str), "Should return JSON string"
            
            result2 = search_external_drive_service("test query", "https://test.api", {})
            assert isinstance(result2, str), "Should return JSON string"
            
            result3 = read_content_from_url("https://test.url")
            assert isinstance(result3, str), "Should return JSON string"
            
            result4 = convert_markdown_to_google_doc("# Test Markdown", "test.md")
            assert isinstance(result4, str), "Should return JSON string"
            
            print("✅ Integration with task executor works correctly")
            
            return True
            
        except Exception as e:
            print(f"❌ Integration test failed: {e}")
            import traceback
            traceback.print_exc()
            return False
    
    def test_response_format_consistency(self):
        """Test that all functions return consistent JSON response formats"""
        print("\nTesting response format consistency...")
        
        try:
            from ai_assistant.tools.google_drive_utils import (
                search_unops_google_drive,
                read_content_from_url
            )
            
            # Test search response format
            with patch('ai_assistant.tools.google_drive_utils.search_external_drive_service') as mock_search:
                mock_search.return_value = json.dumps({
                    "content": "Test content",
                    "sources": [{"title": "Test", "url": "https://test.com"}],
                    "stats": {"total_found": 1, "content_read": 1, "query": "test"}
                })
                
                result = search_unops_google_drive("test")
                parsed_result = json.loads(result)
                
                # Check required fields
                assert "content" in parsed_result, "Search should have content field"
                assert "sources" in parsed_result, "Search should have sources field"
                assert isinstance(parsed_result["sources"], list), "Sources should be a list"
                
                print("✅ Search response format is consistent")
            
            # Test content reading response format
            with patch('ai_assistant.utils.common_callbacks.invoke_api_tool') as mock_invoke:
                mock_invoke.return_value = {
                    "status": "success",
                    "response": {"data": "Test content"}
                }
                
                result = read_content_from_url("https://test.url")
                parsed_result = json.loads(result)
                
                # Check required fields
                assert "content" in parsed_result, "Content reading should have content field"
                assert "url" in parsed_result, "Content reading should have url field"
                assert "success" in parsed_result, "Content reading should have success field"
                
                print("✅ Content reading response format is consistent")
            
            return True
            
        except Exception as e:
            print(f"❌ Response format consistency test failed: {e}")
            import traceback
            traceback.print_exc()
            return False


def run_google_drive_external_api_tests():
    """Run all external API Google Drive tests"""
    print("🌐 Running External API Google Drive Test Suite")
    print("=" * 70)
    
    test_instance = TestGoogleDriveExternalAPI()
    
    tests = [
        ("External API Import Test", test_instance.test_external_api_imports),
        ("UNOPS Google Drive Search Test", test_instance.test_search_unops_google_drive),
        ("External Drive Service Search Test", test_instance.test_search_external_drive_service),
        ("URL Content Reading Test", test_instance.test_read_content_from_url),
        ("Markdown to Google Doc Conversion Test", test_instance.test_convert_markdown_to_google_doc),
        ("Error Handling Test", test_instance.test_error_handling),
        ("Integration with Task Executor Test", test_instance.test_integration_with_task_executor),
        ("Response Format Consistency Test", test_instance.test_response_format_consistency),
    ]
    
    results = {}
    
    for test_name, test_func in tests:
        try:
            results[test_name] = test_func()
        except Exception as e:
            print(f"❌ {test_name} failed with exception: {e}")
            results[test_name] = False
    
    # Summary
    print("\n" + "=" * 70)
    print("🌐 EXTERNAL API GOOGLE DRIVE TEST SUMMARY:")
    
    passed = 0
    total = len(results)
    
    for test_name, result in results.items():
        status = "✅ PASS" if result else "❌ FAIL"
        print(f"{status} - {test_name}")
        if result:
            passed += 1
    
    print(f"\n📊 Overall: {passed}/{total} tests passed")
    
    if passed == total:
        print("🎉 ALL EXTERNAL API GOOGLE DRIVE TESTS PASSED!")
        print("✅ External API Google Drive tools are ready for production use!")
    elif passed >= total * 0.75:
        print("⚠️ Most tests passed, but some issues need fixing.")
    else:
        print("❌ Multiple issues found. Significant fixes needed.")
    
    return passed == total


if __name__ == "__main__":
    success = run_google_drive_external_api_tests()
    sys.exit(0 if success else 1)
