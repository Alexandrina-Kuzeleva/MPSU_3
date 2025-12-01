from fastapi import Query
from pydantic import BaseModel, Field

class PaginationParams(BaseModel):
    limit: int = Field(default=10, ge=1, le=100, description="Количество записей")
    offset: int = Field(default=0, ge=0, description="Смещение")

async def get_pagination_params(
    limit: int = Query(10, ge=1, le=100),
    offset: int = Query(0, ge=0)
) -> PaginationParams:
    return PaginationParams(limit=limit, offset=offset)