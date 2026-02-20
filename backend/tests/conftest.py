"""
Pytest configuration and fixtures for SupplyConsensus tests.
"""

import pytest
import asyncio
from datetime import datetime
from unittest.mock import Mock, AsyncMock, patch
import sys
import os

# Add app directory to path
sys.path.insert(0, os.path.join(os.path.dirname(__file__), '..'))

from app.autogen import (
    OrderDetails,
    SupplierResponse,
    InventoryResponse,
    LogisticsResponse,
    SupplyChainResult,
    SupplyChainAgentSystem,
    MockSupplyChainAgentSystem,
    process_supply_chain_order,
    get_agent_system,
)


@pytest.fixture
def event_loop():
    """Create an instance of the default event loop for each test case."""
    loop = asyncio.get_event_loop_policy().new_event_loop()
    yield loop
    loop.close()


@pytest.fixture
def sample_order():
    """Return a sample order for testing."""
    return OrderDetails(
        order_id="ORD-TEST-001",
        product_name="Test Widget",
        quantity=50,
        priority="normal",
        destination="Warehouse A",
        required_date="2026-03-01",
        notes="Test order for unit tests"
    )


@pytest.fixture
def urgent_order():
    """Return an urgent priority order for testing."""
    return OrderDetails(
        order_id="ORD-TEST-URGENT",
        product_name="Critical Component",
        quantity=200,
        priority="urgent",
        destination="Emergency Facility",
        required_date="2026-02-20",
        notes="Urgent order requiring expedited shipping"
    )


@pytest.fixture
def high_priority_order():
    """Return a high priority order for testing."""
    return OrderDetails(
        order_id="ORD-TEST-HIGH",
        product_name="High Priority Item",
        quantity=75,
        priority="high",
        destination="Main Warehouse",
        required_date="2026-02-25",
        notes="High priority order"
    )


@pytest.fixture
def low_priority_order():
    """Return a low priority order for testing."""
    return OrderDetails(
        order_id="ORD-TEST-LOW",
        product_name="Low Priority Item",
        quantity=10,
        priority="low",
        destination="Secondary Warehouse",
        required_date="2026-03-15",
        notes="Low priority order"
    )


@pytest.fixture
def large_quantity_order():
    """Return a large quantity order for testing volume discounts."""
    return OrderDetails(
        order_id="ORD-TEST-BULK",
        product_name="Bulk Widget",
        quantity=150,
        priority="normal",
        destination="Distribution Center",
        required_date="2026-03-10",
        notes="Large quantity order for volume pricing"
    )


@pytest.fixture
def sample_supplier_response():
    """Return a sample supplier response."""
    return SupplierResponse(
        available=True,
        supplier_id="SUP-001",
        unit_price=25.50,
        lead_time_days=3,
        capacity=1000,
        notes="Reliable supplier"
    )


@pytest.fixture
def sample_inventory_response():
    """Return a sample inventory response."""
    return InventoryResponse(
        in_stock=500,
        reserved=100,
        available=400,
        reorder_point=200,
        warehouse_location="WH-EAST-01",
        last_updated=datetime.utcnow().isoformat()
    )


@pytest.fixture
def sample_logistics_response():
    """Return a sample logistics response."""
    return LogisticsResponse(
        carrier="FastShip Logistics",
        shipping_cost=45.00,
        estimated_delivery=datetime.utcnow().isoformat(),
        tracking_number="TRACK123456",
        shipping_method="express"
    )


@pytest.fixture
def mock_agent_system():
    """Return a mock agent system for testing."""
    return MockSupplyChainAgentSystem()


@pytest.fixture
def mock_model_client():
    """Return a mock model client."""
    mock_client = Mock()
    mock_client.create = AsyncMock(return_value=Mock(
        content="Mock response from LLM",
        usage=Mock(total_tokens=100)
    ))
    return mock_client


@pytest.fixture(autouse=True)
def reset_agent_system():
    """Reset the global agent system before each test."""
    # Clear the global instance
    import app.autogen as autogen_module
    autogen_module._agent_system = None
    yield
    # Clean up after test
    autogen_module._agent_system = None


@pytest.fixture
def sample_order_dict():
    """Return a sample order as a dictionary."""
    return {
        "order_id": "ORD-DICT-001",
        "product_name": "Dictionary Product",
        "quantity": 25,
        "priority": "normal",
        "destination": "Test Destination",
        "required_date": "2026-03-01",
        "notes": "Order from dictionary"
    }


@pytest.fixture
def minimal_order_dict():
    """Return a minimal order dictionary with only required fields."""
    return {
        "product_name": "Minimal Product",
        "quantity": 5
    }
