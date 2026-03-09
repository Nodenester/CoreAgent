# CoreAgent

> **Archived** -- Built April 2024

An autonomous AI agent framework that runs a goal-driven task loop against a local LLM. You give it a goal, it creates a plan, breaks it into tasks, and executes them in a loop using tool calls -- all powered by a local llama.cpp inference server.

## How it works

1. You define a goal and optional context for the agent
2. The agent generates a plan via `SetPlan`, then creates a task list with `AddTasks`
3. It runs an execution loop: build a ChatML prompt with system instructions, task state, and conversation history, send it to the LLM, parse `<tool_call>` responses, execute them, repeat
4. Agent state (goal, plan, tasks, internal dialog) is persisted to JSON between steps
5. A basic vector store with cosine similarity retrieval provides memory/RAG for the agent's context window

## Built-in tools

- `SetPlan` -- store a strategic plan
- `AddTasks` -- add tasks with IDs and descriptions
- `CompleteTask` -- mark a task done and move it to completed list
- `RemoveTask` -- delete a task
- `IsGoalAchieved` -- signal that the goal is reached and stop the loop

## Tech stack

- C# / .NET 8
- Local LLM inference via llama.cpp HTTP API (expects `localhost:5037`)
- ChatML prompt format with tool-calling conventions
- JSON file-based persistence (agent data, dialog history, vector DB)
- Cosine similarity vector search for memory retrieval

## Running

1. Start a llama.cpp-compatible inference server on port 5037
2. `dotnet run` from the `CoreAgent/` directory
3. Enter a storage folder name, set a goal, and let it run

## License

MIT
