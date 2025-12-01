from pydantic import BaseModel
from datetime import datetime
from typing import List
from decimal import Decimal

class TokenResponse(BaseModel):
    token: str

class AuthRequest(BaseModel):
    name: str
    password: str

class OrderResponse(BaseModel):
    id: int
    user_id: int
    created_at: datetime

class GoodResponse(BaseModel):
    id: int
    name: str
    count: int
    price: Decimal

class OrderDetailsResponse(BaseModel):
    order: OrderResponse
    goods: List[GoodResponse]

class UserInfo(BaseModel):
    id: int
    name: str