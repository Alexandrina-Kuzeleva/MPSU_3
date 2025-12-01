import os
from sqlalchemy.ext.asyncio import create_async_engine, AsyncSession
from sqlalchemy.orm import sessionmaker
from dotenv import load_dotenv

load_dotenv()

DATABASE_URL = os.getenv("DATABASE_URL")

engine = None
AsyncSessionLocal = None

def init_db():
    global engine, AsyncSessionLocal
    
    if DATABASE_URL.startswith("postgresql://"):
        DATABASE_URL_ASYNC = DATABASE_URL.replace("postgresql://", "postgresql+asyncpg://")
    else:
        DATABASE_URL_ASYNC = DATABASE_URL
    
    engine = create_async_engine(
        DATABASE_URL_ASYNC,
        echo=True,
        future=True,
        pool_pre_ping=True
    )
    
    AsyncSessionLocal = sessionmaker(
        engine, 
        class_=AsyncSession, 
        expire_on_commit=False
    )

async def get_db():
    if AsyncSessionLocal is None:
        init_db()
    
    async with AsyncSessionLocal() as session:
        try:
            yield session
            await session.commit()
        except Exception:
            await session.rollback()
            raise
        finally:
            await session.close()