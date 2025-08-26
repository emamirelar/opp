# Sub-agents package initialization
# This file exports all available sub-agents for easy import

# Import available sub-agents (with error handling for complex dependencies)
try:
    from .user_detail_agent import user_detail_agent
except ImportError as e:
    print(f"Warning: Could not import user_detail_agent: {e}")
    user_detail_agent = None

try:
    from .screen_context_agent import screen_context_agent
except ImportError as e:
    print(f"Warning: Could not import screen_context_agent: {e}")
    screen_context_agent = None

try:
    from .task_planner_agent import task_planner_agent
except ImportError as e:
    print(f"Warning: Could not import task_planner_agent: {e}")
    task_planner_agent = None

try:
    from .geo_time_agent import geo_time_agent
except ImportError as e:
    print(f"Warning: Could not import geo_time_agent: {e}")
    geo_time_agent = None

try:
    from .contextual_agent import contextual_agent
except ImportError as e:
    print(f"Warning: Could not import contextual_agent: {e}")
    contextual_agent = None

try:
    from .task_executor_agent import task_executor_agent
except ImportError as e:
    print(f"Warning: Could not import task_executor_agent: {e}")
    task_executor_agent = None

try:
    from .response_formatter_agent import response_formatter_agent
except ImportError as e:
    print(f"Warning: Could not import response_formatter_agent: {e}")
    response_formatter_agent = None

try:
    from .worker_agent import worker_agent
except ImportError as e:
    print(f"Warning: Could not import worker_agent: {e}")
    worker_agent = None

try:
    from .user_request_agent import user_request_agent
except ImportError as e:
    print(f"Warning: Could not import user_request_agent: {e}")
    user_request_agent = None

# Export all available agents
__all__ = [
    "user_detail_agent",
    "screen_context_agent", 
    "task_planner_agent",
    "geo_time_agent",
    "contextual_agent",
    "task_executor_agent",
    "response_formatter_agent",
    "worker_agent",
    "user_request_agent",
] 