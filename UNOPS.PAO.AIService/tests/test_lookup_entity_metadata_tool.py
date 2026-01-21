"""
Tests for the Entity Metadata Lookup Tool.
Tests the get_json_for_entity tool and its helper functions.
"""

import pytest
from ai_assistant.tools.lookup_entity_metadata_tool import (
    get_json_for_entity, 
    _format_single_entity_metadata
)


class TestLookupEntityMetadataTool:
    """Test suite for entity metadata lookup functionality."""

    def test_get_json_for_entity_no_params_returns_summary(self):
        """Test calling with no params returns entity summary."""
        # Act
        result = get_json_for_entity()

        # Assert
        assert result is not None, "Should return summary when no params provided"
        assert len(result) > 0, "Summary should not be empty"
        assert "available" in result.lower() or "entities" in result.lower(), \
            "Summary should mention available entities"

    def test_get_json_for_entity_by_name_returns_entity_details(self):
        """Test looking up entity by name returns formatted metadata."""
        # Arrange
        entity_name = "Opportunity"

        # Act
        result = get_json_for_entity(entity_name=entity_name)

        # Assert
        assert result is not None, "Should return entity details"
        assert "Opportunity" in result, "Result should contain entity name"
        # Check for typical metadata sections
        has_data_model_or_endpoints = (
            "Data Model:" in result or 
            "API Endpoints:" in result or
            "description" in result.lower()
        )
        assert has_data_model_or_endpoints, "Should contain metadata sections"

    def test_get_json_for_entity_by_endpoint_returns_endpoint_details(self):
        """Test looking up entity by endpoint path."""
        # Arrange
        endpoint_path = "/api/opportunity/create"

        # Act
        result = get_json_for_entity(endpoint_path=endpoint_path)

        # Assert
        assert result is not None, "Should return endpoint details"
        assert "/api/opportunity" in result or "opportunity" in result.lower(), \
            "Result should mention opportunity endpoint"

    def test_get_json_for_entity_invalid_name_returns_error(self):
        """Test invalid entity name returns helpful error or empty result."""
        # Arrange
        invalid_name = "NonExistentEntity12345"

        # Act
        result = get_json_for_entity(entity_name=invalid_name)

        # Assert
        assert result is not None, "Should return some result even for invalid entity"
        # Could be error message or empty result
        is_error_or_empty = (
            "not found" in result.lower() or 
            "no entity" in result.lower() or 
            len(result) < 100
        )
        assert is_error_or_empty, "Should indicate entity not found or return minimal result"

    def test_get_json_for_entity_partner_name_returns_partner_details(self):
        """Test looking up Partner entity."""
        # Act
        result = get_json_for_entity(entity_name="Partner")

        # Assert
        assert result is not None, "Should return Partner entity details"
        assert "Partner" in result, "Result should mention Partner"

    def test_get_json_for_entity_contact_name_returns_contact_details(self):
        """Test looking up Contact entity."""
        # Act
        result = get_json_for_entity(entity_name="Contact")

        # Assert
        assert result is not None, "Should return Contact entity details"
        assert "Contact" in result, "Result should mention Contact"

    def test_format_single_entity_includes_description(self):
        """Test metadata formatting includes description."""
        # Arrange
        entity_info = {
            "description": "Test entity description",
            "apiEndpoints": []
        }

        # Act
        result = _format_single_entity_metadata("TestEntity", entity_info)

        # Assert
        assert "Test entity description" in result, "Should include description"
        assert "TestEntity" in result, "Should include entity name"

    def test_format_single_entity_includes_data_model(self):
        """Test metadata formatting includes data model fields."""
        # Arrange
        entity_info = {
            "description": "Test entity",
            "dataModel": {
                "fields": [
                    {
                        "name": "id",
                        "dataType": "number",
                        "required": True,
                        "description": "Unique identifier"
                    },
                    {
                        "name": "name",
                        "dataType": "string",
                        "required": True,
                        "description": "Entity name"
                    }
                ]
            }
        }

        # Act
        result = _format_single_entity_metadata("TestEntity", entity_info)

        # Assert
        assert "Data Model:" in result, "Should include data model section"
        assert "id" in result, "Should include field names"
        assert "name" in result, "Should include all fields"
        assert "required" in result.lower(), "Should indicate required fields"

    def test_format_single_entity_includes_api_endpoints(self):
        """Test metadata formatting includes API endpoints."""
        # Arrange
        entity_info = {
            "description": "Test entity",
            "apiEndpoints": [
                {
                    "endpoint": "/api/test/create",
                    "method": "POST",
                    "description": "Create a new test entity"
                },
                {
                    "endpoint": "/api/test/{id}",
                    "method": "GET",
                    "description": "Get test entity by ID"
                }
            ]
        }

        # Act
        result = _format_single_entity_metadata("TestEntity", entity_info)

        # Assert
        assert "API Endpoints:" in result, "Should include API endpoints section"
        assert "/api/test/create" in result, "Should include create endpoint"
        assert "/api/test/{id}" in result, "Should include get endpoint"
        assert "POST" in result, "Should include HTTP methods"
        assert "GET" in result, "Should include all HTTP methods"
