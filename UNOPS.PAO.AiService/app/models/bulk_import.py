from pydantic import BaseModel
from typing import List, Optional, Any
from datetime import datetime

class BulkImportRequest(BaseModel):
    batch_data: str
    user_id: str
    entity_name: str
    prompt_type: Optional[str] = None

class ProcessingResult(BaseModel):
    success: bool
    message: str
    processed_records: int
    total_records: int
    results: Optional[List[Any]] = None
    error: Optional[str] = None 