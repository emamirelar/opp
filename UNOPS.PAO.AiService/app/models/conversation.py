from datetime import datetime
from app.config.database import db

class Conversation(db.Model):
    __tablename__ = 'conversations'

    id = db.Column(db.Integer, primary_key=True)
    prompt = db.Column(db.Text, nullable=False)
    response = db.Column(db.Text, nullable=False)
    created_at = db.Column(db.DateTime, default=datetime.utcnow)

    def __init__(self, prompt, response):
        self.prompt = prompt
        self.response = response

    def to_dict(self):
        return {
            'id': self.id,
            'prompt': self.prompt,
            'response': self.response,
            'created_at': self.created_at.isoformat()
        } 