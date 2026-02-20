"""
Tests for MockSupplyChainAgentSystem.

Tests the mock implementation used for development and testing
without requiring LLM API access.
"""

import pytest
from datetime import datetime
from app.autogen import (
    OrderDetails,
    MockSupplyChainAgentSystem,
    SupplyChainResult,
)


class TestMockAgentSystemInitialization:
    """Tests for MockSupplyChainAgentSystem initialization."""

    def test_mock_system_creation(self):
        """Test that MockSupplyChainAgentSystem can be created."""
        system = MockSupplyChainAgentSystem()
        assert system is not None

    @pytest.mark.asyncio
    async def test_mock_system_process_order_basic(self, sample_order):
        """Test processing a basic order."""
        system = MockSupplyChainAgentSystem()
        result = await system.process_order(sample_order)

        assert isinstance(result, SupplyChainResult)
        assert result.order_id == sample_order.order_id
        assert result.status in ["CONFIRMED", "PENDING"]


class TestMockAgentSystemOrderProcessing:
    """Tests for order processing with various scenarios."""

    @pytest.mark.asyncio
    async def test_process_normal_order(self, sample_order):
        """Test processing a normal priority order."""
        system = MockSupplyChainAgentSystem()
        result = await system.process_order(sample_order)

        assert result.order_id == "ORD-TEST-001"
        assert result.status == "CONFIRMED"
        assert result.supplier is not None
        assert result.inventory is not None
        assert result.logistics is not None

        # Check supplier details
        assert result.supplier.available is True
        assert result.supplier.supplier_id.startswith("SUP-")
        assert result.supplier.unit_price > 0
        assert result.supplier.lead_time_days > 0

        # Check inventory details
        assert result.inventory.in_stock >= result.inventory.reserved
        assert result.inventory.available >= 0
        assert result.inventory.warehouse_location is not None

        # Check logistics details
        assert result.logistics.carrier is not None
        assert result.logistics.shipping_cost > 0
        assert result.logistics.estimated_delivery is not None

    @pytest.mark.asyncio
    async def test_process_urgent_order(self, urgent_order):
        """Test processing an urgent priority order."""
        system = MockSupplyChainAgentSystem()
        result = await system.process_order(urgent_order)

        assert result.order_id == "ORD-TEST-URGENT"
        # Urgent orders should have overnight shipping
        assert result.logistics.shipping_method == "overnight"
        assert result.logistics.shipping_cost == 75.00
        assert result.supplier.lead_time_days == 1

    @pytest.mark.asyncio
    async def test_process_high_priority_order(self, high_priority_order):
        """Test processing a high priority order."""
        system = MockSupplyChainAgentSystem()
        result = await system.process_order(high_priority_order)

        assert result.order_id == "ORD-TEST-HIGH"
        # High priority should have express shipping
        assert result.logistics.shipping_method == "express"
        assert result.logistics.shipping_cost == 45.00
        assert result.supplier.lead_time_days == 2

    @pytest.mark.asyncio
    async def test_process_low_priority_order(self, low_priority_order):
        """Test processing a low priority order."""
        system = MockSupplyChainAgentSystem()
        result = await system.process_order(low_priority_order)

        assert result.order_id == "ORD-TEST-LOW"
        # Low priority should have standard shipping
        assert result.logistics.shipping_method == "standard"
        assert result.logistics.shipping_cost == 25.00
        assert result.supplier.lead_time_days == 5
        assert "Standard" in result.supplier.notes


class TestMockAgentSystemVolumePricing:
    """Tests for volume-based pricing logic."""

    @pytest.mark.asyncio
    async def test_volume_pricing_tier_1_small_quantity(self):
        """Test pricing for small quantities (base price)."""
        system = MockSupplyChainAgentSystem()
        order = OrderDetails(
            order_id="ORD-SMALL",
            product_name="Widget",
            quantity=5,
            priority="normal"
        )
        result = await system.process_order(order)

        # Quantity < 10: base price $30.00
        assert result.supplier.unit_price == 30.00

    @pytest.mark.asyncio
    async def test_volume_pricing_tier_2_medium_quantity(self):
        """Test pricing for medium quantities (10-49)."""
        system = MockSupplyChainAgentSystem()
        order = OrderDetails(
            order_id="ORD-MEDIUM",
            product_name="Widget",
            quantity=25,
            priority="normal"
        )
        result = await system.process_order(order)

        # Quantity 10-49: $27.50 per unit
        assert result.supplier.unit_price == 27.50

    @pytest.mark.asyncio
    async def test_volume_pricing_tier_3_large_quantity(self):
        """Test pricing for large quantities (50-99)."""
        system = MockSupplyChainAgentSystem()
        order = OrderDetails(
            order_id="ORD-LARGE",
            product_name="Widget",
            quantity=75,
            priority="normal"
        )
        result = await system.process_order(order)

        # Quantity 50-99: $25.00 per unit
        assert result.supplier.unit_price == 25.00

    @pytest.mark.asyncio
    async def test_volume_pricing_tier_4_bulk_quantity(self, large_quantity_order):
        """Test pricing for bulk quantities (100+)."""
        system = MockSupplyChainAgentSystem()
        result = await system.process_order(large_quantity_order)

        # Quantity >= 100: $22.50 per unit
        assert result.supplier.unit_price == 22.50

    @pytest.mark.asyncio
    async def test_volume_pricing_boundary_at_10(self):
        """Test pricing boundary at exactly 10 units."""
        system = MockSupplyChainAgentSystem()
        order = OrderDetails(
            order_id="ORD-BOUNDARY-10",
            product_name="Widget",
            quantity=10,
            priority="normal"
        )
        result = await system.process_order(order)

        # Exactly 10 units should get tier 2 pricing
        assert result.supplier.unit_price == 27.50

    @pytest.mark.asyncio
    async def test_volume_pricing_boundary_at_50(self):
        """Test pricing boundary at exactly 50 units."""
        system = MockSupplyChainAgentSystem()
        order = OrderDetails(
            order_id="ORD-BOUNDARY-50",
            product_name="Widget",
            quantity=50,
            priority="normal"
        )
        result = await system.process_order(order)

        # Exactly 50 units should get tier 3 pricing
        assert result.supplier.unit_price == 25.00

    @pytest.mark.asyncio
    async def test_volume_pricing_boundary_at_100(self):
        """Test pricing boundary at exactly 100 units."""
        system = MockSupplyChainAgentSystem()
        order = OrderDetails(
            order_id="ORD-BOUNDARY-100",
            product_name="Widget",
            quantity=100,
            priority="normal"
        )
        result = await system.process_order(order)

        # Exactly 100 units should get tier 4 pricing
        assert result.supplier.unit_price == 22.50


class TestMockAgentSystemStockAvailability:
    """Tests for stock availability logic."""

    @pytest.mark.asyncio
    async def test_confirmed_when_stock_available(self):
        """Test order is confirmed when stock is available."""
        system = MockSupplyChainAgentSystem()
        order = OrderDetails(
            order_id="ORD-IN-STOCK",
            product_name="Widget",
            quantity=100,  # Less than available (400)
            priority="normal"
        )
        result = await system.process_order(order)

        assert result.status == "CONFIRMED"

    @pytest.mark.asyncio
    async def test_pending_when_stock_unavailable(self):
        """Test order is pending when stock is insufficient."""
        system = MockSupplyChainAgentSystem()
        order = OrderDetails(
            order_id="ORD-OUT-OF-STOCK",
            product_name="Widget",
            quantity=501,  # More than available (500)
            priority="normal"
        )
        result = await system.process_order(order)

        assert result.status == "PENDING"

    @pytest.mark.asyncio
    async def test_boundary_at_exact_available_stock(self):
        """Test order at exact available stock boundary."""
        system = MockSupplyChainAgentSystem()
        # Available stock is 500 (600 in_stock - 100 reserved)
        order = OrderDetails(
            order_id="ORD-BOUNDARY-STOCK",
            product_name="Widget",
            quantity=500,
            priority="normal"
        )
        result = await system.process_order(order)

        # Exactly at available stock should be confirmed
        assert result.status == "CONFIRMED"


class TestMockAgentSystemRecommendations:
    """Tests for recommendation generation."""

    @pytest.mark.asyncio
    async def test_recommendation_for_insufficient_stock(self):
        """Test recommendation when stock is insufficient."""
        system = MockSupplyChainAgentSystem()
        order = OrderDetails(
            order_id="ORD-REC-STOCK",
            product_name="Widget",
            quantity=501,  # More than available (500)
            priority="normal"
        )
        result = await system.process_order(order)

        assert result.status == "PENDING"
        assert len(result.recommendations) > 0
        assert any("Insufficient stock" in rec for rec in result.recommendations)

    @pytest.mark.asyncio
    async def test_recommendation_for_large_urgent_order(self):
        """Test recommendation for large urgent orders."""
        system = MockSupplyChainAgentSystem()
        order = OrderDetails(
            order_id="ORD-REC-URGENT",
            product_name="Widget",
            quantity=150,  # Large quantity
            priority="urgent"
        )
        result = await system.process_order(order)

        assert any("split shipments" in rec for rec in result.recommendations)

    @pytest.mark.asyncio
    async def test_recommendation_for_high_price(self):
        """Test recommendation for orders not getting volume discount."""
        system = MockSupplyChainAgentSystem()
        order = OrderDetails(
            order_id="ORD-REC-PRICE",
            product_name="Widget",
            quantity=5,  # Small quantity = high price
            priority="normal"
        )
        result = await system.process_order(order)

        assert any("volume pricing" in rec for rec in result.recommendations)

    @pytest.mark.asyncio
    async def test_no_price_recommendation_at_good_price(self):
        """Test no pricing recommendation when already at good price."""
        system = MockSupplyChainAgentSystem()
        order = OrderDetails(
            order_id="ORD-NO-REC",
            product_name="Widget",
            quantity=100,  # Gets best price
            priority="normal"
        )
        result = await system.process_order(order)

        # Should not have volume pricing recommendation
        assert not any("volume pricing" in rec for rec in result.recommendations)


class TestMockAgentSystemCoordinatorSummary:
    """Tests for coordinator summary generation."""

    @pytest.mark.asyncio
    async def test_coordinator_summary_contains_order_id(self, sample_order):
        """Test that coordinator summary includes order ID."""
        system = MockSupplyChainAgentSystem()
        result = await system.process_order(sample_order)

        assert sample_order.order_id in result.coordinator_summary

    @pytest.mark.asyncio
    async def test_coordinator_summary_contains_quantity(self, sample_order):
        """Test that coordinator summary includes quantity."""
        system = MockSupplyChainAgentSystem()
        result = await system.process_order(sample_order)

        assert str(sample_order.quantity) in result.coordinator_summary

    @pytest.mark.asyncio
    async def test_coordinator_summary_contains_product(self, sample_order):
        """Test that coordinator summary includes product name."""
        system = MockSupplyChainAgentSystem()
        result = await system.process_order(sample_order)

        assert sample_order.product_name in result.coordinator_summary

    @pytest.mark.asyncio
    async def test_coordinator_summary_contains_status(self, sample_order):
        """Test that coordinator summary includes status."""
        system = MockSupplyChainAgentSystem()
        result = await system.process_order(sample_order)

        assert result.status.lower() in result.coordinator_summary.lower()

    @pytest.mark.asyncio
    async def test_coordinator_summary_contains_shipping_method(self, urgent_order):
        """Test that coordinator summary includes shipping method."""
        system = MockSupplyChainAgentSystem()
        result = await system.process_order(urgent_order)

        assert result.logistics.shipping_method in result.coordinator_summary


class TestMockAgentSystemSupplierDetails:
    """Tests for supplier-specific details."""

    @pytest.mark.asyncio
    async def test_supplier_id_format(self, sample_order):
        """Test that supplier ID follows expected format."""
        system = MockSupplyChainAgentSystem()
        result = await system.process_order(sample_order)

        # Supplier ID should be SUP-{product_name prefix}
        expected_prefix = f"SUP-{sample_order.product_name[:3].upper()}"
        assert result.supplier.supplier_id.startswith(expected_prefix)

    @pytest.mark.asyncio
    async def test_supplier_notes_for_preferred_tier(self, high_priority_order):
        """Test supplier notes for preferred tier (non-low priority)."""
        system = MockSupplyChainAgentSystem()
        result = await system.process_order(high_priority_order)

        assert "Preferred" in result.supplier.notes

    @pytest.mark.asyncio
    async def test_supplier_notes_for_standard_tier(self, low_priority_order):
        """Test supplier notes for standard tier (low priority)."""
        system = MockSupplyChainAgentSystem()
        result = await system.process_order(low_priority_order)

        assert "Standard" in result.supplier.notes

    @pytest.mark.asyncio
    async def test_supplier_capacity(self, sample_order):
        """Test that supplier has capacity information."""
        system = MockSupplyChainAgentSystem()
        result = await system.process_order(sample_order)

        assert result.supplier.capacity > 0
        assert result.supplier.capacity >= sample_order.quantity
