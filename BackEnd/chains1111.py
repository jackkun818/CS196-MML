from langchain_core.runnables import RunnablePassthrough
from langchain_core.output_parsers import StrOutputParser
from langchain_openai import ChatOpenAI # type: ignore
from json import dumps, loads
from typing import List, Dict, Any
from vectorstores111 import vectorstore_backgroundinfo, vectorstore_explain, vectorstore_comfort,vectorstore_guide 
from prompts111 import generate_prompt, answer_prompt,comfort_prompt, fallback_prompt, hallucination_prompt, prescription_recommendation_prompt, keyword_prompt, recommened_paper_prompt, explain_prompt,guide_prompt,report_analysis_prompt, encourage_prompt
# from structs import HallucinationEvaluator
from langchain.schema.runnable import RunnableLambda

# ✅ 使用你的真实 OpenAI API 密钥
llm = ChatOpenAI(
    model="gpt-4.1",
    openai_api_key="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",
    temperature=1.0,
    top_p=1.0
)

# llm = ChatOpenAI(model="Qwen2",openai_api_base=API_BASE_URL, openai_api_key=API_KEY,temperature=0.2)
background_retriever = vectorstore_backgroundinfo.as_retriever(search_kwargs={'k': 5})
explain_retriever = vectorstore_explain.as_retriever(search_kwargs={'k': 1})
guide_retriever = vectorstore_guide.as_retriever(search_kwargs={'k': 1})
#comfort module
comfort_retriever = vectorstore_comfort.as_retriever(search_kwargs={'k': 3})


class DictRunnable(RunnableLambda):
    def __init__(self, mapping: Dict[str, RunnableLambda]):
        self.mapping = mapping

    async def new_invoke(self, input_dict: Dict[str, Any]):
        results = {}
        
        context = input_dict.get('context', None)
        params = input_dict.get('params', {})

        if context is not None:
            results['context'] = await self.mapping['context'].invoke(context)
        if params:
            results['params'] = await self.mapping['params'].invoke(params)
        
        return results
    
def params_analysis(input_dict: dict):
    if input_dict.get("params").get("module_id") == 1:
        return{
            "patient_name": input_dict.get("params").get("patient_name"),
            "module_name": input_dict.get("params").get("module_name"),
            "language": input_dict.get("params").get("language"),
            # "temperature": input_dict.get("params").get("temperature"),
            # "sensitivity": input_dict.get("params").get("sensitivity"),
            "module_id": input_dict.get("params").get("module_id"),
            "context": input_dict.get("context")
        }
    if input_dict.get("params").get("module_id") == 2:
        return{
            "module_name":input_dict.get("params").get("module_name"),
            "patient_name": input_dict.get("params").get("patient_name"),
            "language": input_dict.get("params").get("language"),
            "digital_person_name": input_dict.get("params").get("digital_person_name"),
            "relationship": input_dict.get("params").get("relationship"),
            "patient_sex": input_dict.get("params").get("patient_sex"),
            #"current_details": input_dict.get("params").get("current_details"),
            # "temperature": input_dict.get("params").get("temperature"),
            # "sensitivity": input_dict.get("params").get("sensitivity"),
            "module_id": input_dict.get("params").get("module_id"),
            "context": input_dict.get("context")
        }
    if input_dict.get("params").get("module_id") == 3:
        return{
            "patient_name": input_dict.get("params").get("patient_name"),
            "module_name": input_dict.get("params").get("module_name"),
            "question": input_dict.get("params").get("question"),
            "language": input_dict.get("params").get("language"),
            "digital_person_name": input_dict.get("params").get("digital_person_name"),
            "relationship": input_dict.get("params").get("relationship"),
            "patient_sex": input_dict.get("params").get("patient_sex"),
            # "temperature": input_dict.get("params").get("temperature"),
            # "sensitivity": input_dict.get("params").get("sensitivity"),
            "module_id": input_dict.get("params").get("module_id"),
            "context": input_dict.get("context")
        }
        
def tpye_convert_explain(dict: dict)-> list:
    module_name = dict.get("module_name")
    print("module_name={}".format(module_name))
    return[module_name]

# def tpye_convert_comfort(dict: dict)-> list:
#     current_details = dict.get("current_details")
#     module_name = dict.get("module_name")
#     return[current_details,module_name]

def tpye_convert_guide(dict: dict)-> list:
    module_name = dict.get("module_name")
    question = dict.get("question")
    return[module_name,question]

generate_queries = (
    {"question": RunnablePassthrough()}
    | generate_prompt
    | llm
    | StrOutputParser()
    | (lambda x: x.split("\n"))
)

def get_context_union(docs: List[List]):
    all_docs = [dumps({"page_content": d.page_content, "metadata": d.metadata}) for doc in docs for d in doc]
    unique_docs = list(set(all_docs))
    return [loads(doc)["page_content"] for doc in unique_docs]

retrieval_chain = (
    {'question': RunnablePassthrough()}
    | generate_queries
    | background_retriever.map()
    | get_context_union
)
#explain module
retrieval_chain_explain = (
    RunnablePassthrough()
    | RunnableLambda(tpye_convert_explain)
    | explain_retriever.map()
)

#comfort module
# retrieval_chain_comfort = (
#     RunnablePassthrough()
#     | RunnableLambda(tpye_convert_comfort)
#     | comfort_retriever.map()
# )

# #guide module
# retrieval_chain_guide = (
#     RunnablePassthrough()
#     | RunnableLambda(tpye_convert_guide)
#     | guide_retriever.map()
# )
#guide module
def retrieval_chain_guide(module_name=None,question=None):
    content=module_name+'模块'
    sim_docs = vectorstore_guide.similarity_search(content,k=5)
    return sim_docs

def select_relevance_guide(module_name=None,sim_docs=None):
    if not sim_docs or len(sim_docs) == 0:
        # 如果没有找到相关文档，返回一个默认的文档
        return {
            "page_content": f"这是一个关于{module_name}模块的默认回答。请告诉我您想了解什么具体内容？"
        }
    
    # 如果有文档，尝试找到最相关的
    for sim_doc in sim_docs:
        if module_name in sim_doc.page_content:
            return sim_doc
    
    # 如果没找到包含模块名的文档，返回第一个文档
    return sim_docs[0]

# multi_query_chain = (
#     RunnablePassthrough() |
#     RunnableLambda(params_analysis) |
#     answer_prompt |
#     llm |
#     StrOutputParser()
# )
multi_query_chain = (
    {'context': retrieval_chain, 'question': RunnablePassthrough()}
    | answer_prompt
    | llm
    | StrOutputParser()
)

#explain module
multi_query_chain_explain = (
    RunnablePassthrough() |
    RunnableLambda(params_analysis) |
    explain_prompt |
    llm |
    StrOutputParser()
)

#confort module
multi_query_chain_comfort = (
    RunnablePassthrough() |
    RunnableLambda(params_analysis) |
    comfort_prompt |
    llm |
    StrOutputParser()
)

multi_query_chain_encourage = (
    RunnablePassthrough() |
    RunnableLambda(params_analysis) |
    encourage_prompt |
    llm |
    StrOutputParser()
)

#guide module
multi_query_chain_guide = (
    RunnablePassthrough() |
    guide_prompt |
    llm |
    StrOutputParser()
)

def answer_hallucination_check_guide(params: dict, docs: List[str]):
    return multi_query_chain_guide.invoke({"question": params.get("question", "")})

fallback_chain = fallback_prompt | llm | StrOutputParser()

# hallucination_llm = llm.with_structured_output(HallucinationEvaluator)

hallucination_chain = hallucination_prompt | llm


prescription_recommendation_chain = (
    {'patient_data': RunnablePassthrough(), 'similar_cases_prescription': RunnablePassthrough()}
    | prescription_recommendation_prompt
    | llm
    | StrOutputParser()
)

keyword_chain = keyword_prompt | llm | StrOutputParser()

recommened_paper_chain = (
    {'description': RunnablePassthrough(), 'abstract': RunnablePassthrough()}
    | recommened_paper_prompt
    | llm
    | StrOutputParser()
)

report_analysis_chain = (
    {'module_name': RunnablePassthrough(),'module_info':  RunnablePassthrough(), 'data': RunnablePassthrough()}
    | report_analysis_prompt
    | llm
    | StrOutputParser()
)