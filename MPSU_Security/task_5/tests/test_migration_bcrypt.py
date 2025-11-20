import hashlib
from passlib.context import CryptContext

from user import InMemoryUserStorage, User
import auth


def test_md5_user_is_migrated_to_bcrypt_on_successful_login(monkeypatch):
    store = InMemoryUserStorage()

    username = "bob"
    raw = "Pass123!"
    md5_hex = hashlib.md5(raw.encode()).hexdigest()
    User(username=username, email="bob@example.com", password_hash=md5_hex).save(store)

    assert auth.verify_credentials(store, username, raw) is True

    migrated = User.load(store, username)
    assert migrated is not None

    ctx = CryptContext(schemes=["bcrypt"])
    assert ctx.verify(raw, migrated.password_hash)


def test_register_stores_bcrypt_hash(monkeypatch):
    store = InMemoryUserStorage()

    _ = auth.register_user(store, "bob", "bob@example.com", "Pass123!")
    saved = User.load(store, "bob")
    assert saved is not None

    ctx = CryptContext(schemes=["bcrypt"])
    assert ctx.verify("Pass123!", saved.password_hash)
