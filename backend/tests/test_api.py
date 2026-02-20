"""
Tests for API endpoints and process_supply_chain_order function.

Tests the public API interface exposed by the autogen module.
"""

import pytest
from unittest.mock import patch, Mock
import asyncio
from datetime import datetime
from app.autogen import (
    process_supply_chain_order,
    get_agent_system,
    OrderDetails,
    SupplyChainResult,
)


class TestGetAgentSystem:
    """Tests for get_agent_system function."""

    def test_get_agent_system_returns_mock_by_default(self):
        """Test that get_agent_system returns mock system by default."""
        system = get_agent_system()
        from app.autogen import MockSupplyChainAgentSystem
        assert isinstance(system, MockSupplyChainAgentSystem)

    def test_get_agent_system_returns_singleton(self):
        """Test that get_agent_system returns the same instance (singleton)."""
        system1 = get_agent_system()
        system2 = get_agent_system()
        assert system1 is system2

    def test_get_agent_system_with_mock_true(self):
        """Test explicitly requesting mock system."""
        system = get_agent_system(use_mock=True)
        from app.autogen import MockSupplyChainAgentSystem
        assert isinstance(system, MockSupplyChainAgentSystem)

    def test_get_agent_system_with_mock_false(self):
        """Test requesting real agent system."""
        # Reset global first
        import app.autogen as autogen_module
        autogen_module._agent_system = None

        system = get_agent_system(use_mock=False)
        from app.autogen import SupplyChainAgentSystem
        assert isinstance(system, SupplyChainAgentSystem)

    def test_get_agent_system_caching(self):
        """Test that agent system is cached and reused."""
        import app.autogen as autogen_module
        autogen_module._agent_system = None

        system1 = get_agent_system(use_mock=True)
        system2 = get_agent_system(use_mock=False)  # Should return cached mock

        # Should return the cached instance regardless of use_mock parameter
        assert system1 is system2


class TestProcessSupplyChainOrder:
    """Tests for process_supply_chain_order function."""

    @pytest.mark.asyncio
    async def test_process_order_with_complete_data(self):
        """Test processing order with complete data."""
        order_data = {
            "order_id": "ORD-API-001",
            "product_name": "API Test Product",
            "quantity": 50,
            "priority": "high",
            "destination": "API Warehouse",
            "required_date": "2026-03-15",
            "notes": "API test notes"
        }

        result = await process_supply_chain_order(order_data)

        assert result["order_id"] == "ORD-API-001"
        assert result["status"] in ["CONFIRMED", "PENDING"]
        assert "timestamp" in result
        assert "supplier" in result
        assert "inventory" in result
        assert "logistics" in result
        assert "coordinator_summary" in result
        assert "recommendations" in result

    @pytest.mark.asyncio
    async def test_process_order_with_minimal_data(self, minimal_order_dict):
        """Test processing order with minimal required data."""
        result = await process_supply_chain_order(minimal_order_dict)

        assert result["order_id"] is not None  # Auto-generated
        assert result["status"] in ["CONFIRMED", "PENDING"]
        assert result["supplier"] is not None
        assert result["inventory"] is not None
        assert result["logistics"] is not None

    @pytest.mark.asyncio
    async def test_process_order_generates_order_id(self):
        """Test that order_id is generated if not provided."""
        order_data = {
            "product_name": "No ID Product",
            "quantity": 10
        }

        result = await process_supply_chain_order(order_data)

        assert result["order_id"] is not None
        assert result["order_id"].startswith("ORD-")

    @pytest.mark.asyncio
    async def test_process_order_uses_provided_order_id(self):
        """Test that provided order_id is used."""
        order_data = {
            "order_id": "CUSTOM-ID-123",
            "product_name": "Custom ID Product",
            "quantity": 10
        }

        result = await process_supply_chain_order(order_data)

        assert result["order_id"] == "CUSTOM-ID-123"

    @pytest.mark.asyncio
    async def test_process_order_default_priority(self):
        """Test that default priority is 'normal'."""
        order_data = {
            "product_name": "Default Priority Product",
            "quantity": 10
        }

        result = await process_supply_chain_order(order_data)

        # Check logistics for standard shipping (normal priority)
        assert result["logistics"]["shipping_method"] == "standard"

    @pytest.mark.asyncio
    async def test_process_order_default_destination(self):
        """Test that default destination is empty string."""
        order_data = {
            "product_name": "No Destination Product",
            "quantity": 10
        }

        result = await process_supply_chain_order(order_data)

        # Coordinator summary should mention default destination
        assert "default destination" in result["coordinator_summary"]

    @pytest.mark.asyncio
    async def test_process_order_supplier_response_structure(self):
        """Test that supplier response has correct structure."""
        order_data = {
            "product_name": "Supplier Test Product",
            "quantity": 25
        }

        result = await process_supply_chain_order(order_data)

        supplier = result["supplier"]
        assert "available" in supplier
        assert "supplier_id" in supplier
        assert "unit_price" in supplier
        assert "lead_time_days" in supplier
        assert "capacity" in supplier
        assert "notes" in supplier

    @pytest.mark.asyncio
    async def test_process_order_inventory_response_structure(self):
        """Test that inventory response has correct structure."""
        order_data = {
            "product_name": "Inventory Test Product",
            "quantity": 25
        }

        result = await process_supply_chain_order(order_data)

        inventory = result["inventory"]
        assert "in_stock" in inventory
        assert "reserved" in inventory
        assert "available" in inventory
        assert "reorder_point" in inventory
        assert "warehouse_location" in inventory
        assert "last_updated" in inventory

    @pytest.mark.asyncio
    async def test_process_order_logistics_response_structure(self):
        """Test that logistics response has correct structure."""
        order_data = {
            "product_name": "Logistics Test Product",
            "quantity": 25
        }

        result = await process_supply_chain_order(order_data)

        logistics = result["logistics"]
        assert "carrier" in logistics
        assert "shipping_cost" in logistics
        assert "estimated_delivery" in logistics
        assert "shipping_method" in logistics

    @pytest.mark.asyncio
    async def test_process_order_with_urgent_priority(self):
        """Test processing urgent priority order."""
        order_data = {
            "product_name": "Urgent Product",
            "quantity": 50,
            "priority": "urgent"
        }

        result = await process_supply_chain_order(order_data)

        assert result["logistics"]["shipping_method"] == "overnight"
        assert result["logistics"]["shipping_cost"] == 75.00

    @pytest.mark.asyncio
    async def test_process_order_with_high_priority(self):
        """Test processing high priority order."""
        order_data = {
            "product_name": "High Priority Product",
            "quantity": 50,
            "priority": "high"
        }

        result = await process_supply_chain_order(order_data)

        assert result["logistics"]["shipping_method"] == "express"
        assert result["logistics"]["shipping_cost"] == 45.00

    @pytest.mark.asyncio
    async def test_process_order_timestamp_format(self):
        """Test that timestamp is in ISO format."""
        order_data = {
            "product_name": "Timestamp Test Product",
            "quantity": 10
        }

        result = await process_supply_chain_order(order_data)

        # Should be able to parse as datetime
        timestamp = result["timestamp"]
        parsed = datetime.fromisoformat(timestamp.replace('Z', '+00:00'))
        assert parsed is not None


class TestProcessSupplyChainOrderEdgeCases:
    """Tests for edge cases in process_supply_chain_order."""

    @pytest.mark.asyncio
    async def test_process_order_with_zero_quantity(self):
        """Test processing order with zero quantity."""
        order_data = {
            "product_name": "Zero Quantity Product",
            "quantity": 0
        }

        result = await process_supply_chain_order(order_data)

        assert result["order_id"] is not None
        assert "supplier" in result

    @pytest.mark.asyncio
    async def test_process_order_with_negative_quantity(self):
        """Test processing order with negative quantity."""
        order_data = {
            "product_name": "Negative Quantity Product",
            "quantity": -10
        }

        # Should handle gracefully
        result = await process_supply_chain_order(order_data)
        assert result is not None

    @pytest.mark.asyncio
    async def test_process_order_with_very_large_quantity(self):
        """Test processing order with very large quantity."""
        order_data = {
            "product_name": "Bulk Product",
            "quantity": 10000
        }

        result = await process_supply_chain_order(order_data)

        assert result["status"] == "PENDING"  # Should be pending due to stock shortage
        assert len(result["recommendations"]) > 0

    @pytest.mark.asyncio
    async def test_process_order_with_empty_product_name(self):
        """Test processing order with empty product name."""
        order_data = {
            "product_name": "",
            "quantity": 10
        }

        result = await process_supply_chain_order(order_data)

        assert result["order_id"] is not None

    @pytest.mark.asyncio
    async def test_process_order_with_special_characters(self):
        """Test processing order with special characters in fields."""
        order_data = {
            "order_id": "ORD-SPEC-@#$%",
            "product_name": "Special Chars !@#$% Product",
            "quantity": 10,
            "notes": "Notes with special chars: <>&\"'"
        }

        result = await process_supply_chain_order(order_data)

        assert result["order_id"] == "ORD-SPEC-@#$%"


class TestAPIResponseConsistency:
    """Tests for API response consistency and contract."""

    @pytest.mark.asyncio
    async def test_response_has_all_required_top_level_fields(self):
        """Test that response has all required top-level fields."""
        order_data = {
            "product_name": "Field Test Product",
            "quantity": 10
        }

        result = await process_supply_chain_order(order_data)

        required_fields = [
            "order_id",
            "status",
            "timestamp",
            "supplier",
            "inventory",
            "logistics",
            "coordinator_summary",
            "recommendations"
        ]

        for field in required_fields:
            assert field in result, f"Missing required field: {field}"

    @pytest.mark.asyncio
    async def test_supplier_fields_not_none_when_available(self):
        """Test supplier fields are populated when available."""
        order_data = {
            "product_name": "Available Product",
            "quantity": 10
        }

        result = await process_supply_chain_order(order_data)

        supplier = result["supplier"]
        if supplier["available"]:
            assert supplier["supplier_id"] is not None
            assert supplier["unit_price"] is not None
            assert supplier["lead_time_days"] is not None

    @pytest.mark.asyncio
    async def test_recommendations_is_list(self):
        """Test that recommendations is always a list."""
        order_data = {
            "product_name": "Recommendation Test Product",
            "quantity": 10
        }

        result = await process_supply_chain_order(order_data)

        assert isinstance(result["recommendations"], list)

    @pytest.mark.asyncio
    async def test_multiple_orders_unique_ids(self):
        """Test that multiple orders get unique IDs when generated."""
        import asyncio
        order_data = {
            "product_name": "Unique ID Product",
            "quantity": 10
        }

        result1 = await process_supply_chain_order(order_data)
        await asyncio.sleep(1.5)  # Delay to ensure different timestamps (1 second resolution)
        result2 = await process_supply_chain_order(order_data)

        assert result1["order_id"] != result2["order_id"]

    @pytest.mark.asyncio
    async def test_response_types(self):
        """Test that response fields have correct types."""
        order_data = {
            "product_name": "Type Test Product",
            "quantity": 10
        }

        result = await process_supply_chain_order(order_data)

        assert isinstance(result["order_id"], str)
        assert isinstance(result["status"], str)
        assert isinstance(result["timestamp"], str)
        assert isinstance(result["supplier"], dict)
        assert isinstance(result["inventory"], dict)
        assert isinstance(result["logistics"], dict)
        assert isinstance(result["coordinator_summary"], str)
        assert isinstance(result["recommendations"], list)
