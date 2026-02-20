"""
AutoGen multi-agent supply chain workflow for SupplyConsensus.

This module implements a coordinated multi-agent system using AutoGen
for supply chain management with specialized agents for suppliers,
inventory, logistics, and coordination.
"""

import asyncio
import json
from dataclasses import dataclass, field
from typing import Any, Dict, List, Optional
from datetime import datetime

from autogen_agentchat.agents import AssistantAgent
from autogen_agentchat.conditions import MaxMessageTermination, TextMentionTermination
from autogen_agentchat.teams import RoundRobinGroupChat
from autogen_agentchat.messages import TextMessage
from autogen_ext.models.openai import OpenAIChatCompletionClient


@dataclass
class OrderDetails:
    """Supply chain order details."""
    order_id: str
    product_name: str
    quantity: int
    priority: str = "normal"  # low, normal, high, urgent
    destination: str = ""
    required_date: Optional[str] = None
    notes: str = ""


@dataclass
class SupplierResponse:
    """Supplier agent response."""
    available: bool
    supplier_id: str
    unit_price: float
    lead_time_days: int
    capacity: int
    notes: str = ""


@dataclass
class InventoryResponse:
    """Inventory agent response."""
    in_stock: int
    reserved: int
    available: int
    reorder_point: int
    warehouse_location: str
    last_updated: str = field(default_factory=lambda: datetime.utcnow().isoformat())


@dataclass
class LogisticsResponse:
    """Logistics agent response."""
    carrier: str
    shipping_cost: float
    estimated_delivery: str
    tracking_number: Optional[str] = None
    shipping_method: str = "standard"


@dataclass
class SupplyChainResult:
    """Complete supply chain coordination result."""
    order_id: str
    status: str
    supplier: Optional[SupplierResponse] = None
    inventory: Optional[InventoryResponse] = None
    logistics: Optional[LogisticsResponse] = None
    coordinator_summary: str = ""
    recommendations: List[str] = field(default_factory=list)
    timestamp: str = field(default_factory=lambda: datetime.utcnow().isoformat())


class SupplyChainAgentSystem:
    """
    Multi-agent supply chain coordination system using AutoGen.

    Agents:
        - SupplierAgent: Manages supplier communications and availability
        - InventoryAgent: Tracks inventory levels and stock status
        - LogisticsAgent: Handles shipping and transportation
        - CoordinatorAgent: Orchestrates the workflow and makes decisions
    """

    def __init__(self, api_key: Optional[str] = None):
        """Initialize the supply chain agent system."""
        self.api_key = api_key
        self.model_client = self._create_model_client()
        self.agents = self._create_agents()

    def _create_model_client(self) -> OpenAIChatCompletionClient:
        """Create the model client for agents."""
        # Use a mock/local setup if no API key provided
        # In production, this would use a real LLM API
        return OpenAIChatCompletionClient(
            model="gpt-4o-mini",
            api_key=self.api_key or "mock-key-for-demo",
        )

    def _create_agents(self) -> Dict[str, AssistantAgent]:
        """Create all supply chain agents."""

        # Supplier Agent - manages supplier relationships and availability
        supplier_agent = AssistantAgent(
            name="SupplierAgent",
            model_client=self.model_client,
            description="Manages supplier communications, pricing, and availability",
            system_message="""You are the Supplier Agent for a supply chain system.

Your responsibilities:
1. Check supplier availability for requested products
2. Provide pricing information and lead times
3. Assess supplier capacity and reliability
4. Recommend the best supplier based on cost, speed, and reliability

When responding:
- Always provide specific supplier_id, unit_price, and lead_time_days
- Indicate if the product is available
- Include capacity information
- Add relevant notes about supplier reliability or special conditions

Respond in a structured format with clear values for each field."""
        )

        # Inventory Agent - tracks stock levels and warehouse status
        inventory_agent = AssistantAgent(
            name="InventoryAgent",
            model_client=self.model_client,
            description="Tracks inventory levels, stock status, and warehouse operations",
            system_message="""You are the Inventory Agent for a supply chain system.

Your responsibilities:
1. Check current stock levels for requested products
2. Track reserved vs available inventory
3. Monitor reorder points and stock alerts
4. Identify warehouse locations with available stock

When responding:
- Provide in_stock, reserved, and available quantities
- Indicate the warehouse_location
- Note if stock is below reorder_point
- Include timestamp of last inventory update

Respond in a structured format with clear values for each field."""
        )

        # Logistics Agent - handles shipping and transportation
        logistics_agent = AssistantAgent(
            name="LogisticsAgent",
            model_client=self.model_client,
            description="Handles shipping, carriers, and delivery coordination",
            system_message="""You are the Logistics Agent for a supply chain system.

Your responsibilities:
1. Calculate shipping costs and methods
2. Estimate delivery timeframes
3. Select appropriate carriers based on priority and destination
4. Provide tracking information when available

When responding:
- Specify the carrier and shipping_method
- Provide shipping_cost as a number
- Give estimated_delivery date/time
- Include tracking_number if available

Consider order priority (low/normal/high/urgent) when selecting shipping options.
Respond in a structured format with clear values for each field."""
        )

        # Coordinator Agent - orchestrates the workflow
        coordinator_agent = AssistantAgent(
            name="CoordinatorAgent",
            model_client=self.model_client,
            description="Orchestrates supply chain workflow and makes final decisions",
            system_message="""You are the Coordinator Agent for a supply chain system.

Your responsibilities:
1. Synthesize information from Supplier, Inventory, and Logistics agents
2. Make final decisions on order fulfillment
3. Identify risks and provide recommendations
4. Ensure all aspects of the order are coordinated

When responding:
- Provide a clear status: CONFIRMED, PENDING, or REJECTED
- Summarize the coordination results
- List specific recommendations for the order
- Highlight any risks or issues that need attention

Your response should be actionable and comprehensive."""
        )

        return {
            "supplier": supplier_agent,
            "inventory": inventory_agent,
            "logistics": logistics_agent,
            "coordinator": coordinator_agent,
        }

    async def process_order(self, order: OrderDetails) -> SupplyChainResult:
        """
        Process a supply chain order through the multi-agent workflow.

        Args:
            order: The order details to process

        Returns:
            SupplyChainResult with coordinated response from all agents
        """
        # Create the team with all agents in round-robin order
        team = RoundRobinGroupChat(
            participants=[
                self.agents["supplier"],
                self.agents["inventory"],
                self.agents["logistics"],
                self.agents["coordinator"],
            ],
            termination_condition=TextMentionTermination("COORDINATION_COMPLETE"),
        )

        # Build the task message
        task = f"""Process supply chain order:

Order ID: {order.order_id}
Product: {order.product_name}
Quantity: {order.quantity}
Priority: {order.priority}
Destination: {order.destination}
Required Date: {order.required_date or 'Not specified'}
Notes: {order.notes or 'None'}

Each agent should provide their assessment. Coordinator should finalize with status and recommendations.
End with "COORDINATION_COMPLETE" when finished."""

        # Run the team workflow
        messages = []
        async for message in team.run_stream(task=task):
            if isinstance(message, TextMessage):
                messages.append({
                    "source": message.source,
                    "content": message.content,
                })

        # Parse responses from agents (simplified parsing for demo)
        result = self._parse_agent_responses(order.order_id, messages)
        return result

    def _parse_agent_responses(
        self, order_id: str, messages: List[Dict[str, str]]
    ) -> SupplyChainResult:
        """Parse agent responses into structured result."""
        # In a production system, this would use structured output parsing
        # For this demo, we create a structured result with simulated data

        # Find coordinator's final message
        coordinator_msg = ""
        status = "PENDING"
        recommendations = []

        for msg in messages:
            if msg["source"] == "CoordinatorAgent":
                coordinator_msg = msg["content"]
                if "CONFIRMED" in coordinator_msg.upper():
                    status = "CONFIRMED"
                elif "REJECTED" in coordinator_msg.upper():
                    status = "REJECTED"

        # Extract recommendations from coordinator message
        if "recommend" in coordinator_msg.lower():
            # Simple extraction - in production use proper parsing
            recommendations = [
                "Verify supplier capacity before confirming",
                "Consider expedited shipping for high priority orders",
                "Monitor inventory levels for future orders",
            ]

        return SupplyChainResult(
            order_id=order_id,
            status=status,
            supplier=SupplierResponse(
                available=True,
                supplier_id="SUP-001",
                unit_price=25.50,
                lead_time_days=3,
                capacity=1000,
                notes="Reliable supplier with good track record",
            ),
            inventory=InventoryResponse(
                in_stock=500,
                reserved=100,
                available=400,
                reorder_point=200,
                warehouse_location="WH-EAST-01",
            ),
            logistics=LogisticsResponse(
                carrier="FastShip Logistics",
                shipping_cost=45.00,
                estimated_delivery=(datetime.utcnow()).isoformat(),
                shipping_method="express" if "urgent" in coordinator_msg.lower() else "standard",
            ),
            coordinator_summary=coordinator_msg or "Order processed through multi-agent coordination",
            recommendations=recommendations,
        )


class MockSupplyChainAgentSystem:
    """
    Mock implementation for testing without LLM API access.
    Provides deterministic responses for development and testing.
    """

    async def process_order(self, order: OrderDetails) -> SupplyChainResult:
        """Process order with mock responses."""

        # Determine availability based on quantity
        available_qty = 500
        reserved_qty = 100
        in_stock = available_qty + reserved_qty

        # Calculate if order can be fulfilled
        can_fulfill = order.quantity <= (in_stock - reserved_qty)

        # Determine shipping based on priority
        shipping_cost = 25.00
        shipping_method = "standard"
        lead_time = 5

        if order.priority == "urgent":
            shipping_cost = 75.00
            shipping_method = "overnight"
            lead_time = 1
        elif order.priority == "high":
            shipping_cost = 45.00
            shipping_method = "express"
            lead_time = 2

        # Calculate unit price based on quantity (volume discount)
        base_price = 30.00
        if order.quantity >= 100:
            base_price = 22.50
        elif order.quantity >= 50:
            base_price = 25.00
        elif order.quantity >= 10:
            base_price = 27.50

        status = "CONFIRMED" if can_fulfill else "PENDING"

        recommendations = []
        if not can_fulfill:
            recommendations.append(
                f"Insufficient stock. Consider reducing order quantity to {in_stock - reserved_qty} "
                "or placing a backorder."
            )
        if order.priority == "urgent" and order.quantity > 100:
            recommendations.append(
                "Large urgent orders may require split shipments from multiple warehouses."
            )
        if base_price > 25.00:
            recommendations.append(
                f"Consider increasing order to 50 units for volume pricing (${25.00:.2f}/unit)."
            )

        return SupplyChainResult(
            order_id=order.order_id,
            status=status,
            supplier=SupplierResponse(
                available=True,
                supplier_id=f"SUP-{order.product_name[:3].upper():0>3}",
                unit_price=base_price,
                lead_time_days=lead_time,
                capacity=1000,
                notes=f"{'Preferred' if order.priority != 'low' else 'Standard'} supplier tier",
            ),
            inventory=InventoryResponse(
                in_stock=in_stock,
                reserved=reserved_qty,
                available=in_stock - reserved_qty,
                reorder_point=200,
                warehouse_location="WH-EAST-01",
            ),
            logistics=LogisticsResponse(
                carrier="FastShip Logistics",
                shipping_cost=shipping_cost,
                estimated_delivery=(
                    datetime.utcnow()
                ).isoformat(),
                shipping_method=shipping_method,
            ),
            coordinator_summary=(
                f"Order {order.order_id} for {order.quantity}x {order.product_name} "
                f"has been {status.lower()}. "
                f"{'Stock is available for immediate fulfillment.' if can_fulfill else 'Stock shortage requires supplier coordination.'} "
                f"Shipping via {shipping_method} to {order.destination or 'default destination'}."
            ),
            recommendations=recommendations,
        )


# Global instance (use Mock for development, SupplyChainAgentSystem for production)
_agent_system: Optional[Any] = None


def get_agent_system(use_mock: bool = True) -> Any:
    """Get or create the global agent system instance."""
    global _agent_system
    if _agent_system is None:
        if use_mock:
            _agent_system = MockSupplyChainAgentSystem()
        else:
            _agent_system = SupplyChainAgentSystem()
    return _agent_system


async def process_supply_chain_order(order_data: Dict[str, Any]) -> Dict[str, Any]:
    """
    Process a supply chain order and return the coordinated result.

    Args:
        order_data: Dictionary containing order details

    Returns:
        Dictionary with the complete supply chain coordination result
    """
    order = OrderDetails(
        order_id=order_data.get("order_id", f"ORD-{datetime.utcnow().strftime('%Y%m%d%H%M%S')}"),
        product_name=order_data.get("product_name", "Unknown Product"),
        quantity=order_data.get("quantity", 1),
        priority=order_data.get("priority", "normal"),
        destination=order_data.get("destination", ""),
        required_date=order_data.get("required_date"),
        notes=order_data.get("notes", ""),
    )

    agent_system = get_agent_system(use_mock=True)
    result = await agent_system.process_order(order)

    return {
        "order_id": result.order_id,
        "status": result.status,
        "timestamp": result.timestamp,
        "supplier": {
            "available": result.supplier.available if result.supplier else False,
            "supplier_id": result.supplier.supplier_id if result.supplier else None,
            "unit_price": result.supplier.unit_price if result.supplier else None,
            "lead_time_days": result.supplier.lead_time_days if result.supplier else None,
            "capacity": result.supplier.capacity if result.supplier else None,
            "notes": result.supplier.notes if result.supplier else None,
        },
        "inventory": {
            "in_stock": result.inventory.in_stock if result.inventory else 0,
            "reserved": result.inventory.reserved if result.inventory else 0,
            "available": result.inventory.available if result.inventory else 0,
            "reorder_point": result.inventory.reorder_point if result.inventory else 0,
            "warehouse_location": result.inventory.warehouse_location if result.inventory else None,
            "last_updated": result.inventory.last_updated if result.inventory else None,
        },
        "logistics": {
            "carrier": result.logistics.carrier if result.logistics else None,
            "shipping_cost": result.logistics.shipping_cost if result.logistics else None,
            "estimated_delivery": result.logistics.estimated_delivery if result.logistics else None,
            "shipping_method": result.logistics.shipping_method if result.logistics else None,
        },
        "coordinator_summary": result.coordinator_summary,
        "recommendations": result.recommendations,
    }
