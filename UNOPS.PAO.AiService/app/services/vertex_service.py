import os
import vertexai
from vertexai.language_models import TextEmbeddingModel
import threading
import time
from concurrent.futures import ThreadPoolExecutor, TimeoutError
import google.auth
import google.auth.transport.requests
from google.auth.exceptions import DefaultCredentialsError
from app.config.settings import PROJECT_ID, LOCATION
import numpy as np

embedding_model = None

# Function to convert embedding to PostgreSQL-compatible string format
# def convert_embedding_to_pg_string(embedding_array):
#     """
#     Convert embedding numpy array to PostgreSQL vector string format.
#     Example: [0.1, 0.2, 0.3] -> '[0.1,0.2,0.3]'
#     
#     Args:
#         embedding_array: List or numpy array of floats
#     Returns:
#         String in PostgreSQL vector format
#     """
#     # Convert to numpy array if not already
#     if not isinstance(embedding_array, np.ndarray):
#         embedding_array = np.array(embedding_array)
#     
#     # Format with specific precision (e.g., 8 decimal places)
#     formatted_values = np.array2string(
#         embedding_array,
#         separator=',',
#         formatter={'float_kind': lambda x: "%.8f" % x}
#     )
#     
#     # Clean up the string (remove spaces and extra brackets)
#     cleaned_string = formatted_values.replace(' ', '').replace('\n', '')
#     
#     return cleaned_string

def check_credentials():
    """Verify Google Cloud credentials are properly set up."""
    try:
        print("Checking Google Cloud credentials...")
        credentials, project = google.auth.default()
        
        if not credentials.valid:
            print("Refreshing credentials...")
            request = google.auth.transport.requests.Request()
            credentials.refresh(request)
        
        print(f"✓ Credentials verified (project: {project})")
        return True
    except DefaultCredentialsError as e:
        print("\nERROR: Google Cloud credentials not found or invalid")
        print("Please ensure you have:")
        print("1. Set up application default credentials")
        print("2. Set GOOGLE_APPLICATION_CREDENTIALS environment variable")
        print(f"Detailed error: {str(e)}")
        return False
    except Exception as e:
        print(f"\nERROR checking credentials: {str(e)}")
        return False

def load_model_with_timeout(timeout_seconds=60):
    """Load the model with a timeout using ThreadPoolExecutor."""
    with ThreadPoolExecutor(max_workers=1) as executor:
        future = executor.submit(TextEmbeddingModel.from_pretrained, "text-embedding-005")
        try:
            return future.result(timeout=timeout_seconds)
        except TimeoutError:
            future.cancel()
            raise Exception(f"Model loading timed out after {timeout_seconds} seconds")
        except Exception as e:
            raise

def init_vertex_ai():
    """Initialize Vertex AI with project and location settings."""
    try:
        if not check_credentials():
            raise Exception("Failed to verify Google Cloud credentials")
        
        vertexai.init(project=PROJECT_ID, location=LOCATION)
        model = load_model_with_timeout(60)
        init_embedding_model(model)
        
    except Exception as e:
        print(f"ERROR during Vertex AI initialization: {str(e)}")
        raise

def init_embedding_model(model):
    """Initialize the embedding model for use in the service."""
    global embedding_model
    embedding_model = model

def get_text_embedding(text):
    """
    Generate text embeddings using Vertex AI's text-embedding model.
    Returns a numpy array of embeddings.
    """
    try:
        print(f"\nDebug - Generating embedding for text: {text[:100]}...")  # First 100 chars
        
        if embedding_model is None:
            raise ValueError("Embedding model not initialized")
        
        # Generate embeddings using the globally initialized model
        embeddings = embedding_model.get_embeddings([text])
        
        if embeddings and len(embeddings) > 0:
            # For text-embedding-005, the values are directly in the embedding object
            values = embeddings[0].values
            print(f"Successfully generated embedding of dimension: {len(values)}")
            
            # Convert to numpy array and ensure float32 type
            embedding_array = np.array(values, dtype=np.float32)
            print(f"Converted to numpy array of shape: {embedding_array.shape}")
            
            return embedding_array
        else:
            raise ValueError("No embedding values returned from the model")
            
    except Exception as e:
        print(f"\nError generating embedding: {str(e)}")
        print(f"Error type: {type(e)}")
        raise Exception(f"Error generating text embedding: {str(e)}") 