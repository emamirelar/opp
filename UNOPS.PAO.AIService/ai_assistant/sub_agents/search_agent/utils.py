"""
Search Agent Utilities

This module contains utility functions for optimizing search queries,
processing search results, and enhancing search quality.
"""

import re
from typing import List, Dict, Any, Optional
from datetime import datetime


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


def format_search_response(content: str, sources: List[Dict[str, Any]]) -> Dict[str, Any]:
    """
    Format search results into the standardized response structure.
    
    Args:
        content: Summarized search content
        sources: List of source dictionaries
        
    Returns:
        dict: Formatted response structure
    """
    # Validate and filter sources
    valid_sources = [source for source in sources if validate_search_source(source)]
    
    # Limit to maximum 5 sources
    if len(valid_sources) > 5:
        valid_sources = valid_sources[:5]
    
    # Ensure minimum content quality
    if len(content.strip()) < 20:
        content = "Limited information available from search results. Please refine your query for better results."
    
    return {
        "content": content.strip(),
        "sources": valid_sources
    }


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


def generate_search_summary(search_results: List[Dict[str, Any]], max_length: int = 500) -> str:
    """
    Generate a concise summary from search results.
    
    Args:
        search_results: Raw search results
        max_length: Maximum summary length
        
    Returns:
        str: Generated summary
    """
    if not search_results:
        return "No relevant search results found."
    
    # Extract snippets and titles
    content_pieces = []
    for result in search_results[:5]:  # Use top 5 results
        title = result.get("title", "")
        snippet = result.get("snippet", "")
        
        if title:
            content_pieces.append(title)
        if snippet and snippet not in content_pieces:
            content_pieces.append(snippet)
    
    # Combine and truncate
    combined_content = " ".join(content_pieces)
    
    if len(combined_content) > max_length:
        # Truncate at word boundary
        truncated = combined_content[:max_length]
        last_space = truncated.rfind(" ")
        if last_space > max_length * 0.8:  # Only truncate at word if it's not too short
            combined_content = truncated[:last_space] + "..."
        else:
            combined_content = truncated + "..."
    
    return combined_content


def validate_search_response(response: Dict[str, Any]) -> bool:
    """
    Validate that a search response meets quality standards.
    
    Args:
        response: Search response to validate
        
    Returns:
        bool: True if response is valid
    """
    # Check required structure
    if not isinstance(response, dict):
        return False
    
    if "content" not in response or "sources" not in response:
        return False
    
    # Validate content
    content = response["content"]
    if not isinstance(content, str) or len(content.strip()) < 10:
        return False
    
    # Validate sources
    sources = response["sources"]
    if not isinstance(sources, list) or len(sources) == 0:
        return False
    
    # Validate each source
    for source in sources:
        if not validate_search_source(source):
            return False
    
    return True
