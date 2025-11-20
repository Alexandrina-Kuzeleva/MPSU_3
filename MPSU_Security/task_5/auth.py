# auth.py
import hashlib
from passlib.context import CryptContext
from validation import validate_password
from user import User, UserStorage

pwd_context = CryptContext(schemes=["bcrypt"], deprecated="auto")


def register_user(storage: UserStorage, username: str, email: str, password: str) -> User:
    if User.exists(storage, username):
        raise ValueError("Пользователь с таким username уже существует")
    validate_password(password)
    bcrypt_hash = pwd_context.hash(password)
    user = User(username=username, email=email, password_hash=bcrypt_hash)
    user.save(storage)
    return user


def _load_user(storage: UserStorage, username: str):
    return User.load(storage, username)


def _update_user(storage: UserStorage, username: str, **updates):
    user = _load_user(storage, username)
    if user is None:
        return
    for key, value in updates.items():
        setattr(user, key, value)
    user.save(storage)  


def _is_legacy_md5(hash_value: str) -> bool:
    return len(hash_value) == 32 and all(c in "0123456789abcdefABCDEF" for c in hash_value)


def _increment_failed_attempts(storage: UserStorage, username: str):
    user = _load_user(storage, username)
    if user is None:
        return
    current = getattr(user, "failed_login_attempts", 0)
    new_count = current + 1
    _update_user(
        storage,
        username,
        failed_login_attempts=new_count,
        account_locked=(new_count >= 5),
    )


def _reset_failed_attempts(storage: UserStorage, username: str):
    _update_user(
        storage,
        username,
        failed_login_attempts=0,
        account_locked=False,
    )


def is_account_locked(storage: UserStorage, username: str) -> bool:
    user = _load_user(storage, username)
    return user is not None and getattr(user, "account_locked", False)


def verify_credentials(storage: UserStorage, username: str, password: str) -> bool:
    user = _load_user(storage, username)
    if user is None:
        return False

    if getattr(user, "account_locked", False):
        return False

    current_hash = user.password_hash
    correct = False

    if _is_legacy_md5(current_hash):
        candidate = hashlib.md5(password.encode("utf-8")).hexdigest()
        correct = (candidate == current_hash)
    else:
        correct = pwd_context.verify(password, current_hash)

    if correct:
        if _is_legacy_md5(current_hash):
            new_hash = pwd_context.hash(password)
            _update_user(
                storage,
                username,
                password_hash=new_hash,
                failed_login_attempts=0,
                account_locked=False,
            )
        else:
            _reset_failed_attempts(storage, username)
        return True
    else:
        _increment_failed_attempts(storage, username)
        return False