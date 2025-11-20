from typing import Any, Tuple
import time

import crypto
import user
import storage


def record_token(payload: dict[str, Any]) -> None:
    db = storage.load_tokens()
    db["tokens"].append({"jti": payload["jti"], "exp": payload["exp"]})
    storage.save_tokens(db)


def revoke_by_jti(jti: str) -> None:
    db = storage.load_tokens()
    db["tokens"] = [t for t in db["tokens"] if t["jti"] != jti]
    revoked = db.setdefault("revoked", [])
    revoked.append({"jti": jti, "revoked_at": int(time.time())})
    storage.save_tokens(db)


def is_revoked(jti: str) -> bool:
    db = storage.load_tokens()
    return any(t["jti"] == jti for t in db.get("revoked", []))


def is_expired(exp: int) -> bool:
    return int(time.time()) > exp


def login(username: str, password: str) -> Tuple[str, str]:
    u = user.get_user(username)
    if not u or not user.verify_password(u, password):
        raise ValueError("invalid credentials")

    access, access_payload = crypto.issue_access(username)
    refresh, refresh_payload = crypto.issue_refresh(username)

    record_token(access_payload)
    record_token(refresh_payload)

    return access, refresh


def verify_access(access: str) -> dict[str, Any]:
    payload = crypto.decode(access)
    if payload.get("typ") != "access":
        raise ValueError("wrong token type")
    if is_revoked(payload["jti"]) or is_expired(payload["exp"]):
        raise ValueError("token revoked or expired")
    return payload


def refresh_pair(refresh_token: str) -> Tuple[str, str]:
    payload = crypto.decode(refresh_token)
    if payload.get("typ") != "refresh":
        raise ValueError("wrong token type")
    if is_revoked(payload["jti"]) or is_expired(payload["exp"]):
        raise ValueError("refresh token revoked or expired")

    revoke_by_jti(payload["jti"])

    sub = payload["sub"]
    access, access_payload = crypto.issue_access(sub)
    refresh, refresh_payload = crypto.issue_refresh(sub)

    record_token(access_payload)
    record_token(refresh_payload)

    return access, refresh


def revoke(token: str) -> None:
    payload = crypto.decode(token)
    revoke_by_jti(payload["jti"])


def introspect(token: str) -> dict[str, Any]:
    try:
        payload = crypto.decode(token)
        active = not is_revoked(payload["jti"]) and not is_expired(payload["exp"])
        return {
            "active": active,
            "sub": payload.get("sub"),
            "typ": payload.get("typ"),
            "exp": payload.get("exp"),
            "jti": payload.get("jti"),
        }
    except Exception:
        return {"active": False}