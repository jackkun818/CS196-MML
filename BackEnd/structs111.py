from langchain_core.pydantic_v1 import BaseModel, Field
from typing import List, Dict, Literal

from langserve import CustomUserType

class HallucinationEvaluator(BaseModel):
    """Binary score for hallucination present in generation answer."""

    grade: str = Field(...,
        description="Whether answer is grounded in the facts, 'yes' or 'no'"
    )

class RecommendationResponse(BaseModel):
    similar_cases: List[Dict]
    recommendation: str
    
class ReportAnalysis(CustomUserType):
    id: str
    module: Literal['PUME','REVE']
    date: str