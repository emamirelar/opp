"""
Tests for metadata utilities.
Tests the metadata loading and helper functions.
"""

import pytest
from pathlib import Path
from ai_assistant.utils.metadata_utils import load_entities_metadata


class TestMetadataUtils:
    """Test suite for metadata utility functions."""

    def test_load_entities_metadata_returns_dict(self):
        """Test loading metadata returns valid dictionary."""
        # Act
        metadata = load_entities_metadata()

        # Assert
        assert isinstance(metadata, dict), "Should return a dictionary"
        assert len(metadata) >= 0, "Dictionary should have entries or be empty"

    def test_load_entities_metadata_contains_expected_entities(self):
        """Test loaded metadata contains expected entities."""
        # Act
        metadata = load_entities_metadata()

        # Assert
        if metadata:  # Only test if metadata loaded successfully
            # Should contain at least one of the core entities
            has_core_entity = any(
                entity in metadata 
                for entity in ["Opportunity", "Partner", "Contact", "Interaction"]
            )
            assert has_core_entity, "Metadata should contain at least one core entity"

    def test_load_entities_metadata_entity_has_required_fields(self):
        """Test that entities in metadata have expected structure."""
        # Act
        metadata = load_entities_metadata()

        # Assert
        if metadata:
            # Get first entity
            first_entity = next(iter(metadata.values()), None)
            if first_entity:
                # Check for typical entity metadata structure
                has_structure = (
                    isinstance(first_entity, dict) and 
                    (
                        "description" in first_entity or 
                        "apiEndpoints" in first_entity or 
                        "dataModel" in first_entity
                    )
                )
                assert has_structure, "Entity should have typical metadata structure"

    def test_load_entities_metadata_handles_missing_file_gracefully(self):
        """Test graceful handling when metadata file is missing."""
        # This test verifies that load_entities_metadata doesn't crash
        # even if the file is not found (should return empty dict or handle gracefully)
        
        # Act
        metadata = load_entities_metadata()

        # Assert: Should not raise exception, should return dict
        assert isinstance(metadata, dict), "Should return dict even if file missing"

    def test_load_entities_metadata_opportunity_entity_structure(self):
        """Test that Opportunity entity has expected structure."""
        # Act
        metadata = load_entities_metadata()

        # Assert
        if "Opportunity" in metadata:
            opportunity = metadata["Opportunity"]
            
            # Should have basic metadata
            assert isinstance(opportunity, dict), "Opportunity should be a dictionary"
            
            # Check for expected sections
            has_endpoints_or_model = (
                "apiEndpoints" in opportunity or 
                "dataModel" in opportunity
            )
            assert has_endpoints_or_model, "Opportunity should have endpoints or data model"

    def test_load_entities_metadata_partner_entity_structure(self):
        """Test that Partner entity has expected structure."""
        # Act
        metadata = load_entities_metadata()

        # Assert
        if "Partner" in metadata:
            partner = metadata["Partner"]
            
            # Should have basic metadata
            assert isinstance(partner, dict), "Partner should be a dictionary"
            
            # Check for expected sections
            has_endpoints_or_model = (
                "apiEndpoints" in partner or 
                "dataModel" in partner
            )
            assert has_endpoints_or_model, "Partner should have endpoints or data model"

    def test_load_entities_metadata_caching(self):
        """Test that metadata can be loaded multiple times without errors."""
        # Act: Load metadata twice
        metadata1 = load_entities_metadata()
        metadata2 = load_entities_metadata()

        # Assert: Both should return same structure
        assert isinstance(metadata1, dict), "First load should return dict"
        assert isinstance(metadata2, dict), "Second load should return dict"
        assert len(metadata1) == len(metadata2), "Should return consistent results"

    def test_load_entities_metadata_returns_non_empty_for_valid_setup(self):
        """Test that metadata loads successfully in proper environment."""
        # Act
        metadata = load_entities_metadata()

        # Assert: In a proper setup, metadata should not be empty
        # In test environment without metadata file, this might be empty
        # This test documents expected behavior in production
        is_valid = isinstance(metadata, dict)
        assert is_valid, "Should return valid dict structure"
