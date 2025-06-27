#!/usr/bin/env python3
"""
Knowledge Manager for RAG-based information retrieval.
Supports multiple file formats: JSON, TXT, PDF, DOCX, Google Docs
"""

import json
import os
from typing import Dict, List, Any
from functools import lru_cache
import re


class KnowledgeManager:
    """Manages knowledge base from text file source"""
    
    def __init__(self, config_path: str = 'config/knowledge_base.txt'):
        self.config_path = config_path
        self.knowledge_chunks = []
        self.loaded = False
        
    def _load_text_file(self, file_path: str) -> str:
        """Load content from text file"""
        if not os.path.exists(file_path):
            print(f"Warning: Knowledge base file {file_path} not found")
            return ""
            
        try:
            with open(file_path, 'r', encoding='utf-8') as f:
                return f.read()
        except Exception as e:
            print(f"Error reading knowledge base file: {e}")
            return ""
    
    def load_all_knowledge(self) -> None:
        """Load all knowledge from the text file"""
        if self.loaded:
            return
            
        content = self._load_text_file(self.config_path)
        
        # Split content into paragraphs (chunks separated by blank lines)
        paragraphs = [p.strip() for p in content.split('\n\n') if p.strip()]
        
        # Store each paragraph as a knowledge chunk
        for para in paragraphs:
            self.knowledge_chunks.append({
                'text': para,
                'source': self.config_path,
                'type': 'text'
            })
        
        self.loaded = True
        print(f"✅ Loaded {len(self.knowledge_chunks)} knowledge chunks from {self.config_path}")
    
    def search_knowledge(self, query: str, max_results: int = 3) -> List[Dict[str, str]]:
        """Search knowledge base for relevant information"""
        if not self.knowledge_chunks:
            self.load_all_knowledge()
        
        print(f"🔍 Searching knowledge base for: '{query}'")
        
        query_lower = query.lower()
        query_words = set(query_lower.split())
        
        # Score each chunk based on keyword matching
        scored_chunks = []
        
        for chunk in self.knowledge_chunks:
            text_lower = chunk['text'].lower()
            chunk_words = set(text_lower.split())
            
            # Calculate relevance score
            score = 0
            
            # Exact phrase match (high score)
            if query_lower in text_lower:
                score += 10
            
            # Word overlap score
            common_words = query_words.intersection(chunk_words)
            score += len(common_words) * 2
            
            # Longer matches might be more relevant
            score += len(common_words) / len(chunk_words) * 5
            
            if score > 0:
                scored_chunks.append((score, chunk))
        
        # Sort by score and return top results
        scored_chunks.sort(key=lambda x: x[0], reverse=True)
        results = [chunk for score, chunk in scored_chunks[:max_results]]
        
        print(f"✅ Found {len(results)} relevant knowledge chunks")
        return results
    
    def get_all_faqs(self) -> List[Dict[str, str]]:
        """Get all FAQ entries"""
        if not self.knowledge_chunks:
            self.load_all_knowledge()
        
        return [chunk for chunk in self.knowledge_chunks if chunk.get('type') == 'faq']
    
    def get_all_concepts(self) -> List[Dict[str, str]]:
        """Get all concept entries"""
        if not self.knowledge_chunks:
            self.load_all_knowledge()
        
        return [chunk for chunk in self.knowledge_chunks if chunk.get('type') == 'concept']

    def get_knowledge_base_content(self) -> str:
        """Get the full content of the primary knowledge base for global instructions"""
        try:
            # Try to load the primary knowledge base file directly
            knowledge_base_path = "config/knowledge_base.txt"
            if os.path.exists(knowledge_base_path):
                with open(knowledge_base_path, 'r', encoding='utf-8') as f:
                    return f.read()
            else:
                print(f"⚠️ Primary knowledge base not found: {knowledge_base_path}")
                return ""
        except Exception as e:
            print(f"❌ Error loading knowledge base: {e}")
            return ""

    def get_essential_knowledge_summary(self) -> str:
        """Get a condensed summary of essential knowledge for global instructions"""
        if not self.knowledge_chunks:
            self.load_all_knowledge()
        
        # Get high-priority chunks from the primary knowledge base
        essential_chunks = []
        for chunk in self.knowledge_chunks:
            if 'knowledge_base.txt' in chunk.get('source', ''):
                essential_chunks.append(chunk['text'])
        
        if essential_chunks:
            return "\n\n".join(essential_chunks[:10])  # Limit to first 10 chunks
        else:
            return "No essential knowledge available."

    def create_knowledge_enhanced_instruction(self, base_instruction: str) -> str:
        """Create a knowledge-enhanced global instruction by combining base instruction with knowledge base"""
        knowledge_content = self.get_knowledge_base_content()
        
        if not knowledge_content:
            return base_instruction
        
        enhanced_instruction = f"""{base_instruction}

**KNOWLEDGE BASE INFORMATION:**

{knowledge_content}

**IMPORTANT:** Use the above knowledge base information to:
- Understand user roles and their specific permissions
- Provide accurate information about system features and capabilities
- Tailor responses based on organizational units and access control
- Reference specific project context when answering questions
- Ensure compliance with security and permission guidelines

When answering questions, always consider:
1. The user's role and associated permissions
2. Their organizational unit restrictions (if applicable)
3. The current page context and available actions
4. Relevant system features and capabilities from the knowledge base
"""
        
        return enhanced_instruction

    def get_role_specific_knowledge(self, user_roles: List[str]) -> str:
        """Get knowledge specific to user roles"""
        if not self.knowledge_chunks:
            self.load_all_knowledge()
        
        role_knowledge = []
        
        for chunk in self.knowledge_chunks:
            chunk_text = chunk['text'].lower()
            # Check if chunk contains information about any of the user's roles
            for role in user_roles:
                if role.lower() in chunk_text:
                    role_knowledge.append(chunk['text'])
                    break
        
        return "\n\n".join(role_knowledge) if role_knowledge else ""


# Global instance (loaded once)
knowledge_manager = KnowledgeManager() 