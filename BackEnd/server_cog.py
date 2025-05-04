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
import uvicorn

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

# Recommendation module
rec_runnable = RunnableLambda(get_recommendation).with_types(
    input_type=str,
)

rec_prompt = ChatPromptTemplate.from_template("{id}")

add_routes(
    app,
    rec_prompt | (lambda x: x.messages[0].content) | rec_runnable,
    path='/gpt/recommendation'
)

# Papers module
paper_runnable = RunnableLambda(get_paper_recommendation).with_types(
    input_type=str,
)

paper_prompt = ChatPromptTemplate.from_template("{description}")

add_routes(
    app,
    paper_prompt | (lambda x: x.messages[0].content) | paper_runnable,
    path='/gpt/papers'
)
# report analysis module

report_analysis_runnable = RunnableLambda(get_report_anaysis).with_types(
    input_type= ReportAnalysis,
)

add_routes(
    app,
    report_analysis_runnable,
    path = '/gpt/report'
)
# explain module
explain_runnable = RunnableLambda(get_explaination).with_types(
     input_type=dict(params=dict),
)
prompt = ChatPromptTemplate.from_template("module_name:{module_name},patient_name:{patient_name}, language:{language}")
add_routes(
    app,
    prompt | (lambda x: extract_params_from_message(x.messages[0], 1)) | explain_runnable,
    path='/gpt/explain'
)

 # Comfort module
comfort_runnable = RunnableLambda(get_comfort).with_types(
     input_type=dict(params=dict),
 )
prompt = ChatPromptTemplate.from_template("module_name:{module_name},patient_name:{patient_name}, language:{language}, digital_person_name:{digital_person_name}, relationship:{relationship}, patient_sex:{patient_sex}")
add_routes(
    app,
    prompt | (lambda x: extract_params_from_message(x.messages[0], 2)) | comfort_runnable,
    path='/gpt/comfort'
)

 # Encourage module
encuorge_runnable = RunnableLambda(get_encourage).with_types(
     input_type=dict(params=dict),
 )
prompt = ChatPromptTemplate.from_template("module_name:{module_name},patient_name:{patient_name}, language:{language}, digital_person_name:{digital_person_name}, relationship:{relationship}, patient_sex:{patient_sex}")
add_routes(
    app,
    prompt | (lambda x: extract_params_from_message(x.messages[0], 2)) | encuorge_runnable,
    path='/gpt/encourage'
)

#  guide module
guide_runnable = RunnableLambda(get_guidance).with_types(
     input_type=dict(params=dict),
)
prompt = ChatPromptTemplate.from_template("module_name:{module_name},question:{question}, patient_name:{patient_name}, language:{language}, digital_person_name:{digital_person_name}, relationship:{relationship}, patient_sex:{patient_sex}")
add_routes(
    app,
    prompt | (lambda x: extract_params_from_message(x.messages[0], 3)) | guide_runnable,
    path='/gpt/guide'
)





if __name__ == "__main__":

    uvicorn.run(app, host="127.0.0.1", port=8001)