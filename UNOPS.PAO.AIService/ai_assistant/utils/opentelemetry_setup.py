#!/usr/bin/env python3
"""
OpenTelemetry Setup Utilities

This module handles OpenTelemetry configuration and context management
to avoid duplication across the application.
"""

import os
import warnings


def configure_opentelemetry():
    """
    Configure OpenTelemetry settings and suppress context warnings.
    This should be called early in the application startup process.
    """
    # Fix OpenTelemetry context issues
    warnings.filterwarnings("ignore", category=UserWarning, message=".*opentelemetry.*")

    # Disable OpenTelemetry completely via environment variable
    os.environ["OTEL_SDK_DISABLED"] = "true"

    # Additional OpenTelemetry suppression
    try:
        from opentelemetry.context import _RUNTIME_CONTEXT
        # Monkey patch to suppress context detach errors
        original_detach = _RUNTIME_CONTEXT.detach
        
        def safe_detach(token):
            try:
                return original_detach(token)
            except ValueError as e:
                if "was created in a different Context" in str(e):
                    # Silently ignore context errors that don't affect functionality
                    pass
                else:
                    raise
        
        _RUNTIME_CONTEXT.detach = safe_detach
    except ImportError:
        # OpenTelemetry not installed or different version
        pass
