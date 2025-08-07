"""
AI Action Log Database Model

This module defines the database model for logging AI interactions and actions.
"""

from datetime import datetime
from typing import Optional
from sqlalchemy import Column, Integer, String, Text, DateTime, JSON
from sqlalchemy.ext.declarative import declarative_base
from sqlalchemy.sql import func

Base = declarative_base()


class AiActionLog(Base):
    """
    AI Action Log model for tracking user interactions with the AI system.
    
    This table logs all successful AI interactions to provide analytics,
    user behavior insights, and generate personalized suggestions.
    """
    __tablename__ = "ai_action_log"

    id = Column(Integer, primary_key=True, autoincrement=True)
    summary = Column(String(500), nullable=False, comment="Brief summary of the action/interaction")
    entity_type = Column(String(100), nullable=True, comment="Type of entity involved in the action")
    entity_id = Column(Integer, nullable=True, comment="ID of the specific entity")
    session_id = Column(String(100), nullable=True, comment="Session ID for tracking conversation flow")
    changes_log = Column(JSON, nullable=True, comment="JSON field containing detailed changes/actions performed")
    user_id = Column(Integer, nullable=False, comment="ID of the user who performed the action")
    last_update = Column(DateTime, nullable=False, default=datetime.utcnow, onupdate=func.now(), comment="Last update timestamp")
    created_date = Column(DateTime, nullable=False, default=datetime.utcnow, comment="Creation timestamp")
    created_by = Column(Integer, nullable=False, comment="ID of the user who created this log entry")

    def __repr__(self):
        return f"<AiActionLog(id={self.id}, user_id={self.user_id}, summary='{self.summary[:50]}...', created_date={self.created_date})>"

    def to_dict(self):
        """Convert the model to a dictionary for JSON serialization."""
        return {
            'id': self.id,
            'summary': self.summary,
            'entity_type': self.entity_type,
            'entity_id': self.entity_id,
            'session_id': self.session_id,
            'changes_log': self.changes_log,
            'user_id': self.user_id,
            'last_update': self.last_update.isoformat() if self.last_update else None,
            'created_date': self.created_date.isoformat() if self.created_date else None,
            'created_by': self.created_by
        }