"""
Dynamic Tool Registry

This module provides the DynamicToolRegistry class for discovering relevant tools
based on user queries using semantic analysis.
"""

from typing import List, Dict, Any
import json

class DynamicToolRegistry:
    """
    Registry for dynamically discovering relevant tools based on user queries
    """
    
    def __init__(self):
        """Initialize the tool registry with available tools"""
        self.all_tools = self._load_all_tools()
    
    def _load_all_tools(self) -> List[Dict[str, Any]]:
        """
        Load all available tools with their metadata
        
        Returns:
            List of tool dictionaries with name, description, and scoring info
        """
        tools = [
            {
                "name": "api_tool_agent",
                "description": "Automatically finds and calls API endpoints for any entity and intent",
                "keywords": ["api", "call", "endpoint", "data", "fetch", "get", "create", "update", "delete", 
                           "list", "retrieve", "entity"],
                "score_weight": 100
            },
            {
                "name": "search_agent", 
                "description": "Search web and knowledge bases for latest news, information, and research",
                "keywords": ["search", "web", "google", "find", "information", "lookup", "query", "research", 
                           "news", "latest", "current", "recent", "today", "trending", "updates", "breaking"],
                "score_weight": 90
            },
            {
                "name": "read_content_from_url",
                "description": "Read and extract content from web URLs",
                "keywords": ["url", "web", "content", "read", "extract", "webpage", "link", "fetch"],
                "score_weight": 70
            }
        ]
        
        return tools
    
    def find_relevant_tools(self, user_query: str, max_tools: int = 3, ) -> List[Dict[str, Any]]:
        """
        Find the most relevant tools for a given user query with fallback strategies
        
        Args:
            user_query: The user's request to analyze
            max_tools: Maximum number of tools to return
            
        Returns:
            List of relevant tools with scores and fallback strategies
        """
        
        query_lower = user_query.lower()
        
        # Detect if this is an ambiguous query that needs multiple approaches
        needs_fallback = self._detect_ambiguous_query(query_lower)
        
        if needs_fallback:
            return self._get_fallback_strategies(query_lower, max_tools)
        else:
            # Standard single-tool approach
            scored_tools = []
            
            for tool in self.all_tools:
                score = self._calculate_tool_score(tool, query_lower)
                if score > 0:
                    scored_tools.append({
                        "name": tool["name"],
                        "description": tool["description"],
                        "score": score,
                        "parameters": self._get_tool_parameters(tool["name"])
                    })
            
            # Sort by score (highest first) and limit results
            scored_tools.sort(key=lambda x: x["score"], reverse=True)
            top_tools = scored_tools[:max_tools]
            
            for i, tool in enumerate(top_tools, 1):
            
            return top_tools
    
    def _calculate_tool_score(self, tool: Dict[str, Any], query_lower: str) -> int:
        """
        Calculate relevance score for a tool based on the query
        
        Args:
            tool: Tool dictionary with metadata
            query_lower: Lowercased user query
            
        Returns:
            Relevance score (0-100)
        """
        score = 0
        base_weight = tool.get("score_weight", 50)
        
        # Check for keyword matches
        keywords = tool.get("keywords", [])
        matched_keywords = 0
        
        for keyword in keywords:
            if keyword in query_lower:
                matched_keywords += 1
                score += 10  # Base score per keyword match
        
        # Boost score based on tool type and query patterns
        tool_name = tool["name"]
        
        # API-related queries get high score for invoke_api_tool
        if tool_name == "invoke_api_tool":
            # Check if this is a news/web search query - if so, don't boost API tool
            news_terms = ["news", "latest", "current", "recent", "today", "trending", "updates", "breaking"]
            is_news_query = any(word in query_lower for word in news_terms)
            
            if not is_news_query:
                api_indicators = ["get", "list", "fetch", "data", "create", "update", "delete", "retrieve"]
                if any(indicator in query_lower for indicator in api_indicators):
                    score += 50
                    
                # Check if query mentions any known entity from config_manager
                try:
                    from ai_assistant.utils.api_config_manager import config_manager
                    available_entities = config_manager.get_available_entities()
                    entity_mentioned = any(entity.lower() in query_lower for entity in available_entities)
                    if entity_mentioned:
                        score += 30
                except Exception as e:
                    # Fallback if config_manager is not available
                    pass
            else:
        
        # Search queries boost search_agent
        elif tool_name == "search_agent":
            # General search terms
            search_terms = ["search", "find", "lookup", "research", "information"]
            if any(word in query_lower for word in search_terms):
                score += 40
            
            # News and current information terms - high priority
            news_terms = ["news", "latest", "current", "recent", "today", "trending", "updates", "breaking"]
            if any(word in query_lower for word in news_terms):
                score += 60  # Higher boost for news queries
            
            # Web search indicators
            web_terms = ["web", "google", "online", "internet"]
            if any(word in query_lower for word in web_terms):
                score += 30
        
        # URL/web content queries boost read_content_from_url
        elif tool_name == "read_content_from_url":
            if any(word in query_lower for word in ["url", "link", "website", "webpage", "http"]):
                score += 40
        
        # Document creation queries boost convert_markdown_to_google_doc
        elif tool_name == "convert_markdown_to_google_doc":
            if any(word in query_lower for word in ["document", "doc", "create", "generate", "report"]):
                score += 40
        
        # Always include exit_loop_on_success with high score
        elif tool_name == "exit_loop_on_success":
            score = 100  # Always include for LoopAgent functionality
        
        # Apply base weight
        if score > 0:
            score = min(100, score + (base_weight // 10))
        
        return score
    
    def _get_tool_parameters(self, tool_name: str) -> List[str]:
        """
        Get parameter names for a specific tool
        
        Args:
            tool_name: Name of the tool
            
        Returns:
            List of parameter names
        """
        parameter_map = {
            "invoke_api_tool": ["entity_name", "intent", "params", "isMultiToolRequest"],
            "find_entity_endpoint": ["entity_name", "intent", "extracted_params"],
            "search_agent": ["query"],
            "read_content_from_url": ["url", "isMultiToolRequest"],
            "convert_markdown_to_google_doc": ["markdown_content", "document_title", "isMultiToolRequest"]
        }
        
        return parameter_map.get(tool_name, [])
    
    def _detect_ambiguous_query(self, query_lower: str) -> bool:
        """
        Detect if a query mentions multiple entities using config_manager data
        
        Args:
            query_lower: Lowercased user query
            
        Returns:
            True if query mentions multiple entities (needs fallback strategies)
        """
        from ai_assistant.utils.api_config_manager import config_manager
        
        # Get all available entities and their synonyms
        detected_entities = self._detect_entities_in_query(query_lower)
        
        
        # If more than 1 entity is mentioned, it's ambiguous
        return len(detected_entities) > 1
    
    def _detect_entities_in_query(self, query_lower: str) -> List[str]:
        """
        Detect which entities are mentioned in the query using config_manager
        
        Args:
            query_lower: Lowercased user query
            
        Returns:
            List of detected entity names
        """
        from ai_assistant.utils.api_config_manager import config_manager
        
        detected_entities = []
        
        try:
            # Get all available entities
            available_entities = config_manager.get_available_entities()
            
            for entity_name in available_entities:
                entity_found = False
                
                # Check if entity name appears in query
                if entity_name.lower() in query_lower:
                    entity_found = True
                
                # Check synonyms
                if not entity_found:
                    try:
                        synonyms = config_manager.get_entity_synonyms(entity_name)
                        for synonym in synonyms:
                            if synonym.lower() in query_lower:
                                entity_found = True
                                break
                    except Exception as e:
                
                if entity_found:
                    detected_entities.append(entity_name)
                    
        except Exception as e:
        
        return detected_entities
    
    def _get_fallback_strategies(self, query_lower: str, max_tools: int) -> List[Dict[str, Any]]:
        """
        Provide multiple invoke_api_tool strategies based on detected entities and their relevance
        
        Args:
            query_lower: Lowercased user query
            max_tools: Maximum tools to return
            
        Returns:
            List of invoke_api_tool instances prioritized by entity relevance scores
        """
        # Get detected entities and score them
        detected_entities = self._detect_entities_in_query(query_lower)
        entity_scores = self._score_entities_for_query(detected_entities, query_lower)
        
        strategies = []
        
        # Create strategies based on entity relevance scores
        # Always include both find_entity_endpoint and invoke_api_tool together
        for i, (entity, score) in enumerate(entity_scores):
            strategy_score = max(100 - (i * 10), 60)  # Decreasing scores: 100, 90, 80, etc.
            
            # Add find_entity_endpoint first (for endpoint discovery)
            strategies.append({
                "name": "find_entity_endpoint", 
                "description": f"Find available endpoints for {entity} entity (Relevance score: {score})",
                "score": strategy_score + 1,  # Slightly higher score to encourage discovery first
                "entity_focus": entity,
                "entity_relevance": score,
                "parameters": self._get_tool_parameters("find_entity_endpoint")
            })
            
            # Add invoke_api_tool second (for actual API calls)
            strategies.append({
                "name": "invoke_api_tool",
                "description": f"Execute API calls for {entity} entity (Relevance score: {score})",
                "score": strategy_score,
                "entity_focus": entity,
                "entity_relevance": score,
                "parameters": self._get_tool_parameters("invoke_api_tool")
            })
        
        # If no entities detected, provide default strategy with both tools
        if not strategies:
            strategies = [
                {
                    "name": "find_entity_endpoint",
                    "description": "Discover available API endpoints",
                    "score": 101,
                    "entity_focus": "auto_detect",
                    "entity_relevance": 50,
                    "parameters": self._get_tool_parameters("find_entity_endpoint")
                },
                {
                    "name": "invoke_api_tool",
                    "description": "Execute API calls with discovered endpoints",
                    "score": 100,
                    "entity_focus": "auto_detect",
                    "entity_relevance": 50,
                    "parameters": self._get_tool_parameters("invoke_api_tool")
                }
            ]
        
        # Limit to requested number - NO exit tool in discovery
        limited_strategies = strategies[:max_tools]
        
        for i, strategy in enumerate(limited_strategies, 1):
            if strategy["name"] == "invoke_api_tool":
                entity_info = strategy.get('entity_focus', 'unknown')
                relevance = strategy.get('entity_relevance', 0)
            else:
        
        return limited_strategies
    
    def _score_entities_for_query(self, detected_entities: List[str], query_lower: str) -> List[tuple]:
        """
        Score detected entities based on their relevance to the query
        
        Args:
            detected_entities: List of entity names found in query
            query_lower: Lowercased user query
            
        Returns:
            List of (entity_name, relevance_score) tuples, sorted by score descending
        """
        from ai_assistant.utils.api_config_manager import config_manager
        
        entity_scores = []
        
        for entity in detected_entities:
            score = 0
            
            # Base score for entity name match
            if entity.lower() in query_lower:
                score += 50
            
            # Additional score for synonym matches
            try:
                synonyms = config_manager.get_entity_synonyms(entity)
                for synonym in synonyms:
                    if synonym.lower() in query_lower:
                        score += 30  # Synonym matches are worth less than direct matches
                        break
            except Exception:
                pass
            
            # Boost score based on query context (simple keyword analysis)
            context_keywords = {
                'partner': ['partner', 'organization', 'company', 'vendor'],
                'engagement': ['engagement', 'project', 'contract', 'agreement'],
                'contact': ['contact', 'person', 'individual', 'representative'],
                'interaction': ['interaction', 'communication', 'meeting', 'call']
            }
            
            entity_lower = entity.lower()
            if entity_lower in context_keywords:
                for keyword in context_keywords[entity_lower]:
                    if keyword in query_lower:
                        score += 20
            
            entity_scores.append((entity, score))
        
        # Sort by score descending (highest relevance first)
        entity_scores.sort(key=lambda x: x[1], reverse=True)
        
        return entity_scores
