from fastapi import FastAPI, Depends, HTTPException, Path
from typing import Annotated
from sqlalchemy.ext.asyncio import AsyncSession
from sqlalchemy import select
from contextlib import asynccontextmanager
import secrets

from app.db import init_db, get_db
from app.auth import get_user_by_token, verify_password
from app.models import User, Token, Order, Good
from app.schemas import AuthRequest, TokenResponse, OrderResponse, OrderDetailsResponse, UserInfo
from app.dependencies import get_pagination_params, PaginationParams

@asynccontextmanager
async def lifespan(app: FastAPI):
    init_db()
    yield

app = FastAPI(title="SQLi Lab (ORM Edition)", lifespan=lifespan)

@app.post("/auth/token", response_model=TokenResponse)
async def auth_token(
    body: AuthRequest,
    db: AsyncSession = Depends(get_db)
):
    stmt = select(User).where(User.name == body.name)
    result = await db.execute(stmt)
    user = result.scalar_one_or_none()
    
    if not user:
        raise HTTPException(status_code=401, detail="Invalid credentials")
    
    if not verify_password(body.password, user.password_hash):
        raise HTTPException(status_code=401, detail="Invalid credentials")
    
    stmt = select(Token).where(
        Token.user_id == user.id,
        Token.is_valid == True
    ).order_by(Token.created_at.desc())
    
    result = await db.execute(stmt)
    existing_token = result.scalar_one_or_none()
    
    if existing_token:
        token = existing_token.value
    else:
        token = secrets.token_urlsafe(64)
        new_token = Token(
            user_id=user.id,
            value=token,
            is_valid=True
        )
        db.add(new_token)
        await db.commit()
        await db.refresh(new_token)
    
    return TokenResponse(token=token)

@app.get("/orders")
async def list_orders(
    user: Annotated[UserInfo, Depends(get_user_by_token)],
    pagination: PaginationParams = Depends(get_pagination_params),
    db: AsyncSession = Depends(get_db)
):
    stmt = select(Order).where(
        Order.user_id == user.id
    ).order_by(
        Order.created_at.desc()
    ).limit(pagination.limit).offset(pagination.offset)
    
    result = await db.execute(stmt)
    orders = result.scalars().all()
    
    return [
        OrderResponse(
            id=order.id,
            user_id=order.user_id,
            created_at=order.created_at
        )
        for order in orders
    ]

@app.get("/orders/{order_id}", response_model=OrderDetailsResponse)
async def order_details(
    user: Annotated[UserInfo, Depends(get_user_by_token)],
    order_id: int = Path(..., title="ID заказа", ge=1),
    db: AsyncSession = Depends(get_db)
):
    stmt = select(Order).where(
        Order.id == order_id,
        Order.user_id == user.id
    )
    
    result = await db.execute(stmt)
    order = result.scalar_one_or_none()
    
    if not order:
        raise HTTPException(status_code=404, detail="Order not found")
    
    stmt = select(Good).where(Good.order_id == order_id)
    result = await db.execute(stmt)
    goods = result.scalars().all()
    
    return OrderDetailsResponse(
        order=OrderResponse(
            id=order.id,
            user_id=order.user_id,
            created_at=order.created_at
        ),
        goods=[
            {
                "id": good.id,
                "name": good.name,
                "count": good.count,
                "price": good.price
            }
            for good in goods
        ]
    )