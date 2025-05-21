from app.extensions import db
from datetime import datetime
from uuid import uuid4

class AiChatSession(db.Model):
    __tablename__ = 'AiChatSession'
    __table_args__ = {'schema': 'public'}

    Id = db.Column('Id', db.String(36), primary_key=True, default=lambda: str(uuid4()))
    StartTime = db.Column('StartTime', db.DateTime, nullable=False, default=datetime.utcnow)
    EndTime = db.Column('EndTime', db.DateTime, nullable=True)
    UserId = db.Column('UserId', db.Integer, nullable=False)
    Status = db.Column('Status', db.String(50), nullable=False, default='Active')
    TextToSpeech = db.Column('TextToSpeech', db.Boolean, nullable=False, default=False)
    
    # Relationship with AiChatHistory
    Chats = db.relationship('AiChatHistory', back_populates='Session', lazy='dynamic')

    def to_dict(self):
        return {
            'Id': self.Id,
            'StartTime': self.StartTime.isoformat() if self.StartTime else None,
            'EndTime': self.EndTime.isoformat() if self.EndTime else None,
            'UserId': self.UserId,
            'Status': self.Status,
            'TextToSpeech': self.TextToSpeech
        }

class AiChatHistory(db.Model):
    __tablename__ = 'AiChatHistory'
    __table_args__ = {'schema': 'public'}

    Id = db.Column('Id', db.Integer, primary_key=True)
    Sender = db.Column('Sender', db.String(100), nullable=False)
    Message = db.Column('Message', db.Text, nullable=False)
    RawMessage = db.Column('RawMessage', db.Text, nullable=True)
    TimeStamp = db.Column('TimeStamp', db.DateTime, nullable=False, default=datetime.utcnow)
    Type = db.Column('Type', db.String(50), nullable=False)
    EntityType = db.Column('EntityType', db.String(50), nullable=True)
    RequestType = db.Column('RequestType', db.String(50), nullable=True)
    MediaUrl = db.Column('MediaUrl', db.String(500), nullable=True)
    MediaType = db.Column('MediaType', db.String(50), nullable=True)
    SessionId = db.Column('SessionId', db.String(36), db.ForeignKey('public.AiChatSession.Id'), nullable=True)
    
    # Relationship with AiChatSession
    Session = db.relationship('AiChatSession', back_populates='Chats')

    def to_dict(self):
        return {
            'Id': self.Id,
            'Sender': self.Sender,
            'Message': self.Message,
            'RawMessage': self.RawMessage,
            'TimeStamp': self.TimeStamp.isoformat() if self.TimeStamp else None,
            'Type': self.Type,
            'EntityType': self.EntityType,
            'RequestType': self.RequestType,
            'MediaUrl': self.MediaUrl,
            'MediaType': self.MediaType,
            'SessionId': self.SessionId
        } 