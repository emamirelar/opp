from app.extensions import db
from sqlalchemy import text
from typing import List, Dict, Any
import json

def retrieve_similarity_results(entity_name: str, search_text: str, limit: int = 5, embedding: str = None, extra_where: str = None) -> List[Dict[str, Any]]:
    """
    Get similar entities:
    1. Get entityIds from similarity search
    2. Get EntityData for those IDs
    """
    try:
        print(f"\n=== Starting similarity search ===")
        print(f"Entity name: {entity_name}")
        print(f"Search text: {search_text}")
        print(f"Limit: {limit}")
        print(f"Has embedding: {embedding is not None}")
        print(f"Extra where: {extra_where}")
        
        # Step 1: Get entityIds from similarity search
        similarity_query = text("""
            SELECT entityid, score 
            FROM public.retrieve_similarity_results(
                :entity_name, 
                :search_text, 
                :embedding,
                :limit,
                :extra_where
            )
        """)
        
        print(f"\nExecuting similarity query with parameters:")
        print(f"entity_name: {entity_name}")
        print(f"search_text: {search_text}")
        print(f"embedding: {embedding}")
        print(f"limit: {limit}")
        print(f"extra_where: {extra_where}")
        
        similarity_result = db.session.execute(
            similarity_query,
            {
                "entity_name": entity_name,
                "search_text": search_text,
                "embedding": embedding,
                "limit": limit,
                "extra_where": extra_where
            }
        )
        
        # Get the entityIds
        entity_ids = [row.entityid for row in similarity_result]
        print(f"\nFound entity IDs from similarity search: {entity_ids}")
        
        if not entity_ids:
            print("No entity IDs found from similarity search")
            return []
            
        # Step 2: Get EntityData for these IDs
        data_query = text("""
            SELECT "EntityData" 
            FROM public."EntityEmbeddings" 
            WHERE "EntityId" IN :entity_ids
            AND "EntityName" = :entity_name
        """)
        
        print(f"\nExecuting data query for entity IDs: {entity_ids}")
        
        data_result = db.session.execute(
            data_query,
            {
                "entity_ids": tuple(entity_ids),
                "entity_name": entity_name
            }
        )
        
        # Get and debug print the EntityData values
        results = [row.EntityData for row in data_result]
        print("\nRetrieved EntityData values:")
        for idx, data in enumerate(results, 1):
            print(f"\nEntity {idx}:")
            print(json.dumps(data, indent=2))
            
        print(f"\nTotal results found: {len(results)}")
        return results
        
    except Exception as e:
        print(f"Error in similarity search: {str(e)}")
        print(f"Error type: {type(e)}")
        print(f"Error details: {e.__dict__}")
        return [] 