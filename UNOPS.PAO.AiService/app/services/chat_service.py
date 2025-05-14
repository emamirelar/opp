from app.extensions import db
from app.models.ai_chat import AiChatSession, AiChatHistory
from datetime import datetime
from typing import List, Optional
import logging

logger = logging.getLogger(__name__)

def get_user_sessions(user_id: int) -> List[dict]:
    """Get all chat sessions for a user."""
    try:
        sessions = AiChatSession.query.filter_by(UserId=user_id).order_by(AiChatSession.StartTime.desc()).all()
        return [session.to_dict() for session in sessions]
    except Exception as e:
        logger.error(f"Error getting user sessions: {str(e)}")
        raise

def get_session_history(session_id: str, user_id: int) -> List[dict]:
    """Get chat history for a specific session."""
    try:
        # Verify session belongs to user
        session = AiChatSession.query.filter_by(Id=session_id, UserId=user_id).first()
        if not session:
            raise ValueError("Session not found or does not belong to user")
            
        # Get chat history
        history = AiChatHistory.query.filter_by(SessionId=session_id).order_by(AiChatHistory.TimeStamp).all()
        return [chat.to_dict() for chat in history]
    except Exception as e:
        logger.error(f"Error getting session history: {str(e)}")
        raise

def create_session(user_id: int) -> dict:
    """Create a new chat session and end existing active sessions."""
    try:
        # End all active sessions for the user
        active_sessions = AiChatSession.query.filter_by(
            UserId=user_id,
            Status='Active',
            EndTime=None
        ).all()
        
        for session in active_sessions:
            session.Status = 'Ended'
            session.EndTime = datetime.utcnow()
        
        # Create new session
        new_session = AiChatSession(UserId=user_id)
        db.session.add(new_session)
        db.session.commit()
        
        return new_session.to_dict()
    except Exception as e:
        db.session.rollback()
        logger.error(f"Error creating session: {str(e)}")
        raise

def end_session(session_id: str) -> dict:
    """End a specific chat session."""
    try:
        session = AiChatSession.query.filter_by(Id=session_id, Status='Active').first()
        if not session:
            raise ValueError("Active session not found")
            
        session.Status = 'Ended'
        session.EndTime = datetime.utcnow()
        db.session.commit()
        
        return session.to_dict()
    except Exception as e:
        db.session.rollback()
        logger.error(f"Error ending session: {str(e)}")
        raise 