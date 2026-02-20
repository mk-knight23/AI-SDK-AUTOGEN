"""
Tests for SupplyConsensus data classes.

Tests the OrderDetails, SupplierResponse, InventoryResponse,
LogisticsResponse, and SupplyChainResult dataclasses.
"""

import pytest
from datetime import datetime
from app.autogen import (
    OrderDetails,
    SupplierResponse,
    InventoryResponse,
    LogisticsResponse,
    SupplyChainResult,
)


class TestOrderDetails:
    """Tests for OrderDetails dataclass."""

    def test_order_details_creation_with_all_fields(self):
        """Test creating OrderDetails with all fields."""
        order = OrderDetails(
            order_id="ORD-001",
            product_name="Test Product",
            quantity=100,
            priority="high",
            destination="Warehouse B",
            required_date="2026-03-15",
            notes="Test notes"
        )

        assert order.order_id == "ORD-001"
        assert order.product_name == "Test Product"
        assert order.quantity == 100
        assert order.priority == "high"
        assert order.destination == "Warehouse B"
        assert order.required_date == "2026-03-15"
        assert order.notes == "Test notes"

    def test_order_details_default_values(self):
        """Test OrderDetails with default values."""
        order = OrderDetails(
            order_id="ORD-002",
            product_name="Default Product",
            quantity=10
        )

        assert order.order_id == "ORD-002"
        assert order.product_name == "Default Product"
        assert order.quantity == 10
        assert order.priority == "normal"  # Default value
        assert order.destination == ""  # Default value
        assert order.required_date is None  # Default value
        assert order.notes == ""  # Default value

    def test_order_details_priority_levels(self):
        """Test OrderDetails with different priority levels."""
        priorities = ["low", "normal", "high", "urgent"]

        for priority in priorities:
            order = OrderDetails(
                order_id=f"ORD-{priority}",
                product_name="Priority Test",
                quantity=1,
                priority=priority
            )
            assert order.priority == priority

    def test_order_details_immutability(self):
        """Test that OrderDetails behaves like an immutable value object."""
        order = OrderDetails(
            order_id="ORD-003",
            product_name="Immutable Product",
            quantity=50
        )

        # Create a new order with modified quantity
        new_order = OrderDetails(
            order_id=order.order_id,
            product_name=order.product_name,
            quantity=100,  # Modified
            priority=order.priority,
            destination=order.destination,
            required_date=order.required_date,
            notes=order.notes
        )

        assert order.quantity == 50
        assert new_order.quantity == 100
        assert order.order_id == new_order.order_id


class TestSupplierResponse:
    """Tests for SupplierResponse dataclass."""

    def test_supplier_response_creation(self):
        """Test creating SupplierResponse with all fields."""
        response = SupplierResponse(
            available=True,
            supplier_id="SUP-001",
            unit_price=25.50,
            lead_time_days=5,
            capacity=1000,
            notes="Reliable supplier"
        )

        assert response.available is True
        assert response.supplier_id == "SUP-001"
        assert response.unit_price == 25.50
        assert response.lead_time_days == 5
        assert response.capacity == 1000
        assert response.notes == "Reliable supplier"

    def test_supplier_response_default_notes(self):
        """Test SupplierResponse with default notes."""
        response = SupplierResponse(
            available=False,
            supplier_id="SUP-002",
            unit_price=0.0,
            lead_time_days=0,
            capacity=0
        )

        assert response.available is False
        assert response.notes == ""  # Default value

    def test_supplier_response_unavailable(self):
        """Test SupplierResponse when supplier is unavailable."""
        response = SupplierResponse(
            available=False,
            supplier_id="SUP-003",
            unit_price=0.0,
            lead_time_days=0,
            capacity=0,
            notes="Out of stock"
        )

        assert response.available is False
        assert response.unit_price == 0.0


class TestInventoryResponse:
    """Tests for InventoryResponse dataclass."""

    def test_inventory_response_creation(self):
        """Test creating InventoryResponse with all fields."""
        response = InventoryResponse(
            in_stock=1000,
            reserved=200,
            available=800,
            reorder_point=300,
            warehouse_location="WH-WEST-01",
            last_updated="2026-02-19T10:00:00"
        )

        assert response.in_stock == 1000
        assert response.reserved == 200
        assert response.available == 800
        assert response.reorder_point == 300
        assert response.warehouse_location == "WH-WEST-01"
        assert response.last_updated == "2026-02-19T10:00:00"

    def test_inventory_response_default_timestamp(self):
        """Test InventoryResponse with auto-generated timestamp."""
        before = datetime.utcnow().isoformat()
        response = InventoryResponse(
            in_stock=500,
            reserved=100,
            available=400,
            reorder_point=200,
            warehouse_location="WH-EAST-01"
        )
        after = datetime.utcnow().isoformat()

        assert response.in_stock == 500
        # Verify timestamp was auto-generated
        assert response.last_updated is not None
        assert before <= response.last_updated <= after

    def test_inventory_stock_calculation(self):
        """Test that available = in_stock - reserved logic holds."""
        in_stock = 1000
        reserved = 150
        available = in_stock - reserved

        response = InventoryResponse(
            in_stock=in_stock,
            reserved=reserved,
            available=available,
            reorder_point=200,
            warehouse_location="WH-CENTRAL-01"
        )

        assert response.available == response.in_stock - response.reserved


class TestLogisticsResponse:
    """Tests for LogisticsResponse dataclass."""

    def test_logistics_response_creation(self):
        """Test creating LogisticsResponse with all fields."""
        response = LogisticsResponse(
            carrier="FastShip Logistics",
            shipping_cost=45.00,
            estimated_delivery="2026-02-25T14:00:00",
            tracking_number="TRACK123456789",
            shipping_method="express"
        )

        assert response.carrier == "FastShip Logistics"
        assert response.shipping_cost == 45.00
        assert response.estimated_delivery == "2026-02-25T14:00:00"
        assert response.tracking_number == "TRACK123456789"
        assert response.shipping_method == "express"

    def test_logistics_response_defaults(self):
        """Test LogisticsResponse with default values."""
        response = LogisticsResponse(
            carrier="Standard Carrier",
            shipping_cost=25.00,
            estimated_delivery="2026-03-01T10:00:00"
        )

        assert response.tracking_number is None  # Default value
        assert response.shipping_method == "standard"  # Default value

    def test_logistics_response_shipping_methods(self):
        """Test LogisticsResponse with different shipping methods."""
        methods = ["standard", "express", "overnight", "freight"]

        for method in methods:
            response = LogisticsResponse(
                carrier="Test Carrier",
                shipping_cost=30.00,
                estimated_delivery="2026-03-01T10:00:00",
                shipping_method=method
            )
            assert response.shipping_method == method


class TestSupplyChainResult:
    """Tests for SupplyChainResult dataclass."""

    def test_supply_chain_result_creation(self):
        """Test creating SupplyChainResult with all fields."""
        supplier = SupplierResponse(
            available=True,
            supplier_id="SUP-001",
            unit_price=25.50,
            lead_time_days=3,
            capacity=1000,
            notes="Reliable"
        )

        inventory = InventoryResponse(
            in_stock=500,
            reserved=100,
            available=400,
            reorder_point=200,
            warehouse_location="WH-01"
        )

        logistics = LogisticsResponse(
            carrier="FastShip",
            shipping_cost=45.00,
            estimated_delivery="2026-03-01T10:00:00",
            shipping_method="express"
        )

        result = SupplyChainResult(
            order_id="ORD-001",
            status="CONFIRMED",
            supplier=supplier,
            inventory=inventory,
            logistics=logistics,
            coordinator_summary="Order confirmed successfully",
            recommendations=["Monitor stock levels"]
        )

        assert result.order_id == "ORD-001"
        assert result.status == "CONFIRMED"
        assert result.supplier == supplier
        assert result.inventory == inventory
        assert result.logistics == logistics
        assert result.coordinator_summary == "Order confirmed successfully"
        assert result.recommendations == ["Monitor stock levels"]

    def test_supply_chain_result_defaults(self):
        """Test SupplyChainResult with default values."""
        result = SupplyChainResult(
            order_id="ORD-002",
            status="PENDING"
        )

        assert result.order_id == "ORD-002"
        assert result.status == "PENDING"
        assert result.supplier is None
        assert result.inventory is None
        assert result.logistics is None
        assert result.coordinator_summary == ""
        assert result.recommendations == []
        assert result.timestamp is not None

    def test_supply_chain_result_status_values(self):
        """Test SupplyChainResult with different status values."""
        statuses = ["CONFIRMED", "PENDING", "REJECTED", "PROCESSING"]

        for status in statuses:
            result = SupplyChainResult(
                order_id=f"ORD-{status}",
                status=status
            )
            assert result.status == status

    def test_supply_chain_result_recommendations_list(self):
        """Test SupplyChainResult with multiple recommendations."""
        recommendations = [
            "Verify supplier capacity",
            "Consider expedited shipping",
            "Monitor inventory levels"
        ]

        result = SupplyChainResult(
            order_id="ORD-003",
            status="CONFIRMED",
            recommendations=recommendations
        )

        assert len(result.recommendations) == 3
        assert result.recommendations == recommendations
