from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
from langchain.prompts import ChatPromptTemplate
from langchain_core.output_parsers import StrOutputParser
from langserve import add_routes 
import json
from langchain.schema.runnable import RunnableLambda
from langserve.schema import CustomUserType 
from structs111 import RecommendationResponse,ReportAnalysis
from functions111 import get_answer, extract_params_from_message, get_explaination, get_comfort, get_guidance,get_recommendation,get_paper_recommendation,get_report_anaysis, get_encourage
from models import GuideRequest, GuideResponse
from langchain_openai import ChatOpenAI
import uvicorn
import logging
import base64
from PIL import Image
import io

# 配置日志
logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

app = FastAPI(
    title="LangChain Server",
    version="1.0",
    description="A simple api server using Langchain's Runnable interfaces",
)

# Set all CORS enabled origins
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
    expose_headers=["*"],
)

# 创建 GPT 客户端
llm = ChatOpenAI(
    model="gpt-4o",  # 使用支持图像的模型
    openai_api_key="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",
    temperature=1.0,
    top_p=1.0
)

# 创建简单的提示模板
guide_prompt = ChatPromptTemplate.from_template("""
你是一个专业的医疗助手。请根据用户的问题和提供的图片来回答：

问题：{question}

请用简洁专业的语言回答。如果问题涉及图片内容，请根据图片内容来回答。
""")

@app.post("/gpt/guide/invoke", response_model=GuideResponse)
async def guide_endpoint(request: GuideRequest):
    try:
        # 记录接收到的请求数据
        request_dict = request.dict()
        logger.info(f"Received request: {request_dict}")
        
        # 检查screenshot字段
        if request.screenshot:
            logger.info(f"Screenshot received, length: {len(request.screenshot)}")
            logger.info(f"First 100 chars of screenshot: {request.screenshot[:100]}")
        else:
            logger.warning("No screenshot received in request")
        
        # 处理图片数据
        if request.screenshot:
            try:
                # 解码Base64图片数据
                image_data = base64.b64decode(request.screenshot)
                logger.info(f"Successfully decoded base64 image, size: {len(image_data)} bytes")
                
                image = Image.open(io.BytesIO(image_data))
                logger.info(f"Successfully opened image: size={image.size}, format={image.format}, mode={image.mode}")
                
                # 确保base64数据包含正确的data URL头部
                if not request.screenshot.startswith('data:image/'):
                    data_url = f"data:image/{image.format.lower()};base64,{request.screenshot}"
                else:
                    data_url = request.screenshot
                
                # 检查图片大小
                if len(image_data) > 20_000_000:  # 20MB limit
                    logger.warning(f"Image too large: {len(image_data)} bytes")
                    result = "图片太大，请压缩后重试"
                else:
                    # 构建GPT消息
                    messages = [
                        {
                            "role": "user",
                            "content": [
                                {
                                    "type": "text",
                                    "text": request.question
                                },
                                {
                                    "type": "image_url",
                                    "image_url": {
                                        "url": data_url
                                    }
                                }
                            ]
                        }
                    ]
                    
                    # 使用 GPT 处理问题和图片
                    response = llm.invoke(messages)
                    result = response.content
                
            except Exception as e:
                logger.error(f"Error processing image: {str(e)}")
                logger.error(f"Error details: {type(e).__name__}")
                result = "图片处理失败，请重试"
        else:
            # 如果没有图片，只处理问题
            messages = [
                {
                    "role": "user",
                    "content": [
                        {
                            "type": "text",
                            "text": request.question
                        }
                    ]
                }
            ]
            response = llm.invoke(messages)
            result = response.content
        
        logger.info(f"GPT response: {result}")
        
        response = GuideResponse(output=result)
        logger.info(f"Sending response: {response.dict()}")
        
        return response
    except Exception as e:
        error_msg = f"Error in guide_endpoint: {str(e)}"
        logger.error(error_msg)
        logger.error(f"Error type: {type(e).__name__}")
        logger.error(f"Error details: {str(e)}")
        return GuideResponse(output=f"Error: {str(e)}")

# 保留其他 LangServe 路由
# Q&A module
runnable = RunnableLambda(get_answer).with_types(
    input_type=str,
)

prompt = ChatPromptTemplate.from_template("{question}")

add_routes(
    app,
    prompt | (lambda x: x.messages[0].content) | runnable,
    path='/gpt/QA'
)

# ... 其他路由保持不变 ...

if __name__ == "__main__":
    uvicorn.run(app, host="127.0.0.1", port=8001, log_level="info")