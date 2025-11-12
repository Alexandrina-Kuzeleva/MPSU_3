import hashlib
import time
from validation import validate_password
from user import User, UserStorage
from passlib.hash import bcrypt
from passlib.context import CryptContext

pwd_context = CryptContext(
    schemes=["bcrypt"],
    bcrypt__ident="2b",
    bcrypt__min_rounds=12
)

def register_user(storage: UserStorage, username: str, email: str, password: str) -> User:
    if User.exists(storage, username):
        raise ValueError("Пользователь с таким username уже существует")

    validate_password(password)
    password_hash = pwd_context.hash(password)
    user = User(username=username, email=email, password_hash=password_hash)
    user.save(storage)
    return user

def verify_credentials(storage: UserStorage, username: str, password: str) -> bool:
    user = User.load(storage, username)
    if user is None:
        return False
    
    if len(user.password_hash) == 32:
        if user.password_hash == hashlib.md5(password.encode()).hexdigest():
            user.password_hash = pwd_context.hash(password)
            user.save(storage)
            return True
        else:
            return False
    
    try:
        return pwd_context.verify(password, user.password_hash)
    except Exception:
        return False