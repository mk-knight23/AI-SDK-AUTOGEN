"""
AutoGen Service for SupplyConsensus
Provides multi-agent conversation capabilities for supply chain analysis
"""

import os
import asyncio
from typing import List, Dict, Any, Optional
from contextlib import asynccontextmanager

from fastapi import FastAPI, HTTPException, BackgroundTasks
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel, Field
from dotenv import load_dotenv

import pyautogen
from pyautogen import AssistantAgent, UserProxyAgent, GroupChat, GroupChatManager

# Load environment variables
load_dotenv()

# Global variables for agent instances
agent_manager = None

class ChatRequest(BaseModel):
    message: str = Field(..., description="User message to the agents")
    session_id: Optional[str] = Field(None, description="Session ID for conversation continuity")
    context: Optional[Dict[str, Any]] = Field(None, description="Additional context for the conversation")

class ChatResponse(BaseModel):
    response: str = Field(..., description="Agent response")
    session_id: str = Field(..., description="Session ID")
    agents_involved: List[str] = Field(default=[], description="List of agents that participated")
    timestamp: str = Field(..., description="Response timestamp")

class AgentInfo(BaseModel):
    name: str
    description: str
    system_message: str

class SupplyChainAnalysisRequest(BaseModel):
    suppliers: List[Dict[str, Any]] = Field(..., description="List of supplier data")
    constraints: Optional[Dict[str, Any]] = Field(None, description="Supply chain constraints")
    objective: str = Field(..., description="Analysis objective")

class HealthResponse(BaseModel):
    status: str
    service: str
    version: str
    autogen_version: str
    timestamp: str


class AutoGenManager:
    """Manages AutoGen agents and conversations"""

    def __init__(self):
        self.config_list = self._get_config_list()
        self.llm_config = {
            "config_list": self.config_list,
            "temperature": float(os.getenv("TEMPERATURE", 0.7)),
        }
        self.sessions: Dict[str, Any] = {}
        self.agents = self._create_agents()

    def _get_config_list(self) -> List[Dict]:
        """Get OpenAI configuration"""
        api_key = os.getenv("OPENAI_API_KEY")
        if not api_key:
            return []
        return [{
            "model": os.getenv("OPENAI_MODEL", "gpt-4"),
            "api_key": api_key,
        }]

    def _create_agents(self) -> Dict[str, AssistantAgent]:
        """Create supply chain specialist agents"""

        # Supply Chain Analyst Agent
        analyst = AssistantAgent(
            name="supply_analyst",
            system_message="""You are a Supply Chain Analyst specializing in data analysis and optimization.
Your role is to:
- Analyze supplier performance metrics
- Identify bottlenecks and risks
- Provide data-driven recommendations
- Evaluate cost-efficiency trade-offs

Be concise and focus on actionable insights. Always base your analysis on provided data.""",
            llm_config=self.llm_config,
        )

        # Procurement Specialist Agent
        procurement = AssistantAgent(
            name="procurement_specialist",
            system_message="""You are a Procurement Specialist with expertise in vendor management and negotiations.
Your role is to:
- Evaluate supplier contracts and terms
- Suggest procurement strategies
- Assess vendor reliability
- Recommend negotiation approaches

Focus on practical procurement advice and risk mitigation.""",
            llm_config=self.llm_config,
        )

        # Logistics Coordinator Agent
        logistics = AssistantAgent(
            name="logistics_coordinator",
            system_message="""You are a Logistics Coordinator expert in transportation and distribution.
Your role is to:
- Optimize shipping routes and methods
- Analyze delivery timeframes
- Suggest inventory placement
- Identify logistics risks

Provide specific, actionable logistics recommendations.""",
            llm_config=self.llm_config,
        )

        # Consensus Facilitator Agent
        facilitator = AssistantAgent(
            name="consensus_facilitator",
            system_message="""You are a Consensus Facilitator who synthesizes different perspectives into actionable decisions.
Your role is to:
- Summarize different viewpoints from other agents
- Identify areas of agreement and disagreement
- Propose consensus solutions
- Ensure all perspectives are considered

Focus on finding common ground and practical solutions that balance different concerns.""",
            llm_config=self.llm_config,
        )

        return {
            "analyst": analyst,
            "procurement": procurement,
            "logistics": logistics,
            "facilitator": facilitator,
        }

    async def chat(self, message: str, session_id: Optional[str] = None, context: Optional[Dict] = None) -> Dict:
        """Process a chat message through the agent group"""

        if not self.config_list:
            return {
                "response": "AutoGen service is not configured with API keys. Please set OPENAI_API_KEY.",
                "session_id": session_id or "demo",
                "agents_involved": [],
                "timestamp": self._get_timestamp(),
            }

        # Create or retrieve session
        if session_id and session_id in self.sessions:
            chat_manager = self.sessions[session_id]
        else:
            session_id = session_id or f"session_{asyncio.get_event_loop().time()}"

            # Create group chat
            groupchat = GroupChat(
                agents=list(self.agents.values()),
                messages=[],
                max_round=10,
            )
            chat_manager = GroupChatManager(
                groupchat=groupchat,
                llm_config=self.llm_config,
            )
            self.sessions[session_id] = chat_manager

        # Create user proxy for this interaction
        user_proxy = UserProxyAgent(
            name="user_proxy",
            human_input_mode="NEVER",
            max_consecutive_auto_reply=0,
            code_execution_config={"use_docker": False},
        )

        # Initiate chat
        user_proxy.initiate_chat(
            chat_manager,
            message=message,
        )

        # Get the last message from the chat history
        messages = chat_manager.groupchat.messages
        last_message = messages[-1] if messages else {"content": "No response generated"}

        # Extract agents involved
        agents_involved = list(set([msg.get("name", "unknown") for msg in messages]))

        return {
            "response": last_message.get("content", ""),
            "session_id": session_id,
            "agents_involved": agents_involved,
            "timestamp": self._get_timestamp(),
        }

    async def analyze_supply_chain(self, request: SupplyChainAnalysisRequest) -> Dict:
        """Perform multi-agent supply chain analysis"""

        if not self.config_list:
            return {
                "response": "AutoGen service is not configured with API keys. Please set OPENAI_API_KEY.",
                "session_id": "analysis_demo",
                "agents_involved": [],
                "timestamp": self._get_timestamp(),
            }

        # Create analysis prompt
        prompt = f"""Please analyze the following supply chain scenario:

Objective: {request.objective}

Suppliers Data:
{request.suppliers}

Constraints:
{request.constraints or 'None specified'}

Please provide:
1. Supply Analyst: Performance analysis and risk assessment
2. Procurement Specialist: Vendor evaluation and contract recommendations
3. Logistics Coordinator: Distribution and inventory recommendations
4. Consensus Facilitator: Synthesize findings into actionable consensus
"""

        return await self.chat(prompt, context={"type": "supply_chain_analysis"})

    def _get_timestamp(self) -> str:
        from datetime import datetime
        return datetime.utcnow().isoformat()

    def get_agent_info(self) -> List[AgentInfo]:
        """Get information about available agents"""
        return [
            AgentInfo(
                name=name,
                description=agent.system_message.split("\n")[0],
                system_message=agent.system_message,
            )
            for name, agent in self.agents.items()
        ]


@asynccontextmanager
async def lifespan(app: FastAPI):
    """Manage application lifespan"""
    global agent_manager
    agent_manager = AutoGenManager()
    yield
    # Cleanup if needed


app = FastAPI(
    title="SupplyConsensus AutoGen Service",
    description="Multi-agent AI service for supply chain consensus",
    version="1.0.0",
    lifespan=lifespan,
)

# CORS middleware
app.add_middleware(
    CORSMiddleware,
    allow_origins=["http://localhost:3000", "http://localhost:8080"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)


@app.get("/health", response_model=HealthResponse)
async def health_check():
    """Health check endpoint"""
    from datetime import datetime
    return HealthResponse(
        status="healthy",
        service="SupplyConsensus AutoGen Service",
        version="1.0.0",
        autogen_version="0.2.0",  # pyautogen version
        timestamp=datetime.utcnow().isoformat(),
    )


@app.get("/agents", response_model=List[AgentInfo])
async def list_agents():
    """List available agents"""
    return agent_manager.get_agent_info()


@app.post("/chat", response_model=ChatResponse)
async def chat(request: ChatRequest):
    """Send a message to the agent group"""
    try:
        result = await agent_manager.chat(
            message=request.message,
            session_id=request.session_id,
            context=request.context,
        )
        return ChatResponse(**result)
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))


@app.post("/analyze", response_model=ChatResponse)
async def analyze_supply_chain(request: SupplyChainAnalysisRequest):
    """Perform supply chain analysis with multiple agents"""
    try:
        result = await agent_manager.analyze_supply_chain(request)
        return ChatResponse(**result)
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))


if __name__ == "__main__":
    import uvicorn
    port = int(os.getenv("PORT", 8000))
    uvicorn.run(app, host="0.0.0.0", port=port)
