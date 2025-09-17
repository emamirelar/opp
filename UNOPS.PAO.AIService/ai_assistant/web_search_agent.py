"""
Google Search Agent

This module defines a specialized agent for handling Google search queries
when information is not available in the knowledge base or for current events.
"""

from google.adk.agents import LlmAgent
from google.adk.tools import google_search
from google.adk.callback_context import CallbackContext
from google.adk.llm_response import LlmResponse
from google.genai import types
from ai_assistant.utils.api_config_manager import config_manager
import json
import copy
import re
from typing import Optional, List, Dict, Any
from datetime import datetime


# Enhanced Search Utilities (integrated from sub_agents/search_agent/utils.py)

def optimize_search_query(user_query: str, current_year: int = 2024) -> str:
    """
    Optimize a user query for better Google search results.
    
    Args:
        user_query: The original user query
        current_year: Current year for temporal focus
        
    Returns:
        str: Optimized search query
    """
    query = user_query.lower().strip()
    
    # Add current year for news/recent information
    temporal_keywords = ["latest", "recent", "current", "new", "updates", "news"]
    if any(keyword in query for keyword in temporal_keywords):
        if str(current_year) not in query:
            query += f" {current_year}"
    
    # Add authority terms for better source quality
    if "news" in query or "information" in query:
        if "official" not in query:
            query += " official news"
    
    return query


def assess_source_authority(url: str) -> int:
    """
    Assess the authority level of a source based on its URL.
    
    Args:
        url: Source URL
        
    Returns:
        int: Authority score (1-5, higher is better)
    """
    url_lower = url.lower()
    
    # High authority domains
    if any(domain in url_lower for domain in [".gov", ".edu", ".org"]):
        return 5
    
    # Major news outlets
    if any(outlet in url_lower for outlet in ["reuters", "bbc", "ap.org", "bloomberg", "wsj"]):
        return 4
    
    # Other news sites
    if any(term in url_lower for term in ["news", "times", "post", "guardian"]):
        return 3
    
    # Corporate/commercial sites
    if ".com" in url_lower:
        return 2
    
    # Default
    return 1


def validate_search_source(source: Dict[str, Any]) -> bool:
    """
    Validate that a search source has required fields and quality.
    
    Args:
        source: Source dictionary to validate
        
    Returns:
        bool: True if source is valid
    """
    required_fields = ["title", "url", "description"]
    
    # Check required fields exist and are not empty
    for field in required_fields:
        if field not in source or not source[field] or not isinstance(source[field], str):
            return False
    
    # Basic URL validation
    url = source["url"]
    if not url.startswith(("http://", "https://")):
        return False
    
    # Check for minimum content quality
    if len(source["title"].strip()) < 5:
        return False
        
    if len(source["description"].strip()) < 10:
        return False
    
    return True


def filter_sources_by_quality(sources: List[Dict[str, Any]], min_authority: int = 2) -> List[Dict[str, Any]]:
    """
    Filter sources based on quality and authority.
    
    Args:
        sources: List of source dictionaries
        min_authority: Minimum authority score required
        
    Returns:
        list: Filtered list of high-quality sources
    """
    quality_sources = []
    
    for source in sources:
        if not validate_search_source(source):
            continue
            
        authority_score = assess_source_authority(source["url"])
        if authority_score >= min_authority:
            # Add authority score for potential sorting
            source["authority_score"] = authority_score
            quality_sources.append(source)
    
    # Sort by authority score (highest first)
    quality_sources.sort(key=lambda x: x.get("authority_score", 0), reverse=True)
    
    # Remove authority score before returning (internal use only)
    for source in quality_sources:
        source.pop("authority_score", None)
    
    return quality_sources


def extract_key_terms(user_query: str) -> List[str]:
    """
    Extract key terms from user query for search optimization.
    
    Args:
        user_query: User's search query
        
    Returns:
        list: List of key terms
    """
    # Remove common stop words
    stop_words = {
        "the", "is", "at", "which", "on", "a", "an", "and", "or", "but", "in", "with", "to", "for", "of", "as", "by"
    }
    
    # Extract words, clean and filter
    words = re.findall(r'\b\w+\b', user_query.lower())
    key_terms = [word for word in words if word not in stop_words and len(word) > 2]
    
    return key_terms


def search_agent_after_model_callback(
    callback_context: CallbackContext, llm_response: LlmResponse
) -> Optional[LlmResponse]:
    """
    After model callback for search_agent to format output as consistent JSON.
    """
    try:
        # Check if this is a text response (not a function call)
        if llm_response.content and llm_response.content.parts:
            for part in llm_response.content.parts:
                if part.text and not part.function_call:
                    # This is a text response - format it as the required JSON
                    search_content = part.text
                    
                    # Extract sources from the content if possible (look for URLs)
                    sources = []
                    url_pattern = r'https?://[^\s\)]+|www\.[^\s\)]+'
                    urls = re.findall(url_pattern, search_content)
                    
                    # Create sources with enhanced metadata
                    raw_sources = []
                    for url in urls[:10]:  # Get more URLs initially
                        raw_sources.append({
                            "url": url,
                            "title": "Search Result",
                            "description": f"Source from search results"
                        })
                    
                    # Filter and enhance sources using utilities
                    sources = filter_sources_by_quality(raw_sources, min_authority=1)[:5]  # Keep top 5
                    
                    # Format as markdown with proper markers
                    markdown_content = f"# Search Results\n\n{search_content}"
                    
                    # Create the standardized JSON response
                    json_response = {
                        "tool_name": "search_agent",
                        "message": markdown_content,
                        "type": "markdown",
                        "sources": sources
                    }
                    
                    # Create a new response with the JSON format
                    new_response = LlmResponse(
                        content=types.Content(
                            role="model", 
                            parts=[types.Part(text=json.dumps(json_response))]
                        ),
                        grounding_metadata=llm_response.grounding_metadata
                    )
                    return new_response
        
        # For function calls or other cases, return None to use original response
        return None
        
    except Exception:
        # On any error, return None to use original response
        return None

web_search_agent = LlmAgent(
    name="web_search_agent",
    model=config_manager.get_gemini_model(),
    description="Specialized agent for performing Google searches for current events, external information, and topics not covered in the knowledge base",
    instruction="""
# Google Search Agent

You are a highly specialized Google search agent designed to retrieve current, external, and authoritative information that is **not available in the internal knowledge base.** Your mission is to supplement internal data with verified, real-time external insights.

## Core Responsibilities

**Primary Functions:**
- Execute targeted Google searches for real-time and up-to-date information.
- Research current events, breaking news, and external market developments.
- Find information about external organizations, competitors, and their activities.
- Retrieve the latest updates and trending topics relevant to the user's query.
- Synthesize findings from authoritative external sources to provide comprehensive answers.

**Activation Criteria (When to use this agent):**
- User explicitly requests current events or breaking news.
- Queries about external organizations, companies, or competitors.
- Requests for the latest developments, recent updates, or trending topics.
- Topics that are explicitly identified as not covered by or needing external validation beyond the internal knowledge base.
- Requests for market research, industry trends, or external statistical data.

## Tools

- `google_search(query: str, num_results: int = 5)`:
    - **Description:** Performs a web search using Google and returns a list of relevant results.
    - **Inputs:**
        - `query` (string, required): The precise search query string, optimized for Google.
        - `num_results` (integer, optional): The maximum number of search results to return (default: 5).
    - **Output:** A JSON array of search results, where each result typically includes `title`, `url`, and a `snippet` (brief summary).

## Search Strategy & Execution

**Query Optimization (for `google_search` tool):**
1.  **Keywords Selection**: Use highly specific, targeted terms. Break down complex queries into sub-queries if necessary.
2.  **Temporal Focus**: Always include the current year (2024) or terms like "latest", "recent", "updates", "trends" for time-sensitive queries.
3.  **Source Targeting**: Add explicit terms like "official website", "news", "report", "study", "analysis", "press release", "government" for authoritative sources.
4.  **Entity Specificity**: Include precise organization names, specific industry terms, product names, and relevant geographic qualifiers (e.g., "Denmark", "Europe").

**Search Execution Steps:**
1.  Analyze the user's request thoroughly to identify all core information needs and implicit external requirements.
2.  Formulate one or more highly optimized search queries using the `google_search` tool.
3.  Process and critically evaluate the search results for relevance, credibility, and currency.
4.  Extract all key insights, verifiable facts, and conclusions that directly address the user's query.
5.  Compile a structured JSON response based on the "Response Format & Quality Standards" below, integrating findings and verified sources.

## Response Format & Quality Standards

**JSON Response Structure:**
```json
{
  "content": "Comprehensive summary of findings with key insights, facts, and conclusions based solely on the search results. Clearly distinguish between facts, opinions, and projections. If conflicting information is found, explicitly mention the conflict and the sources.",
  "sources": [
    {
      "title": "Exact title from source",
      "url": "Complete URL",
      "description": "Brief summary of source content and why it's relevant to the query. Include publication date if available and relevant."
    }
    // Include 2-5 of the most relevant and authoritative sources.
  ]
}
```

## Examples

**Query: "Latest UNOPS sustainability initiatives 2024"**
Search: `google_search("UNOPS sustainability initiatives 2024 latest projects", 5)`
Focus: Recent environmental projects, green commitments, policy updates

**Query: "Current development finance trends"**  
Search: `google_search("development finance trends 2024 multilateral funding", 5)`
Focus: Market patterns, funding mechanisms, emerging approaches""",
    tools=[google_search],
    after_model_callback=search_agent_after_model_callback,
    output_key="search_results",
    disallow_transfer_to_parent=True,
    disallow_transfer_to_peers=True
) 