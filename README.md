# AI-SDK-AUTOGEN

[![AI-SDK Ecosystem](https://img.shields.io/badge/AI--SDK-ECOSYSTEM-part%20of-blue)](https://github.com/mk-knight23/AI-SDK-ECOSYSTEM)
[![AutoGen](https://img.shields.io/badge/AutoGen-0.4.0-purple)](https://github.com/microsoft/autogen)
[![Vue 3](https://img.shields.io/badge/Vue-3-5.0-green)](https://vuejs.org/)
[![.NET 9](https://img.shields.io/badge/.NET-9.0-purple)](https://dotnet.microsoft.com/)

> **Framework**: Microsoft AutoGen (Multi-Agent Communication)
> **Stack**: Vue 3 + .NET 9 + gRPC

---

## 🎯 Project Overview

**AI-SDK-AUTOGEN** demonstrates distributed multi-agent systems using Microsoft AutoGen. It shows agents running on separate cloud providers communicating via gRPC, enabling truly distributed AI systems with fault tolerance and scalability.

### Key Features

- 🤖 **Multi-Agent Communication** - gRPC-based agent messaging
- ☁️ **Multi-Cloud Deployment** - Agents on different cloud providers
- 🔄 **Fault Tolerance** - Agent recovery and redundancy
- 📊 **Supply Chain Logic** - Real-world distributed scenarios
- 🔌 **Provider Integration** - Azure OpenAI, MiniMax, Cerebras

---

## 🛠 Tech Stack

| Technology | Purpose |
|-------------|---------|
| Vue 3 | Frontend framework |
| .NET 9 | Backend framework |
| AutoGen 0.4 | Agent framework |
| gRPC | Agent communication |
| RabbitMQ | Message queue |
| Pinia | State management |

---

## 🚀 Quick Start

```bash
# Frontend
cd frontend && npm install && npm run dev

# Backend
cd backend && dotnet run
```

---

## 🔌 API Integrations

| Provider | Usage |
|----------|-------|
| Azure OpenAI | Primary LLM |
| MiniMax | Chinese language |
| Cerebras | Ultra-fast inference |
| NVIDIA NIM | GPU acceleration |

---

## 📦 Deployment

**Azure Container Apps**

```bash
az container app up
```

---

## 📝 License

MIT License - see [LICENSE](LICENSE) for details.

---

**Part of the [AI-SDK Ecosystem](https://github.com/mk-knight23/AI-SDK-ECOSYSTEM)**
