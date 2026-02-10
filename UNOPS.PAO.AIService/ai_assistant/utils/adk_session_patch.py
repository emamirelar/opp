"""
Patch google-adk DatabaseSessionService to use naive UTC timestamps for PostgreSQL.

The ADK uses timezone-aware datetime.now(timezone.utc) for session create_time/update_time
but only strips timezone for SQLite. PostgreSQL with asyncpg and TIMESTAMP WITHOUT TIME ZONE
(or the driver's cast in the SQL) fails with "can't subtract offset-naive and offset-aware
datetimes". The migration changes the DB columns to TIMESTAMP WITH TIME ZONE, but the ADK
still emits SQL that casts parameters to TIMESTAMP WITHOUT TIME ZONE, so the error happens
in the driver. We replace create_session with a version that always uses naive UTC.

Apply by importing this module early in main.py (before any DatabaseSessionService usage).
"""

import logging
from datetime import datetime, timezone
from typing import Any, Optional

logger = logging.getLogger(__name__)


def apply_adk_session_timestamp_patch() -> None:
    """Replace ADK create_session with a version that uses naive UTC (PostgreSQL compatibility)."""
    try:
        from google.adk.sessions import database_session_service as dss
    except ImportError:
        logger.warning(
            "google.adk.sessions.database_session_service not found, skipping session timestamp patch"
        )
        return

    if getattr(dss, "_session_timestamp_patch_applied", False):
        return

    _session_util = dss._session_util
    _merge_state = dss._merge_state
    AlreadyExistsError = dss.AlreadyExistsError  # noqa: N806

    async def create_session_naive_utc(
        self,
        *,
        app_name: str,
        user_id: str,
        state: Optional[dict[str, Any]] = None,
        session_id: Optional[str] = None,
    ):
        await self._prepare_tables()
        schema = self._get_schema_classes()
        async with self._rollback_on_exception_session() as sql_session:
            if session_id and await sql_session.get(
                schema.StorageSession, (app_name, user_id, session_id)
            ):
                raise AlreadyExistsError(
                    f"Session with id {session_id} already exists."
                )
            storage_app_state = await sql_session.get(
                schema.StorageAppState, (app_name)
            )
            storage_user_state = await sql_session.get(
                schema.StorageUserState, (app_name, user_id)
            )
            if not storage_app_state:
                storage_app_state = schema.StorageAppState(
                    app_name=app_name, state={}
                )
                sql_session.add(storage_app_state)
            if not storage_user_state:
                storage_user_state = schema.StorageUserState(
                    app_name=app_name, user_id=user_id, state={}
                )
                sql_session.add(storage_user_state)
            state_deltas = _session_util.extract_state_delta(state)
            app_state_delta = state_deltas["app"]
            user_state_delta = state_deltas["user"]
            session_state = state_deltas["session"]
            if app_state_delta:
                storage_app_state.state = (
                    storage_app_state.state | app_state_delta
                )
            if user_state_delta:
                storage_user_state.state = (
                    storage_user_state.state | user_state_delta
                )
            # Use naive UTC for all backends so PostgreSQL/asyncpg accept the value
            now = datetime.now(timezone.utc).replace(tzinfo=None)
            is_sqlite = self.db_engine.dialect.name == "sqlite"
            storage_session = schema.StorageSession(
                app_name=app_name,
                user_id=user_id,
                id=session_id,
                state=session_state,
                create_time=now,
                update_time=now,
            )
            sql_session.add(storage_session)
            await sql_session.commit()
            merged_state = _merge_state(
                storage_app_state.state,
                storage_user_state.state,
                session_state,
            )
            session = storage_session.to_session(
                state=merged_state, is_sqlite=is_sqlite
            )
        return session

    dss.DatabaseSessionService.create_session = create_session_naive_utc
    dss._session_timestamp_patch_applied = True
    logger.info(
        "Applied ADK session timestamp patch (naive UTC for PostgreSQL)"
    )
