from app.extensions import db
from datetime import datetime

class AiPrompt(db.Model):
    __tablename__ = 'AiPrompt'
    __table_args__ = {'schema': 'public'}
    
    Id = db.Column('Id', db.Integer, primary_key=True)
    Type = db.Column('Type', db.String(100), nullable=False)
    Prompt = db.Column('Prompt', db.Text, nullable=True)
    CreatedAt = db.Column('CreatedAt', db.DateTime, nullable=False, default=datetime.utcnow)
    GenerationConfig = db.Column('GenerationConfig', db.String, nullable=False)
    ContentConfig = db.Column('ContentConfig', db.String, nullable=False)
    ToolsConfig = db.Column('ToolsConfig', db.String, nullable=True)
    SafetySettings = db.Column('SafetySettings', db.String, nullable=True)
    Project = db.Column('Project', db.String, nullable=False)
    Location = db.Column('Location', db.String, nullable=False)
    Model = db.Column('Model', db.String, nullable=False)

    def to_dict(self):
        return {
            'Id': self.Id,
            'Type': self.Type,
            'Prompt': self.Prompt,
            'CreatedAt': self.CreatedAt.isoformat() if self.CreatedAt else None,
            'GenerationConfig': self.GenerationConfig,
            'ContentConfig': self.ContentConfig,
            'ToolsConfig': self.ToolsConfig,
            'SafetySettings': self.SafetySettings,
            'Project': self.Project,
            'Location': self.Location,
            'Model': self.Model
        } 