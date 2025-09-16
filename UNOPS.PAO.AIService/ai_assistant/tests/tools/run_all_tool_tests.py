#!/usr/bin/env python3
"""
Master test runner for all AI Assistant tools.
Executes comprehensive tests for all tools and provides detailed summary.
"""

import sys
import os
import time
from datetime import datetime

# Add the project root to the path
sys.path.insert(0, os.path.abspath(os.path.join(os.path.dirname(__file__), '..', '..', '..')))

# Import all tool test suites
try:
    from test_api_tool import run_api_tool_tests
    API_TESTS_AVAILABLE = True
except ImportError as e:
    print(f"⚠️ API tool tests not available: {e}")
    API_TESTS_AVAILABLE = False

try:
    from test_google_drive_search import run_google_drive_search_tests
    DRIVE_TESTS_AVAILABLE = True
except ImportError as e:
    print(f"⚠️ Google Drive search tests not available: {e}")
    DRIVE_TESTS_AVAILABLE = False

try:
    from test_google_sheet_creation import run_google_sheet_creation_tests
    SHEETS_TESTS_AVAILABLE = True
except ImportError as e:
    print(f"⚠️ Google Sheets creation tests not available: {e}")
    SHEETS_TESTS_AVAILABLE = False

try:
    from test_google_doc_creation import run_google_doc_creation_tests
    DOCS_TESTS_AVAILABLE = True
except ImportError as e:
    print(f"⚠️ Google Docs creation tests not available: {e}")
    DOCS_TESTS_AVAILABLE = False

try:
    from test_web_search_tool import run_web_search_tool_tests
    WEB_SEARCH_TESTS_AVAILABLE = True
except ImportError as e:
    print(f"⚠️ Web search tool tests not available: {e}")
    WEB_SEARCH_TESTS_AVAILABLE = False


def print_header():
    """Print test suite header"""
    print("🧪 " + "=" * 80)
    print("🧪 AI ASSISTANT TOOLS - COMPREHENSIVE TEST SUITE")
    print("🧪 " + "=" * 80)
    print(f"🧪 Test Run Started: {datetime.now().strftime('%Y-%m-%d %H:%M:%S')}")
    print("🧪 " + "=" * 80)


def print_section_header(section_name, emoji="🔧"):
    """Print section header"""
    print(f"\n{emoji} " + "─" * 60)
    print(f"{emoji} {section_name}")
    print(f"{emoji} " + "─" * 60)


def run_tool_test_suite(test_name, test_function, emoji="🔧"):
    """Run a tool test suite and capture results"""
    print_section_header(f"Running {test_name}", emoji)
    
    start_time = time.time()
    
    try:
        success = test_function()
        end_time = time.time()
        duration = end_time - start_time
        
        return {
            "name": test_name,
            "success": success,
            "duration": duration,
            "error": None
        }
    
    except Exception as e:
        end_time = time.time()
        duration = end_time - start_time
        
        print(f"❌ {test_name} failed with exception: {e}")
        
        return {
            "name": test_name,
            "success": False,
            "duration": duration,
            "error": str(e)
        }


def print_comprehensive_summary(results, total_duration):
    """Print comprehensive test summary"""
    print("\n🧪 " + "=" * 80)
    print("🧪 COMPREHENSIVE TEST SUITE SUMMARY")
    print("🧪 " + "=" * 80)
    
    # Calculate statistics
    total_suites = len(results)
    passed_suites = sum(1 for r in results if r["success"])
    failed_suites = total_suites - passed_suites
    
    # Overall status
    overall_success = failed_suites == 0
    status_emoji = "🎉" if overall_success else "⚠️" if passed_suites >= total_suites * 0.75 else "❌"
    
    print(f"\n{status_emoji} OVERALL RESULTS:")
    print(f"   📊 Total Test Suites: {total_suites}")
    print(f"   ✅ Passed: {passed_suites}")
    print(f"   ❌ Failed: {failed_suites}")
    print(f"   📈 Success Rate: {(passed_suites/total_suites)*100:.1f}%")
    print(f"   ⏱️  Total Duration: {total_duration:.2f} seconds")
    
    # Detailed results
    print(f"\n📋 DETAILED RESULTS:")
    for result in results:
        status = "✅ PASS" if result["success"] else "❌ FAIL"
        duration_str = f"({result['duration']:.2f}s)"
        print(f"   {status} - {result['name']} {duration_str}")
        if result["error"]:
            print(f"      💥 Error: {result['error']}")
    
    # Performance analysis
    print(f"\n⚡ PERFORMANCE ANALYSIS:")
    if results:
        fastest = min(results, key=lambda x: x["duration"])
        slowest = max(results, key=lambda x: x["duration"])
        avg_duration = sum(r["duration"] for r in results) / len(results)
        
        print(f"   🚀 Fastest: {fastest['name']} ({fastest['duration']:.2f}s)")
        print(f"   🐌 Slowest: {slowest['name']} ({slowest['duration']:.2f}s)")
        print(f"   📊 Average: {avg_duration:.2f}s")
    
    # Recommendations
    print(f"\n💡 RECOMMENDATIONS:")
    if overall_success:
        print("   🎯 All test suites passed! Tools are ready for production deployment.")
        print("   📈 Consider adding integration tests for end-to-end workflows.")
        print("   🔍 Monitor performance in production environments.")
    elif passed_suites >= total_suites * 0.75:
        print("   ⚡ Most test suites passed. Address failing tests before deployment.")
        print("   🔧 Focus on fixing critical tool functionality issues.")
        print("   📝 Review error messages and update configurations as needed.")
    else:
        print("   🚨 Multiple test suites failed. Significant fixes required.")
        print("   🛠️  Prioritize core functionality repairs.")
        print("   🔄 Re-run tests after each fix to track progress.")
    
    # Tool-specific guidance
    print(f"\n🛠️  TOOL-SPECIFIC STATUS:")
    tool_categories = {
        "Core Functionality": ["API Tool Tests"],
        "Google Workspace": ["Google Drive Search Tests", "Google Sheets Creation Tests", "Google Docs Creation Tests"],
        "External Services": ["Web Search Tool Tests"]
    }
    
    for category, tool_names in tool_categories.items():
        category_results = [r for r in results if r["name"] in tool_names]
        if category_results:
            category_passed = sum(1 for r in category_results if r["success"])
            category_total = len(category_results)
            category_rate = (category_passed / category_total) * 100
            
            status_icon = "✅" if category_rate == 100 else "⚠️" if category_rate >= 75 else "❌"
            print(f"   {status_icon} {category}: {category_passed}/{category_total} ({category_rate:.0f}%)")
    
    print("\n🧪 " + "=" * 80)
    print(f"🧪 Test Run Completed: {datetime.now().strftime('%Y-%m-%d %H:%M:%S')}")
    print("🧪 " + "=" * 80)


def main():
    """Main test runner function"""
    print_header()
    
    # Track overall timing
    overall_start_time = time.time()
    
    # Define test suites to run
    test_suites = []
    
    # Add available test suites
    if API_TESTS_AVAILABLE:
        test_suites.append(("API Tool Tests", run_api_tool_tests, "🔧"))
    
    if DRIVE_TESTS_AVAILABLE:
        test_suites.append(("Google Drive Search Tests", run_google_drive_search_tests, "📂"))
    
    if SHEETS_TESTS_AVAILABLE:
        test_suites.append(("Google Sheets Creation Tests", run_google_sheet_creation_tests, "📊"))
    
    if DOCS_TESTS_AVAILABLE:
        test_suites.append(("Google Docs Creation Tests", run_google_doc_creation_tests, "📄"))
    
    if WEB_SEARCH_TESTS_AVAILABLE:
        test_suites.append(("Web Search Tool Tests", run_web_search_tool_tests, "🔍"))
    
    # Check if any tests are available
    if not test_suites:
        print("❌ No test suites available to run!")
        print("   Make sure all test files are present and properly configured.")
        return False
    
    print(f"🎯 Found {len(test_suites)} test suites to execute...")
    
    # Run all test suites
    results = []
    for test_name, test_func, emoji in test_suites:
        result = run_tool_test_suite(test_name, test_func, emoji)
        results.append(result)
    
    # Calculate total duration
    overall_end_time = time.time()
    total_duration = overall_end_time - overall_start_time
    
    # Print comprehensive summary
    print_comprehensive_summary(results, total_duration)
    
    # Return overall success
    return all(result["success"] for result in results)


if __name__ == "__main__":
    success = main()
    sys.exit(0 if success else 1) 