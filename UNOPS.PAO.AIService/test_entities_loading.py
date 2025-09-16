#!/usr/bin/env python3
"""
Test script to verify entities loading from api_config_manager in UNOPS.PAO.AIService
"""

from ai_assistant.utils.api_config_manager import config_manager
from ai_assistant.task_executor_agent import get_task_executor_instruction

def test_entities_loading():
    print("🧪 Testing entities loading from api_config_manager...")
    
    print("\n📋 Available entities:")
    available_entities = config_manager.get_available_entities()
    for entity in available_entities:
        print(f"  - {entity}")
    
    print(f"\n📊 Total entities discovered: {len(available_entities)}")
    
    print("\n🔍 Getting entities with full configurations:")
    entities = config_manager.get_entities()
    print(f"📊 Entities with configurations: {len(entities)}")
    
    for entity in entities[:5]:  # Show first 5 entities
        name = entity.get('entity', 'Unknown')
        desc = entity.get('description', 'No description')[:60] + "..."
        synonyms = entity.get('synonyms', [])
        print(f"  • {name}: {desc}")
        if synonyms:
            print(f"    Synonyms: {', '.join(synonyms[:3])}{'...' if len(synonyms) > 3 else ''}")
    
    print("\n📝 Formatted entities information for TaskExecutorAgent:")
    entities_info = get_task_executor_instruction()
    print(entities_info[:500] + "..." if len(entities_info) > 500 else entities_info)

if __name__ == "__main__":
    test_entities_loading()
