from sqlalchemy import Column, Integer, String, DateTime
from app.config.database import db
from datetime import datetime

class AiScreenMapping(db.Model):
    __tablename__ = 'AiScreenMapping'
    
    id = Column('Id', Integer, primary_key=True)
    type = Column('Type', String, nullable=False)
    table_name = Column('TableName', String, nullable=False)
    comparison_key = Column('ComparisonKey', String, nullable=False, default='Id')
    related_entity = Column('RelatedEntity', String)
    related_entity_key = Column('RelatedEntityKey', String)
    query_conditions = Column('QueryConditions', String)
    order = Column('Order', Integer, nullable=False)
    created_at = Column('CreatedAt', DateTime, default=datetime.utcnow)

    def to_dict(self):
        return {
            'id': self.id,
            'type': self.type,
            'table_name': self.table_name,
            'comparison_key': self.comparison_key,
            'related_entity': self.related_entity,
            'related_entity_key': self.related_entity_key,
            'query_conditions': self.query_conditions,
            'order': self.order,
            'created_at': self.created_at.isoformat()
        } 