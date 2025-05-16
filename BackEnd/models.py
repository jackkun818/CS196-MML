from pydantic import BaseModel, Field
from typing import List, Optional

class GuideRequest(BaseModel):
    module_name: str = "注意力分配"
    question: str
    patient_name: str = "Alice Johnson"
    patient_sex: str = "女"
    language: str = "Chinese"
    context: str = ""
    screenshot: str = None  # 添加screenshot字段，用于接收Base64编码的图片数据

class GuideResponse(BaseModel):
    output: str

# Rebuild all models
GuideRequest.model_rebuild()
GuideResponse.model_rebuild() 