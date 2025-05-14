from app.config.database import db
from app.models.ai_screen_mapping import AiScreenMapping
from app.models.entity_embeddings import EntityEmbeddings
from app.services.vertex_service import get_text_embedding
from sqlalchemy import text
import json
import numpy as np

def get_nested_json(entity_name: str, entity_id: int) -> dict:
    """
    Execute the fn_get_nested_json function and get related entities based on screen mappings
    """
    try:
        # Get screen mappings for this entity
        type_value = f'retrieve_{entity_name.lower().rstrip("s")}_information'
        mappings = get_screen_mappings(type_value)
        
        # Get main entity data
        sql = text("SELECT fn_get_nested_json(:entity_name, :entity_id)")
        result = db.session.execute(sql, {'entity_name': entity_name, 'entity_id': entity_id})
        json_data = result.scalar()
        
        if json_data is None:
            return None
            
        # Convert to dict if it's a string
        if isinstance(json_data, str):
            json_data = json.loads(json_data)
        
        # Process related entities from mappings
        for mapping in sorted(mappings, key=lambda x: x['order']):
            if mapping['related_entity'] and mapping['related_entity_key']:
                # Get the ID of the related entity
                related_id = None
                if mapping['comparison_key'] in json_data:
                    related_id = json_data[mapping['comparison_key']]
                
                if related_id:
                    # Get related entity data
                    sql = text("SELECT fn_get_nested_json(:entity_name, :entity_id)")
                    result = db.session.execute(sql, {
                        'entity_name': mapping['related_entity'],
                        'entity_id': related_id
                    })
                    related_data = result.scalar()
                    
                    if related_data:
                        if isinstance(related_data, str):
                            related_data = json.loads(related_data)
                        # Add related entity data to the main JSON
                        json_data[mapping['related_entity']] = related_data
        
        return json_data
            
    except Exception as e:
        print(f"Error getting nested JSON: {str(e)}")
        raise Exception(f"Error getting nested JSON: {str(e)}")

def format_as_graph_rag(json_data: dict) -> str:
    """
    Format the nested JSON as GraphRAG text with a more structured and readable output
    """
    def get_entity_display_name(entity_data: dict) -> str:
        """Get display name for an entity"""
        if 'Name' in entity_data:
            return entity_data['Name']
        if 'FirstName' in entity_data and 'LastName' in entity_data:
            parts = []
            if entity_data.get('Salutation'):
                parts.append(entity_data['Salutation'])
            parts.append(entity_data['FirstName'])
            if entity_data.get('MiddleName'):
                parts.append(entity_data['MiddleName'])
            parts.append(entity_data['LastName'])
            return ' '.join(filter(None, parts))
        return None

    def format_value(key: str, value: any, entity_names: dict = None) -> str:
        """Format a value based on its type and key"""
        if value is None:
            return "null"
        if isinstance(value, bool):
            return str(value)
        if isinstance(value, (int, str)):
            # Handle ID references
            if key.endswith('Id') and entity_names and str(value) in entity_names:
                return f"{entity_names[str(value)]} (Id: {value})"
            return str(value)
        return str(value)

    def process_entity(data: dict, entity_names: dict) -> list:
        """Process a single entity"""
        lines = []
        for key, value in data.items():
            formatted_value = format_value(key, value, entity_names)
            lines.append(f"- {key}: {formatted_value}")
        return lines

    # Build entity name mapping
    entity_names = {}
    for entity_type, data in json_data.items():
        if isinstance(data, dict):
            name = get_entity_display_name(data)
            if name and 'Id' in data:
                entity_names[str(data['Id'])] = name
        elif isinstance(data, list) and data:
            for item in data:
                if isinstance(item, dict):
                    name = get_entity_display_name(item)
                    if name and 'Id' in item:
                        entity_names[str(item['Id'])] = name

    # Format output
    output_lines = []
    
    # Process Contact
    if 'Contacts' in json_data:
        contact_data = json_data['Contacts']
        contact_name = get_entity_display_name(contact_data)
        output_lines.append(f"Contact: {contact_name} (Id: {contact_data['Id']})")
        output_lines.extend(process_entity(contact_data, entity_names))
        output_lines.append("")

    # Process Partner
    if 'Partners' in json_data and json_data['Partners']:
        partner = json_data['Partners'][0]  # Assuming first partner
        partner_name = get_entity_display_name(partner)
        output_lines.append(f"Partner: {partner_name} (Id: {partner['Id']})")
        output_lines.extend(process_entity(partner, entity_names))
        output_lines.append("")

    # Process Interactions
    if 'Interactions' in json_data and json_data['Interactions']:
        output_lines.append("Interactions:")
        for interaction in json_data['Interactions']:
            contact_name = entity_names.get(str(interaction.get('ContactId')), 'Unknown Contact')
            date = interaction.get('Date', '').split('T')[0]
            output_lines.append(f"Interaction on {date} with {contact_name}")
            output_lines.extend(process_entity(interaction, entity_names))
            output_lines.append("")

    return '\n'.join(output_lines).strip()

def get_screen_mappings(type_value: str) -> list:
    """
    Get screen mappings for a given type
    """
    try:
        mappings = AiScreenMapping.query.filter_by(type=type_value).order_by(AiScreenMapping.order).all()
        return [mapping.to_dict() for mapping in mappings]
    except Exception as e:
        raise Exception(f"Error getting screen mappings: {str(e)}")

def format_entity_name(entity_name: str) -> str:
    """
    Convert entity name to proper casing and pluralization
    """
    # Capitalize first letter
    entity_name = entity_name.capitalize()
    
    # Handle basic pluralization
    if entity_name.endswith('y'):
        return entity_name[:-1] + 'ies'
    elif entity_name.endswith('s'):
        return entity_name
    else:
        return entity_name + 's'

def store_entity_embedding(entity_name: str, entity_id: int, rag_text: str) -> None:
    """
    Create embedding from RAG text and store using InsertEntityEmbedding procedure
    """
    try:
        # Generate embedding using Vertex AI
        embedding_array = get_text_embedding(rag_text)
        
        # Convert numpy array to PostgreSQL array format
        # Format: [0.1,0.2,0.3,...]
        embedding_text = '[' + ','.join(str(x) for x in embedding_array) + ']'
        
        # Ensure text is properly encoded
        entity_name = entity_name.encode('utf-8').decode('utf-8')
        rag_text = rag_text.encode('utf-8').decode('utf-8')
        
        # Call the PostgreSQL procedure
        sql = text('CALL public."InsertEntityEmbedding"(:entity_name, :entity_id, :entity_data, :embedding)')
        
        db.session.execute(
            sql,
            {
                'entity_name': entity_name,
                'entity_id': entity_id,
                'entity_data': rag_text,
                'embedding': embedding_text
            }
        )
        
        db.session.commit()
        print(f"Successfully called InsertEntityEmbedding for {entity_name} ID: {entity_id}")
        print(f"Embedding array shape: {embedding_array.shape}")
        print(f"RAG text size: {len(rag_text)} characters")
        
    except Exception as e:
        db.session.rollback()
        print(f"Error details - Type: {type(e)}, Message: {str(e)}")
        print(f"Embedding array shape: {embedding_array.shape}")
        print(f"RAG text length: {len(rag_text)}")
        raise Exception(f"Error storing entity embedding: {str(e)}")

def generate_rag_text(entity_name: str, entity_id: int) -> str:
    """
    Generate RAG text for an entity and its related entities
    """
    try:
        # Format entity name (e.g., 'contact' -> 'Contacts')
        formatted_entity = format_entity_name(entity_name)
        
        # Get screen mappings for the type
        type_value = f'retrieve_{entity_name}_information'
        mappings = get_screen_mappings(type_value)
        
        if not mappings:
            raise Exception(f'No screen mappings found for type: {type_value}')

        # Get nested JSON data
        json_data = get_nested_json(formatted_entity, entity_id)
        if not json_data:
            raise Exception(f'No data found for {formatted_entity} with ID: {entity_id}')

        # Format as GraphRAG text
        return format_as_graph_rag(json_data)
        
    except Exception as e:
        print(f"Error generating RAG text: {str(e)}")
        raise Exception(f"Error generating RAG text: {str(e)}") 