from sqlalchemy import Column, Integer, String, LargeBinary
from app.config.database import db

class EntityEmbeddings(db.Model):
    __tablename__ = 'EntityEmbeddings'
    
    id = Column('Id', Integer, primary_key=True)
    entity_name = Column('EntityName', String, nullable=False)
    entity_id = Column('EntityId', Integer, nullable=False)
    entity_data = Column('EntityData', String, nullable=False)
    full_embedding = Column('FullEmbedding', LargeBinary, nullable=False)

    def to_dict(self):
        return {
            'id': self.id,
            'entity_name': self.entity_name,
            'entity_id': self.entity_id,
            'entity_data': self.entity_data
            # Excluding full_embedding as it's binary data
        } 