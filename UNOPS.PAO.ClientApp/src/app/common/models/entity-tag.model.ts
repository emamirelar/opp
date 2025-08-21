/**
 * Generic entity tag interface that can be used across all entities
 */
export interface EntityTag {
  tag: string;
  color: string;
}

/**
 * Interface for entities that support conditional tags
 */
export interface TaggedEntity {
  tags?: EntityTag[] | null;
}
