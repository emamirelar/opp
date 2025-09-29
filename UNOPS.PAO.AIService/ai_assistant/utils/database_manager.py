"""
Database Manager for AI Action Logging

This module provides utilities for managing custom database tables,
particularly for the AI Action Logging feature.
"""

import logging
from typing import Optional
from sqlalchemy import create_engine, inspect
from sqlalchemy.orm import sessionmaker
from sqlalchemy.exc import SQLAlchemyError

from ai_assistant.utils.config import get_database_url

logger = logging.getLogger(__name__)


class DatabaseManager:
    """
    Database manager for handling custom tables and operations
    separate from the ADK session management.
    """
    
    def __init__(self, database_url: Optional[str] = None):
        """
        Initialize the database manager.
        
        Args:
            database_url: Optional database URL. If not provided, will use config manager.
        """
        self.database_url = database_url or get_database_url()
        self.engine = None
        self.SessionLocal = None
        self._initialized = False
    
    def initialize(self):
        """Initialize the database engine and session factory."""
        try:
            self.engine = create_engine(self.database_url)
            self.SessionLocal = sessionmaker(autocommit=False, autoflush=False, bind=self.engine)
            self._initialized = True
            logger.info("✅ Database manager initialized successfully")
        except Exception as e:
            logger.error(f"❌ Failed to initialize database manager: {e}")
            raise
    
    def create_action_log_table_if_not_exists(self):
        """
        Create the AI Action Log table if it doesn't exist.
        
        Returns:
            bool: True if table was created or already exists, False if creation failed.
        """
        if not self._initialized:
            self.initialize()
        
        try:
            # Check if table exists
            inspector = inspect(self.engine)
            table_exists = inspector.has_table(AiActionLog.__tablename__)
            
            if table_exists:
                logger.info(f"✅ Table '{AiActionLog.__tablename__}' already exists")
                return True
            
            # Create the table
            logger.info(f"🔨 Creating table '{AiActionLog.__tablename__}'...")
            Base.metadata.create_all(bind=self.engine, tables=[AiActionLog.__table__])
            logger.info(f"✅ Table '{AiActionLog.__tablename__}' created successfully")
            return True
            
        except SQLAlchemyError as e:
            logger.error(f"❌ Database error while creating table: {e}")
            return False
        except Exception as e:
            logger.error(f"❌ Unexpected error while creating table: {e}")
            return False
    
    def get_session(self):
        """
        Get a database session for performing operations.
        
        Returns:
            Session: SQLAlchemy session object
        """
        if not self._initialized:
            self.initialize()
        
        return self.SessionLocal()
    
    def insert_action_log(self, summary: str, user_id: int, created_by: int, 
                         entity_type: Optional[str] = None, entity_id: Optional[int] = None,
                         session_id: Optional[str] = None, changes_log: Optional[dict] = None):
        """
        Insert a new action log entry.
        
        Args:
            summary: Brief summary of the action
            user_id: ID of the user who performed the action
            created_by: ID of the user who created this log entry
            entity_type: Optional entity type involved
            entity_id: Optional entity ID
            session_id: Optional session ID
            changes_log: Optional detailed changes as dictionary
            
        Returns:
            bool: True if insertion was successful, False otherwise
        """
        try:
            session = self.get_session()
            
            action_log = AiActionLog(
                summary=summary,
                entity_type=entity_type,
                entity_id=entity_id,
                session_id=session_id,
                changes_log=changes_log,
                user_id=user_id,
                created_by=created_by
            )
            
            session.add(action_log)
            session.commit()
            session.close()
            
            logger.info(f"✅ Action log inserted successfully for user {user_id}")
            return True
            
        except SQLAlchemyError as e:
            logger.error(f"❌ Database error while inserting action log: {e}")
            if session:
                session.rollback()
                session.close()
            return False
        except Exception as e:
            logger.error(f"❌ Unexpected error while inserting action log: {e}")
            if session:
                session.rollback()
                session.close()
            return False
    
    def get_recent_actions_for_user(self, user_id: int, limit: int = 5):
        """
        Get recent action logs for a specific user.
        
        Args:
            user_id: User ID to get actions for
            limit: Maximum number of actions to retrieve
            
        Returns:
            list: List of AiActionLog objects, ordered by created_date desc
        """
        try:
            session = self.get_session()
            
            actions = session.query(AiActionLog)\
                           .filter(AiActionLog.user_id == user_id)\
                           .order_by(AiActionLog.created_date.desc())\
                           .limit(limit)\
                           .all()
            
            session.close()
            return actions
            
        except SQLAlchemyError as e:
            logger.error(f"❌ Database error while retrieving actions: {e}")
            if session:
                session.close()
            return []
        except Exception as e:
            logger.error(f"❌ Unexpected error while retrieving actions: {e}")
            if session:
                session.close()
            return []


# Global database manager instance
db_manager = DatabaseManager()